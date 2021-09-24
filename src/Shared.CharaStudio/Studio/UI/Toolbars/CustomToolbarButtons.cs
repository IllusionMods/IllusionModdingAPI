using System;
using System.Collections.Generic;
using KKAPI.Maker.UI;
using UniRx;
using UnityEngine;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Add custom buttons to studio toolbars
    /// </summary>
    public static class CustomToolbarButtons
    {
        private static readonly List<BaseGuiEntry> _controlsToSpawn = new List<BaseGuiEntry>();

        private static bool _studioLoaded;

        /// <summary>
        /// Add a toggle button to the top of the left studio toolbar.
        /// Clicking on the button will toggle it between being on and off.
        /// </summary>
        /// <param name="iconTex">
        /// A 32x32 icon used for the button.
        /// You can find a template here
        /// https://github.com/IllusionMods/IllusionModdingAPI/blob/master/doc/studio%20icon%20template.png
        /// For best performance and smallest size save your thumbnail as 8bit grayscale png (or indexed if you need colors) with no alpha channel.
        /// </param>
        /// <param name="initialValue">Initial state of the toggle.</param>
        /// <param name="onValueChanged">
        /// Action fired each time user clicks on the toggle button.
        /// The value is true if the toggle is enabled by the click, false if disabled.
        /// </param>
        public static ToolbarToggle AddLeftToolbarToggle(Texture2D iconTex, bool initialValue = false,
            Action<bool> onValueChanged = null)
        {
            if (iconTex == null) throw new ArgumentNullException(nameof(iconTex));
            if (iconTex.width != 32 || iconTex.height != 32)
                KoikatuAPI.Logger.LogWarning($"Icon texture passed to AddLeftToolbarToggle has wrong size, it should be 32x32 but is {iconTex.width}x{iconTex.height}");

            if (!StudioAPI.InsideStudio)
            {
                KoikatuAPI.Logger.LogDebug("Tried to run StudioAPI.AddLeftToolbarToggle outside of studio!");
                return null;
            }

            var x = new ToolbarToggle(iconTex, initialValue);

            if (_studioLoaded) x.CreateControl(null);
            else _controlsToSpawn.Add(x);

            if (onValueChanged != null) x.ValueChanged.Subscribe(onValueChanged);

            return x;
        }

        /// <summary>
        /// Add a simple button to the top of the left studio toolbar.
        /// </summary>
        /// ///
        /// <param name="iconTex">
        /// A 32x32 icon used for the button.
        /// You can find a template here
        /// https://github.com/ManlyMarco/QuickAccessBox/blob/master/Shared_QuickAccessBox/UI/toolbar-icon.png
        /// </param>
        /// <param name="onClicked">Action fired each time user clicks on the button</param>
        public static ToolbarButton AddLeftToolbarButton(Texture2D iconTex, Action onClicked = null)
        {
            if (iconTex == null) throw new ArgumentNullException(nameof(iconTex));
            if (iconTex.width != 32 || iconTex.height != 32)
                KoikatuAPI.Logger.LogWarning($"Icon texture passed to AddLeftToolbarButton has wrong size, it should be 32x32 but is {iconTex.width}x{iconTex.height}");

            if (!StudioAPI.InsideStudio)
            {
                KoikatuAPI.Logger.LogDebug("Tried to run StudioAPI.AddLeftToolbarButton outside of studio!");
                return null;
            }

            var x = new ToolbarButton(iconTex);

            if (_studioLoaded) x.CreateControl(null);
            else _controlsToSpawn.Add(x);

            if (onClicked != null) x.Clicked.Subscribe(unit => onClicked());

            return x;
        }

        internal static void OnStudioLoaded()
        {
            _studioLoaded = true;

            foreach (var customToolbarToggle in _controlsToSpawn)
                customToolbarToggle.CreateControl(null);
        }
    }
}