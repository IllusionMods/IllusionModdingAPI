using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KKAPI.Studio.UI.Toolbars
{
    /// <summary>
    /// Add custom buttons to studio toolbars. Thread-safe.
    /// You can find a button template here https://github.com/IllusionMods/IllusionModdingAPI/blob/master/doc/studio%20icon%20template.png
    /// </summary>
    public static class ToolbarManager
    {
        private static readonly HashSet<ToolbarControlBase> _buttons = new HashSet<ToolbarControlBase>();
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

            lock (_buttons)
            {
                if (_buttons.Add(button))
                {
                    ToolbarDataStorage.ApplyInitialPosition(button);
                    RequestToolbarRelayout();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the button from the toolbar and destroys it. The button must be recreated to be used again.
        /// </summary>
        public static void RemoveControl(ToolbarControlBase toolbarControlBase)
        {
            lock (_buttons)
            {
                _buttons.Remove(toolbarControlBase);
                toolbarControlBase.Dispose();
            }
        }

        /// <summary>
        /// Get all toolbar buttons added so far.
        /// </summary>
        public static ToolbarControlBase[] GetAllButtons()
        {
            lock (_buttons)
                return _buttons.ToArray();
        }

        /// <summary>
        /// Queues an update of the toolbar interface if necessary.
        /// </summary>
        public static void RequestToolbarRelayout()
        {
            lock (_buttons)
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
            lock (_buttons)
            {
                _dirty = true;

                foreach (var stockButton in ToolbarControlBase.InitToolbar())
                {
                    _buttons.Add(stockButton);
                    ToolbarDataStorage.ApplyInitialPosition(stockButton);
                }

                _studioLoaded = true;
                UpdateInterface();
            }
        }

        // Must be called on main thread
        private static void UpdateInterface()
        {
            lock (_buttons)
            {
                if (!_studioLoaded) return;
                if (!_dirty) return;

                var takenPositions = new HashSet<KeyValuePair<int, int>>();
                var positionNotSet = new List<ToolbarControlBase>();
                var nonVisibleWithPosition = new List<ToolbarControlBase>();

                // First pass: create controls and classify buttons
                foreach (var customToolbarToggle in _buttons.OrderByDescending(x => x is ToolbarControlAdapter).ThenBy(x => x.ButtonID).ThenBy(x => x.Owner.Info.Metadata.GUID))
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

                ToolbarDataStorage.SaveButtonPositions(_buttons);
            }
        }
    }
}
