using System;
using System.Collections.Generic;
using System.Linq;
using H;
using HarmonyLib;
using Illusion.Extensions;
using Illusion.Game;
using IllusionUtility.SetUtility;
using Manager;
using SceneAssist;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.MainGame
{
    /// <summary>
    /// Used to add custom buttons to the H Scene "trespassing" menu (red button below position selections on the right, originally used when having H near another girl so you can start 3P).
    /// </summary>
    public static class CustomTrespassingHsceneButtons
    {
        private static readonly List<CustomTrespassingButton> _customTrespassingButtons = new List<CustomTrespassingButton>();

        /// <summary>
        /// Add a new button to the H Scene "trespassing" menu (red button below position selections on the right, originally used when having H near another girl so you can start 3P).
        /// After user clicks the button, a confirmation dialog will be shown.
        /// </summary>
        /// <inheritdoc cref="AddHsceneTrespassingButton"/>
        /// <param name="confirmBoxTitle">Title of the confirmation box after user clicked the button.</param>
        /// <param name="confirmBoxSentence">Explanation text of the confirmation box after user clicked the button. Can have newlines but try to keep it short.</param>
        /// <param name="onConfirmed">Called when user clicks the button and presses "Yes" in the confirmation box. If the user presses "No" nothing is done.</param>
        public static IDisposable AddHsceneTrespassingButtonWithConfirmation(
            string buttonText, Func<HSprite, bool> spawnConditionCheck,
            string confirmBoxTitle, string confirmBoxSentence, Action<HSprite> onConfirmed)
        {
            return AddHsceneTrespassingButton(buttonText, spawnConditionCheck,
                hSprite =>
                {
                    var confirmBox = ConfirmDialog.status;
                    confirmBox.Title = confirmBoxTitle;
                    confirmBox.Sentence = confirmBoxSentence;
                    confirmBox.Yes = () => onConfirmed(hSprite);
                    confirmBox.No = () => { };
                    ConfirmDialog.Load();
                });
        }

        /// <summary>
        /// Add a new button to the H Scene "trespassing" menu (red button below position selections on the right, originally used when having H near another girl so you can start 3P).
        /// </summary>
        /// <param name="buttonText">Text on the button. The button is styled as a speech bubble, so consider spelling it like the MC is saying this.</param>
        /// <param name="spawnConditionCheck">
        /// Check if the button should be shown in the current H scene.
        /// Called at scene start. Return true if button should be spawned, false if not.
        /// Set to null if the button should always be spawned.
        /// </param>
        /// <param name="onButtonClicked">Called when user clicks the button and it is interactable.</param>
        /// <returns>Returns a disposable that when disposed removes this button permanently.</returns>
        public static IDisposable AddHsceneTrespassingButton(
            string buttonText, Func<HSprite, bool> spawnConditionCheck,
            Action<HSprite> onButtonClicked)
        {
            var btn = new CustomTrespassingButton(buttonText, spawnConditionCheck, onButtonClicked);

            _customTrespassingButtons.Add(btn);

            Hooks.ApplyHooks();

            return Disposable.Create(() =>
            {
                btn.Dispose();
                _customTrespassingButtons.Remove(btn);
            });
        }

        private static int SpawnButtons(HSprite hSprite, int existingButtons)
        {
            var spawnedButtons = 0;

            foreach (var trespassingButton in _customTrespassingButtons)
            {
                try
                {
                    trespassingButton.Dispose();

                    if (SpawnSingleButton(spawnedButtons + existingButtons, hSprite, trespassingButton))
                        spawnedButtons++;
                }
                catch (Exception ex)
                {
                    //todo use logger
                    Console.WriteLine($"Failed to spawn CustomTrespassingButton text={trespassingButton.ButtonText.Replace('\r', ' ').Replace('\n', ' ')}\n{ex}");
                }
            }

            if (spawnedButtons > 0)
            {
                Console.WriteLine($"Created {spawnedButtons} CustomTrespassingButtons ({existingButtons} existing)");
            }

            return spawnedButtons;
        }

        private static bool SpawnSingleButton(int id, HSprite hSprite, CustomTrespassingButton buttonData)
        {
            if (buttonData.SpawnConditionCheck != null && !buttonData.SpawnConditionCheck(hSprite)) return false;

            Console.WriteLine($"spawn id={id} name=" + buttonData.ButtonText);

            var defaultBtn = hSprite.menuActionSub.GetObject(7);

            var copyBtn = Object.Instantiate(defaultBtn, defaultBtn.transform.parent, false);
            try
            {
                copyBtn.name = "CustomTrespassButton_" + id;

                Object.DestroyImmediate(copyBtn.GetComponent<TextChangeCtrl>());
                Object.DestroyImmediate(copyBtn.GetComponent<HSpriteAutoDisable>());

                var pa = copyBtn.GetComponent<PointerAction>();
                pa.listDownAction.Add(hSprite.OnMouseDownSlider);

                copyBtn.GetComponentInChildren<TextMeshProUGUI>().text = buttonData.ButtonText;

                // Offset by 100 for each new button, don't offset first if the default button isn't shown
                copyBtn.transform.SetLocalPositionY(copyBtn.transform.localPosition.y - 100 * id);

                var btn = copyBtn.GetComponent<Button>();

                var evt = btn.onClick;
                evt.m_Calls.Clear();
                evt.m_Calls.ClearPersistent();
                evt.m_PersistentCalls.Clear();
                evt.m_CallsDirty = false;

                btn.onClick.AddListener(() =>
                {
                    if (Scene.IsNowLoadingFade) return;
                    if (Scene.NowSceneNames[0] != "HProc") return;
                    if (!hSprite.IsSpriteAciotn()) return;

                    Utils.Sound.Play(SystemSE.sel);

                    buttonData.OnButtonClicked(hSprite);
                });

                hSprite.menuActionSub.lstObj.Add(copyBtn);
                hSprite.menuActionSub.makeParent.Add(copyBtn);

                buttonData.Instance = copyBtn;
            }
            catch
            {
                Object.Destroy(copyBtn);
                throw;
            }

            return true;
        }

        private static class Hooks
        {
            private static bool? _defaultBtnShown;

            public static void ApplyHooks()
            {
                Harmony.CreateAndPatchAll(typeof(Hooks), typeof(Hooks).FullName);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.InitTrespassingHelp))]
            private static void InitTrespassingHelpPost(HSprite __instance)
            {
                var defaultBtnShown = __instance.btnTrespassing.gameObject.activeSelf;
                var spawnedCount = SpawnButtons(__instance, defaultBtnShown ? 1 : 0);
                if (spawnedCount > 0)
                {
                    __instance.btnTrespassing.gameObject.SetActiveIfDifferent(true);
                    _defaultBtnShown = defaultBtnShown;
                }
                else
                {
                    _defaultBtnShown = null;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.InitPointMenuAndHelp))]
            private static void InitPointMenuAndHelpPost(HSprite __instance)
            {
                if (_defaultBtnShown == false)
                {
                    // Prevent the "someone is watching" hint from appearing since no one actually is
                    __instance.autoDisableTrespassingHelp.ForceFlag();
                    __instance.autoDisableTrespassingHelp.gameObject.SetActiveIfDifferent(false);
                    __instance.menuActionSub.SetActive(false, 7);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnActionMenu))]
            private static void InitPointMenuAndHelpPost(HSprite __instance, int _kind)
            {
                // Not initialized / no custom buttons
                if (_defaultBtnShown == null) return;

                Console.WriteLine(
                    $"_kind={_kind} _defaultBtnShown={(_defaultBtnShown != null ? _defaultBtnShown.Value.ToString() : "NULL")} active={__instance.autoDisableTrespassingHelp.gameObject.activeSelf}");

                if (_kind == 3)
                {
                    bool menuIsVisible;
                    if (_defaultBtnShown == false)
                    {
                        __instance.menuActionSub.SetActive(false, 7);

                        // Toggle the menu based on our own buttons since the default button is always off
                        menuIsVisible =
                            !_customTrespassingButtons.Any(x => x.Instance != null && x.Instance.activeSelf);
                    }
                    else
                    {
                        menuIsVisible = !__instance.autoDisableTrespassingHelp.gameObject.activeSelf;
                    }

                    if (menuIsVisible)
                    {
                        foreach (var customItem in _customTrespassingButtons)
                            if (customItem.Instance != null)
                                customItem.Instance.SetActiveIfDifferent(true);
                        return;
                    }
                }

                // Disable all if not showing trespassing submenu
                foreach (var customItem in _customTrespassingButtons)
                    if (customItem.Instance != null)
                        customItem.Instance.SetActiveIfDifferent(false);
            }
        }

        private sealed class CustomTrespassingButton : IDisposable
        {
            public readonly string ButtonText;
            public readonly Action<HSprite> OnButtonClicked;
            public readonly Func<HSprite, bool> SpawnConditionCheck;
            public GameObject Instance;

            public CustomTrespassingButton(string buttonText, Func<HSprite, bool> spawnConditionCheck, Action<HSprite> onButtonClicked)
            {
                if (buttonText == null) throw new ArgumentNullException(nameof(buttonText));
                if (onButtonClicked == null) throw new ArgumentNullException(nameof(onButtonClicked));

                ButtonText = buttonText;
                SpawnConditionCheck = spawnConditionCheck;
                OnButtonClicked = onButtonClicked;
            }

            public void Dispose()
            {
                if (Instance != null)
                {
                    var hSprite = Object.FindObjectOfType<HSprite>();
                    if (hSprite != null)
                    {
                        hSprite.menuActionSub.lstObj.Remove(Instance);
                        hSprite.menuActionSub.makeParent.Remove(Instance);
                    }

                    Object.Destroy(Instance);
                }

                Instance = null;
            }
        }
    }
}