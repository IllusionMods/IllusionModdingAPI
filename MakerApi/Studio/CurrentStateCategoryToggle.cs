using System;
using System.Linq;
using Studio;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MakerAPI.Studio
{
    public class CurrentStateCategoryToggle : CurrentStateCategorySubItemBase
    {
        public int ToggleCount { get; }

        private readonly Func<OCIChar, int> _updateFunc;

        public CurrentStateCategoryToggle(string name, int toggleCount, Func<OCIChar, int> updateFunc) : base(name)
        {
            if (toggleCount > 4 || toggleCount < 2) throw new ArgumentException("Need to set 2 to 4 toggle buttons", nameof(toggleCount));
            ToggleCount = toggleCount;

            _updateFunc = updateFunc ?? throw new ArgumentNullException(nameof(updateFunc));

            SelectedIndex = new BehaviorSubject<int>(0);
        }

        public BehaviorSubject<int> SelectedIndex { get; }

        protected internal override void CreateItem(GameObject categoryObject)
        {
            var original = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Etc/Tears");

            var copy = Object.Instantiate(original, categoryObject.transform, true);
            copy.name = "CustomToggle-" + Name;

            var text = copy.transform.Find("Text Tears");
            text.name = "Text " + Name;
            var text1 = text.GetComponent<Text>();
            text1.text = Name;

            var items = copy.transform.Cast<Transform>().ToList();

            foreach (var transform in items.Select(x => x.GetComponent<RectTransform>()))
            {
                transform.offsetMin = new Vector2(transform.offsetMin.x + 21, transform.offsetMin.y);
                transform.offsetMax = new Vector2(transform.offsetMax.x + 21, transform.offsetMax.y);
            }

            var buttons = items.Skip(1).Select(x => x.GetComponent<Button>()).ToList();
            for (var i = 0; i < 4; i++)
            {
                var btn = buttons[i];
                btn.transform.name = $"Button {Name} {i}";
                btn.onClick.RemoveAllListeners();

                if (ToggleCount > i)
                {
                    btn.gameObject.SetActive(true);
                    btn.onClick.AddListener(() => SelectedIndex.OnNext(buttons.IndexOf(btn)));
                }
                else
                    Object.Destroy(btn.gameObject);
            }

            SelectedIndex.Subscribe(
                newval =>
                {
                    for (var i = 0; i < buttons.Count; i++)
                    {
                        var b = buttons[i];
                        if (b == null) continue;
                        b.image.color = !b.interactable || i != newval ? Color.white : Color.green;
                    }
                });
        }

        protected internal override void UpdateInfo(OCIChar ociChar)
        {
            SelectedIndex.OnNext(_updateFunc.Invoke(ociChar));
        }
    }
}
