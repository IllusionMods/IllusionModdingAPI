#if AI || HS2
#define TMP
#endif

using Illusion.Extensions;
using KKAPI.Utilities;
using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if TMP
using Text = TMPro.TextMeshProUGUI;
#endif

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Custom control that draws from 2 to 4 radio buttons (they are drawn like toggles)
    /// </summary>
    public class CurrentStateCategoryToggle : BaseCurrentStateEditableGuiEntry<int>
    {
        private static GameObject _originalToggle;
#if TMP
        private const float LineSpacing = -20;
#else
        private const float LineSpacing = 0.5f;
#endif

        /// <summary>
        /// Number of the radio buttons, can be 2, 3 or 4
        /// </summary>
        public int ToggleCount { get; }

        /// <summary>
        /// A toggle list for the Chara &gt; CurrentState studio menu.
        /// </summary>
        /// <param name="name">Name of the list, shown on left</param>
        /// <param name="toggleCount">Number of the toggles, can be 2, 3 or 4</param>
        /// <param name="onUpdateSelection">Function called when the current character changes and the selected index needs to be updated.
        /// <code>OCIChar</code> is the newly selected character. Return the new selected index. Can't be null.</param>
        public CurrentStateCategoryToggle(string name, int toggleCount, Func<OCIChar, int> onUpdateSelection) : base(name, onUpdateSelection)
        {
            if (toggleCount > 4 || toggleCount < 2) throw new ArgumentException("Need to set 2 to 4 toggle buttons", nameof(toggleCount));
            ToggleCount = toggleCount;
        }

        /// <summary>
        /// Currently selected button (starts at 0)
        /// </summary>
        [Obsolete("Use Value instead")]
        public BehaviorSubject<int> SelectedIndex => Value;

        /// <inheritdoc />
        protected override GameObject CreateItem(GameObject categoryObject)
        {
            if (_originalToggle == null)
                _originalToggle = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Etc/Tears");

            var copy = Object.Instantiate(_originalToggle, categoryObject.transform, true);
            copy.name = "CustomToggle-" + Name;
            copy.transform.localScale = Vector3.one;

            var children = copy.transform.Children();
            foreach (var transform in children.Skip(1))
                Object.Destroy(transform.gameObject);

            var textTr = children[0];
            var text = textTr.GetComponent<Text>();
            text.lineSpacing = LineSpacing;
            textTr.name = "Text " + Name;
            text.text = Name;

            var trt = text.rectTransform;
            trt.offsetMin = new Vector2(0, -20);
            trt.offsetMax = new Vector2(100, 0);

            var copiedToggles = new List<Button>(ToggleCount);
            var liquidToggles = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Liquid");
            // Skip the first text and take the 3 toggle buttons
            var origToggles = liquidToggles.transform.Children().Skip(1).Take(3).ToList();
            foreach (var origToggle in origToggles)
            {
                if (ToggleCount <= copiedToggles.Count) break;

                var toggleCopy = Object.Instantiate(origToggle, copy.transform, true);
                var btn = toggleCopy.GetComponent<Button>();
                btn.onClick.ActuallyRemoveAllListeners();
                copiedToggles.Add(btn);

                if (copiedToggles.Count == 3 && ToggleCount == 4)
                {
                    toggleCopy = Object.Instantiate(origToggle, copy.transform, true);
                    btn = toggleCopy.GetComponent<Button>();
                    btn.onClick.ActuallyRemoveAllListeners();
                    copiedToggles.Add(btn);

#if PH
                    toggleCopy.localPosition = origToggles[0].transform.localPosition;
#else
                    toggleCopy.localPosition += copiedToggles[1].transform.localPosition - copiedToggles[0].transform.localPosition;
#endif
                }
            }

            for (var i = 0; i < copiedToggles.Count; i++)
            {
                var copiedToggle = copiedToggles[i];

                var buttonIndex = i;
                copiedToggle.transform.name = $"Button {Name} {buttonIndex}";
                copiedToggle.onClick.AddListener(() => Value.OnNext(buttonIndex));

#if PH
                copiedToggle.transform.localPosition = origToggles[0].transform.localPosition + new Vector3(17, 0, 0) * i;
#else
                copiedToggle.transform.localPosition = new Vector3(copiedToggle.transform.localPosition.x, 0, 0);
#endif
                copiedToggle.transform.localScale = Vector3.one;
                copiedToggle.gameObject.SetActive(true);
            }

            Value.Subscribe(
                newval =>
                {
                    for (var i = 0; i < copiedToggles.Count; i++)
                    {
                        var b = copiedToggles[i];
                        if (b == null) continue;
                        b.image.color = !b.interactable || i != newval ? Color.white : Color.green;
                    }
                });

            return copy;
        }
    }
}
