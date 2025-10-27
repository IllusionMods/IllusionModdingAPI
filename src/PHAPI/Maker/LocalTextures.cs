using BepInEx.Configuration;
using KKAPI.Utilities;
using System.Linq;

namespace KKAPI.Maker
{
    /// <summary>
    /// API for global toggling of locally saved textures in Maker.
    /// The module is only activated if Activate is called, SaveType is read / set, or if an action is registered to SaveTypeChangedEvent.
    /// </summary>
    public static class LocalTextures
    {
        /// <summary>
        /// Fired whenever the SaveType changes.
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
            try
            {
                var hello = Studio.LocalTextures.SaveType;
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static ConfigEntry<TextureSaveType> ConfTexSaveType { get; private set; }

        static LocalTextures()
        {
            string description = "Whether external textures used by plugins should be bundled with the card or saved to a local folder.\nCards with local textures save storage space but cannot be shared.";
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Card Save Type", TextureSaveType.Bundled, new ConfigDescription(description, new AcceptableValueEnums<TextureSaveType>(TextureSaveType.Bundled, TextureSaveType.Local), new ConfigurationManagerAttributes { IsAdvanced = true }));
            ConfTexSaveType.SettingChanged += OnSaveTypeChanged;

            // Activates Studio LocalTexture API
            Studio.LocalTextures.SaveType.ToString();
        }

        private static void OnSaveTypeChanged(object x, System.EventArgs y)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(SaveTypeChangedEvent), "");
            SaveTypeChangedEvent.SafeInvokeWithLogging(handler => handler.Invoke(null, new LocalSaveChangedEventArgs(SaveType)), nameof(SaveTypeChangedEvent), eLogger);
            eLogger.End();
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
