#if KK || KKS || AI || HS2
#define TMP
#endif

using IllusionUtility.GetUtility;
using KKAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if TMP
using Text = TMPro.TextMeshProUGUI;
#endif

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Class that adds a new subcategory to the Scene Effects menu. Create a new instance and then add SliderSets and ToggleSets.
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
        private static GameObject _inputSource;
        private static GameObject _buttonSource;
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
#elif AI || HS2
            const string labelSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Draw/TextMeshPro";
            const string toggleSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Draw/Toggle";
            const string sliderSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Focal Size/Slider";
            const string inputSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Focal Size/InputField";
            const string buttonSourcePath = "Screen Effect/Viewport/Content/Depth of Field/Focal Size/Button Default";
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
        /// Toggles that have been added.
        /// </summary>
        public List<SceneEffectsToggleSet> Toggles = new List<SceneEffectsToggleSet>();
        /// <summary>
        /// Sliders that have been added.
        /// </summary>
        public List<SceneEffectsSliderSet> Sliders = new List<SceneEffectsSliderSet>();

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

            foreach (Transform child in Content.transform)
                Object.Destroy(child.gameObject);
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

        private float GetCurrentOffset() => OffsetMultiplier * (Toggles.Count + Sliders.Count);
    }
}
