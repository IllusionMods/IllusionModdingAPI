using BepInEx;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private int _Rows = 1;
        private static Transform _radioCopy;

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

        /// <summary>
        /// Number of Rows to create; Defaults to 1
        /// Recommended: adding a Null MakerText per additional row not including seperators
        /// </summary>
        public int Rows
        {
            get { return _Rows; }
            set { _Rows = Math.Min(Math.Max(1, value), _buttons.Length); }
        }

        /// <summary>
        /// Only use if control is added with AddAccessoryWindowControl
        /// Syncs the state of all radio buttons for accessory window
        /// Without this the value displayed will be wrong on accessories
        /// </summary>
        public bool Unify_AccessoryWindowControl = false;

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_radioCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(RadioCopy, subCategoryList, false);

            tr.name = "rb";

            var settingName = tr.Find("textTglTitle").GetComponent<TextMeshProUGUI>();
            settingName.text = _settingName;
            settingName.color = TextColor;

            var sourceToggle = tr.Find("rb00");
            Buttons = _buttons.Select(
                     (x, i) =>
                     {
                         if (i == 0)
                             return sourceToggle;

                         var newButton = Object.Instantiate(sourceToggle, tr, false);
                         newButton.name = "rb0" + i;

                         return newButton;
                     })
                 .Select(x => x.GetComponent<Toggle>())
                 .ToList()
                 .AsReadOnly();

            RadioToggleGroup RadioGroup = new RadioToggleGroup();
            var Ratio = Buttons.Count / _Rows + (Buttons.Count % _Rows) % 2;
            var singleToggleWidth = 280 / Ratio;
            for (var index = 0; index < Buttons.Count; index++)
            {
                var toggle = Buttons[index];

                RadioGroup.RegisterToggle(toggle);
                toggle.isOn = false;
                var rt = toggle.GetComponent<RectTransform>();
                rt.offsetMin = new Vector2(singleToggleWidth * (index % Ratio) - 280, 8);
                rt.offsetMax = new Vector2(singleToggleWidth * (index % Ratio + 1) - 280, -8);
                rt.position += new Vector3(0, -24 * (index / Ratio), 0);
                toggle.GetComponentInChildren<TextMeshProUGUI>().text = _buttons[index];

                var indexCopy = index;
                toggle.onValueChanged.AddListener(
                    a =>
                    {
                        if (a || indexCopy == Value)
                        {
                            SetValue(indexCopy);
                            if (!RadioGroup.AnyTogglesOn())
                            {
                                toggle.isOn = true;
                            }
                            RadioGroup.NotifyToggleOn(toggle);
                        }
                    });
            }

            BufferedValueChanged.Subscribe(
                i =>
                {
                    //for (var index = 0; index < Buttons.Count; index++)
                    //{
                    //    var tgl = Buttons[index];
                    //    tgl.isOn = index == i;
                    //}
                    if (Unify_AccessoryWindowControl)
                    {
                        for (int j = 0, n = ControlObjects.Count(); j < n; j++)
                        {
                            ControlObjects.ElementAt(j).GetComponentsInChildren<Toggle>()[Value].isOn = true;
                        }
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
            var originalSlider = GetExistingControl("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglEye02", "rbEyeSettingType");
            _radioCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, false);
            _radioCopy.gameObject.SetActive(false);
            _radioCopy.name = "rbSide";

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

            RemoveLocalisation(_radioCopy.gameObject);
        }

        private class RadioToggleGroup : ToggleGroup
        { }
    }
}
