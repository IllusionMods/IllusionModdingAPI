using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Utility methods for working with IMGUI / OnGui.
    /// </summary>
    public static class IMGUIUtils
    {
        private static Texture2D SolidBoxTex { get; set; }

        /// <summary>
        /// Draw a gray non-transparent GUI.Box at the specified rect. Use before a window or other controls to get rid of 
        /// the default transparency and make the GUI easier to read.
        /// </summary>
        public static void DrawSolidBox(Rect boxRect)
        {
            if (SolidBoxTex == null)
            {
                var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                windowBackground.SetPixel(0, 0, new Color(0.84f, 0.84f, 0.84f));
                windowBackground.Apply();
                SolidBoxTex = windowBackground;
            }

            // It's necessary to make a new GUIStyle here or the texture doesn't show up
            GUI.Box(boxRect, GUIContent.none, new GUIStyle { normal = new GUIStyleState { background = SolidBoxTex } });
        }

        public static void DrawLabelWithOutline(Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size)
        {
            var halfSize = size * 0.5F;

            var backupGuiColor = GUI.color;
            var backupTextColor = style.normal.textColor;

            style.normal.textColor = outColor;
            GUI.color = outColor;

            rect.x -= halfSize;
            GUI.Label(rect, text, style);

            rect.x += size;
            GUI.Label(rect, text, style);

            rect.x -= halfSize;
            rect.y -= halfSize;
            GUI.Label(rect, text, style);

            rect.y += size;
            GUI.Label(rect, text, style);

            rect.y -= halfSize;
            style.normal.textColor = inColor;
            GUI.color = backupGuiColor;
            GUI.Label(rect, text, style);

            style.normal.textColor = backupTextColor;
        }

        public static void DrawLabelWithShadow(Rect rect, GUIContent content, GUIStyle style, Color txtColor, Color shadowColor, Vector2 direction)
        {
            var backupColor = style.normal.textColor;

            style.normal.textColor = shadowColor;
            rect.x += direction.x;
            rect.y += direction.y;
            GUI.Label(rect, content, style);

            style.normal.textColor = txtColor;
            rect.x -= direction.x;
            rect.y -= direction.y;
            GUI.Label(rect, content, style);

            style.normal.textColor = backupColor;
        }

        public static void DrawLayoutLabelWithShadow(GUIContent content, GUIStyle style, Color txtColor, Color shadowColor, Vector2 direction, params GUILayoutOption[] options)
        {
            DrawLabelWithShadow(GUILayoutUtility.GetRect(content, style, options), content, style, txtColor, shadowColor, direction);
        }

        public static bool DrawButtonWithShadow(Rect r, GUIContent content, GUIStyle style, float shadowAlpha, Vector2 direction)
        {
            var letters = new GUIStyle(style);
            letters.normal.background = null;
            letters.hover.background = null;
            letters.active.background = null;

            var result = GUI.Button(r, content, style);

            var color = r.Contains(Event.current.mousePosition) ? letters.hover.textColor : letters.normal.textColor;

            DrawLabelWithShadow(r, content, letters, color, new Color(0f, 0f, 0f, shadowAlpha), direction);

            return result;
        }

        public static bool DrawLayoutButtonWithShadow(GUIContent content, GUIStyle style, float shadowAlpha, Vector2 direction, params GUILayoutOption[] options)
        {
            return DrawButtonWithShadow(GUILayoutUtility.GetRect(content, style, options), content, style, shadowAlpha, direction);
        }
    }
}