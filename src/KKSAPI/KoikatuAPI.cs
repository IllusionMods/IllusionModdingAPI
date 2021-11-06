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
        /// <summary>
        /// The VR module process name for use with <see cref="BepInProcess"/> attributes.
        /// This is for the jp release. In almost all cases should be used together with the steam version.
        /// </summary>
        public const string VRProcessName = "KoikatsuSunshine_VR";

        private void Awake()
        {
            BaseAwake();

            var insideStudio = Application.productName == "CharaStudio";
            MakerAPI.Init(insideStudio);
            StudioAPI.Init(insideStudio);
            CharacterApi.Init();
            GameAPI.Init(insideStudio);

            GlobalHooks.Init();
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
            return GameSystem.GameVersion;
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

        private static readonly bool _isVr = Application.productName == VRProcessName;
        /// <summary>
        /// Check if this is the official VR module. Main game VR mods are ignored (returns false).
        /// </summary>
        public static bool IsVR()
        {
            return _isVr;
        }

        private static class GlobalHooks
        {
            public static void Init()
            {
                Harmony.CreateAndPatchAll(typeof(GlobalHooks));
            }

            /// <summary>
            /// UniTasks can fail silently if an exception is thrown and it bubbles all the way up to a unity event method that was made async.
            /// This enusres that the exception is logged. Only warning because it might be manually caught or it might not be in an unity event, in both cases not causing a silent crash.
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Cysharp.Threading.Tasks.ExceptionHolder), nameof(Cysharp.Threading.Tasks.ExceptionHolder.GetException))]
            private static void LogUnitaskException(System.Runtime.ExceptionServices.ExceptionDispatchInfo __result)
            {
                if (__result != null)
                    UnityEngine.Debug.LogWarning("Exception has been thrown inside a UniTask, it might crash the task!\n" + __result.SourceException);
            }
        }
    }
}
