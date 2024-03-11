#if !PH
#if AI || HS2
#define TMP
#endif

using KKAPI.Utilities;
using Studio;
using GameStudio = Studio;
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
    internal class CurrentStateCategoryColorPicker : BaseCurrentStateEditableGuiEntry<Color>
    {
        private static GameObject _originalObject;
#if TMP
        private const float LineSpacing = -20;
#else
        private const float LineSpacing = 0.5f;
#endif
        public Action<Color> OnValueChanged;

        /// <summary>
        /// Custom control that draws a slider in the Chara > CurrentState studio menu.
        /// </summary>
        /// <param name="name">Name of the button, shown on left</param> 
        /// <param name="updateValue">Function called when the current character changes and the slider value needs to be updated.
        /// <code>OCIChar</code> is the newly selected character. Return the new state. Can't be null.</param>
        /// <param name="onValueChanged">Action to perform when changing the color in the color picker</param>
        public CurrentStateCategoryColorPicker(string name, Func<OCIChar, Color> updateValue, Action<Color> onValueChanged) : base(name, updateValue)
        {
            OnValueChanged = onValueChanged;
        }

        /// <inheritdoc />
        protected override GameObject CreateItem(GameObject categoryObject)
        {
            if (_originalObject == null)
                _originalObject = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Etc/Color");

            var copy = Object.Instantiate(_originalObject, categoryObject.transform, true);
            copy.gameObject.SetActive(true);
            copy.name = "CustomColor " + Name;
            copy.transform.localScale = Vector3.one;

            var text = copy.GetComponentInChildren<Text>(true);
            text.lineSpacing = LineSpacing;
            text.gameObject.SetActive(true);
            text.gameObject.name = "Text " + Name;
            text.text = Name;

            var button = copy.GetComponentInChildren<Button>(true);
            button.gameObject.SetActive(true);

            var image = button.GetComponentInChildren<Image>(true);
            image.gameObject.SetActive(true);

            image.color = Value.Value;
            button.onClick.ActuallyRemoveAllListeners();
            button.onClick.AddListener(() => Value.OnNext(Value.Value));
            button.onClick.AddListener(() =>
            {
                Singleton<GameStudio.Studio>.Instance.colorPalette.Setup(Name, Value.Value, c => { OnValueChanged(c); image.color = c; }, true);
                Singleton<GameStudio.Studio>.Instance.colorPalette.visible = true;
            });
            Value.Subscribe(newValue => image.color = newValue);

            return copy;
        }
    }
}
#endif