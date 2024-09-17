#if KK || KKS || AI || HS2
#define TMP
#endif

using System;
using UnityEngine;
using UnityEngine.UI;
using Studio;
using GameStudio = Studio;
#if TMP
using Text = TMPro.TextMeshProUGUI;
#endif

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// A container for the value of a ColorPicker, associated label and button, and the setter method that triggers on value change.
    /// </summary>
    public class SceneEffectsColorPickerSet
    {
        private Color CurrentValue;
        private bool EventsEnabled = true;

        #region Backing Fields

        private string _text;

        #endregion Backing Fields

        /// <summary>
        /// Label UI element.
        /// </summary>
        public Text Label { get; set; }
        /// <summary>
        /// Button UI element.
        /// </summary>
        public Button Button { get; set; }
        /// <summary>
        /// Color image, actual click functionality handled by button.
        /// </summary>
        public Image ColorImage { get; set; }
        /// <summary>
        /// Method called when the value of the Color is changed.
        /// </summary>
        public Action<Color> Setter { get; set; }
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
        /// Get or set the value of the ColorPicker.
        /// </summary>
        public Color Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        /// <summary>
        /// Create a new ColorPicker. Typically, you want to use SceneEffectsCategory.AddColorPicker instead of creating these manually.
        /// </summary>
        /// <param name="label">Label UI element</param>
        /// <param name="button">Button component</param>
        /// <param name="text">Label text</param>
        /// <param name="setter">Method that will be called on value change</param>
        /// <param name="initialValue">The initial value to show on the ColorPicker.</param>
        public SceneEffectsColorPickerSet(Text label, Button button, string text, Action<Color> setter, Color initialValue)
        {
            Label = label;
            Button = button;
            ColorImage = button.GetComponent<Image>();
            Setter = setter;
            Text = text;

            ColorImage.color = initialValue;
            CurrentValue = initialValue;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
#if !PH
                Singleton<GameStudio.Studio>.Instance.colorPalette.Setup(Text, Value, SetValue, true);
                Singleton<GameStudio.Studio>.Instance.colorPalette.visible = true;
#elif PH
                Singleton<GameStudio.Studio>.Instance.colorMenu.updateColorFunc = new GameStudio.UI_ColorInfo.UpdateColor(SetValue);
                Singleton<GameStudio.Studio>.Instance.colorMenu.SetColor(ColorImage.color, UI_ColorInfo.ControlType.PresetsSample);
                Singleton<GameStudio.Studio>.Instance.colorPaletteCtrl.visible = true;
#endif
            });

            Label.gameObject.name = $"Label {Text}";
            Button.gameObject.name = $"ColorPicker {Text}";
        }

        /// <summary>
        /// Get the value of the ColorPicker.
        /// </summary>
        /// <returns>Value of the ColorPicker</returns>
        public Color GetValue() => CurrentValue;
        /// <summary>
        /// Set the value of the ColorPicker and trigger the Setter method.
        /// </summary>
        /// <param name="value">Value to set the ColorPicker</param>
        public void SetValue(Color value) => SetValue(value, true);
        /// <summary>
        /// Set the value of the
        /// </summary>
        /// <param name="value">Value to set the ColorPicker</param>
        /// <param name="triggerEvents">Whether to trigger the Setter method</param>
        public void SetValue(Color value, bool triggerEvents)
        {
            EventsEnabled = false;
            CurrentValue = value;
            ColorImage.color = value;
            EventsEnabled = true;
            if (triggerEvents)
                Setter.Invoke(value);
        }
    }
}