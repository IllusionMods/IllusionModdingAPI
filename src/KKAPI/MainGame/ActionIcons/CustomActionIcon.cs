using System;
using System.Collections.Generic;
using ActionGame;
using ActionGame.Chara;
using HarmonyLib;
using Illusion.Component;
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
            public readonly Sprite IconOff, IconOn;
            public readonly int MapNo;
            public readonly Vector3 Position;
            public readonly Action OnOpen;
            public readonly Action<TriggerEnterExitEvent> OnCreated;

            public ActionIconEntry(int mapNo, Vector3 position, Sprite iconOn, Sprite iconOff, Action onOpen, Action<TriggerEnterExitEvent> onCreated)
            {
                IconOff = iconOff;
                OnOpen = onOpen;
                OnCreated = onCreated;
                IconOn = iconOn;
                MapNo = mapNo;
                Position = position;
            }
        }

        private static readonly List<ActionIconEntry> _entries = new List<ActionIconEntry>();

        public static void AddActionIcon(int mapNo, Vector3 position, Sprite iconOn, Sprite iconOff, Action onOpen, Action<TriggerEnterExitEvent> onCreated = null)
        {
            if (iconOn == null) throw new ArgumentNullException(nameof(iconOn));
            if (iconOff == null) throw new ArgumentNullException(nameof(iconOff));
            if (onOpen == null) throw new ArgumentNullException(nameof(onOpen));

            Object.DontDestroyOnLoad(iconOn);
            Object.DontDestroyOnLoad(iconOff);

            var entry = new ActionIconEntry(mapNo, position, iconOn, iconOff, onOpen, onCreated);
            _entries.Add(entry);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActionMap), "Reserve")]
        private static void OnMapChangedHook(ActionMap __instance)
        {
            if (__instance.mapRoot == null || __instance.isMapLoading) return;

            var created = 0;

            foreach (var iconEntry in _entries)
            {
                if (iconEntry.MapNo == __instance.no)
                {
                    try
                    {
                        SpawnActionPoint(iconEntry);
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

        private static void SpawnActionPoint(ActionIconEntry iconEntry)
        {
            var inst = CommonLib.LoadAsset<GameObject>("map/playeractionpoint/00.unity3d", "PlayerActionPoint_05", true);
            var parent = GameObject.Find("Map/ActionPoints");
            inst.transform.SetParent(parent.transform, true);

            var pap = inst.GetComponentInChildren<PlayerActionPoint>();
            var iconRootObject = pap.gameObject;
            var iconRootTransform = pap.transform;
            Object.DestroyImmediate(pap, false);

            iconRootTransform.position = iconEntry.Position;

            var evt = iconRootObject.AddComponent<TriggerEnterExitEvent>();
            var animator = iconRootObject.GetComponentInChildren<Animator>();
            var rendererIcon = iconRootObject.GetComponentInChildren<SpriteRenderer>();
            rendererIcon.sprite = iconEntry.IconOff;
            var playerInRange = false;
            evt.onTriggerEnter += c =>
            {
                if (!c.CompareTag("Player")) return;
                playerInRange = true;
                animator.Play("icon_action");
                rendererIcon.sprite = iconEntry.IconOn;
                c.GetComponent<Player>().actionPointList.Add(evt);
            };
            evt.onTriggerExit += c =>
            {
                if (!c.CompareTag("Player")) return;
                playerInRange = false;
                animator.Play("icon_stop");
                rendererIcon.sprite = iconEntry.IconOff;
                c.GetComponent<Player>().actionPointList.Remove(evt);
            };

            var player = Singleton<Game>.Instance.actScene.Player;
            evt.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    // Hide in H scenes and other places
                    var isVisible = Singleton<Game>.IsInstance() && !Singleton<Game>.Instance.IsRegulate(true);
                    if (rendererIcon.enabled != isVisible)
                        rendererIcon.enabled = isVisible;

                    // Check if player clicked this point
                    if (isVisible && playerInRange && ActionInput.isAction && !player.isActionNow)
                        iconEntry.OnOpen();
                })
                .AddTo(evt);

            iconEntry.OnCreated?.Invoke(evt);
        }
    }
}
