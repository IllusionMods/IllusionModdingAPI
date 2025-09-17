using System;
using BepInEx;
using CharaCustom;
using KKAPI.Utilities;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that draws a slider and a text box (both are used to edit the same value)
    /// </summary>
    public class MakerSlider : BaseEditableGuiEntry<float>
    {
        private static Transform _sliderCopy;

        private readonly string _settingName;

        private readonly float _maxValue;
        private readonly float _minValue;

        /// <summary>
        /// Value used when user presses the Reset button.
        /// </summary>
        public float DefaultValue { get; set; }

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="settingName">Text displayed next to the slider</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        /// <param name="minValue">Lowest allowed value (inclusive)</param>
        /// <param name="maxValue">Highest allowed value (inclusive)</param>
        /// <param name="defaultValue">Value the slider will be set to after creation</param>
        public MakerSlider(MakerCategory category, string settingName, float minValue, float maxValue, float defaultValue, BaseUnityPlugin owner) : base(category, defaultValue, owner)
        {
            _settingName = settingName;

            _minValue = minValue;
            _maxValue = maxValue;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Custom converter from text in the textbox to the slider value.
        /// If not set, <code>float.Parse(txt) / 100f</code> is used.
        /// You may need to also change <see cref="ValueToString"/> and <see cref="MouseScrollValueChange"/> if you set this.
        /// </summary>
        public Func<string, float> StringToValue { get; set; }

        /// <summary>
        /// Custom converter from the slider value to what's displayed in the textbox.
        /// If not set, <code>Mathf.RoundToInt(f * 100).ToString()</code> is used.
        /// You may need to also change <see cref="StringToValue"/> and <see cref="MouseScrollValueChange"/> if you set this.
        /// </summary>
        public Func<float, string> ValueToString { get; set; }

        /// <summary>
        /// Override for the default mouse scroll behaviour. It should return a value that should be added to the slider value (will be clamped, can be negative).
        /// The Vector2 parameter is the mouse scroll delta. Most mice only use the Y scroll value. Delta can be negative and larger than 1.
        /// If not set, the slider value will be changed based on the slider range (1% of log10 of the max value per vertical mouse wheel tick).
        /// </summary>
        public Func<Vector2, float> MouseScrollValueChange { get; set; }

        private static Transform SliderCopy
        {
            get
            {
                if (_sliderCopy == null)
                    MakeCopy();

                return _sliderCopy;
            }
        }

        private static void MakeCopy()
        {
            var originalSlider = UnityEngine.Object.FindObjectOfType<CustomSliderSet>().transform;

            _sliderCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, false);
            _sliderCopy.gameObject.SetActive(false);
            _sliderCopy.name = "sldTemp";

            var slider = _sliderCopy.GetComponent<CustomSliderSet>();
            slider.onChange = null;
            slider.onPointerUp = null;
            slider.onSetDefaultValue = null;

            foreach (var renderer in _sliderCopy.GetComponentsInChildren<Image>())
                renderer.raycastTarget = true;

            RemoveLocalisation(_sliderCopy.gameObject);
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_sliderCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(SliderCopy, subCategoryList, false);

            tr.name = "SliderSet";

            var sliderSet = tr.GetComponent<CustomSliderSet>();

            sliderSet.onChange = null;
            sliderSet.onPointerUp = null;
            sliderSet.onSetDefaultValue = null;
            GameObject.Destroy(sliderSet);

            sliderSet.title.text = _settingName;
            sliderSet.title.color = TextColor;
            SetTextAutosize(sliderSet.title);

            var slider = sliderSet.slider;
            slider.minValue = _minValue;
            slider.maxValue = _maxValue;
            slider.onValueChanged.ActuallyRemoveAllListeners();
            slider.onValueChanged.AddListener(SetValue);

            slider.GetComponent<ObservableScrollTrigger>()
                .OnScrollAsObservable()
                .Subscribe(
                    data =>
                    {
                        if (MouseScrollValueChange != null)
                        {
                            var valueChangeCustom = MouseScrollValueChange(data.scrollDelta);
                            slider.value += valueChangeCustom;
                        }
                        else
                        {
                            // The goal is to make the slider value change by 1 unit that's visible in the textbox next to the slider
                            // This assumes that the slider value shown in the text box has 3 digits visible at max value
                            // 1% of log10 of the max value (e.g. 1 for 100 max, 1 for 300, 0.1 for 90, 0.01 for 1)
                            var valueChange = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(Mathf.Max(Mathf.Abs(slider.minValue), Mathf.Abs(slider.maxValue)) / 100)));

                            var scrollDelta = data.scrollDelta.y;
                            if (scrollDelta < 0f)
                                slider.value += valueChange;
                            else if (scrollDelta > 0f)
                                slider.value -= valueChange;
                        }
                    });

            var inputField = sliderSet.input;
            inputField.onEndEdit.AddListener(
                txt =>
                {
                    try
                    {
                        var result = StringToValue?.Invoke(txt) ?? float.Parse(txt) / 100f;
                        slider.value = Mathf.Clamp(result, slider.minValue, slider.maxValue);
                    }
                    catch
                    {
                        // Ignore parsing errors, lets user keep typing
                    }
                });

            slider.onValueChanged.AddListener(
                f =>
                {
                    if (ValueToString != null)
                        inputField.text = ValueToString(f);
                    else
                        inputField.text = Mathf.RoundToInt(f * 100).ToString();
                });

            
            var resetButton = sliderSet.button;
            resetButton.onClick.ActuallyRemoveAllListeners();
            resetButton.onClick.AddListener(() => slider.value = DefaultValue);

            BufferedValueChanged.Subscribe(f => slider.value = f);

            return tr.gameObject;
        }
    }
}
