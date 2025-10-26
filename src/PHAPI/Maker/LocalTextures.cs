using BepInEx.Configuration;
using KKAPI.Utilities;

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
            ConfTexSaveType = KoikatuAPI.Instance.Config.Bind("Local Textures", "Card Save Type", TextureSaveType.Bundled, new ConfigDescription("Whether external textures used by plugins should be bundled with the card or saved to a local folder.\nCards with local textures save storage space but cannot be shared."));
        }

        private static void OnSaveTypeChanged(object sender, System.EventArgs args, ApiEventExecutionLogger eventLogger)
        {
            SaveTypeChangedEvent.SafeInvokeWithLogging(handler => handler.Invoke(sender, args), nameof(SaveTypeChangedEvent), eventLogger);
        }
    }


}
