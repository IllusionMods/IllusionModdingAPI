using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CharaCustom;
using HarmonyLib;
using UnityEngine.UI;

namespace KKAPI.Maker
{
    /// <summary>
    /// API for modifying the process of saving cards in maker.
    /// </summary>
    public static class MakerCardSave
    {
        static MakerCardSave()
        {
            Harmony.CreateAndPatchAll(typeof(MakerCardSave));
        }

        /// <summary>
        /// Used for modifying card save paths. Parameter is the original path, return the changed path.
        /// </summary>
        public delegate string DirectoryPathModifier(string currentDirectoryPath);
        /// <summary>
        /// Used for modifying card file names. Parameter is the original name, return the changed name.
        /// </summary>
        public delegate string CardNameModifier(string currentCardName);

        private static readonly List<KeyValuePair<DirectoryPathModifier, CardNameModifier>> _modifiers = new List<KeyValuePair<DirectoryPathModifier, CardNameModifier>>();

        /// <summary>
        /// Add a function that can modify the path of the saved cards.
        /// Use sparingly and insert/replace parts of the path instead of overwriting the whole path to keep compatibility with other plugins.
        /// </summary>
        /// <param name="directoryPathModifier">Modifier for the directory the card is saved to. Set to null if no change is required.</param>
        /// <param name="filenameModifier">Modifier for the name the card file itself. Set to null if no change is required.</param>
        public static void RegisterNewCardSavePathModifier(DirectoryPathModifier directoryPathModifier, CardNameModifier filenameModifier)
        {
            _modifiers.Add(new KeyValuePair<DirectoryPathModifier, CardNameModifier>(directoryPathModifier, filenameModifier));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CvsCaptureMenu), "Start")]
        internal static void AddSaveCallback(CvsCaptureMenu __instance, Button ___btnSave)
        {
            // Trigger before the original onClick for card save happens
            ___btnSave.onClick.AddListener(CardSavePatch);
        }

        private static void CardSavePatch()
        {
            try
            {
                if (!MakerAPI.InsideMaker || !_modifiers.Any(x => x.Key != null || x.Value != null)) return;

                var customBase = CustomBase.Instance;
                if (!customBase.customCtrl.saveMode || !string.IsNullOrEmpty(customBase.customCtrl.overwriteSavePath)) return;

                var isMale = MakerAPI.GetMakerSex() == 0;
                var folder = UserData.Path + (isMale ? "chara/male/" : "chara/female/");
#if HS2
                var fileName = $"HS2Cha{(isMale ? "M" : "F")}_{DateTime.Now:yyyyMMddHHmmssfff}";
#elif AI
                var fileName = $"AISCha{(isMale ? "M" : "F")}_{DateTime.Now:yyyyMMddHHmmssfff}";
#endif

                foreach (var kvp in _modifiers)
                {
                    if (kvp.Key != null)
                        folder = kvp.Key(folder);
                    if (kvp.Value != null)
                        fileName = kvp.Value(fileName);
                }

                var fullPath = Path.Combine(folder, fileName);

                // Force the game code to save to a specific path (as if it was overwriting a card)
                customBase.customCtrl.overwriteSavePath = fullPath;
            }
            catch (Exception ex)
            {
                KoikatuAPI.Logger.LogError(ex);
            }
        }
    }
}
