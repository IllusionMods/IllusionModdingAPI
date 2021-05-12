using BepInEx;
using System;
using TMPro;
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
        private readonly float _defaultValue;

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
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Custom converter from text in the textbox to the slider value.
        /// If not set, <code>float.Parse(txt) / 100f</code> is used.
        /// </summary>
        public Func<string, float> StringToValue { get; set; }

        /// <summary>
        /// Custom converter from the slider value to what's displayed in the textbox.
        /// If not set, <code>Mathf.RoundToInt(f * 100).ToString()</code> is used.
        /// </summary>
        public Func<float, string> ValueToString { get; set; }

        /// <summary>
        /// Use integers instead of floats
        /// </summary>
        public bool WholeNumbers { get; set; }

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
            // Exists in male and female maker
            var originalSlider = GetExistingControl("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglAll", "sldTemp");

            _sliderCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, false);
            _sliderCopy.gameObject.SetActive(false);
            _sliderCopy.name = "sldTemp";

            var slider = _sliderCopy.Find("Slider").GetComponent<Slider>();
            slider.onValueChanged.RemoveAllListeners();

            var inputField = _sliderCopy.Find("InputField").GetComponent<TMP_InputField>();
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onSubmit.RemoveAllListeners();
            inputField.onEndEdit.RemoveAllListeners();

            var resetButton = _sliderCopy.Find("Button").GetComponent<Button>();
            resetButton.onClick.RemoveAllListeners();

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

            tr.name = "sldTemp";

            var textMesh = tr.Find("textShape").GetComponent<TextMeshProUGUI>();
            textMesh.text = _settingName;
            textMesh.color = TextColor;

            var slider = tr.Find("Slider").GetComponent<Slider>();
            slider.minValue = _minValue;
            slider.maxValue = _maxValue;
            slider.wholeNumbers = WholeNumbers;
            slider.onValueChanged.AddListener(SetValue);
            slider.value = _defaultValue;
            slider.GetComponent<ObservableScrollTrigger>()
                .OnScrollAsObservable()
                .Subscribe(
                    data =>
                    {
                        var scrollDelta = data.scrollDelta.y;
                        var valueChange = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(Mathf.Max(Mathf.Abs(slider.minValue), Mathf.Abs(slider.maxValue)) / 100)));

                        if (scrollDelta < 0f)
                            slider.value += valueChange;
                        else if (scrollDelta > 0f)
                            slider.value -= valueChange;
                    });

            var inputField = tr.Find("InputField").GetComponent<TMP_InputField>();
            if (MakerAPI.InsideMaker) Singleton<ChaCustom.CustomBase>.Instance.lstTmpInputField.Add(inputField);

            InputField(_defaultValue, inputField);

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
                    InputField(f, inputField);
                });

            var resetButton = tr.Find("Button").GetComponent<Button>();
            resetButton.onClick.AddListener(() => slider.value = _defaultValue);

            BufferedValueChanged.Subscribe(f => slider.value = f);

            return tr.gameObject;
        }

        private void InputField(float f, TMP_InputField inputField)
        {
            if (ValueToString != null)
                inputField.text = ValueToString(f);
            else if (WholeNumbers)
                inputField.text = f.ToString();
            else
                inputField.text = Mathf.RoundToInt(f * 100).ToString();
        }
    }
}
