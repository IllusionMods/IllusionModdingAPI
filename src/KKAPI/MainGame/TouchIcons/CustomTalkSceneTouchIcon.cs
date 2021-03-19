using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.MainGame
{
    internal static class CustomTalkSceneTouchIcon
    {
        private sealed class TouchIconEntry
        {
            public readonly Sprite Icon;
            public readonly int Row;
            public readonly int Order;
            public readonly Action<Button> OnCreated;

            public TouchIconEntry(Sprite icon, Action<Button> onCreated, int row, int order)
            {
                Icon = icon;
                OnCreated = onCreated;
                Row = row;
                Order = order;
            }
        }

        private static readonly List<TouchIconEntry> _buttons = new List<TouchIconEntry>();

        public static void AddTouchIcon(Sprite icon, Action<Button> onCreated, int row = 1, int order = 0)
        {
            if (icon == null) throw new ArgumentNullException(nameof(icon));
            if (onCreated == null) throw new ArgumentNullException(nameof(onCreated));
            if (row < 0 || row > 5) throw new ArgumentOutOfRangeException(nameof(row), "row should be either 0, 1 or 2. It can't be below 0 or higher than 5");

            Object.DontDestroyOnLoad(icon);

            _buttons.Add(new TouchIconEntry(icon, onCreated, row, order));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TalkScene), "Awake")]
        private static void AwakePost(Button[] ___buttonTouch)
        {
            var sourceTransform = ___buttonTouch[0].transform;
            var otherSourceTransform = ___buttonTouch[1].transform;
            var change = (sourceTransform.localPosition - otherSourceTransform.localPosition).x;

            foreach (var entryRow in _buttons.GroupBy(x => x.Row))
            {
                var xOffset = -change;
                // Take account of the 2 stock buttons
                if (entryRow.Key == 0) xOffset += 2 * change;
                // Tweak the offset to make buttons in row 2 fit on the black letterbox part
                var yOffset = change * entryRow.Key * 0.97f;
                var lastPosition = otherSourceTransform.localPosition + new Vector3(xOffset, yOffset, 0);

                foreach (var entry in entryRow.OrderBy(x => x.Order).ThenBy(x => _buttons.IndexOf(x)))
                {
                    var copy = Object.Instantiate(sourceTransform.gameObject, sourceTransform.parent, false);

                    copy.transform.localPosition = lastPosition + new Vector3(change, 0, 0);
                    lastPosition = copy.transform.localPosition;

                    var btn = copy.GetComponent<Button>();
                    btn.onClick.ActuallyRemoveAllListeners();

                    btn.image.sprite = entry.Icon;

                    entry.OnCreated(btn);
                }
            }
        }
    }
}
