using System;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Base class for IMGUI windows that are implemented as full MonoBehaviours.
    /// Instantiate to add the window, only one instance should ever exist.
    /// Turn drawing the window on and off by setting the enable property (off by default).
    /// </summary>
    public abstract class ImguiWindow<T> : MonoBehaviour where T : ImguiWindow<T>
    {
        /// <summary>
        /// Instance of the window. Null if none were created yet.
        /// </summary>
        public static T Instance { get; private set; }

        /// <summary>
        /// Base constructor
        /// </summary>
        protected ImguiWindow()
        {
            WindowId = base.GetHashCode();
            Instance = (T)this;
            enabled = false;
        }

        /// <summary>
        /// Make sure to call base.OnGUI when overriding!
        /// </summary>
        protected virtual void OnGUI()
        {
            WindowRect = GUILayout.Window(WindowId, WindowRect, DrawContentsInt, Title);
            if (WindowRect.width < MinimumSize.x)
            {
                var rect = WindowRect;
                rect.width = MinimumSize.x;
                WindowRect = rect;
            }

            if (WindowRect.height < MinimumSize.y)
            {
                var rect = WindowRect;
                rect.height = MinimumSize.y;
                WindowRect = rect;
            }
        }

        private void DrawContentsInt(int id)
        {
            int visibleAreaSize = GUI.skin.window.border.top - 4;// 10;
            if (GUI.Button(new Rect(WindowRect.width - visibleAreaSize - 2, 2, visibleAreaSize, visibleAreaSize), "X"))
            {
                enabled = false;
                return;
            }

            try
            {
                DrawContents();
                IMGUIUtils.DrawTooltip(WindowRect);
            }
            catch (Exception ex)
            {
                // Ignore mismatch exceptions caused by virtual lists, there will be an unity error shown anyways
                if (!ex.Message.Contains("GUILayout"))
                    KoikatuAPI.Logger.LogError($"[{Title ?? GetType().FullName}] GUI crash: {ex}");
            }

            WindowRect = IMGUIUtils.DragResizeEatWindow(id, WindowRect);
        }

        /// <summary>
        /// Should return the initial desired size of the window, adjusted to fit inside the screen space.
        /// </summary>
        protected abstract Rect GetDefaultWindowRect(Rect screenRect);

        /// <summary>
        /// Draw contents of the IMGUI window (this is inside of the GUILayout.Window func).
        /// Use GUILayout instead of GUI, and expect the window size to change during runtime.
        /// </summary>
        protected abstract void DrawContents();

        /// <summary>
        /// Make sure to call base.OnEnable when overriding!
        /// </summary>
        protected virtual void OnEnable()
        {
            if (WindowRect == default) ResetWindowRect();
        }

        /// <summary>
        /// Reset the window rect (position and size) to its default value.
        /// </summary>
        public void ResetWindowRect()
        {
            //var screenRect = new Rect(
            //    ScreenOffset,
            //    ScreenOffset,
            //    Screen.width - ScreenOffset * 2,
            //    Screen.height - ScreenOffset * 2);
            var screenRect = new Rect(0, 0, Screen.width, Screen.height);
            WindowRect = GetDefaultWindowRect(screenRect);
        }

        /// <summary>
        /// Title of the window.
        /// </summary>
        public virtual string Title { get; set; }
        /// <summary>
        /// ID of the window, set to a random number by default.
        /// </summary>
        public int WindowId { get; set; }
        /// <summary>
        /// Position and size of the window.
        /// </summary>
        public virtual Rect WindowRect { get; set; }
        /// <summary>
        /// Minimum size of the window.
        /// </summary>
        public Vector2 MinimumSize { get; set; } = new Vector2(100, 100);
    }
}
