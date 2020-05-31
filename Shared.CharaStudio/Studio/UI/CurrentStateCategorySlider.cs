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
    /// Custom control that draws a slider in the Chara > CurrentState studio menu.
    /// </summary>
    public class CurrentStateCategorySlider : BaseCurrentStateEditableGuiEntry<float>
    {
        private static GameObject _originalObject;

        /// <summary>
        /// Minimum value of the slider
        /// </summary>
        public float MinValue { get; }

        /// <summary>
        /// Maximum value of the slider
        /// </summary>
        public float MaxValue { get; }

        /// <summary>
        /// Custom control that draws a slider in the Chara > CurrentState studio menu.
        /// </summary>
        /// <param name="name">Name of the button, shown on left</param> 
        /// <param name="updateValue">Function called when the current character changes and the slider value needs to be updated.
        /// <code>OCIChar</code> is the newly selected character. Return the new state. Can't be null.</param>
        /// <param name="minValue">Minimum value of the slider</param>
        /// <param name="maxValue">Maximum value of the slider</param>
        public CurrentStateCategorySlider(string name, Func<OCIChar, float> updateValue, float minValue = 0f, float maxValue = 1f) : base(name, updateValue)
        {
            if (minValue >= maxValue) throw new ArgumentException("minValue has to be smaller than maxValue");
            MinValue = minValue;
            MaxValue = maxValue;
        }

        /// <inheritdoc />
        protected override GameObject CreateItem(GameObject categoryObject)
        {
            if (_originalObject == null)
                _originalObject = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Etc/Cheek");

            var copy = Object.Instantiate(_originalObject, categoryObject.transform, true);
            copy.gameObject.SetActive(true);
            copy.name = "CustomSlider " + Name;
            copy.transform.localScale = Vector3.one;

#if AI || HS2
            var text = copy.GetComponentInChildren<TextMeshProUGUI>(true);
            text.lineSpacing = -20;
#else
            var text = copy.GetComponentInChildren<Text>(true);
            text.lineSpacing = 0.5f;
#endif
            text.gameObject.SetActive(true);
            text.gameObject.name = "Text " + Name;
            text.text = Name;

            var trt = text.rectTransform;
            trt.offsetMin = new Vector2(0, -20);
            trt.offsetMax = new Vector2(100, 0);

            var slider = copy.GetComponentInChildren<Slider>(true);
            slider.gameObject.SetActive(true);
            slider.gameObject.name = $"Slider {Name}";

            slider.minValue = MinValue;
            slider.maxValue = MaxValue;
            slider.value = Value.Value;

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(Value.OnNext);

            Value.Subscribe(newValue => slider.value = newValue);

            return copy;
        }
    }
}
