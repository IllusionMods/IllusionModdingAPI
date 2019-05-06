using System;
using System.ComponentModel;
using BepInEx;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Studio;
using KKAPI.Studio.SaveLoad;
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
        [DisplayName("Show debug messages")]
        [Description("Enables display of additional log messages when certain events are triggered within KKAPI. " +
                     "Useful for plugin devs to understand when controller messages are fired.\n\n" +
                     "Changes take effect after game restart.")]
        private static ConfigWrapper<bool> EnableDebugLoggingSetting { get; }

        internal static void Log(LogLevel level, object obj) => BepInEx.Logger.Log(level, obj);

        static KoikatuAPI()
        {
            EnableDebugLoggingSetting = new ConfigWrapper<bool>("EnableDebugLogging", GUID, false);
        }

        private void Start()
        {
            if (!CheckIncompatibilities()) return;

            var insideStudio = Application.productName == "CharaStudio";

            MakerAPI.Init(insideStudio);
            StudioAPI.Init(insideStudio);
            StudioSaveLoadApi.Init(insideStudio);
            CharacterApi.Init();
            GameAPI.Init(insideStudio);
        }

        /// <summary>
        /// Get current game mode. 
        /// </summary>
        public static GameMode GetCurrentGameMode()
        {
            if (StudioAPI.InsideStudio) return GameMode.Studio;
            if (MakerAPI.InsideMaker) return GameMode.Maker;
            if (Game.Instance != null) return GameMode.MainGame;
            return GameMode.Unknown;
        }

        /// <summary>
        /// Get current version of the game.
        /// </summary>
        public static Version GetGameVersion()
        {
            return Game.Version;
        }
    }
}
