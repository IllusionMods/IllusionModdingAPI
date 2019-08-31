using System;
using BepInEx;
using BepInEx.Configuration;
using KKAPI.Chara;
using KKAPI.Maker;
using Manager;
using UnityEngine;

namespace KKAPI
{
    [BepInPlugin(GUID, "Modding API", VersionConst)]
    public partial class KoikatuAPI : BaseUnityPlugin
    {
        private static ConfigWrapper<bool> EnableDebugLoggingSetting { get; set; }

        // todo unify in all versions
        internal static void Log(BepInEx.Logging.LogLevel level, object obj) => Instance.Logger.Log(level, obj);

        private void Awake()
        {
            // todo unify in all versions
            EnableDebugLoggingSetting = Config.Wrap("Debug", "Show debug messages", "Enables display of additional log messages when certain events are triggered within KKAPI. Useful for plugin devs to understand when controller messages are fired. Changes take effect after game restart.", false);

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
