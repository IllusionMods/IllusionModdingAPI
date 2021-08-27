using System;
using BepInEx;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.MainGame;
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
        /// The studio process name for use with <see cref="BepInProcess"/> attributes.
        /// </summary>
        public const string StudioProcessName = "CharaStudio";
        /// <summary>
        /// The game process name for use with <see cref="BepInProcess"/> attributes.
        /// This is for the jp release. In almost all cases should be used together with the steam version.
        /// </summary>
        public const string GameProcessName = "KoikatsuSunshine";

        private void Awake()
        {
            BaseAwake();

            var insideStudio = Application.productName == "CharaStudio";
            MakerAPI.Init(insideStudio);
            StudioAPI.Init(insideStudio);
            CharacterApi.Init();
            //todo GameAPI.Init(insideStudio);
        }

        private void Start()
        {
            // Needs to be called after moreaccessories has a chance to load
            AccessoriesApi.Init();
        }

        /// <summary>
        /// Get current game mode. 
        /// </summary>
        public static GameMode GetCurrentGameMode()
        {
            if (StudioAPI.InsideStudio) return GameMode.Studio;
            if (MakerAPI.InsideMaker) return GameMode.Maker;
            if (Game.initialized) return GameMode.MainGame;
            return GameMode.Unknown;
        }

        /// <summary>
        /// Get current version of the game.
        /// </summary>
        public static Version GetGameVersion()
        {
            return Game.Version;
        }

        /// <summary>
        /// Check if the game is the Steam release instead of the original Japanese release.
        /// <remarks>It's best to not rely on this and instead make the same code work in both versions (if possible).</remarks>
        /// </summary>
        [Obsolete("Not implemented yet, always false")]
        public static bool IsSteamRelease()
        {
            return false; //typeof(DownloadScene).GetProperty("isSteam", AccessTools.all) != null; //todo
        }
    }
}
