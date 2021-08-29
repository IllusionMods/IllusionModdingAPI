using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HarmonyLib;
using Illusion.Extensions;
using KKAPI.Utilities;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.MainGame
{
    internal static class TalkSceneCustomButtons
    {
        private static readonly List<TalkButtonEntry> _buttons = new List<TalkButtonEntry>();

        private sealed class TalkButtonEntry
        {
            public readonly string Text;
            public readonly Action<Button> OnCreated;
            public readonly TalkSceneActionKind TargetMenu;
            internal Object Instance;

            public TalkButtonEntry(string text, Action<Button> onCreated, TalkSceneActionKind targetMenu)
            {
                TargetMenu = targetMenu;
                Text = text;
                OnCreated = onCreated;
            }

            public void Dispose()
            {
                Object.Destroy(Instance);
            }
        }

        public static IDisposable AddTalkButton(string icon, Action<Button> onCreated, TalkSceneActionKind targetMenu)
        {
            if (icon == null) throw new ArgumentNullException(nameof(icon));
            if (onCreated == null) throw new ArgumentNullException(nameof(onCreated));
            if (targetMenu == TalkSceneActionKind.Listen) throw new NotSupportedException("TalkSceneActionKind.Listen does not support adding custom buttons");
            if (!Enum.IsDefined(typeof(TalkSceneActionKind), targetMenu)) throw new InvalidEnumArgumentException(nameof(targetMenu), (int)targetMenu, typeof(TalkSceneActionKind));

            var entry = new TalkButtonEntry(icon, onCreated, targetMenu);
            _buttons.Add(entry);

            return Disposable.Create(() =>
            {
                _buttons.Remove(entry);
                entry.Dispose();
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TalkScene), nameof(TalkScene.Start))]
        private static void Initialize(Button[] ___buttonTalkContents, Button[] ___buttonEventContents)
        {
            if (_buttons.Count == 0) return;

            try
            {
                // Fix inconsistent size of default buttons
                void NormalizeSizes(Button[] buttons)
                {
                    foreach (var button in buttons)
                    {
                        var rt = button.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(264, 48);
                        var txt = button.GetComponentInChildren<TextMeshProUGUI>();
                        txt.enableAutoSizing = true;
                        txt.fontSizeMax = 36;
                    }
                }
                NormalizeSizes(___buttonTalkContents);
                NormalizeSizes(___buttonEventContents);

                // Create custom buttons
                Button GetOriginalButton(TalkSceneActionKind actionKind)
                {
                    switch (actionKind)
                    {
                        case TalkSceneActionKind.Talk:
                            return ___buttonTalkContents[0];
                        case TalkSceneActionKind.Event:
                            return ___buttonEventContents[0];
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                foreach (var entryRow in _buttons)
                {
                    var origButton = GetOriginalButton(entryRow.TargetMenu);
                    var parent = origButton.transform.parent;
                    var childCount = parent.childCount;

                    var copy = Object.Instantiate(origButton.gameObject, parent, false);
                    copy.SetActiveIfDifferent(true);
                    copy.name = origButton.gameObject.name + childCount + " (KKAPI)";
                    copy.transform.localScale = Vector3.one;

                    var btn = copy.GetComponent<Button>();
                    btn.onClick.ActuallyRemoveAllListeners();

                    var txt = copy.GetComponentInChildren<TextMeshProUGUI>();
                    txt.text = entryRow.Text;

                    entryRow.Instance = copy;
                    entryRow.OnCreated(btn);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TalkScene), nameof(TalkScene.UpdateUI), typeof(bool))]
        private static void UpdateUIpost(Button[] ___buttonTalkContents, Button[] ___buttonEventContents)
        {
            try
            {
                // Move the list up if active buttons run off-screen to ensure all buttons are visible
                void AdjustPosition(Transform parent, int originalY, int countBeforeAdjusting)
                {
                    // Restore original position first since this method can get called multiple times
                    var newY = originalY;
                    var activeCount = parent.Children().Count(x => x.gameObject.activeSelf);
                    if (activeCount > countBeforeAdjusting) newY += 48 * (activeCount - countBeforeAdjusting);
                    parent.localPosition = new Vector3(608, newY, 0);
                }
                AdjustPosition(___buttonTalkContents[0].transform.parent, 100, 13);
                AdjustPosition(___buttonEventContents[0].transform.parent, -94, 9);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}