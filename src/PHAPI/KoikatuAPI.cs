using System;
using BepInEx;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;
using Manager;
using UnityEngine;

namespace KKAPI
{
    [BepInPlugin(GUID, "Modding API", VersionConst)]
    public partial class KoikatuAPI : BaseUnityPlugin
    {
        /// <summary>
        /// The game process name for use with <see cref="BepInProcess"/> attributes.
        /// This is for the 64 bit version. In almost all cases should be used together with the 32 bit version.
        /// </summary>
        public const string GameProcessName = "PlayHome64bit";
        /// <summary>
        /// The game process name for use with <see cref="BepInProcess"/> attributes.
        /// This is for the 32 bit version. In almost all cases should be used together with the 64 bit version.
        /// </summary>
        public const string GameProcessName32bit = "PlayHome32bit";
        /// <summary>
        /// The studio process name for use with <see cref="BepInProcess"/> attributes.
        /// This is for the 64 bit version. In almost all cases should be used together with the 32 bit version.
        /// </summary>
        public const string StudioProcessName = "PlayHomeStudio64bit";
        /// <summary>
        /// The studio process name for use with <see cref="BepInProcess"/> attributes.
        /// This is for the 32 bit version. In almost all cases should be used together with the 64 bit version.
        /// </summary>
        public const string StudioProcessName32bit = "PlayHomeStudio32bit";

        private void Awake()
        {
            BaseAwake();

            var insideStudio = Application.productName.StartsWith("PlayHomeStudio");
            MakerAPI.Init(insideStudio);
            StudioAPI.Init(insideStudio);
            CharacterApi.Init();
            AccessoriesApi.Init();
        }

        /// <summary>
        /// Get current game mode. 
        /// </summary>
        public static GameMode GetCurrentGameMode()
        {
            if (StudioAPI.InsideStudio) return GameMode.Studio;
            if (MakerAPI.InsideMaker) return GameMode.Maker;
            //todo implement
            //if (Game.IsInstance()) return GameMode.MainGame;
            return GameMode.Unknown;
        }

        /// <summary>
        /// Get current version of the game.
        /// </summary>
        public static Version GetGameVersion()
        {
            //todo implement
            return new Version(1, 4);
        }
    }
}
