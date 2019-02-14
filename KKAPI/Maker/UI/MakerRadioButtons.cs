using System;
using System.Collections.ObjectModel;
using System.Linq;
using BepInEx;
using TMPro;
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

        private static Transform _radioCopy;

        /// <summary>
        /// Objects of all of the radio buttons
        /// </summary>
        public ReadOnlyCollection<Toggle> Buttons { get; private set; }

        public MakerRadioButtons(MakerCategory category, BaseUnityPlugin owner, string settingName, params string[] buttons) : base(category, 0, owner)
        {
            if (buttons.Length < 2) throw new ArgumentException("Need at least two buttons.", nameof(buttons));

            _settingName = settingName;
            _buttons = buttons;
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_radioCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(RadioCopy, subCategoryList, true);

            tr.name = "rb" + GuiApiNameAppendix;

            var settingName = tr.Find("textTglTitle").GetComponent<TextMeshProUGUI>();
            settingName.text = _settingName;
            settingName.color = TextColor;

            var sourceToggle = tr.Find("rb00");

            Buttons = _buttons.Select(
                    (x, i) =>
                    {
                        if (i == 0)
                            return sourceToggle;

                        var newButton = Object.Instantiate(sourceToggle, tr, true);
                        newButton.name = "rb0" + i;
                    
                        return newButton;
                    }).Select(x => x.GetComponent<Toggle>())
                .ToList().AsReadOnly();
            
            var singleToggleWidth = 280 / Buttons.Count;
            for (var index = 0; index < Buttons.Count; index++)
            {
                var toggle = Buttons[index];

                var rt = toggle.GetComponent<RectTransform>();
                rt.offsetMin = new Vector2(singleToggleWidth * index - 280, 8);
                rt.offsetMax = new Vector2(singleToggleWidth * (index + 1) - 280, -8);

                toggle.GetComponentInChildren<TextMeshProUGUI>().text = _buttons[index];

                var indexCopy = index;
                toggle.onValueChanged.AddListener(a =>
                {
                    if (a || indexCopy == Value)
                        SetNewValue(indexCopy);
                });
            }
            
            BufferedValueChanged.Subscribe(i =>
            {
                for (var index = 0; index < Buttons.Count; index++)
                {
                    var tgl = Buttons[index];
                    tgl.isOn = index == i;
                }
            });
            
            return tr.gameObject;
        }

        private static Transform RadioCopy
        {
            get
            {
                if (_radioCopy == null)
                    MakeCopy();

                return _radioCopy;
            }
        }

        private static void MakeCopy()
        {
            // Exists in male and female maker
            var originalSlider = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglEye02/Eye02Top/rbEyeSettingType").transform;

            _radioCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, true);
            _radioCopy.gameObject.SetActive(false);
            _radioCopy.name = "rbSide" + GuiApiNameAppendix;

            Object.DestroyImmediate(_radioCopy.GetComponent<ToggleGroup>());

            foreach (var toggle in _radioCopy.GetComponentsInChildren<Toggle>())
            {
                toggle.onValueChanged.RemoveAllListeners();

                var gameObjectName = "rb" + toggle.gameObject.name.Substring(8);
                if (gameObjectName == "rb00")
                {
                    toggle.image.raycastTarget = true;
                    toggle.graphic.raycastTarget = true;
                    toggle.gameObject.name = gameObjectName;
                }
                else
                {
                    Object.Destroy(toggle.gameObject);
                }
            }
        }
    }
}