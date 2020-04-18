using System;
using BepInEx;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;
using Manager;
using UnityEngine;

namespace KKAPI
{
    [BepInPlugin(GUID, "Modding API", VersionConst)]
    public partial class KoikatuAPI : BaseUnityPlugin
    {
        private void Awake()
        {
            var insideStudio = Application.productName.StartsWith("PlayHomeStudio");

            //todo implement
            MakerAPI.Init(insideStudio);
            //StudioAPI.Init(insideStudio);
            CharacterApi.Init();
        }

        private void Start()
        {
            // Needs to be called after moreaccessories has a chance to load
            //todo implement
            //AccessoriesApi.Init();
        }

        /// <summary>
        /// Get current game mode. 
        /// </summary>
        public static GameMode GetCurrentGameMode()
        {
            //todo implement
            //if (StudioAPI.InsideStudio) return GameMode.Studio;
            if (MakerAPI.InsideMaker) return GameMode.Maker;
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
