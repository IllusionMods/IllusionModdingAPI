using System;
using BepInEx;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MakerAPI
{
    public class MakerSlider : BaseEditableGuiEntry<float>
    {
        private static Transform _sliderCopy;

        private readonly string _settingName;

        private readonly float _maxValue;
        private readonly float _minValue;
        private readonly float _defaultValue;
        
        public MakerSlider(MakerCategory category, string settingName, float minValue, float maxValue, float defaultValue, BaseUnityPlugin owner) : base(category, defaultValue, owner)
        {
            _settingName = settingName;

            _minValue = minValue;
            _maxValue = maxValue;
            _defaultValue = defaultValue;
        }

        public Func<string, float> StringToValue { get; set; }
        public Func<float, string> ValueToString { get; set; }
        
        private static Transform SliderCopy
        {
            get
            {
                if (_sliderCopy == null)
                {
                    // Exists in male and female maker
                    var originalSlider = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglAll/AllTop/sldTemp").transform;

                    _sliderCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, true);
                    _sliderCopy.gameObject.SetActive(false);
                    _sliderCopy.name = "sldTemp" + GuiApiNameAppendix;

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
                }

                return _sliderCopy;
            }
        }

        protected internal override void CreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(SliderCopy, subCategoryList, true);

            tr.name = "sldTemp" + GuiApiNameAppendix;

            var textMesh = tr.Find("textShape").GetComponent<TextMeshProUGUI>();
            textMesh.text = _settingName;
            textMesh.color = TextColor;

            var slider = tr.Find("Slider").GetComponent<Slider>();
            slider.minValue = _minValue;
            slider.maxValue = _maxValue;
            slider.onValueChanged.AddListener(SetNewValue);

            slider.GetComponent<ObservableScrollTrigger>().OnScrollAsObservable().Subscribe(data =>
            {
                var scrollDelta = data.scrollDelta.y;
                var valueChange = Mathf.Pow(10, Mathf.Round(Mathf.Log10(slider.maxValue / 100)));

                if (scrollDelta < 0f)
                    slider.value += valueChange;
                else if (scrollDelta > 0f)
                    slider.value -= valueChange;
            });

            var inputField = tr.Find("InputField").GetComponent<TMP_InputField>();
            inputField.onEndEdit.AddListener(txt =>
            {
                var result = StringToValue?.Invoke(txt) ?? float.Parse(txt) / 100f;
                slider.value = Mathf.Clamp(result, slider.minValue, slider.maxValue);
            });

            slider.onValueChanged.AddListener(f =>
            {
                if (ValueToString != null)
                    inputField.text = ValueToString(f);
                else
                    inputField.text = Mathf.RoundToInt(f * 100).ToString();
            });

            var resetButton = tr.Find("Button").GetComponent<Button>();
            resetButton.onClick.AddListener(() => slider.value = _defaultValue);

            BufferedValueChanged.Subscribe(f => slider.value = f);

            tr.gameObject.SetActive(true);
        }
    }
}