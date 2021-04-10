using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Utility methods for working with IMGUI / OnGui.
    /// </summary>
    public static class IMGUIUtils
    {
        #region Draw a box

        private static Texture2D SolidBoxTex { get; set; }

        /// <summary>
        /// Draw a gray non-transparent GUI.Box at the specified rect. Use before a GUI.Window or other controls to get rid of 
        /// the default transparency and make the GUI easier to read.
        /// <example>
        /// IMGUIUtils.DrawSolidBox(screenRect);
        /// GUILayout.Window(362, screenRect, TreeWindow, "Select character folder");
        /// </example>
        /// </summary>
        public static void DrawSolidBox(Rect boxRect)
        {
            if (SolidBoxTex == null)
            {
                var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
#if KK || EC // Take the color correction filter into account
                windowBackground.SetPixel(0, 0, new Color(0.84f, 0.84f, 0.84f));
#else
                windowBackground.SetPixel(0, 0, new Color(0.4f, 0.4f, 0.4f));
#endif
                windowBackground.Apply();
                SolidBoxTex = windowBackground;
            }

            // It's necessary to make a new GUIStyle here or the texture doesn't show up
            GUI.Box(boxRect, GUIContent.none, new GUIStyle { normal = new GUIStyleState { background = SolidBoxTex } });
        }

        #endregion

        /// <summary>
        /// Block input from going through to the game/canvases if the mouse cursor is within the specified Rect.
        /// Use after a GUI.Window call or the window will not be able to get the inputs either.
        /// <example>
        /// GUILayout.Window(362, screenRect, TreeWindow, "Select character folder");
        /// Utils.EatInputInRect(screenRect);
        /// </example>
        /// </summary>
        /// <param name="eatRect"></param>
        public static void EatInputInRect(Rect eatRect)
        {
            if (eatRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                Input.ResetInputAxes();
        }

        #region Outline controls

        /// <summary>
        /// Draw a label with an outline
        /// </summary>
        /// <param name="rect">Size of the control</param>
        /// <param name="text">Text of the label</param>
        /// <param name="style">Style to be applied to the label</param>
        /// <param name="txtColor">Color of the text</param>
        /// <param name="outlineColor">Color of the outline</param>
        /// <param name="outlineThickness">Thickness of the outline in pixels</param>
        public static void DrawLabelWithOutline(Rect rect, string text, GUIStyle style, Color txtColor, Color outlineColor, int outlineThickness)
        {
            var backupColor = style.normal.textColor;
            var backupGuiColor = GUI.color;

            style.normal.textColor = outlineColor;
            GUI.color = outlineColor;

            var baseRect = rect;

            rect.x -= outlineThickness;
            rect.y -= outlineThickness;

            while (rect.x++ < baseRect.x + outlineThickness)
                GUI.Label(rect, text, style);
            rect.x--;

            while (rect.y++ < baseRect.y + outlineThickness)
                GUI.Label(rect, text, style);
            rect.y--;

            while (rect.x-- > baseRect.x - outlineThickness)
                GUI.Label(rect, text, style);
            rect.x++;

            while (rect.y-- > baseRect.y - outlineThickness)
                GUI.Label(rect, text, style);

            style.normal.textColor = txtColor;
            GUI.color = txtColor;

            GUI.Label(baseRect, text, style);

            style.normal.textColor = backupColor;
            GUI.color = backupGuiColor;
        }

        /// <summary>
        /// Draw a label with a shadow
        /// </summary>        
        /// <param name="rect">Size of the control</param>
        /// <param name="content">Contents of the label</param>
        /// <param name="style">Style to be applied to the label</param>
        /// <param name="txtColor">Color of the outline</param>
        /// <param name="shadowColor">Color of the text</param>
        /// <param name="shadowOffset">Offset of the shadow in pixels</param>
        public static void DrawLabelWithShadow(Rect rect, GUIContent content, GUIStyle style, Color txtColor, Color shadowColor, Vector2 shadowOffset)
        {
            var backupColor = style.normal.textColor;

            style.normal.textColor = shadowColor;
            rect.x += shadowOffset.x;
            rect.y += shadowOffset.y;
            GUI.Label(rect, content, style);

            style.normal.textColor = txtColor;
            rect.x -= shadowOffset.x;
            rect.y -= shadowOffset.y;
            GUI.Label(rect, content, style);

            style.normal.textColor = backupColor;
        }

        /// <inheritdoc cref="DrawLabelWithShadow"/>
        public static void DrawLayoutLabelWithShadow(GUIContent content, GUIStyle style, Color txtColor, Color shadowColor, Vector2 direction, params GUILayoutOption[] options)
        {
            DrawLabelWithShadow(GUILayoutUtility.GetRect(content, style, options), content, style, txtColor, shadowColor, direction);
        }

        /// <inheritdoc cref="DrawLabelWithShadow"/>
        public static bool DrawButtonWithShadow(Rect r, GUIContent content, GUIStyle style, float shadowAlpha, Vector2 direction)
        {
            GUIStyle letters = new GUIStyle(style);
            letters.normal.background = null;
            letters.hover.background = null;
            letters.active.background = null;

            bool result = GUI.Button(r, content, style);

            Color color = r.Contains(Event.current.mousePosition) ? letters.hover.textColor : letters.normal.textColor;

            DrawLabelWithShadow(r, content, letters, color, new Color(0f, 0f, 0f, shadowAlpha), direction);

            return result;
        }

        /// <inheritdoc cref="DrawLabelWithShadow"/>
        public static bool DrawLayoutButtonWithShadow(GUIContent content, GUIStyle style, float shadowAlpha, Vector2 direction, params GUILayoutOption[] options)
        {
            return DrawButtonWithShadow(GUILayoutUtility.GetRect(content, style, options), content, style, shadowAlpha, direction);
        }

        #endregion

        #region Drag / resize window

        private static bool _resizeHandleClicked;
        private static Vector3 _resizeClickedPosition;
        private static Rect _resizeOriginalWindow;
        private static int _resizeCurrentWindowId;

        /// <summary>
        /// Handle both dragging and resizing of OnGUI windows.
        /// Use this instead of GUI.DragWindow(), don't use both at the same time.
        /// To use, place this at the end of your Window method: _windowRect = IMGUIUtils.DragResizeWindow(windowId, _windowRect);
        /// </summary>
        /// <param name="windowId">The ID passed to your window method</param>
        /// <param name="windowRect">The rect of your window. Make sure to set it to the result of this method</param>
        public static Rect DragResizeWindow(int windowId, Rect windowRect)
        {
            const int visibleAreaSize = 13;
            const int functionalAreaSize = 25;

            // Draw a visual hint that resizing is possible
            GUI.Box(new Rect(windowRect.width - visibleAreaSize, windowRect.height - visibleAreaSize, visibleAreaSize, visibleAreaSize), GUIContent.none);

            if (_resizeCurrentWindowId != 0 && _resizeCurrentWindowId != windowId) return windowRect;

            var mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y; // Convert to GUI coords

            var winRect = windowRect;
            var windowHandle = new Rect(
                winRect.x + winRect.width - functionalAreaSize,
                winRect.y + winRect.height - functionalAreaSize,
                functionalAreaSize,
                functionalAreaSize);

            // Can't use Input class because inputs inside of window rect might be eaten
            var mouseButtonDown = Event.current.type == EventType.MouseDown && Event.current.button == 0;
            if (mouseButtonDown && windowHandle.Contains(mousePos))
            {
                _resizeHandleClicked = true;
                _resizeClickedPosition = mousePos;
                _resizeOriginalWindow = winRect;
                _resizeCurrentWindowId = windowId;
            }

            if (_resizeHandleClicked)
            {
                // Resize window by dragging
                var listWinRect = winRect;
                listWinRect.width = Mathf.Clamp(_resizeOriginalWindow.width + (mousePos.x - _resizeClickedPosition.x), 100, Screen.width);
                listWinRect.height = Mathf.Clamp(_resizeOriginalWindow.height + (mousePos.y - _resizeClickedPosition.y), 100, Screen.height);
                windowRect = listWinRect;

                var mouseButtonUp = Event.current.type == EventType.MouseUp && Event.current.button == 0;
                if (mouseButtonUp)
                {
                    _resizeHandleClicked = false;
                    _resizeCurrentWindowId = 0;
                }
            }
            else
            {
                // Handle dragging only if not resizing else things break
                GUI.DragWindow();
            }
            return windowRect;
        }

        /// <summary>
        /// Handle both dragging and resizing of OnGUI windows, as well as eat mouse inputs when cursor is over the window.
        /// Use this instead of <see cref="GUI.DragWindow(Rect)"/> and <see cref="EatInputInRect"/>. Don't use these methods at the same time as DragResizeEatWindow.
        /// To use, place this at the end of your Window method: _windowRect = IMGUIUtils.DragResizeEatWindow(windowId, _windowRect);
        /// </summary>
        /// <param name="windowId">The ID passed to your window method</param>
        /// <param name="windowRect">The rect of your window. Make sure to set it to the result of this method</param>
        public static Rect DragResizeEatWindow(int windowId, Rect windowRect)
        {
            var result = DragResizeWindow(windowId, windowRect);
            EatInputInRect(result);
            return result;
        }

        #endregion
    }
}
