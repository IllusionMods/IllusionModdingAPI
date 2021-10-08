using System;
using System.IO;
using BepInEx;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;
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
        /// </summary>
        public const string GameProcessName = "HoneySelect2";
        /// <summary>
        /// The VR module process name for use with <see cref="BepInProcess"/> attributes.
        /// </summary>
        public const string VRProcessName = "HoneySelect2VR";

        private void Awake()
        {
            BaseAwake();

            var insideStudio = Application.productName == "StudioNEOV2";
            MakerAPI.Init(insideStudio);
            StudioAPI.Init(insideStudio);
            CharacterApi.Init();
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
            try
            {
                if (Scene.IsFind("Home")) return GameMode.MainGame;
            }
            catch (ArgumentNullException) { }
            //if(HSceneFlagCtrl.IsInstance()) return 
            return GameMode.Unknown;
        }

        private static Version _gameVersion;
        /// <summary>
        /// Get current version of the game.
        /// </summary>
        public static Version GetGameVersion()
        {
            if (_gameVersion == null)
            {
                _gameVersion = new Version();
                var versionFile = Path.Combine(DefaultData.Path, "system\\version.dat");
                if (File.Exists(versionFile))
                {
                    var version = File.ReadAllText(versionFile);
                    if (!string.IsNullOrWhiteSpace(version))
                        _gameVersion = new Version(version);
                }
            }
            return _gameVersion;
        }

        ///// <summary>
        ///// Check if the game is the Steam release instead of the original Japanese release.
        ///// <remarks>It's best to not rely on this and instead make the same code work in both versions (if possible).</remarks>
        ///// </summary>
        //public static bool IsSteamRelease()
        //{
        //    return GameSystem.Instance.cultureNames.Length > 1;
        //}

        private static readonly bool _isVr = Application.productName == VRProcessName;
        /// <summary>
        /// Check if this is the official VR module. Main game VR mods are ignored (returns false).
        /// </summary>
        public static bool IsVR()
        {
            return _isVr;
        }
    }
}
