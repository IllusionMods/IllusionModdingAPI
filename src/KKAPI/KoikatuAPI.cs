﻿using System;
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
        public const string GameProcessName = "Koikatu";
        /// <summary>
        /// The game process name for use with <see cref="BepInProcess"/> attributes.
        /// This is for the steam release. In almost all cases should be used together with the jp version.
        /// </summary>
        public const string GameProcessNameSteam = "Koikatsu Party";
        /// <summary>
        /// The VR module process name for use with <see cref="BepInProcess"/> attributes.
        /// This is for the jp release. In almost all cases should be used together with the steam version.
        /// </summary>
        public const string VRProcessName = "KoikatuVR";
        /// <summary>
        /// The VR module process name for use with <see cref="BepInProcess"/> attributes.
        /// This is for the steam release. In almost all cases should be used together with the jp version.
        /// </summary>
        public const string VRProcessNameSteam = "Koikatsu Party VR";

        private void Awake()
        {
            BaseAwake();

            var insideStudio = Application.productName == "CharaStudio";
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
            if (StudioAPI.InsideStudio) return GameMode.Studio;
            if (MakerAPI.InsideMaker) return GameMode.Maker;
            if (Game.IsInstance()) return GameMode.MainGame;
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
            return typeof(DownloadScene).GetProperty("isSteam", AccessTools.all) != null;
        }

        /// <summary>
        /// Check if the game is running the Darkness version
        /// <remarks>It's best to not rely on this and instead make the same code works either way (if possible).</remarks>
        /// </summary>
        public static bool IsDarkness()
        {
            return typeof(ChaControl).GetProperty("exType", AccessTools.all) != null;
        }
    }
}
