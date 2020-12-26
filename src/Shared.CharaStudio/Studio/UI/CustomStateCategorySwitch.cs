#if AI || HS2
#define TMP
#endif

using Studio;
using System;
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
    /// Custom control that draws a single, circular button with an on/off state in the Chara > CurrentState studio menu.
    /// </summary>
    public class CurrentStateCategorySwitch : BaseCurrentStateEditableGuiEntry<bool>
    {
        private static GameObject _originalObject;
#if TMP
        private const float LineSpacing = -20;
#else
        private const float LineSpacing = 0.5f;
#endif

        /// <summary>
        /// A single button for the Chara > CurrentState studio menu.
        /// </summary>
        /// <param name="name">Name of the button, shown on left</param> 
        /// <param name="updateValue">Function called when the current character changes and the on/off state needs to be updated.
        /// <code>OCIChar</code> is the newly selected character. Return the new state. Can't be null.</param>
        public CurrentStateCategorySwitch(string name, Func<OCIChar, bool> updateValue) : base(name, updateValue) { }

        /// <inheritdoc />
        protected override GameObject CreateItem(GameObject categoryObject)
        {
            if (_originalObject == null)
                _originalObject = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Etc/Son");

            var copy = Object.Instantiate(_originalObject, categoryObject.transform, true);
            copy.gameObject.SetActive(true);
            copy.transform.localScale = Vector3.one;
            copy.name = "CustomSwitch " + Name;

            var text = copy.GetComponentInChildren<Text>(true);
            text.lineSpacing = LineSpacing;
            text.gameObject.SetActive(true);
            text.gameObject.name = "Text " + Name;
            text.text = Name;

            var trt = text.rectTransform;
            trt.offsetMin = new Vector2(0, -20);
            trt.offsetMax = new Vector2(100, 0);

            var toggle = copy.GetComponentInChildren<Toggle>(true);
            toggle.gameObject.SetActive(true);
            toggle.gameObject.name = $"Button {Name}";
#if PH
            toggle.transform.localPosition = new Vector3(100, 0, 0);
#endif

            toggle.isOn = Value.Value;

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(Value.OnNext);
            Value.Subscribe(newSet => toggle.isOn = newSet);

            return copy;
        }
    }
}
