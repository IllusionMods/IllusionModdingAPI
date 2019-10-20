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
    public static class MakerCardSave
    {
        private static Harmony _harmony;

        static MakerCardSave()
        {
            _harmony = new Harmony(nameof(MakerCardSave));
            HarmonyWrapper.PatchAll(typeof(MakerCardSave), _harmony);
        }

        public delegate string DirectoryPathModifier(string currentDirectoryPath);
        public delegate string CardNameModifier(string currentCardName);

        private static readonly List<KeyValuePair<DirectoryPathModifier, CardNameModifier>> _modifiers = new List<KeyValuePair<DirectoryPathModifier, CardNameModifier>>();

        public static void RegisterNewCardSavePathModifier(DirectoryPathModifier directoryPathModifier, CardNameModifier filenameModifier)
        {
            _modifiers.Add(new KeyValuePair<DirectoryPathModifier, CardNameModifier>(directoryPathModifier, filenameModifier));
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(CustomControl), "Start")]
        internal static IEnumerable<CodeInstruction> FindSaveMethod(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            bool buttonFound = false;

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];

                if (!buttonFound && code.opcode == OpCodes.Ldfld && code.operand == AccessTools.Field(typeof(CustomControl), "btnSave"))
                    buttonFound = true;

                if (buttonFound)
                {
                    if (code.opcode == OpCodes.Ldftn)
                    {
                        if (code.operand is MethodInfo methodInfo)
                        {
                            var patchMethod = AccessTools.Method(typeof(MakerCardSave), nameof(CardSavePatch));
                            _harmony.Patch(methodInfo, new HarmonyMethod(patchMethod));
                            KoikatuAPI.Logger.LogDebug("Save method found for patching MakerCardSave - " + methodInfo.Name);
                        }

                        break;
                    }
                }
            }

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
#if KK
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
