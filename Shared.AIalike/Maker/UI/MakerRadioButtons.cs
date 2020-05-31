using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BepInEx;
using Illusion.Extensions;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that displays multiple radio buttons
    /// </summary>
    public class MakerRadioButtons : BaseEditableGuiEntry<int>
    {
        private readonly string _settingName;
        private readonly string[] _buttons;

        /// <summary>
        /// Objects of all of the radio buttons
        /// </summary>
        public ReadOnlyCollection<Toggle> Buttons { get; private set; }

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="settingName">Text displayed next to the buttons</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        /// <param name="buttons">Names of the radio buttons. Need at least 2 buttons.</param>
        public MakerRadioButtons(MakerCategory category, BaseUnityPlugin owner, string settingName, params string[] buttons) : this(category, owner, settingName, 0, buttons) { }

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="settingName">Text displayed next to the buttons</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        /// <param name="initialValue">Initial value of the control</param>
        /// <param name="buttons">Names of the radio buttons. Need at least 2 buttons.</param>
        public MakerRadioButtons(MakerCategory category, BaseUnityPlugin owner, string settingName, int initialValue, params string[] buttons) : base(category, initialValue, owner)
        {
            if (buttons.Length < 2) throw new ArgumentException("Need at least two buttons.", nameof(buttons));

            _settingName = settingName;
            _buttons = buttons;
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var selGo = Object.Instantiate(GameObject.Find("SettingWindow/WinBody/B_Nip/SelectMenu"), subCategoryList);
            selGo.name = "rb";

            var toggleGroup = selGo.GetComponent<ToggleGroup>();
            Transform singleButton = null;
            foreach (var c in selGo.transform.Children())
            {
                var singleToggle = c.GetComponent<UI_ToggleEx>();
                toggleGroup.UnregisterToggle(singleToggle);
                if (c.name == "tgl01")
                {
                    singleToggle.onValueChanged.ActuallyRemoveAllListeners();
                    singleButton = c;
                }
                else
                {
                    Object.DestroyImmediate(c.gameObject);
                }
            }

            var txtGo = Object.Instantiate(GameObject.Find("SettingWindow/WinBody/B_Nip/Setting/Setting02/Scroll View/Viewport/Content/ColorSet/Text"), selGo.transform);
            txtGo.transform.SetAsFirstSibling();
            const int textWidth = 110;
            txtGo.AddComponent<LayoutElement>().minWidth = textWidth;
            var txtCmp = txtGo.GetComponent<Text>();
            txtCmp.text = _settingName;
            txtCmp.color = TextColor;
            SetTextAutosize(txtCmp);

            RemoveLocalisation(selGo);

            var newButtons = new List<Toggle>();

            for (int i = 0; i < _buttons.Length; i++)
            {
                var toggleId = i;

                Transform newBtn;
                if (toggleId <= 0)
                {
                    newBtn = singleButton ?? throw new ArgumentNullException(nameof(singleButton));
                }
                else
                {
                    newBtn = Object.Instantiate(singleButton, selGo.transform);
                    newBtn.name = "tgl0" + toggleId;
                }

                var newTglText = newBtn.GetComponentInChildren<Text>();
                newTglText.text = _buttons[i];
                SetTextAutosize(newTglText);

                var newTgl = newBtn.GetComponent<UI_ToggleEx>();
                newTgl.group = toggleGroup;

                newTgl.onValueChanged.AddListener(
                    val =>
                    {
                        if (val)
                            SetValue(toggleId);
                    });

                newButtons.Add(newTgl);
            }

            Buttons = newButtons.AsReadOnly();


            var singleToggleWidth = (selGo.GetComponent<RectTransform>().sizeDelta.x - textWidth - 10) / Buttons.Count;
            foreach (var button in Buttons)
                button.GetComponent<LayoutElement>().minWidth = singleToggleWidth;

            BufferedValueChanged.Subscribe(
                i =>
                {
                    for (var index = 0; index < Buttons.Count; index++)
                    {
                        var tgl = Buttons[index];
                        tgl.isOn = index == i;
                    }
                });

            return selGo.gameObject;
        }
    }
}
