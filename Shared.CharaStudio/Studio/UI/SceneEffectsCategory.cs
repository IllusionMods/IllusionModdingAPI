﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Class that adds a new subcategory to the Scene Effects menu. Create a new instance and then add SliderSets and ToggleSets.
    /// </summary>
    public class SceneEffectsCategory
    {
        private const float OffsetMultiplier = -25f;

        #region UI Element Paths
        private const string HeaderSourcePath = "StudioScene/Canvas Main Menu/04_System/01_Screen Effect/Screen Effect/Viewport/Content/Image Depth of Field";
        private const string ContentSourcePath = "StudioScene/Canvas Main Menu/04_System/01_Screen Effect/Screen Effect/Viewport/Content/Depth of Field";
        private const string LabelSourcePath = "StudioScene/Canvas Main Menu/04_System/01_Screen Effect/Screen Effect/Viewport/Content/Depth of Field/Draw/TextMeshPro";
        private const string ToggleSourcePath = "StudioScene/Canvas Main Menu/04_System/01_Screen Effect/Screen Effect/Viewport/Content/Depth of Field/Draw/Toggle";
        private const string SliderSourcePath = "StudioScene/Canvas Main Menu/04_System/01_Screen Effect/Screen Effect/Viewport/Content/Depth of Field/Focal Size/Slider";
        private const string InputSourcePath = "StudioScene/Canvas Main Menu/04_System/01_Screen Effect/Screen Effect/Viewport/Content/Depth of Field/Focal Size/InputField";
        private const string ButtonSourcePath = "StudioScene/Canvas Main Menu/04_System/01_Screen Effect/Screen Effect/Viewport/Content/Depth of Field/Focal Size/Button Default";
        #endregion

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
            var headerSource = GameObject.Find(HeaderSourcePath);
            Header = Object.Instantiate(headerSource);
            Header.name = $"Image {labelText}";
            Header.transform.SetParent(headerSource.transform.parent);
            Header.transform.localScale = new Vector3(1f, 1f, 1f);

            var label = Header.GetComponentInChildren<TextMeshProUGUI>();
            label.text = labelText;

            var contentSource = GameObject.Find(ContentSourcePath);
            Content = Object.Instantiate(contentSource);
            Content.name = labelText;
            Content.transform.SetParent(contentSource.transform.parent);
            Content.transform.localScale = new Vector3(1f, 1f, 1f);
            var vlg = Content.GetComponent<VerticalLayoutGroup>();
            Object.Destroy(vlg);
            var layoutElement = Content.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 70;
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
        public SceneEffectsToggleSet AddToggleSet(string text, System.Action<bool> setter, bool initialValue)
        {
            var labelSource = GameObject.Find(LabelSourcePath);
            var toggleSource = GameObject.Find(ToggleSourcePath);

            var containingElement = new GameObject().AddComponent<RectTransform>();
            containingElement.name = text;
            containingElement.SetParent(Content.transform);
            containingElement.transform.localScale = new Vector3(1f, 1f, 1f);
            containingElement.transform.localPosition = new Vector3(0f, Offset, 0f);

            var label = Object.Instantiate(labelSource).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(containingElement.transform);
            label.transform.localScale = new Vector3(1f, 1f, 1f);
            label.transform.localPosition = new Vector3(4f, 0f, 0f);

            var toggle = Object.Instantiate(toggleSource).GetComponent<Toggle>();
            toggle.transform.SetParent(containingElement.transform);
            toggle.transform.localScale = new Vector3(1f, 1f, 1f);
            toggle.transform.localPosition = new Vector3(160f, 0f, 0f);

            var toggleSet = new SceneEffectsToggleSet(label, toggle, text, setter, initialValue);
            Toggles.Add(toggleSet);
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
            var labelSource = GameObject.Find(LabelSourcePath);
            var sliderSource = GameObject.Find(SliderSourcePath);
            var inputSource = GameObject.Find(InputSourcePath);
            var buttonSource = GameObject.Find(ButtonSourcePath);

            var containingElement = new GameObject().AddComponent<RectTransform>();
            containingElement.name = text;
            containingElement.SetParent(Content.transform);
            containingElement.transform.localScale = new Vector3(1f, 1f, 1f);
            containingElement.transform.localPosition = new Vector3(0f, Offset, 0f);

            var label = Object.Instantiate(labelSource).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(containingElement.transform);
            label.transform.localScale = new Vector3(1f, 1f, 1f);
            label.transform.localPosition = new Vector3(4f, 0, 0f);

            var slider = Object.Instantiate(sliderSource).GetComponent<Slider>();
            slider.transform.SetParent(containingElement.transform);
            slider.transform.localScale = new Vector3(1f, 1f, 1f);
            slider.transform.localPosition = new Vector3(160f, 0f, 0f);

            var input = Object.Instantiate(inputSource).GetComponent<InputField>();
            input.transform.SetParent(containingElement.transform);
            input.transform.localScale = new Vector3(1f, 1f, 1f);
            input.transform.localPosition = new Vector3(295f, -10f, 0f);

            var button = Object.Instantiate(buttonSource).GetComponent<Button>();
            button.transform.SetParent(containingElement.transform);
            button.transform.localScale = new Vector3(1f, 1f, 1f);
            button.transform.localPosition = new Vector3(340f, 0f, 0f);

            var sliderSet = new SceneEffectsSliderSet(label, slider, input, button, text, setter, initialValue, sliderMinimum, sliderMaximum);
            Sliders.Add(sliderSet);
            return sliderSet;
        }

        private float Offset => OffsetMultiplier * (Toggles.Count + Sliders.Count);
    }
}
