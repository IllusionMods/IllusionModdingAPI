using System;
using Studio;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Custom control that draws a single, circular button with an on/off state in the Chara > CurrentState studio menu.
    /// </summary>
    public class CurrentStateCategorySwitch : BaseCurrentStateEditableGuiEntry<bool>
    {
        private static GameObject _originalObject;

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

#if AI
            var text = copy.GetComponentInChildren<TextMeshProUGUI>(true);
#else
            var text = copy.GetComponentInChildren<Text>(true);
#endif
            text.gameObject.SetActive(true);
            text.gameObject.name = "Text " + Name;
            text.text = Name;

            var toggle = copy.GetComponentInChildren<Toggle>(true);
            toggle.gameObject.SetActive(true);
            toggle.gameObject.name = $"Button {Name}";

            toggle.isOn = Value.Value;

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(Value.OnNext);
            Value.Subscribe(newSet => toggle.isOn = newSet);

            return copy;
        }
    }
}
