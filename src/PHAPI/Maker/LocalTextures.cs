using BepInEx.Configuration;
using KKAPI.Utilities;
using System.Linq;
using System;

namespace KKAPI.Maker
{
    /// <summary>
    /// API for universal toggling of locally saved textures.
    /// The UI is only set up if SaveType is read / set, or if an action is registered to SaveTypeChangedEvent.
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
                ConfTexSaveType.Value = value;
                var eLogger = ApiEventExecutionLogger.GetEventLogger();
                eLogger.Begin(nameof(SaveTypeChangedEvent), "");
                OnSaveTypeChanged(null, new LocalSaveChangedEventArgs(value), eLogger);
                eLogger.End();
            }
        }

        internal static ConfigEntry<TextureSaveType> ConfTexSaveType { get; private set; }

        static LocalTextures()
        {
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Card Save Type", TextureSaveType.Bundled, new ConfigDescription("Whether external textures used by plugins should be bundled with the card or saved to a local folder.\nCards with local textures save storage space but cannot be shared.", new AcceptableValueEnums<TextureSaveType>(TextureSaveType.Bundled, TextureSaveType.Local), new ConfigurationManagerAttributes { IsAdvanced = true }));
        }

        private static void OnSaveTypeChanged(object sender, System.EventArgs args, ApiEventExecutionLogger eventLogger)
        {
            SaveTypeChangedEvent.SafeInvokeWithLogging(handler => handler.Invoke(sender, args), nameof(SaveTypeChangedEvent), eventLogger);
        }

        private class AcceptableValueEnums<T> : AcceptableValueBase where T : Enum
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
                    throw new ArgumentNullException("acceptableValues");
                }

                if (acceptableValues.Length == 0)
                {
                    throw new ArgumentException("At least one acceptable value is needed", "acceptableValues");
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
