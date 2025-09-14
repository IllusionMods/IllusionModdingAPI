using BepInEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BepInEx.Configuration;
using KKAPI.Utilities;
using UnityEngine;

namespace KKAPI.Studio.UI.Toolbars
{
    // todo rename ToolbarManager, make MB, move namespace
    /// <summary>
    /// Add custom buttons to studio toolbars.
    /// You can find a button template here https://github.com/IllusionMods/IllusionModdingAPI/blob/master/doc/studio%20icon%20template.png
    /// </summary>
    public static class ToolbarManager
    {
        internal static readonly HashSet<ToolbarControlBase> Buttons = new HashSet<ToolbarControlBase>();
        private static ConfigEntry<string> _positionSetting;

        private static bool _studioLoaded;
        private static bool _dirty;

        /// <summary>
        /// Adds a custom toolbar toggle button to the left toolbar.
        /// </summary>
        /// <param name="button">The custom toolbar button to add.</param>
        /// <returns>True if the button was added, false if already present.</returns>
        public static bool AddLeftToolbarControl(ToolbarControlBase button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (button.IsDisposed) throw new ObjectDisposedException(nameof(button));

            if (!StudioAPI.InsideStudio)
            {
                KoikatuAPI.Logger.LogDebug($"Tried to run StudioAPI.AddLeftToolbarToggle for {button.ButtonID} outside of studio!");
                return false;
            }

            lock (Buttons)
            {
                if (Buttons.Add(button))
                {
                    RequestToolbarRelayout();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Queues an update of the toolbar interface if necessary.
        /// </summary>
        public static void RequestToolbarRelayout()
        {
            lock (Buttons)
            {
                if (!_studioLoaded || _dirty) return;
                _dirty = true;
                ThreadingHelper.Instance.StartSyncInvoke(UpdateInterface);
            }
        }

        /// <summary>
        /// Called when the studio scene is loaded. Initializes and updates the toolbar interface. Must be called on main thread.
        /// </summary>
        internal static void OnStudioLoaded()
        {
            _dirty = true;

            _positionSetting = KoikatuAPI.Instance.Config.Bind("Toolbars", "LeftToolbarButtonPositions", "", new ConfigDescription("Stores desired positions of custom toolbar buttons. Do not edit this setting manually.", null, new BrowsableAttribute(false)));

            ToolbarControlBase.InitToolbar();
            _studioLoaded = true;
            UpdateInterface();
        }

        // Must be called on main thread
        private static void UpdateInterface()
        {
            if (!_studioLoaded) return;

            lock (Buttons)
            {
                if (!_dirty) return;

                //LoadButtonPositions();

                var takenPositions = new HashSet<KeyValuePair<int, int>>();
                var positionNotSet = new List<ToolbarControlBase>();
                var nonVisibleWithPosition = new List<ToolbarControlBase>();

                // First pass: create controls and classify buttons
                foreach (var customToolbarToggle in Buttons.OrderByDescending(x => x is ToolbarControlAdapter).ThenBy(x => x.ButtonID))
                {
                    customToolbarToggle.CreateControl();

                    if (!customToolbarToggle.Visible.Value)
                    {
                        // Track non-visible buttons with a set position
                        if (customToolbarToggle.DesiredRow >= 0 && customToolbarToggle.DesiredColumn >= 0)
                            nonVisibleWithPosition.Add(customToolbarToggle);
                        continue;
                    }

                    // Now try to set position
                    var desiredRow = customToolbarToggle.DesiredRow;
                    var desiredCol = customToolbarToggle.DesiredColumn;
                    if (desiredRow >= 0 && desiredCol >= 0)
                    {
                        // Try to set to desired position, if taken then move right until free spot is found
                        while (takenPositions.Contains(new KeyValuePair<int, int>(desiredRow, desiredCol)))
                            desiredCol++;
                        customToolbarToggle.SetActualPosition(desiredRow, desiredCol, true);
                        takenPositions.Add(new KeyValuePair<int, int>(desiredRow, desiredCol));
                    }
                    else
                    {
                        positionNotSet.Add(customToolbarToggle);
                    }
                }

                // Second pass: move non-visible buttons out of the way if their position is now taken
                foreach (var btn in nonVisibleWithPosition)
                {
                    var desiredRow = btn.DesiredRow;
                    var desiredCol = btn.DesiredColumn;
                    var pos = new KeyValuePair<int, int>(desiredRow, desiredCol);
                    if (takenPositions.Contains(pos))
                    {
                        // Move to next free position in the same row
                        do
                        {
                            desiredCol++;
                            pos = new KeyValuePair<int, int>(desiredRow, desiredCol);
                        } while (takenPositions.Contains(pos));
                        btn.SetActualPosition(desiredRow, desiredCol, true);
                    }
                    takenPositions.Add(pos);
                }

                // Now place all buttons that didn't have a desired position set
                // First find the first unused position in the two leftmost columns
                var addedPos = 0;
                foreach (var btn in positionNotSet)
                {
                    // Find the next free position in the left two columns
                    KeyValuePair<int, int> newPos;
                    do
                    {
                        var row = addedPos / 2;
                        var col = addedPos % 2;
                        newPos = new KeyValuePair<int, int>(row, col);
                        addedPos++;
                    } while (takenPositions.Contains(newPos));

                    btn.SetActualPosition(newPos.Key, newPos.Value, false); // Do not save position
                    takenPositions.Add(newPos);
                }

                _dirty = false;

                //SaveButtonPositions();
            }
        }

        //todo completely borked because button IDs are not unique
        /// <summary>
        /// Saves the desired positions of all toolbar buttons to the config entry.
        /// </summary>
        private static void SaveButtonPositions()
        {
            if (_positionSetting == null)
                return;

            // Format: ButtonID:row:column|ButtonID2:row:column|...
            var entries = Buttons
                          .Where(b => b.DesiredRow >= 0 && b.DesiredColumn >= 0)
                          .OrderBy(b => b.ButtonID)
                          .Select(b => $"{b.ButtonID}:{b.DesiredRow}:{b.DesiredColumn}");

            _positionSetting.Value = string.Join("|", entries.ToArray());
        }

        /// <summary>
        /// Loads button positions from the config entry and applies them to the registered buttons.
        /// </summary>
        private static void LoadButtonPositions()
        {
            if (_positionSetting == null || string.IsNullOrEmpty(_positionSetting.Value))
                return;

            // Format: ButtonID:row:column|ButtonID2:row:column|...
            var entries = _positionSetting.Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var positions = new Dictionary<string, KeyValuePair<int, int>>();
            foreach (var entry in entries)
            {
                var parts = entry.Split(':');
                if (parts.Length != 3) continue;
                var id = parts[0];
                if (int.TryParse(parts[1], out int row) && int.TryParse(parts[2], out int col))
                {
                    positions[id] = new KeyValuePair<int, int>(row, col);
                }
            }

            lock (Buttons)
            {
                foreach (var btn in Buttons)
                {
                    if (positions.TryGetValue(btn.ButtonID, out var pos))
                    {
                        btn.DesiredRow = pos.Key;
                        btn.DesiredColumn = pos.Value;
                    }
                }
            }
        }
    }
}
