﻿using BepInEx.Configuration;
using KKAPI.Utilities;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

namespace KKAPI.Maker {
    /// <summary>
    /// API for global toggling of locally saved textures in Maker.
    /// The module is only activated if Activate is called, SaveType is read / set, or if an action is registered to SaveTypeChangedEvent.
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
                var hello = Studio.LocalTextures.SaveType;
            }
            catch
            {
                return false;
            }
#endif
            return true;
        }

        internal static ConfigEntry<TextureSaveType> ConfTexSaveType { get; set; }
        private static bool saveTypeChanging = false;

        static LocalTextures()
        {
            string description = "Whether external textures used by plugins should be bundled with the card or saved to a local folder.\nCards with local textures save storage space but cannot be shared.";
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Card Save Type", TextureSaveType.Bundled, new ConfigDescription(description, new AcceptableValueEnums<TextureSaveType>(TextureSaveType.Bundled, TextureSaveType.Local), new ConfigurationManagerAttributes { IsAdvanced = true }));
            ConfTexSaveType.SettingChanged += OnSaveTypeChanged;
            MakerAPI.MakerStartedLoading += (x, y) => { SetupUI(); };
            if (MakerAPI.InsideAndLoaded) SetupUI();

#if !EC
            // Activates Studio LocalTexture API
            Studio.LocalTextures.SaveType.ToString();
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
            SaveTypeChangedEvent.SafeInvokeWithLogging(handler => handler.Invoke(null, new LocalSaveChangedEventArgs(SaveType)), nameof(SaveTypeChangedEvent), eLogger);
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
                if (tglBundled != null && SaveType == TextureSaveType.Bundled)
                    tglBundled.isOn = true;
                else if (tglLocal != null && SaveType == TextureSaveType.Local)
                    tglLocal.isOn = true;
            }
        }

        private class AcceptableValueEnums<T> : AcceptableValueBase where T : System.Enum
        {
            //
            // Summary:
            //     List of values that a setting can take.
            public virtual T[] AcceptableValues { get; }

            //
            // Summary:
            //     Specify the list of acceptable values for a setting. If the setting does not
            //     equal any of the values, it will be set to the first one.
            public AcceptableValueEnums(params T[] acceptableValues)
                : base(typeof(T))
            {
                if (acceptableValues == null)
                {
                    throw new System.ArgumentNullException("acceptableValues");
                }

                if (acceptableValues.Length == 0)
                {
                    throw new System.ArgumentException("At least one acceptable value is needed", "acceptableValues");
                }

                AcceptableValues = acceptableValues;
            }

            public override object Clamp(object value)
            {
                if (IsValid(value))
                {
                    return value;
                }

                return AcceptableValues[0];
            }

            public override bool IsValid(object value)
            {
                if (value is T)
                {
                    T v = (T)value;
                    return AcceptableValues.Any((T x) => x.Equals(v));
                }

                return false;
            }

            public override string ToDescriptionString()
            {
                return "# Acceptable values: " + string.Join(", ", AcceptableValues.Select((T x) => x.ToString()).ToArray());
            }
        }
    }
}
