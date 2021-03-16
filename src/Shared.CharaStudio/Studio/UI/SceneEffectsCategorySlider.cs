#if KK || AI || HS2
#define TMP
#endif

using System;
using System.Globalization;
using UnityEngine.UI;
#if TMP
using Text = TMPro.TextMeshProUGUI;
#endif

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// A container for the value of a slider, associated label, slider, textbox, and reset button UI elements, and the setter method that triggers on value change.
    /// </summary>
    public class SceneEffectsSliderSet
    {
        private float _currentValue;
        private bool _eventsEnabled;

        private string _text;
        private float _sliderMinimum;
        private float _sliderMaximum;

        /// <summary>
        /// Label UI element.
        /// </summary>
        public Text Label { get; private set; }
        /// <summary>
        /// Slider UI element.
        /// </summary>
        public Slider Slider { get; private set; }
        /// <summary>
        /// Input field UI element.
        /// </summary>
        public InputField Input { get; private set; }
        /// <summary>
        /// Reset button UI element.
        /// </summary>
        public Button Button { get; private set; }
        /// <summary>
        /// Method called when the value of the toggle is changed.
        /// </summary>
        public Action<float> Setter { get; set; }
        /// <summary>
        /// Initial state of the toggle.
        /// </summary>
        public float InitialValue { get; set; }

        /// <summary>
        /// Show the input field for typing in values.
        /// todo Not actually working?
        /// </summary>
        public bool ShowInput;
        /// <summary>
        /// Show the reset button.
        /// todo Not actually working?
        /// </summary>
        public bool ShowButton;

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
        /// <param name="text">Label text</param>
        /// <param name="setter">Method that will be called on value change</param>
        /// <param name="initialValue">Initial value of the slider and textbox</param>
        /// <param name="sliderMinimum">Minimum value of the slider and textbox</param>
        /// <param name="sliderMaximum">Maximum value of the slider and textbox</param>
        public SceneEffectsSliderSet(Text label, Slider slider, string text, Action<float> setter, float initialValue, float sliderMinimum, float sliderMaximum)
        {
            Label = label;
            Slider = slider;
            Text = text;
            Setter = setter;
            InitialValue = initialValue;
            SliderMinimum = sliderMinimum;
            SliderMaximum = sliderMaximum;
            ShowInput = false;
            ShowButton = false;

            Init();
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
        public SceneEffectsSliderSet(Text label, Slider slider, InputField input, Button button, string text, Action<float> setter, float initialValue, float sliderMinimum, float sliderMaximum)
        {
            Label = label;
            Slider = slider;
            Input = input;
            Button = button;
            Text = text;
            Setter = setter;
            InitialValue = initialValue;
            SliderMinimum = sliderMinimum;
            SliderMaximum = sliderMaximum;
            ShowInput = true;
            ShowButton = true;

            Init();
        }

        private void Init()
        {
            Slider.onValueChanged.RemoveAllListeners();
            if (ShowInput) Input.onValueChanged.RemoveAllListeners();
            if (ShowInput) Input.onEndEdit.RemoveAllListeners();
            if (ShowButton) Button.onClick.RemoveAllListeners();

            Slider.maxValue = SliderMaximum;
            Slider.value = InitialValue;
            if (ShowInput) Input.text = InitialValue.ToString(CultureInfo.InvariantCulture);

            Slider.onValueChanged.AddListener(delegate (float value)
            {
                if (!_eventsEnabled) return;
                SetValue(value);
            });

            if (ShowInput)
                Input.onEndEdit.AddListener(delegate (string value)
                {
                    if (!_eventsEnabled) return;
                    SetValue(value);
                });

            Button.onClick.AddListener(Reset);
            SetValue(InitialValue, false);
            _eventsEnabled = true;

            Label.gameObject.name = $"Label {Text}";
            Slider.gameObject.name = $"Slider {Text}";

            if (ShowInput) Input.gameObject.name = $"InputField {Text}";
            if (ShowButton) Button.gameObject.name = $"Button {Text}";
        }

        /// <summary>
        /// Get the value of the slider set.
        /// </summary>
        /// <returns>Value of the slider set</returns>
        public float GetValue() => _currentValue;
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
            if (!float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var valuef))
                valuef = _currentValue;
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

            _eventsEnabled = false;
            _currentValue = value;
            Slider.value = value;
            if (ShowInput) Input.text = value.ToString(CultureInfo.InvariantCulture);
            _eventsEnabled = true;
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
            _eventsEnabled = false;
            _currentValue = InitialValue;
            Slider.value = InitialValue;
            if (ShowInput) Input.text = InitialValue.ToString(CultureInfo.InvariantCulture);
            _eventsEnabled = true;
            if (triggerEvents)
                Setter.Invoke(InitialValue);
        }
    }
}
