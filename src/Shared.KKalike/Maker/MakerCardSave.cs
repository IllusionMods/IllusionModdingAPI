using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Harmony;
using ChaCustom;
using HarmonyLib;
using Illusion.Game;
using Object = UnityEngine.Object;

namespace KKAPI.Maker
{
    /// <summary>
    /// API for modifying the process of saving cards in maker.
    /// </summary>
    public static class MakerCardSave
    {
        private static readonly Harmony _harmony;

        static MakerCardSave()
        {
            _harmony = new Harmony(nameof(MakerCardSave));
            _harmony.PatchAll(typeof(MakerCardSave));
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

        [HarmonyTranspiler, HarmonyPatch(typeof(CustomControl), "Start")]
        internal static IEnumerable<CodeInstruction> FindSaveMethod(IEnumerable<CodeInstruction> instructions)
        {
            var btnSaveField = AccessTools.Field(typeof(CustomControl), "btnSave") ?? throw new MissingFieldException("could not find btnSave");
            var patchMethod = AccessTools.Method(typeof(MakerCardSave), nameof(CardSavePatch)) ?? throw new MissingMethodException("could not find CardSavePatch");

            var codes = instructions.ToList();
            bool buttonFound = false, success = false;

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];

                if (!buttonFound && code.opcode == OpCodes.Ldfld && code.operand as FieldInfo == btnSaveField)
                    buttonFound = true;

                if (buttonFound && code.opcode == OpCodes.Ldftn)
                {
                    if (code.operand is MethodInfo methodInfo)
                    {
                        _harmony.Patch(methodInfo, new HarmonyMethod(patchMethod));
                        KoikatuAPI.Logger.LogDebug("Save method found for patching MakerCardSave - " + methodInfo.Name);
                        success = true;
                    }

                    break;
                }
            }

            if (!success) KoikatuAPI.Logger.LogWarning("Could not find save method for patching MakerCardSave");

            return codes;
        }

        private static bool CardSavePatch(CustomControl __instance)
        {
            if (!MakerAPI.InsideMaker || !_modifiers.Any(x => x.Key != null || x.Value != null)) return true;

            try
            {
                Utils.Sound.Play(SystemSE.ok_s);

                var instanceChaCtrl = Singleton<CustomBase>.Instance.chaCtrl;

                var isMale = instanceChaCtrl.sex == 0;

                var folder = UserData.Path + (isMale ? "chara/male/" : "chara/female/");

                var fileName = __instance.saveNew ?
#if KK || KKS
                    $"Koikatu_{(isMale ? "M" : "F")}_{DateTime.Now:yyyyMMddHHmmssfff}"
#elif EC
                    $"Emocre_{(isMale ? "M" : "F")}_{DateTime.Now:yyyyMMddHHmmssfff}"
#endif
                    : __instance.saveFileName;

                foreach (var kvp in _modifiers)
                {
                    if (kvp.Key != null)
                        folder = kvp.Key(folder);
                    // Keep old filename if not saving as new file
                    if (kvp.Value != null && __instance.saveNew)
                        fileName = kvp.Value(fileName);
                }

                var fullPath = Path.Combine(folder, fileName);

                instanceChaCtrl.chaFile.SaveCharaFile(fullPath);

                if (__instance.saveFileListCtrl != null)
                {
                    var listCtrl = Object.FindObjectOfType<CustomCharaFile>();
                    if (listCtrl != null)
                        RefreshThumbs(listCtrl);
                }

                __instance.saveMode = false;

                return false;
            }
            catch (Exception ex)
            {
                KoikatuAPI.Logger.LogError(ex);
                return true;
            }
        }

        private static void RefreshThumbs(CustomCharaFile listCtrl)
        {
            var traverse = Traverse.Create(listCtrl);
            // KKP - private bool Initialize(bool isDefaultDataAdd, bool reCreate)
            var target = traverse.Method("Initialize", true, false);
            if (target.MethodExists())
                target.GetValue();
            else
                // KK/EC - private bool Initialize()
                traverse.Method("Initialize").GetValue();
        }
    }
}
