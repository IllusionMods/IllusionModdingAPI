using System;
using System.Linq;
using Studio;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Custom control that draws from 2 to 4 radio buttons (they are drawn like toggles)
    /// </summary>
    public class CurrentStateCategoryToggle : BaseCurrentStateEditableGuiEntry<int>
    {
        private static GameObject _originalToggle;
        
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

            var text = copy.transform.Find("Text Tears");
            text.name = "Text " + Name;
            var text1 = text.GetComponent<Text>();
            text1.text = Name;

            var subItems = copy.transform.Cast<Transform>();
            var buttons = subItems.Skip(1).Select(x => x.GetComponent<Button>()).ToList();
            for (var i = 0; i < 4; i++)
            {
                var btn = buttons[i];
                btn.transform.name = $"Button {Name} {i}";
                btn.onClick.RemoveAllListeners();

                if (ToggleCount > i)
                {
                    btn.gameObject.SetActive(true);
                    btn.onClick.AddListener(() => Value.OnNext(buttons.IndexOf(btn)));
                }
                else
                    Object.Destroy(btn.gameObject);
            }

            Value.Subscribe(
                newval =>
                {
                    for (var i = 0; i < buttons.Count; i++)
                    {
                        var b = buttons[i];
                        if (b == null) continue;
                        b.image.color = !b.interactable || i != newval ? Color.white : Color.green;
                    }
                });

            return copy;
        }
    }
}
