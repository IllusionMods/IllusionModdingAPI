using BepInEx.Configuration;
using KKAPI.Utilities;

namespace KKAPI.Maker
{
    /// <summary>
    /// API for global toggling of locally saved textures in Maker.
    /// The module is only activated if Activate is called, SaveType is read / set, or if an action is registered to SaveTypeChangedEvent.
    /// </summary>
    public static class CharaLocalTextures
    {
        /// <summary>
        /// Fired whenever the SaveType changes.
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
            try
            {
                var hello = Studio.SceneLocalTextures.SaveType;
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static ConfigEntry<CharaTextureSaveType> ConfTexSaveType { get; private set; }

        static CharaLocalTextures()
        {
            string description = "Whether external textures used by plugins should be bundled with the card or saved to a local folder.\nWARNING: Cards with local textures save storage space but cannot be shared.";
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Card Save Type", CharaTextureSaveType.Bundled, new ConfigDescription(description, null, new ConfigurationManagerAttributes { IsAdvanced = true }));
            ConfTexSaveType.SettingChanged += OnSaveTypeChanged;

            // Activates Studio LocalTexture API
            Studio.SceneLocalTextures.SaveType.ToString();
        }

        private static void OnSaveTypeChanged(object x, System.EventArgs y)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(SaveTypeChangedEvent), "");
            SaveTypeChangedEvent.SafeInvokeWithLogging(handler => handler.Invoke(null, new CharaTextureSaveTypeChangedEventArgs(SaveType)), nameof(SaveTypeChangedEvent), eLogger);
            eLogger.End();
        }
    }
}
