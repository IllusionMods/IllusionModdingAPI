#if KK || KKS || AI || HS2
#define TMP
#endif

using System;
using UnityEngine.UI;
#if TMP
using Text = TMPro.TextMeshProUGUI;
#endif

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// A container for the value of a toggle, associated label and toggle UI elements, and the setter method that triggers on value change.
    /// </summary>
    public class SceneEffectsToggleSet
    {
        private bool CurrentValue = false;
        private bool EventsEnabled = true;

        #region Backing Fields
        private string _text;
        #endregion

        /// <summary>
        /// Label UI element.
        /// </summary>
        public Text Label { get; set; }
        /// <summary>
        /// Toggle UI element.
        /// </summary>
        public Toggle Toggle { get; set; }
        /// <summary>
        /// Method called when the value of the toggle is changed.
        /// </summary>
        public Action<bool> Setter { get; set; }
        /// <summary>
        /// Initial state of the toggle.
        /// </summary>
        public bool InitialValue { get; set; } = false;

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
        public bool Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        /// <summary>
        /// Create a new ToggleSet. Typically you want to use SceneEffectsCategory.AddToggleSet instead of creating these manually.
        /// </summary>
        /// <param name="label">Label UI element</param>
        /// <param name="toggle">Toggle UI element</param>
        /// <param name="text">Label text</param>
        /// <param name="setter">Method that will be called on value change</param>
        /// <param name="initialValue">Initial state of the toggle</param>
        public SceneEffectsToggleSet(Text label, Toggle toggle, string text, Action<bool> setter, bool initialValue)
        {
            Label = label;
            Toggle = toggle;
            Setter = setter;
            InitialValue = initialValue;
            Value = InitialValue;
            Text = text;

            Toggle.isOn = Value;
            Toggle.onValueChanged.RemoveAllListeners();
            Toggle.onValueChanged.AddListener(delegate (bool value)
            {
                if (!EventsEnabled) return;
                Value = value;
            });

            Label.gameObject.name = $"Label {Text}";
            Toggle.gameObject.name = $"Toggle {Text}";
        }

        /// <summary>
        /// Get the value of the toggle.
        /// </summary>
        /// <returns>Value of the toggle</returns>
        public bool GetValue() => CurrentValue;
        /// <summary>
        /// Set the value of the toggle and trigger the Setter method.
        /// </summary>
        /// <param name="value">Value to set the toggle</param>
        public void SetValue(bool value) => SetValue(value, true);
        /// <summary>
        /// Set the value of the toggle.
        /// </summary>
        /// <param name="value">Value to set the toggle</param>
        /// <param name="triggerEvents">Whether to trigger the Setter method</param>
        public void SetValue(bool value, bool triggerEvents)
        {
            EventsEnabled = false;
            CurrentValue = value;
            Toggle.isOn = value;
            EventsEnabled = true;
            if (triggerEvents)
                Setter.Invoke(value);
        }

        /// <summary>
        /// Reset the toggle to the initial value and trigger the Setter method.
        /// </summary>
        public void Reset() => Reset(true);
        /// <summary>
        /// Reset the toggle to the initial value.
        /// </summary>
        /// <param name="triggerEvents">Whether to trigger the Setter method</param>
        public void Reset(bool triggerEvents)
        {
            EventsEnabled = false;
            CurrentValue = InitialValue;
            Toggle.isOn = InitialValue;
            EventsEnabled = true;
            if (triggerEvents)
                Setter.Invoke(InitialValue);
        }
    }
}
