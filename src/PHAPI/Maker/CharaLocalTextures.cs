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
                // If local texture support is disabled, always return Bundled
                if (!EnableLocalTextureSupport.Value)
                    return CharaTextureSaveType.Bundled;
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
        internal static ConfigEntry<bool> EnableLocalTextureSupport { get; private set; }

        static CharaLocalTextures()
        {
            EnableLocalTextureSupport = KoikatuAPI.Instance.Config.Bind("Local Textures", "Enable local texture support", true, new ConfigDescription("Enable or disable local texture support. When disabled, the local texture UI is hidden and all textures are always stored in cards.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));
            EnableLocalTextureSupport.SettingChanged += (sender, args) => OnEnableLocalTextureSupportChanged();
            
            string description = "Whether external textures used by plugins should be bundled with the card or saved to a local folder.\nWARNING: Cards with local textures save storage space but cannot be shared.";
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Card Save Type", CharaTextureSaveType.Bundled, new ConfigDescription(description, null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));
            ConfTexSaveType.SettingChanged += OnSaveTypeChanged;
            KoikatuAPI.Instance.Config.Bind("Local Textures", "Audit Local Files", 0, new ConfigDescription("Parse all character / scene files and check for missing or unused local files. Takes a long time if you have many cards and scenes.", null, new ConfigurationManagerAttributes
            {
                CustomDrawer = new System.Action<ConfigEntryBase>(TextureSaveHandlerBase.AuditOptionDrawer),
                Order = 0,
                HideDefaultButton = true,
                IsAdvanced = true,
            }));

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

        private static void OnEnableLocalTextureSupportChanged()
        {
            // Trigger SaveTypeChangedEvent to notify plugins of the change
            OnSaveTypeChanged(null, System.EventArgs.Empty);
        }
    }
}
