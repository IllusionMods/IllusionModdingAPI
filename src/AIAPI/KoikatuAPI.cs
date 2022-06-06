using System;
using BepInEx;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;
using KKAPI.MainGame;
using Manager;
using UnityEngine;

namespace KKAPI
{
    [BepInPlugin(GUID, "Modding API", VersionConst)]
    [BepInDependency(ExtensibleSaveFormat.ExtendedSave.GUID, "12.2")]
    public partial class KoikatuAPI : BaseUnityPlugin
    {
        /// <summary>
        /// The studio process name for use with <see cref="BepInProcess"/> attributes.
        /// </summary>
        public const string StudioProcessName = "StudioNEOV2";
        /// <summary>
        /// The game process name for use with <see cref="BepInProcess"/> attributes.
        /// It's the same for jp and steam releases.
        /// </summary>
        public const string GameProcessName = "AI-Syoujyo";
        
        private void Awake()
        {
            BaseAwake();

            var insideStudio = Application.productName == "StudioNEOV2";
            MakerAPI.Init(insideStudio);
            StudioAPI.Init(insideStudio);
            CharacterApi.Init();
            GameAPI.Init(insideStudio);
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
            if (MakerAPI.InsideMaker) return GameMode.Maker;
            if (StudioAPI.InsideStudio) return GameMode.Studio;
            if (Map.IsInstance() && Map.Instance.MapRoot != null) return GameMode.MainGame;
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
        public static bool IsSteamRelease()
        {
            // The jp version only has Japanese listed
            return GameSystem.Instance.cultureNames.Length > 1;
        }
    }
}
