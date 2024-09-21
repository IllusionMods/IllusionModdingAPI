#if KK || KKS || AI || HS2
#define TMP
#endif

using IllusionUtility.GetUtility;
using KKAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if TMP
using Text = TMPro.TextMeshProUGUI;
#endif

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Class that adds a new subcategory to the Scene Effects menu. Create a new instance and then add SliderSets, ToggleSets, DropdownSets, ColorPickerSets, or just plain LabelSets.
    /// </summary>
    public class SceneEffectsCategory
    {
        private const float OffsetMultiplier = -25f;

        private static bool _wasInitialized;
        private static GameObject _headerSource;
        private static GameObject _contentSource;
        private static GameObject _labelSource;
        private static GameObject _toggleSource;
        private static GameObject _sliderSource;
#if !PH
        private static GameObject _dropdownSource;
        private static GameObject _colorPickerSource;
        private static GameObject _inputSource;
        private static GameObject _buttonSource;

        private static Sprite _expandSprite;
        private static Sprite _collapseSprite;
#endif

        private static void Initialize()
        {
            if (_wasInitialized) return;

            const string headerSourcePath = "Screen Effect/Viewport/Content/Image Depth of Field";
            const string contentSourcePath = "Screen Effect/Viewport/Content/Depth of Field";
#if KK || KKS
            const string labelSourcePath = "Screen Effect/Viewport/Content/Depth of Field/TextMeshPro Draw";
            const string toggleSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Toggle Draw";
            const string sliderSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Slider Focal Size";
            const string inputSourcePath = "Screen Effect/Viewport/Content/Depth of Field/InputField Focal Size";
            const string buttonSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Button Focal Size Default";
            const string dropdownSourcePath = "Screen Effect/Viewport/Content/Amplify Color Effect/Dropdown Lut";
            const string colorPickerSourcePath = "Screen Effect/Viewport/Content/Amplify Occlusion Effect/Button Color";
#elif AI || HS2
            const string labelSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Draw/TextMeshPro";
            const string toggleSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Draw/Toggle";
            const string sliderSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Focal Size/Slider";
            const string inputSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Focal Size/InputField";
            const string buttonSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Focal Size/Button Default";
            const string dropdownSourcePath = "Screen Effect/Viewport/Content/Sky/Pattern/Dropdown";
            const string colorPickerSourcePath = "Screen Effect/Viewport/Content/Ambient Occlusion/Color/Button";
#elif PH
            const string labelSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Text Draw";
            const string toggleSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Toggle Draw";
            const string sliderSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Slider Focal Size";
#endif

            var sbc = global::Studio.Studio.Instance.systemButtonCtrl;
            var sef = sbc.transform.FindLoop("01_Screen Effect");
            _headerSource = sef.transform.Find(headerSourcePath)?.gameObject ?? throw new ArgumentException("Could not find " + headerSourcePath);
            _contentSource = sef.transform.Find(contentSourcePath)?.gameObject ?? throw new ArgumentException("Could not find " + contentSourcePath);
            _labelSource = sef.transform.Find(labelSourcePath)?.gameObject ?? throw new ArgumentException("Could not find " + labelSourcePath);
            _toggleSource = sef.transform.Find(toggleSourcePath)?.gameObject ?? throw new ArgumentException("Could not find " + toggleSourcePath);
            _sliderSource = sef.transform.Find(sliderSourcePath)?.gameObject ?? throw new ArgumentException("Could not find " + sliderSourcePath);
#if !PH
            _inputSource = sef.transform.Find(inputSourcePath)?.gameObject ?? throw new ArgumentException("Could not find " + inputSourcePath);
            _buttonSource = sef.transform.Find(buttonSourcePath)?.gameObject ?? throw new ArgumentException("Could not find " + buttonSourcePath);
            _dropdownSource = sef.transform.Find(dropdownSourcePath)?.gameObject ?? throw new ArgumentException("Could not find " + dropdownSourcePath);
            _colorPickerSource = sef.transform.Find(colorPickerSourcePath)?.gameObject ?? throw new ArgumentException("Could not find " + colorPickerSourcePath);

            //Not too proud of these, but they work fine and should continue to work fine.
            var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            _expandSprite = allSprites.FirstOrDefault(r => r.name.Equals("sp_sn_09_00_05"));
            _collapseSprite = allSprites.FirstOrDefault(r => r.name.Equals("sp_sn_09_00_03"));
#endif

            _wasInitialized = true;
        }

        /// <summary>
        /// Element that contains the header of the category.
        /// </summary>
        public GameObject Header { get; set; }
        /// <summary>
        /// Element that contains the content of the category.
        /// </summary>
        public GameObject Content { get; set; }

        /// <summary>
        /// Labels that have been added.
        /// </summary>
        public List<SceneEffectsLabelSet> Labels = new List<SceneEffectsLabelSet>();
        /// <summary>
        /// Toggles that have been added.
        /// </summary>
        public List<SceneEffectsToggleSet> Toggles = new List<SceneEffectsToggleSet>();
        /// <summary>
        /// Sliders that have been added.
        /// </summary>
        public List<SceneEffectsSliderSet> Sliders = new List<SceneEffectsSliderSet>();
        /// <summary>
        /// Dropdowns that have been added.
        /// </summary>
        public List<SceneEffectsDropdownSet> Dropdowns = new List<SceneEffectsDropdownSet>();
        /// <summary>
        /// Color pickers that have been added.
        /// </summary>
        public List<SceneEffectsColorPickerSet> ColorPickers = new List<SceneEffectsColorPickerSet>();

        /// <summary>
        /// Create a new Screen Effects subcategory.
        /// </summary>
        /// <param name="labelText">Text that will appear on the header of the category</param>
        public SceneEffectsCategory(string labelText)
        {
            Initialize();

            Header = Object.Instantiate(_headerSource);
            Header.name = $"Image {labelText}";
            Header.transform.SetParent(_headerSource.transform.parent, false);

            var label = Header.GetComponentInChildren<Text>();
            label.text = labelText;

            Content = Object.Instantiate(_contentSource);
            Content.name = labelText;
            Content.transform.SetParent(_contentSource.transform.parent, false);
            var vlg = Content.GetComponent<VerticalLayoutGroup>();
            Object.DestroyImmediate(vlg);
            var layoutElement = Content.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 0;
            layoutElement.preferredWidth = 375;

#if !PH
            //No clue if PH even has these buttons so marked it out.
            var button = Header.GetComponentInChildren<Button>();
            button.onClick.AddListener(() =>
            {
                Content.SetActive(!Content.activeSelf);
                button.image.sprite = Content.activeSelf ? _collapseSprite: _expandSprite;
            });
#endif

            foreach (Transform child in Content.transform)
                Object.Destroy(child.gameObject);
        }

        /// <summary>
        /// Add a label to the category, can be used for sectioning.
        /// </summary>
        /// <param name="text">Label text</param>
        /// <returns>Instance of the LabelSet</returns>
        public SceneEffectsLabelSet AddLabelSet(string text)
        {
            var containingElement = new GameObject().AddComponent<RectTransform>();
            containingElement.name = text;
            containingElement.SetParent(Content.transform, false);

            KoikatuAPI.Instance.StartCoroutine(SetPosDelayed(containingElement.transform, GetCurrentOffset()));

            var label = Object.Instantiate(_labelSource).GetComponent<Text>();
            label.transform.SetParent(containingElement.transform, false);
            label.transform.localPosition = new Vector3(4f, 0f, 0f);

            var labelSet = new SceneEffectsLabelSet(label, text);
            Labels.Add(labelSet);

            CorrectCategoryScale();

            return labelSet;
        }

        /// <summary>
        /// Add a toggle to this Screen Effects subcategory.
        /// </summary>
        /// <param name="text">Label text</param>
        /// <param name="setter">Method to be called when the toggle changes value</param>
        /// <param name="initialValue">Initial state of the toggle</param>
        /// <returns>Instance of the ToggleSet</returns>
        public SceneEffectsToggleSet AddToggleSet(string text, Action<bool> setter, bool initialValue)
        {
            var containingElement = new GameObject().AddComponent<RectTransform>();
            containingElement.name = text;
            containingElement.SetParent(Content.transform, false);

            KoikatuAPI.Instance.StartCoroutine(SetPosDelayed(containingElement.transform, GetCurrentOffset()));

            var label = Object.Instantiate(_labelSource).GetComponent<Text>();
            label.transform.SetParent(containingElement.transform, false);
            label.transform.localPosition = new Vector3(4f, 0f, 0f);

            var toggle = Object.Instantiate(_toggleSource).GetComponent<Toggle>();
            toggle.transform.SetParent(containingElement.transform, false);
            toggle.transform.localPosition = new Vector3(160f, 0f, 0f);

            var toggleSet = new SceneEffectsToggleSet(label, toggle, text, setter, initialValue);
            Toggles.Add(toggleSet);

            CorrectCategoryScale();

            return toggleSet;
        }

        /// <summary>
        /// Add a slider with text box to this Screen Effects subcategory.
        /// </summary>
        /// <param name="text">Label text</param>
        /// <param name="setter">Method to be called when the slider or text box changes value</param>
        /// <param name="initialValue">Initial value of the slider and text box</param>
        /// <param name="sliderMinimum">Minimum value the slider can slide. Can be overriden by the user typing in to the text box if EnforceSliderMinimum is set to false.</param>
        /// <param name="sliderMaximum">Maximum value the slider can slide. Can be overriden by the user typing in to the text box if EnforceSliderMaximum is set to false.</param>
        /// <returns>Instance of the SliderSet</returns>
        public SceneEffectsSliderSet AddSliderSet(string text, System.Action<float> setter, float initialValue, float sliderMinimum, float sliderMaximum)
        {
            var containingElement = new GameObject().AddComponent<RectTransform>();
            containingElement.name = text;
            containingElement.SetParent(Content.transform, false);
            KoikatuAPI.Instance.StartCoroutine(SetPosDelayed(containingElement.transform, GetCurrentOffset()));

            var label = Object.Instantiate(_labelSource).GetComponent<Text>();
            label.transform.SetParent(containingElement.transform, false);
            label.transform.localPosition = new Vector3(4f, 0, 0f);

            var slider = Object.Instantiate(_sliderSource).GetComponent<Slider>();
            slider.transform.SetParent(containingElement.transform, false);
            slider.transform.localPosition = new Vector3(160f, 0f, 0f);

#if !PH
            var input = Object.Instantiate(_inputSource).GetComponent<InputField>();
            input.transform.SetParent(containingElement.transform, false);
            input.transform.localPosition = new Vector3(295f, -10f, 0f);

            var button = Object.Instantiate(_buttonSource).GetComponent<Button>();
            button.transform.SetParent(containingElement.transform, false);
            button.transform.localPosition = new Vector3(340f, 0f, 0f);
#endif

#if PH
            var sliderSet = new SceneEffectsSliderSet(label, slider, text, setter, initialValue, sliderMinimum, sliderMaximum);
#else
            var sliderSet = new SceneEffectsSliderSet(label, slider, input, button, text, setter, initialValue, sliderMinimum, sliderMaximum);
#endif
            Sliders.Add(sliderSet);

            CorrectCategoryScale();

            return sliderSet;
        }

#if !PH
        /// <summary>
        /// Add a dropdown to this Screen Effects subcategory.
        /// </summary>
        /// <param name="text">Label text</param>
        /// <param name="setter">Method to be called when the dropdown selection changes.</param>
        /// <param name="options">A list of the options to display in the dropdown.</param>
        /// <param name="initialValue">The initial value to be selected in the dropdown.</param>
        /// <returns>Instance of the DropdownSet</returns>
        public SceneEffectsDropdownSet AddDropdownSet(string text, Action<int> setter, List<string> options, string initialValue)
        {
            var containingElement = new GameObject().AddComponent<RectTransform>();
            containingElement.name = text;
            containingElement.SetParent(Content.transform, false);

            KoikatuAPI.Instance.StartCoroutine(SetPosDelayed(containingElement.transform, GetCurrentOffset()));

            var label = Object.Instantiate(_labelSource).GetComponent<Text>();
            label.transform.SetParent(containingElement.transform, false);
            label.transform.localPosition = new Vector3(4f, 0f, 0f);

            var dropDown = Object.Instantiate(_dropdownSource).GetComponent<Dropdown>();
            dropDown.transform.SetParent(containingElement.transform, false);
            dropDown.transform.localPosition = new Vector3(160f, 0f, 0f);

            var dropDownSet = new SceneEffectsDropdownSet(label, dropDown, text, setter, options, initialValue);
            Dropdowns.Add(dropDownSet);

            CorrectCategoryScale();

            return dropDownSet;
        }
        /// <summary>
        /// Add a color picker to this category.
        /// </summary>
        /// <param name="text">The UI label text.</param>
        /// <param name="setter">Function to be called when the color changes.</param>
        /// <param name="initialValue">The initial color to display.</param>
        /// <returns>Instance of the ColorPickerSet</returns>
        public SceneEffectsColorPickerSet AddColorPickerSet(string text, Action<Color> setter, Color initialValue)
        {
            var containingElement = new GameObject().AddComponent<RectTransform>();
            containingElement.name = text;
            containingElement.SetParent(Content.transform, false);

            KoikatuAPI.Instance.StartCoroutine(SetPosDelayed(containingElement.transform, GetCurrentOffset()));

            var label = Object.Instantiate(_labelSource).GetComponent<Text>();
            label.transform.SetParent(containingElement.transform, false);
            label.transform.localPosition = new Vector3(4f, 0f, 0f);

            var colorPicker = Object.Instantiate(_colorPickerSource).GetComponent<Button>();
            colorPicker.transform.SetParent(containingElement.transform, false);
            colorPicker.transform.localPosition = new Vector3(160f, 0f, 0f);

            var colorPickerSet = new SceneEffectsColorPickerSet(label, colorPicker, text, setter, initialValue);
            ColorPickers.Add(colorPickerSet);

            CorrectCategoryScale();

            return colorPickerSet;
        }
#endif

        private void CorrectCategoryScale()
        {
            var layoutElement = Content.GetComponent<LayoutElement>();
            layoutElement.preferredHeight += 25;
        }

        private static IEnumerator SetPosDelayed(Transform tr, float offset)
        {
            // todo set positions of everything at the same time?
            while (!tr.gameObject.activeInHierarchy)
                yield return new WaitForSeconds(1);

            yield return CoroutineUtils.WaitForEndOfFrame;

            var v = new Vector3(0f, offset, 0f);
            tr.localPosition = v;
        }

        private float GetCurrentOffset() => OffsetMultiplier * (Toggles.Count + Sliders.Count + Dropdowns.Count + ColorPickers.Count + Labels.Count);
    }
}