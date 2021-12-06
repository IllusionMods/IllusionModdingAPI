using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KKAPI.Utilities
{
    internal static class InterfaceMaker
    {
        // These all need to be held as static properties, including textures, to prevent UnloadUnusedAssets from destroying them
        private static Texture2D _boxBackground;
        private static Texture2D _winBackground;
        private static GUISkin _customSkin;

        public static GUISkin CustomSkin
        {
            get
            {
                if (_customSkin == null)
                {
                    try
                    {
                        KoikatuAPI.Logger.LogDebug("Instantiating custom GUISkin");
                        _customSkin = CreateSkin(IMGUIUtils.ColorFilterAffectsImgui);
                    }
                    catch (Exception ex)
                    {
                        KoikatuAPI.Logger.LogWarning("Could not load custom GUISkin - " + ex.Message);
                        _customSkin = GUI.skin;
                    }
                }

                return _customSkin;
            }
        }

        private static GUISkin CreateSkin(bool lightVersion)
        {
            // Reflection because unity 4.x refuses to instantiate if built with newer versions of UnityEngine
            var newSkin = Object.Instantiate(GUI.skin);
            Object.DontDestroyOnLoad(newSkin);

            // Load the custom skin from resources
            _boxBackground = ResourceUtils.GetEmbeddedResource(lightVersion ? "guisharp-box-light.png" : "guisharp-box.png").LoadTexture();
            Object.DontDestroyOnLoad(_boxBackground);
            newSkin.box.onNormal.background = null;
            newSkin.box.normal.background = _boxBackground;
            newSkin.box.normal.textColor = Color.white;

            _winBackground = ResourceUtils.GetEmbeddedResource(lightVersion ? "guisharp-window-light.png" : "guisharp-window.png").LoadTexture();
            Object.DontDestroyOnLoad(_winBackground);
            newSkin.window.onNormal.background = null;
            newSkin.window.normal.background = _winBackground;
            newSkin.window.padding = new RectOffset(6, 6, 22, 6);
            newSkin.window.border = new RectOffset(10, 10, 20, 10);
            newSkin.window.normal.textColor = Color.white;

            newSkin.button.padding = new RectOffset(4, 4, 3, 3);
            newSkin.button.normal.textColor = Color.white;

            newSkin.textField.normal.textColor = Color.white;

            newSkin.label.normal.textColor = Color.white;

            return newSkin;
        }
    }
}
