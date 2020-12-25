using System;
using UnityEngine.UI;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// A container for the value of a slider, associated label, slider, textbox, and reset button UI elements, and the setter method that triggers on value change.
    /// </summary>
    public class SceneEffectsSliderSet
    {
        private float CurrentValue = 0f;
        private bool EventsEnabled = false;

        #region Backing Fields
        private string _text;
        private float _sliderMinimum;
        private float _sliderMaximum;
        #endregion

        /// <summary>
        /// Label UI element.
        /// </summary>
        public Text Label { get; set; }
        /// <summary>
        /// Slider UI element.
        /// </summary>
        public Slider Slider { get; set; }
        ///// <summary>
        ///// Input field UI element.
        ///// </summary>
        //public InputField Input { get; set; }
        ///// <summary>
        ///// Reset button UI element.
        ///// </summary>
        //public Button Button { get; set; }
        /// <summary>
        /// Method called when the value of the toggle is changed.
        /// </summary>
        public Action<float> Setter { get; set; }
        /// <summary>
        /// Initial state of the toggle.
        /// </summary>
        public float InitialValue { get; set; }

        /// <summary>
        /// Minimum value the slider can slide. Can be overriden by the user typing in the textbox if EnforceSliderMinimum is set to false.
        /// </summary>
        public float SliderMinimum
        {
            get => _sliderMinimum;
            set
            {
                _sliderMinimum = value;
                Slider.minValue = value;
            }
        }
        /// <summary>
        /// Maximum value the slider can slide. Can be overriden by the user typing in the textbox if EnforceSliderMaximum is set to false.
        /// </summary>
        public float SliderMaximum
        {
            get => _sliderMaximum;
            set
            {
                _sliderMaximum = value;
                Slider.maxValue = value;
            }
        }

        /// <summary>
        /// Whether to enforce the SliderMinimum value. If false, users can type values in to the textbox that exceed the minimum value.
        /// </summary>
        public bool EnforceSliderMinimum { get; set; } = true;
        /// <summary>
        /// Whether to enforce the SliderMaximum value. If false, users can type values in to the textbox that exceed the maximum value.
        /// </summary>
        public bool EnforceSliderMaximum { get; set; } = true;

        /// <summary>
        /// Get or set the text of the label.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                Label.text = value;
            }
        }

        /// <summary>
        /// Get or set the value of the toggle.
        /// </summary>
        public float Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        /// <summary>
        /// Create a new SliderSet. Typically you want to use SceneEffectsCategory.AddSliderSet instead of creating these manually.
        /// </summary>
        /// <param name="label">Label UI element</param>
        /// <param name="slider">Slider UI element</param>
        /// <param name="input">Input field UI element</param>
        /// <param name="button">Reset button UI element</param>
        /// <param name="text">Label text</param>
        /// <param name="setter">Method that will be called on value change</param>
        /// <param name="initialValue">Initial value of the slider and textbox</param>
        /// <param name="sliderMinimum">Minimum value of the slider and textbox</param>
        /// <param name="sliderMaximum">Maximum value of the slider and textbox</param>
        public SceneEffectsSliderSet(Text label, Slider slider, 
            //InputField input, Button button, 
            string text, Action<float> setter, float initialValue, float sliderMinimum, float sliderMaximum)
        {
            Label = label;
            Slider = slider;
            //Input = input;
            //Button = button;
            Text = text;
            Setter = setter;
            InitialValue = initialValue;
            SliderMinimum = sliderMinimum;
            SliderMaximum = sliderMaximum;

            Init();
        }

        private void Init()
        {
            Slider.onValueChanged.RemoveAllListeners();
            //Input.onValueChanged.RemoveAllListeners();
            //Input.onEndEdit.RemoveAllListeners();
            //Button.onClick.RemoveAllListeners();

            Slider.maxValue = SliderMaximum;
            Slider.value = InitialValue;
            //Input.text = InitialValue.ToString();

            Slider.onValueChanged.AddListener(delegate (float value)
            {
                if (!EventsEnabled) return;
                SetValue(value);
            });

            //Input.onEndEdit.AddListener(delegate (string value)
            //{
            //    if (!EventsEnabled) return;
            //    SetValue(value);
            //});

            //Button.onClick.AddListener(Reset);
            SetValue(InitialValue, false);
            EventsEnabled = true;

            Label.gameObject.name = $"Label {Text}";
            Slider.gameObject.name = $"Slider {Text}";
            //Input.gameObject.name = $"InputField {Text}";
            //Button.gameObject.name = $"Button {Text}";
        }

        /// <summary>
        /// Get the value of the slider set.
        /// </summary>
        /// <returns>Value of the slider set</returns>
        public float GetValue() => CurrentValue;
        /// <summary>
        /// Set the value of the slider set, update the UI elements, and trigger the Setter method.
        /// </summary>
        /// <param name="value">Value to set the toggle set</param>
        public void SetValue(string value) => SetValue(value, true);
        /// <summary>
        /// Set the value of the slider set and update the UI elements.
        /// </summary>
        /// <param name="value">Value to set the toggle set</param>
        /// <param name="triggerEvents">Whether to trigger the Setter method</param>
        public void SetValue(string value, bool triggerEvents)
        {
            if (!float.TryParse(value, out float valuef))
                valuef = CurrentValue;
            SetValue(valuef, triggerEvents);
        }

        /// <summary>
        /// Set the value of the slider set, update the UI elements, and trigger the Setter method.
        /// </summary>
        /// <param name="value">Value to set the toggle set</param>
        public void SetValue(float value) => SetValue(value, true);
        /// <summary>
        /// Set the value of the slider set and update the UI elements.
        /// </summary>
        /// <param name="value">Value to set the toggle set</param>
        /// <param name="triggerEvents">Whether to trigger the Setter method</param>
        public void SetValue(float value, bool triggerEvents)
        {
            if (EnforceSliderMinimum && value < SliderMinimum)
                value = SliderMinimum;
            if (EnforceSliderMaximum && value > SliderMaximum)
                value = SliderMaximum;

            EventsEnabled = false;
            CurrentValue = value;
            Slider.value = value;
            //Input.text = value.ToString();
            EventsEnabled = true;
            if (triggerEvents)
                Setter.Invoke(value);
        }

        /// <summary>
        /// Reset the slider set to the initial value, update UI elements, and trigger the Setter method.
        /// </summary>
        public void Reset() => Reset(true);
        /// <summary>
        /// Reset the slider set to the initial value and update UI elements.
        /// </summary>
        /// <param name="triggerEvents">Whether to trigger the Setter method</param>
        public void Reset(bool triggerEvents)
        {
            EventsEnabled = false;
            CurrentValue = InitialValue;
            Slider.value = InitialValue;
            //Input.text = InitialValue.ToString();
            EventsEnabled = true;
            if (triggerEvents)
                Setter.Invoke(InitialValue);
        }
    }
}
