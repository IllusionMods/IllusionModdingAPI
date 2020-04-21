﻿using System;
using BepInEx;
using HarmonyLib;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that draws a slider and a text box (both are used to edit the same value)
    /// </summary>
    public class MakerSlider : BaseEditableGuiEntry<float>
    {
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
        [Obsolete]
        public Func<string, float> StringToValue { get; set; }

        /// <summary>
        /// Custom converter from the slider value to what's displayed in the textbox.
        /// If not set, <code>Mathf.RoundToInt(f * 100).ToString()</code> is used.
        /// </summary>
        [Obsolete]
        public Func<float, string> ValueToString { get; set; }

        /// <summary>
        /// Custom converter from the slider value to what's displayed in the textbox.
        /// By default it's set to 0.00
        /// </summary>
        public string TextFormat { get; set; } = "0.00";

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var sliderUi = MakerAPI.GetMakerBase().CreateInputSliderUI(subCategoryList.gameObject, _settingName, _minValue, _maxValue, true, _defaultValue, SetValue);
            BufferedValueChanged.Subscribe(sliderUi.SetValue); 
            var text = Traverse.Create(sliderUi).Field<Text>("title").Value;
            text.color = TextColor;
            SetTextAutosize(text);
            Traverse.Create(sliderUi).Field("textFormat").SetValue(TextFormat);
            return sliderUi.gameObject;
        }
    }
}
