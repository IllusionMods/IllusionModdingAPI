using BepInEx.Configuration;
using KKAPI.Utilities;
using UnityEngine.UI;
using UnityEngine;

namespace KKAPI.Maker {
    /// <summary>
    /// API for global toggling of locally saved textures.
    /// The UI is only set up if SaveType is read / set, or if an action is registered to SaveTypeChangedEvent.
    /// </summary>
    public static partial class LocalTextures
    {
        /// <summary>
        /// Fired whenever SaveType changes
        /// </summary>
        public static System.EventHandler SaveTypeChangedEvent;

        /// <summary>
        /// The type of texture saving that plugins should use
        /// </summary>
        public static TextureSaveType SaveType
        {
            get
            {
                return ConfTexSaveType.Value;
            }
            set
            {
                if (ConfTexSaveType.Value == value) return;
                saveTypeChanging = true;
                ConfTexSaveType.Value = value;
                var eLogger = ApiEventExecutionLogger.GetEventLogger();
                eLogger.Begin(nameof(SaveTypeChangedEvent), "");
                OnSaveTypeChanged(null, new LocalSaveChangedEventArgs(value), eLogger);
                eLogger.End();
                saveTypeChanging = false;
            }
        }

        internal static ConfigEntry<TextureSaveType> ConfTexSaveType { get; set; }
        private static bool saveTypeChanging = false;

        static LocalTextures()
        {
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Card Save Type", TextureSaveType.Bundled, new ConfigDescription("Whether external textures used by plugins should be bundled with the card or to a local folder.", null, new ConfigurationManagerAttributes { Browsable = false }));
            MakerAPI.MakerStartedLoading += (x, y) => { SetupUI(); };
            if (MakerAPI.InsideAndLoaded) SetupUI();
        }

        private static void SetTfProps(RectTransform tf, float a, float b, float c, float d, float e, float f, float g, float h, Vector3? scale = null)
        {
            tf.localScale = Vector3.one;
            tf.anchorMin = new Vector2(a, b);
            tf.anchorMax = new Vector2(c, d);
            tf.offsetMin = new Vector2(e, f);
            tf.offsetMax = new Vector2(g, h);

            if (!scale.HasValue) tf.localScale = Vector3.one;
            else tf.localScale = scale.Value;
        }

        private static void SetLayers(Transform rootTransform)
        {
            rootTransform.gameObject.layer = 5; // UI
            for (int i = 0; i < rootTransform.childCount; i++)
            {
                SetLayers(rootTransform.GetChild(i));
            }
        }

        private static Sprite MakeSprite(string file, int border)
        {
            Texture2D spriteTex = ResourceUtils.GetEmbeddedResource(file).LoadTexture();
            Vector2 spriteSize = new Vector2(spriteTex.width, spriteTex.height);
            Rect spriteRect = new Rect(Vector2.zero, spriteSize);
            Vector4 spriteBorder = new Vector4(border, border, border, border);
            return Sprite.Create(spriteTex, spriteRect, spriteSize / 2, 100, 0, SpriteMeshType.FullRect, spriteBorder);
        }

        private static void OnSaveTypeChanged(object sender, System.EventArgs args, ApiEventExecutionLogger eventLogger)
        {
            SaveTypeChangedEvent.SafeInvokeWithLogging(handler => handler.Invoke(sender, args), nameof(SaveTypeChangedEvent), eventLogger);
        }

#if !KKS

        private static bool TryGetComponent<T>(this GameObject go, out T result) where T : Component
        {
            foreach (Component component in go.GetComponents<Component>())
            {
                if (component is T componentT)
                {
                    result = componentT;
                    return true;
                }
            }
            result = null;
            return false;
        }

#endif

        private class ToggleValidator : MonoBehaviour
        {
            private Toggle tglBundled;
            private Toggle tglLocal;

            public void Register(Toggle tglBundled, Toggle tglLocal)
            {
                this.tglBundled = tglBundled;
                this.tglLocal = tglLocal;
            }

            private void OnEnable()
            {
                if (tglBundled != null && SaveType == TextureSaveType.Bundled)
                    tglBundled.isOn = true;
                else if (tglLocal != null && SaveType == TextureSaveType.Local)
                    tglLocal.isOn = true;
            }
        }
    }
}
