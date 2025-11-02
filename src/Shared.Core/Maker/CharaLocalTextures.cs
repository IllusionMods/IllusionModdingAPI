using BepInEx.Configuration;
using KKAPI.Utilities;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

namespace KKAPI.Maker {
    /// <summary>
    /// API for global toggling of locally saved textures in Maker.
    /// This module is activated only if Activate is called, SaveType is read / set, or if an action is registered to SaveTypeChangedEvent.
    /// </summary>
    public static partial class CharaLocalTextures
    {
        /// <summary>
        /// Fired whenever SaveType changes
        /// </summary>
        public static System.EventHandler<CharaTextureSaveTypeChangedEventArgs> SaveTypeChangedEvent;

        /// <summary>
        /// The type of texture saving that plugins should use
        /// </summary>
        public static CharaTextureSaveType SaveType
        {
            get
            {
                return ConfTexSaveType.Value;
            }
            set
            {
                if (ConfTexSaveType.Value == value) return;
                ConfTexSaveType.Value = value;
            }
        }

        /// <summary>
        /// Activates the LocalTextures API
        /// </summary>
        public static bool Activate()
        {
#if !EC
            try
            {
                var hello = Studio.SceneLocalTextures.SaveType;
            }
            catch
            {
                return false;
            }
#endif
            return true;
        }

        internal static ConfigEntry<CharaTextureSaveType> ConfTexSaveType { get; set; }
        private static bool saveTypeChanging = false;

        static CharaLocalTextures()
        {
            string description = "Whether external textures used by plugins should be bundled with the card or saved to a local folder.\nWARNING: Cards with local textures save storage space but cannot be shared.";
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Card Save Type", CharaTextureSaveType.Bundled, new ConfigDescription(description, null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));
            ConfTexSaveType.SettingChanged += OnSaveTypeChanged;
            MakerAPI.MakerStartedLoading += (x, y) => { SetupUI(); };
            if (MakerAPI.InsideAndLoaded) SetupUI();
            MakerCardSave.RegisterNewCardSavePathModifier(null, TextureSaveHandlerBase.AddLocalPrefixToCard);
            KoikatuAPI.Instance.Config.Bind("Local Textures", "Audit Local Files", 0, new ConfigDescription("Parse all character / scene files and check for missing or unused local files. Takes a long time if you have many cards and scenes.", null, new ConfigurationManagerAttributes
            {
                CustomDrawer = new System.Action<ConfigEntryBase>(TextureSaveHandlerBase.AuditOptionDrawer),
                Order = 0,
                HideDefaultButton = true,
                IsAdvanced = true,
            }));

#if !EC
            // Activates Studio LocalTexture API
            Studio.SceneLocalTextures.SaveType.ToString();
#endif
        }

        private static void SetTfProps(RectTransform tf, float a, float b, float c, float d, float e, float f, float g, float h, Vector3? scale = null)
        {
            tf.localScale = Vector3.one;
            tf.anchorMin = new Vector2(a, b);
            tf.anchorMax = new Vector2(c, d);
            tf.offsetMin = new Vector2(e, f);
            tf.offsetMax = new Vector2(g, h);

            if (scale.HasValue) tf.localScale = scale.Value;
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

        private static void OnSaveTypeChanged(object x, System.EventArgs y)
        {
            saveTypeChanging = true;
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(SaveTypeChangedEvent), "");
            SaveTypeChangedEvent.SafeInvokeWithLogging(handler => handler.Invoke(null, new CharaTextureSaveTypeChangedEventArgs(SaveType)), nameof(SaveTypeChangedEvent), eLogger);
            eLogger.End();
            saveTypeChanging = false;
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
                if (tglBundled != null && SaveType == CharaTextureSaveType.Bundled)
                    tglBundled.isOn = true;
                else if (tglLocal != null && SaveType == CharaTextureSaveType.Local)
                    tglLocal.isOn = true;
            }
        }
    }
}
