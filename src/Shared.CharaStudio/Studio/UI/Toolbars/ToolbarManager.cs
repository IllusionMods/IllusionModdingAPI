using BepInEx;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KKAPI.Studio.UI.Toolbars
{
    /// <summary>
    /// Add custom buttons to studio toolbars. Thread-safe.
    /// You can find a button template in "\doc\studio icon template.png"
    /// </summary>
    public static class ToolbarManager
    {
        private static readonly HashSet<ToolbarControlBase> _Buttons = new HashSet<ToolbarControlBase>();
        private static bool _studioLoaded;
        private static bool _dirty;
        private static bool _editMode;
        private static bool EditMode
        {
            get => _editMode;
            set
            {
                if (_editMode != value)
                {
                    _editMode = value;
                    RequestToolbarRelayout();
                }
            }
        }

        private static readonly GlobalContextMenu.Entry[] _ContextMenuItems = {
            GlobalContextMenu.Entry.Create(() => {
                var originalColor = GUI.backgroundColor;
                if (EditMode) GUI.backgroundColor = Color.cyan;

                bool newMode = GUILayout.Toggle(EditMode, new GUIContent("Edit Mode", "Allows you to hide unnecessary buttons from the left toolbar."), "Button");

                if (EditMode) GUI.backgroundColor = originalColor;

                if (newMode != EditMode)
                {
                    EditMode = newMode;
                    RequestToolbarRelayout();

                    return true;
                }
                return false;
            }),
            GlobalContextMenu.Entry.Create(new GUIContent("Rearrange buttons", "Discard current button positions and line them up along the edge of the screen."), () =>
            {
                ToolbarDataStorage.ResetPositions();
                foreach (var butt in _Buttons)
                    butt.DesiredPosition = null;
                RequestToolbarRelayout();
            }),
            GlobalContextMenu.Entry.Create(new GUIContent("Show all buttons", "Reset state of all buttons to be visible."), ToolbarDataStorage.ResetHidden)
        };

        /// <summary>
        /// Adds a custom toolbar toggle button to the left toolbar.
        /// </summary>
        /// <param name="button">The button control to add.</param>
        /// <returns>True if added successfully, false otherwise.</returns>
        public static bool AddLeftToolbarControl(ToolbarControlBase button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (button.IsDisposed) throw new ObjectDisposedException(nameof(button));

            if (!StudioAPI.InsideStudio)
            {
                KoikatuAPI.Logger.LogDebug($"Tried to run StudioAPI.AddLeftToolbarToggle for {button.ButtonID} outside of studio!");
                return false;
            }

            lock (_Buttons)
            {
                if (_Buttons.Add(button))
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
        /// If you want to temporarily hide a button, set its Visible property to false instead.
        /// </summary>
        /// <param name="toolbarControlBase">The button to remove.</param>
        public static void RemoveControl(ToolbarControlBase toolbarControlBase)
        {
            lock (_Buttons)
            {
                _Buttons.Remove(toolbarControlBase);
                toolbarControlBase.Dispose();
            }
        }

        /// <summary>
        /// Get an array of all toolbar buttons added so far.
        /// </summary>
        /// <param name="includeInvisible">If true, returns hidden buttons as well.</param>
        /// <returns>Array of buttons.</returns>
        public static ToolbarControlBase[] GetAllButtons(bool includeInvisible)
        {
            lock (_Buttons)
                return includeInvisible ?
                    _Buttons.ToArray() : _Buttons.Where(x => x.Visible.Value).ToArray();
        }

        /// <summary>
        /// Queues an update of the toolbar interface layout, which will be done on the next frame if necessary.
        /// Shouldn't need to be called manually unless button positions are changed externally.
        /// </summary>
        public static void RequestToolbarRelayout()
        {
            lock (_Buttons)
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
            lock (_Buttons)
            {
                _dirty = true;
                foreach (var stockButton in ToolbarControlBase.InitToolbar())
                {
                    _Buttons.Add(stockButton);
                    ToolbarDataStorage.ApplyInitialPosition(stockButton);
                }

                _studioLoaded = true;
                UpdateInterface();
            }

            var manipulateBtn = GameObject.Find("StudioScene/Canvas System Menu/00_Draw/Button Manipulate");
            if (manipulateBtn != null)
                manipulateBtn.AddComponent<ContextMenuTrigger>();
        }

        private class ContextMenuTrigger : MonoBehaviour, IPointerClickHandler
        {
            public void OnPointerClick(PointerEventData eventData)
            {
                if (eventData.button == PointerEventData.InputButton.Right)
                    GlobalContextMenu.Show("Left Toolbar", _ContextMenuItems);
            }
        }

        private sealed class ToolbarButtonEditOverlay : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
        {
            private static readonly GlobalTooltips.Tooltip _Tooltip = new GlobalTooltips.Tooltip("Currently editing the toolbar.\n" +
                                                                                                 "Left-click to enable or disable a button (red = disabled).\n" +
                                                                                                 "Drag to change button positions." +
                                                                                                 "Middle-click to exit edit mode.");

            public ToolbarControlBase TargetButtonControl;
            public bool IsButtonHidden;

            private Image _imageComponent;

            private void Awake()
            {
                _imageComponent = GetComponent<Image>();
                GlobalTooltips.RegisterTooltip(gameObject, _Tooltip);
            }

            private void Update()
            {
                // Only run animation when in Edit Mode to save performance
                if (EditMode && _imageComponent != null)
                {
                    // Calculate a sine wave for the pulsing effect (Cycle approx. 2 seconds)
                    float sinWave = Mathf.Sin(Time.time * Mathf.PI);

                    if (IsButtonHidden)
                    {
                        // Hidden Item: Flash RED
                        // Alpha oscillates between 0.5 and 0.8 for high visibility
                        float alpha = 0.65f + 0.15f * sinWave;
                        _imageComponent.color = new Color(1f, 0f, 0f, alpha);
                    }
                    else
                    {
                        // Visible Item: Flash WHITE (Subtle)
                        // Indicates that interaction is blocked and Edit Mode is active
                        float alpha = 0.25f + 0.15f * sinWave;
                        _imageComponent.color = new Color(1f, 1f, 1f, alpha);
                    }
                }
            }

            private void ToggleHide()
            {
                if (TargetButtonControl == null) return;
                ToolbarDataStorage.ToggleHidden(TargetButtonControl);
            }

            public void OnPointerClick(PointerEventData eventData)
            {
                if (!EditMode) return;
                if (eventData.dragging) return;

                if (eventData.button == PointerEventData.InputButton.Middle)
                    EditMode = false;
                else
                    ToggleHide();
            }

            // Need these empty pointer handlers because without them OnPointerClick never gets called for some reason (a parent object handles it maybe?)
            public void OnPointerUp(PointerEventData eventData) { }
            public void OnPointerDown(PointerEventData eventData) { }
        }

        private static void UpdateInterface()
        {
            lock (_Buttons)
            {
                if (!_studioLoaded) return;
                if (!_dirty) return;

                var takenPositions = new HashSet<ToolbarPosition>();
                var positionNotSet = new List<ToolbarControlBase>();

                var allButtons = _Buttons.OrderByDescending(x => x is ToolbarControlAdapter) // Base game buttons first
                                         .ThenBy(x => x.ButtonID) // Allow plugins to control order with ButtonID
                                         .ThenBy(x => x.Owner.Info.Metadata.GUID); // Keep order stable if there's duplicate ButtonIDs

                foreach (var button in allButtons)
                {
                    button.CreateControl();
                    var btnObj = button.ButtonObject;

                    if (btnObj)
                    {
                        // --- Overlay Logic ---
                        Transform overlayTr = btnObj.transform.Find("HiderOverlay");
                        GameObject overlayGo;
                        ToolbarButtonEditOverlay editOverlayScript;

                        if (overlayTr == null)
                        {
                            overlayGo = new GameObject("HiderOverlay");
                            overlayGo.transform.SetParent(btnObj.transform, false);

                            var img = overlayGo.AddComponent<Image>();
                            img.raycastTarget = true; // Blocks clicks to the underlying button

                            var rt = overlayGo.GetComponent<RectTransform>();
                            rt.anchorMin = Vector2.zero;
                            rt.anchorMax = Vector2.one;
                            rt.offsetMin = Vector2.zero;
                            rt.offsetMax = Vector2.zero;

                            editOverlayScript = overlayGo.AddComponent<ToolbarButtonEditOverlay>();
                        }
                        else
                        {
                            overlayGo = overlayTr.gameObject;
                            editOverlayScript = overlayGo.GetComponent<ToolbarButtonEditOverlay>();
                        }

                        editOverlayScript.TargetButtonControl = button;
                        overlayGo.transform.SetAsLastSibling(); // Ensure overlay is on top

                        bool isBlacklisted = ToolbarDataStorage.IsHidden(button);

                        if (EditMode)
                        {
                            // Edit Mode: Enable overlay to block input and show visual feedback
                            overlayGo.SetActive(true);
                            editOverlayScript.IsButtonHidden = isBlacklisted;

                            // Ensure the button itself is active so we can see what we are editing
                            if (!btnObj.gameObject.activeSelf) btnObj.gameObject.SetActive(true);
                        }
                        else
                        {
                            // Normal Mode: Disable overlay
                            overlayGo.SetActive(false);

                            if (isBlacklisted)
                            {
                                // Completely hide the button from the layout
                                if (btnObj.gameObject.activeSelf) btnObj.gameObject.SetActive(false);
                                continue;
                            }
                            else
                            {
                                // Restore standard visibility logic
                                bool shouldBeVisible = button.Visible.Value;
                                if (btnObj.gameObject.activeSelf != shouldBeVisible)
                                    btnObj.gameObject.SetActive(shouldBeVisible);

                                if (!shouldBeVisible) continue;
                            }
                        }
                        // -------------------------
                    }
                    else continue;

                    // Positioning Logic
                    if (button.DesiredPosition.HasValue)
                    {
                        // Try to set to desired position, if taken then move right until free spot is found
                        var position = button.DesiredPosition.Value;
                        while (takenPositions.Contains(position))
                            position = new ToolbarPosition(position.Row, position.Column + 1);

                        if (button.SetActualPosition(position, true))
                            takenPositions.Add(position);
                        else
                            positionNotSet.Add(button); // Failed to set position, will assign later
                    }
                    else
                    {
                        positionNotSet.Add(button);
                    }
                }

                // Now place all buttons that didn't have a desired position set
                // First find the first unused position in the two leftmost columns
                var addedPos = 0;
                foreach (var btn in positionNotSet)
                {
                    ToolbarPosition newPos;
                    do
                    {
                        var row = addedPos / 2;
                        var col = addedPos % 2;
                        newPos = new ToolbarPosition(row, col);
                        addedPos++;
                    } while (takenPositions.Contains(newPos));

                    btn.SetActualPosition(newPos, false); // Do not save position
                    takenPositions.Add(newPos);
                }
                _dirty = false;
                ToolbarDataStorage.SaveButtonPositions(_Buttons);
            }
        }
    }
}
