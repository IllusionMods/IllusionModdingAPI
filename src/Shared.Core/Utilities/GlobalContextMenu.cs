using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Context menu that can be opened at mouse or screen position.
    /// <example>
    /// To open a right-click context menu when clicking on a Button object, add a click handler and check
    /// if right mouse button was used (e.g. with the help of `.OnPointerClickAsObservable()`) and then call
    /// `GlobalContextMenu.Show("Title", GlobalContextMenu.Entry.Create(...), More entries...)`.
    /// 
    /// If you are using IMGUI, you can do:
    /// ```
    /// if(GUI/GUILayout.Button(...))
    /// {
    ///     if(IMGUIUtils.IsMouseRightClick())
    ///         GlobalContextMenu.Show(...);
    ///     else
    ///         // Left click
    /// }
    /// ```
    /// </example>
    /// </summary>
    public static class GlobalContextMenu
    {
        private static string _title;
        private static Entry[] _contents;

        private static Rect _windowRect;
        private static int _windowId = 5739610; // Random starting ID, high to avoid conflicts, incremented by 1 for each new menu

        /// <summary>
        /// Is the menu currently visible. Use Show and Hide methods to change this.
        /// </summary>
        public static bool Enabled => _contents != null;

        /// <summary>
        /// Show context menu at the cursor's current screen position with the given title and menu entries.
        /// Any currently open context menu will be replaced by the new one.
        /// </summary>
        /// <param name="title">The title to display at the top of the context menu.</param>
        /// <param name="items">The collection of context menu entries to show.</param>
        /// <exception cref="ArgumentNullException">Thrown if the title or items parameter is null.</exception>
        public static void Show(string title, params Entry[] items)
        {
            var m = UnityInput.Current.mousePosition;
            var clickPoint = new Vector2(m.x, Screen.height - m.y);
            Show(clickPoint, title, items);
        }

        /// <summary>
        /// Displays a context menu window at the specified screen position with the given title and menu entries.
        /// Any currently open context menu will be replaced by the new one.
        /// </summary>
        /// <param name="screenPoint">The screen position where the context menu should appear.</param>
        /// <param name="title">The title to display at the top of the context menu.</param>
        /// <param name="items">The collection of context menu entries to show. Must not be null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown if the title or items parameter is null.</exception>
        public static void Show(Vector2 screenPoint, string title, params Entry[] items)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Length == 0) throw new ArgumentException("Menu must have at least one entry", nameof(items));

            _windowRect = new Rect(screenPoint.x, screenPoint.y, 100, 100); 
            _title = title;
            _contents = items;

            // hack to discard old state of the window and make sure it appears correctly when rapidly opened on different items
            _windowId++;
        }

        /// <summary>
        /// Hide currently displayed context menu, if there is any.
        /// </summary>
        public static void Hide()
        {
            _contents = null;
            _title = null;
        }

        internal static void OnGUI()
        {
            if (!Enabled) return;

            // Make an invisible window in the back to reliably capture mouse clicks outside of the context menu.
            // If mouse clicks in a different window then the input will be eaten and context menu would go to the back and stay open instead of closing.
            // Can't use GUI.Box and such because it will always be behind GUI(Layout).Window no matter what you do.
            // Use a label skin with no content to make both the window and button invisible while spanning the entire screen without borders.
            var backdropWindowId = _windowId - 10;
            GUILayout.Window(backdropWindowId, new Rect(0, 0, Screen.width, Screen.height), DisableOnClickWindowFunc, string.Empty, GUI.skin.label);

            if (!Enabled) return;

            IMGUIUtils.DrawSolidBox(_windowRect);

            _windowRect = GUILayout.Window(_windowId, _windowRect, DrawMenu, _title);

            IMGUIUtils.EatInputInRect(_windowRect);

            // Ensure the context menu always stays on top while it's open. Can't use FocusWindow here because it locks up everything else.
            GUI.BringWindowToFront(backdropWindowId);
            GUI.BringWindowToFront(_windowId);

            if (_windowRect.xMax > Screen.width) _windowRect.x = Screen.width - _windowRect.width;
            if (_windowRect.yMax > Screen.height) _windowRect.y = Screen.height - _windowRect.height;
        }

        private static void DisableOnClickWindowFunc(int id)
        {
            if (GUILayout.Button(GUIContent.none, GUI.skin.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) Hide();
        }

        private static void DrawMenu(int id)
        {
            if (_contents.Length == 0)
            {
                Hide();
                return;
            }

            GUILayout.BeginVertical();
            {
                foreach (var menuEntry in _contents)
                {
                    if (menuEntry.Draw())
                        Hide();
                }
            }
            GUILayout.EndVertical();
        }
        
        /// <summary>
        /// A single entry in the context menu.
        /// </summary>
        public readonly struct Entry
        {
            /// <summary>
            /// Specifies the possible states of a context menu entry.
            /// </summary>
            public enum EntryState
            {
                /// <summary>
                /// Menu item is visible and can be interacted with.
                /// </summary>
                Normal = 0,
                /// <summary>
                /// Menu item is visible but cannot be interacted with. It is typically displayed in a grayed-out style to indicate its disabled state.
                /// </summary>
                Disabled,
                /// <summary>
                /// Menu item is hidden.
                /// </summary>
                Hidden
            }

            /// <summary>
            /// A list separator.
            /// </summary>
            public static readonly Entry Separator = new Entry();

            /// <summary>
            /// Create a new context menu entry.
            /// </summary>
            /// <param name="onGuiLayout">GUILayout code to draw the button. When it returns true the context menu is closed (e.g. after user clicks the button). Must not be null.</param>
            /// <exception cref="ArgumentNullException">Thrown if the onGuiLayout parameter is null.</exception>
            public static Entry Create(Func<bool> onGuiLayout)
            {
                if (onGuiLayout == null) throw new ArgumentNullException(nameof(onGuiLayout));
                return new Entry(onGuiLayout);
            }

            /// <summary>
            /// Create a new context menu entry.
            /// </summary>
            /// <param name="name">Name of the entry. Must not be null.</param>
            /// <param name="onClick">Action called when user left-clicks on this menu entry. If null, a label will be shown instead.</param>
            /// <param name="onCheckState">Callback that checks if this item is currently visible. If null, the button is always visible and active.</param>
            /// <exception cref="ArgumentNullException">Thrown if the name parameter is null.</exception>
            public static Entry Create(GUIContent name, Action onClick, Func<EntryState> onCheckState = null)
            {
                if (name == null) throw new ArgumentNullException(nameof(name));

                return new Entry(SimpleButton);

                bool SimpleButton()
                {
                    var prevEnabled = GUI.enabled;
                    if (onCheckState != null)
                    {
                        switch (onCheckState())
                        {
                            case EntryState.Normal:
                                break;
                            case EntryState.Disabled:
                                GUI.enabled = false;
                                break;
                            case EntryState.Hidden:
                                return false;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    if (onClick == null)
                    {
                        GUILayout.Label(name);
                    }
                    else if (GUILayout.Button(name))
                    {
                        if (!IMGUIUtils.IsMouseRightClick())
                        {
                            onClick();
                            return true;
                        }
                    }

                    GUI.enabled = prevEnabled;
                    return false;
                }
            }

            internal Entry(Func<bool> onGuiLayout)
            {
                _onGuiLayout = onGuiLayout;
            }

            private readonly Func<bool> _onGuiLayout;

            /// <summary>
            /// Determines whether the current instance represents a separator.
            /// </summary>
            public bool IsSeparator()
            {
                return _onGuiLayout == null;
            }

            /// <summary>
            /// Draw this menu entry. Handles user clicking on the entry too.
            /// </summary>
            public bool Draw()
            {
                if (_onGuiLayout == null)
                {
                    // Separator
                    GUILayout.Space(4);
                    return false;
                }

                return _onGuiLayout();
            }
        }
    }
}
