using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using Manager;
using UnityEngine;

namespace KKAPI
{
    /// <summary>
    /// Provides overall information about the game and the API itself, and gives some useful tools 
    /// like synchronization of threads or checking if required plugins are installed.
    /// More information is available in project wiki at https://github.com/ManlyMarco/KKAPI/wiki
    /// </summary>
    [BepInPlugin(GUID, "Modding API", VersionConst)]
    public partial class KoikatuAPI : BaseUnityPlugin
    {
        private static ConfigWrapper<bool> EnableDebugLoggingSetting { get; set; }

        internal static void Log(LogLevel level, object obj) => Instance.Logger.Log(level, obj);

        /// <summary>
        /// Don't use manually
        /// </summary>
        public KoikatuAPI()
        {
            EnableDebugLoggingSetting = Config.Wrap("", "Show debug messages", "Enables display of additional log messages when certain events are triggered within KKAPI. Useful for plugin devs to understand when controller messages are fired. Changes take effect after game restart.", false);
        }

        private void Start()
        {
            if (CheckIncompatiblePlugin(this, "com.bepis.makerapi", LogLevel.Error))
            {
                Logger.Log(LogLevel.Error | LogLevel.Message, "MakerAPI is no longer supported and is preventing KKAPI from loading!");
                Logger.Log(LogLevel.Error | LogLevel.Message, "Remove MakerAPI.dll and update all mods that used it to fix this.");
                return;
            }

            var insideStudio = Application.productName == "CharaStudio";

            MakerAPI.Init(insideStudio);
            CharacterApi.Init();
        }

        /// <summary>
        /// Get current game mode. 
        /// </summary>
        public static GameMode GetCurrentGameMode()
        {
            if (MakerAPI.InsideMaker) return GameMode.Maker;
            return GameMode.Unknown;
        }

        /// <summary>
        /// Get current version of the game.
        /// </summary>
        public static Version GetGameVersion()
        {
            return new Version(GameSystem.GameSystemVersion);
        }
    }
}
