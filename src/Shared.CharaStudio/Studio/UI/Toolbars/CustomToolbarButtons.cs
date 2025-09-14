using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Add custom buttons to studio toolbars.
    /// You can find a button template here https://github.com/IllusionMods/IllusionModdingAPI/blob/master/doc/studio%20icon%20template.png
    /// </summary>
    public static partial class CustomToolbarButtons
    {
        internal static readonly HashSet<CustomToolbarControlBase> Buttons = new HashSet<CustomToolbarControlBase>();
        private static bool _studioLoaded;
        private static bool _dirty;

        /// <summary>
        /// Adds a custom toolbar toggle button to the left toolbar.
        /// </summary>
        /// <param name="button">The custom toolbar button to add.</param>
        /// <returns>True if the button was added, false if already present.</returns>
        public static bool AddLeftToolbarControl(CustomToolbarControlBase button)
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
                if (_dirty || !_studioLoaded) return;
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
            CustomToolbarControlBase.InitToolbar();
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

                var takenPositions = new HashSet<KeyValuePair<int, int>>();
                var positionNotSet = new List<CustomToolbarControlBase>();
                foreach (var customToolbarToggle in Buttons.OrderByDescending(x => x is ToolbarControlPlaceholder).ThenBy(x => x.ButtonID))
                {
                    customToolbarToggle.CreateControl();

                    if (!customToolbarToggle.Visible.Value)
                        continue;

                    // Now try to set position
                    // TODO: saving and loading positions
                    var desiredRow = customToolbarToggle.DesiredRow;
                    var desiredCol = customToolbarToggle.DesiredColumn;
                    if (desiredRow >= 0 && desiredCol >= 0)
                    {
                        // Try to set to desired position, if taken then move right until free spot is found
                        while (takenPositions.Contains(new KeyValuePair<int, int>(desiredRow, desiredCol)))
                            desiredCol++;
                        customToolbarToggle.SetActualPosition(desiredRow, desiredCol);
                        takenPositions.Add(new KeyValuePair<int, int>(desiredRow, desiredCol));
                    }
                    else
                    {
                        positionNotSet.Add(customToolbarToggle);
                    }
                }

                // Now place all buttons that didn't have a desired position set
                // First find the last used position in the two leftmost columns
                var lastPos = takenPositions.Where(x => x.Value < 2).OrderByDescending(x => x.Key).ThenByDescending(x => x.Value).First();
                var addedPos = (lastPos.Key - 1) * 2 + lastPos.Value;
                foreach (var btn in positionNotSet)
                {
                    // Find the next free position
                    KeyValuePair<int, int> newPos;
                    do
                    {
                        addedPos++;
                        var row = addedPos / 2;
                        var col = addedPos % 2;
                        newPos = new KeyValuePair<int, int>(row, col);
                    } while (takenPositions.Contains(newPos));

                    btn.SetActualPosition(newPos.Key, newPos.Value);
                    takenPositions.Add(newPos);
                }

                _dirty = false;
            }
        }
    }

}