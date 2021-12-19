using System;
using System.Collections.Generic;
using System.Linq;
using ActionGame;
using ActionGame.Chara;
using HarmonyLib;
using Illusion.Component;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KKAPI.MainGame
{
    internal static class CustomActionIcon
    {
        private sealed class ActionIconEntry
        {
            public readonly Texture Icon;
            public readonly Color Color;
            public readonly string PopupText;
            public readonly int MapNo;
            public readonly Vector3 Position;
            public readonly Action OnOpen;
            public readonly Action<TriggerEnterExitEvent> OnCreated;

            public Object Instance;

            public ActionIconEntry(int mapNo, Vector3 position, Texture icon, Color color, string popupText, Action onOpen, Action<TriggerEnterExitEvent> onCreated)
            {
                OnOpen = onOpen;
                OnCreated = onCreated;
                Icon = icon;
                Color = color;
                PopupText = popupText;
                MapNo = mapNo;
                Position = position;
            }

            public void Dispose()
            {
                Object.Destroy(Instance);
            }
        }

        private static readonly List<ActionIconEntry> _entries = new List<ActionIconEntry>();

        public static IDisposable AddActionIcon(int mapNo, Vector3 position, Texture icon, Color color, string popupText, Action onOpen, Action<TriggerEnterExitEvent> onCreated = null, bool delayed = true, bool immediate = false)
        {
            if (icon == null) throw new ArgumentNullException(nameof(icon));
            if (popupText == null) throw new ArgumentNullException(nameof(popupText));
            if (onOpen == null) throw new ArgumentNullException(nameof(onOpen));

            Object.DontDestroyOnLoad(icon);

            var entry = new ActionIconEntry(mapNo, position, icon, color, popupText, onOpen, onCreated);

            if (delayed)
                _entries.Add(entry);

            if (immediate && ActionScene.initialized && ActionScene.instance.Player?.mapNo == mapNo)
                SpawnActionPoint(entry, 100);

            return Disposable.Create(() =>
            {
                _entries.Remove(entry);
                entry.Dispose();
            });
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActionMap), nameof(ActionMap.Reserve))]
        private static void OnMapChangedHook(ActionMap __instance)
        {
            if (__instance.mapRoot == null || __instance.isMapLoading) return;

            var created = 0;

            foreach (var iconEntry in _entries)
            {
                if (iconEntry.MapNo == __instance.no && iconEntry.Instance == null)
                {
                    try
                    {
                        SpawnActionPoint(iconEntry, created);
                        created++;
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError($"Failed to created custom action point on map no {__instance.no} at {iconEntry.Position}\n{e}");
                    }
                }
            }

            if (created > 0)
                KoikatuAPI.Logger.LogDebug($"Created {created} custom action points on map no {__instance.no}");
        }

        private static void SpawnActionPoint(ActionIconEntry iconEntry, int created)
        {
            var inst = CommonLib.LoadAsset<GameObject>("map/playeractionpoint/00.unity3d", "PlayerActionPoint_05", true);
            inst.gameObject.name = "CustomActionPoint_" + created;
            var parent = GameObject.Find("Map/ActionPoints");
            inst.transform.SetParent(parent.transform, true);

            var pap = inst.GetComponentInChildren<PlayerActionPoint>();
            var iconRootObject = pap.gameObject;
            pap.gameObject.name = inst.name;
            var iconRootTransform = pap.transform;

            var rendererIcon = pap.renderers.Reverse().First(x =>
            {
                var tex = x.material.mainTexture;
                return tex.width == 256 && tex.height == 256;
            });
            var animator = pap.animator;

            pap.gameObject.layer = LayerMask.NameToLayer("Action/ActionPoint");

            foreach (Transform child in pap.transform.parent)
            {
                if (child != pap.transform)
                    Object.Destroy(child.gameObject);
            }
            Object.DestroyImmediate(pap, false);

            iconRootTransform.position = iconEntry.Position;

            // Set color to pink
            var pointColor = iconEntry.Color;
            foreach (var rend in iconRootTransform.GetComponentsInChildren<MeshRenderer>())
                rend.material.color = pointColor;
#pragma warning disable 618
            foreach (var rend in iconRootTransform.GetComponentsInChildren<ParticleSystem>())
                rend.startColor = pointColor;
#pragma warning restore 618

            // Hook up event/anim logic
            var evt = iconRootObject.AddComponent<TriggerEnterExitEvent>();
            rendererIcon.material.mainTexture = iconEntry.Icon;
            var playerInRange = false;
            evt.onTriggerEnter += c =>
            {
                if (!c.CompareTag("Player")) return;
                playerInRange = true;
                animator.Play(PAP.Assist.Animation.SpinState);
                Utils.Sound.Play(Manager.Sound.Type.GameSE2D, Utils.Sound.SEClipTable[0x38], 0f);
                c.GetComponent<Player>().actionPointList.Add(evt);
                ActionScene.instance.actionChangeUI.Set(ActionChangeUI.ActionType.Shop);
                ActionScene.instance.actionChangeUI._text.text = iconEntry.PopupText;
            };
            evt.onTriggerExit += c =>
            {
                if (!c.CompareTag("Player")) return;
                playerInRange = false;
                animator.Play(PAP.Assist.Animation.IdleState);
                c.GetComponent<Player>().actionPointList.Remove(evt);
                ActionScene.instance.actionChangeUI.Remove(ActionChangeUI.ActionType.Shop);
            };

            evt.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    // Hide in H scenes and other places
                    var isVisible = !Game.IsRegulate(true);
                    if (rendererIcon.enabled != isVisible)
                        rendererIcon.enabled = isVisible;

                    // Check if player clicked this point
                    if (isVisible && playerInRange && ActionInput.isAction && !ActionScene.instance.Player.isActionNow)
                        iconEntry.OnOpen();
                })
                .AddTo(evt);

            iconEntry.Instance = inst;

            iconEntry.OnCreated?.Invoke(evt);
        }
    }
}
