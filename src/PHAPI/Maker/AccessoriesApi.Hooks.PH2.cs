using System;
using System.Linq;
using HarmonyLib;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker
{
    public static partial class AccessoriesApi
    {
        private static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(AccessoryCustomEdit), "ChangeTab")]
            public static void ChangeSlotPostfix(AccessoryCustomEdit __instance, int newTab)
            {
                OnSelectedMakerSlotChanged(__instance, newTab);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AccessoryCustomEdit), "OnChangeAcceItem")]
            public static void OnAccessoryChangedHook(AccessoryCustomEdit __instance, int slot, CustomSelectSet set)
            {
                OnAccessoryChanged(__instance, slot);
            }

#if KK
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsAccessoryCopy), "CopyAcs")]
            public static void CopyCopyAcsPostfix(CvsAccessoryCopy __instance)
            {
                OnCopyAcs(__instance);
            }
#endif

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AcceCopyHelperUI), "Button_CopySlot")]
            //todo These only copy positions, so don't trigger?
            //[HarmonyPatch(typeof(AcceCopyHelperUI), "Button_CopyPos")]
            //[HarmonyPatch(typeof(AcceCopyHelperUI), "Button_CopyPosRev_H")]
            //[HarmonyPatch(typeof(AcceCopyHelperUI), "Button_CopyPosRev_V")]
            public static void CopyAcsPostfix(AcceCopyHelperUI __instance, Toggle[] ___dstSlots, Toggle[] ___srcSlots)
            {
                var selDst = -1;
                var selSrc = -1;
                for (var i = 0; i < ___dstSlots.Length; i++)
                {
                    if (___srcSlots[i].isOn) selSrc = i;
                    if (___dstSlots[i].isOn) selDst = i;
                }

                OnChangeAcs(__instance, selSrc, selDst);
            }

            private static bool _roadwayToMaker;
            private static bool _usedMakerButton;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CautionScene), "Start")]
            private static void CautionSceneOverridePatch(CautionScene __instance)
            {
                _roadwayToMaker = Environment.GetCommandLineArgs().Any(x => x == "-maker");
                if (_roadwayToMaker)
                    __instance.GC.ChangeScene(__instance.nextScene, __instance.nextMessage, 0);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(LogoScene), "Start")]
            private static void LogoSceneOverridePatch(LogoScene __instance)
            {
                if (_roadwayToMaker)
                    __instance.GC.ChangeScene(__instance.nextScene, __instance.nextMessage, 0);
            }

            private static void StartMaker(float fadeTime, TitleScene __instance)
            {
                var msg = EditScene.CreateMessage("律子", "GameStart");
                __instance.GC.ChangeScene("EditScene", msg, fadeTime);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleScene), "Start")]
            private static void TitleSceneAddMakerButtonPatch(TitleScene __instance, ref Button[] ___buttons)
            {
                if (!_roadwayToMaker)
                {
                    // Create a new Maker button
                    var startButton = ___buttons[1];
                    var newButtonObj =
                        GameObject.Instantiate(startButton.gameObject, startButton.transform.parent, true);
                    var newButtonCmp = newButtonObj.GetComponent<Button>();
                    newButtonCmp.onClick.ActuallyRemoveAllListeners();
                    newButtonCmp.onClick.AddListener(() =>
                    {
                        _usedMakerButton = true;
                        __instance.GC.audioCtrl.Play2DSE(__instance.GC.audioCtrl.systemSE_yes);
                        StartMaker(1f, __instance);
                    });
                    newButtonCmp.GetComponentInChildren<Text>().text = "Character Maker";

                    // Realign the buttons
                    var btnList = ___buttons.ToList();
                    btnList.Insert(2, newButtonCmp);
                    var startPos = btnList[0].transform.localPosition;
                    for (int i = 0; i < btnList.Count; i++)
                        btnList[i].transform.localPosition = startPos + new Vector3(0, -75, 0) * i;

                    // Add new button to button array so it is activated with others
                    ___buttons = btnList.ToArray();
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleScene), "ShowButtons")]
            private static bool TitleSceneOverridePatch(TitleScene __instance,
                ref System.Collections.IEnumerator __result)
            {
                if (_roadwayToMaker)
                {
                    __result = KKAPI.Utilities.CoroutineUtils.CreateCoroutine(() => StartMaker(0f, __instance));
                    return false;
                }

                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EditScene), "Start")]
            private static void EditSceneBackButtonOverridePatch(EditScene __instance, Button ___toReturn)
            {
                if (_roadwayToMaker)
                {
                    ___toReturn.onClick.ActuallyRemoveAllListeners();
                    var txt = ___toReturn.GetComponentInChildren<Text>();
                    txt.text = "Exit game";
                    ___toReturn.onClick.AddListener(() =>
                        __instance.GC.CreateModalYesNoUI(
                            "Are you sure you want to exit the game?\nUnsaved changes will be lost.",
                            UnityEngine.Application.Quit));

                    GameObject.Find("Pause Menue Canvas(Clone)/Buttons/Button Title")?.SetActive(false);
                }
                else if (_usedMakerButton)
                {
                    ___toReturn.onClick.ActuallyRemoveAllListeners();
                    var txt = ___toReturn.GetComponentInChildren<Text>();
                    txt.text = "Back to title";
                    ___toReturn.onClick.AddListener(() => __instance.GC.CreateModalYesNoUI(
                        "Are you sure you want to go back to the title screen?\nUnsaved changes will be lost.",
                        () => { __instance.GC.ChangeScene("TitleScene", "", 1); }));
                }
            }
        }
    }
}