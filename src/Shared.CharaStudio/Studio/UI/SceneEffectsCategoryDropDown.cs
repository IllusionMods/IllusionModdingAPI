#if KK || KKS || AI || HS2
#define TMP
#endif

using System;
using System.Collections.Generic;
using UnityEngine.UI;
#if TMP
using Text = TMPro.TextMeshProUGUI;
#endif

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// A container for the value of a dropdown, associated label and dropdown, and the setter method that triggers on value change.
    /// </summary>
    public class SceneEffectsDropdownSet
    {
        private int CurrentValue = -1;
        private bool EventsEnabled = true;

        #region Backing Fields

        private string _text;

        #endregion Backing Fields

        /// <summary>
        /// Label UI element.
        /// </summary>
        public Text Label { get; set; }
        /// <summary>
        /// DropDown UI element.
        /// </summary>
        public Dropdown Dropdown { get; set; }
        /// <summary>
        /// Method called when the value of the Dropdown is changed.
        /// </summary>
        public Action<int> Setter { get; set; }
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
        /// Get or set the value of the Dropdown.
        /// </summary>
        public int Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        /// <summary>
        /// Create a new Dropdown. Typically, you want to use SceneEffectsCategory.AddDropdown instead of creating these manually.
        /// </summary>
        /// <param name="label">Label UI element</param>
        /// <param name="dropdown">Dropdown component</param>
        /// <param name="text">Label text</param>
        /// <param name="setter">Method that will be called on value change</param>
        /// <param name="optionsList">List of options the Dropdown will display.</param>
        /// <param name="initialValue">The initial value to show on the Dropdown.</param>
        public SceneEffectsDropdownSet(Text label, Dropdown dropdown, string text, Action<int> setter, List<string> optionsList, string initialValue = "")
        {
            Label = label;
            Dropdown = dropdown;
            Setter = setter;
            Dropdown.ClearOptions();
            Dropdown.AddOptions(optionsList);
            Text = text;

            if (initialValue.IsNullOrWhiteSpace() == false)
            {
                Dropdown.value = optionsList.FindIndex(r => r.Equals(initialValue));
                Dropdown.RefreshShownValue();
            }

            Dropdown.onValueChanged.RemoveAllListeners();
            Dropdown.onValueChanged.AddListener(delegate (int value)
            {
                if (!EventsEnabled) return;
                Value = value;
            });

            Label.gameObject.name = $"Label {Text}";
            Dropdown.gameObject.name = $"Dropdown {Text}";
        }

        /// <summary>
        /// Get the value of the Dropdown.
        /// </summary>
        /// <returns>Value of the Dropdown</returns>
        public int GetValue() => CurrentValue;
        /// <summary>
        /// Set the value of the Dropdown and trigger the Setter method.
        /// </summary>
        /// <param name="value">Value to set the Dropdown</param>
        public void SetValue(int value) => SetValue(value, true);
        /// <summary>
        /// Set the value of the Dropdown.
        /// </summary>
        /// <param name="value">Value to set the Dropdown</param>
        /// <param name="triggerEvents">Whether to trigger the Setter method</param>
        public void SetValue(int value, bool triggerEvents)
        {
            EventsEnabled = false;
            CurrentValue = value;
            Dropdown.value = value;
            Dropdown.RefreshShownValue();
            EventsEnabled = true;
            if (triggerEvents)
                Setter.Invoke(value);
        }
        /*
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
        */
    }
}