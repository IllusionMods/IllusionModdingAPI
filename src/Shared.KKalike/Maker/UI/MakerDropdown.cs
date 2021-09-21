using BepInEx;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that draws a dropdown list
    /// </summary>
    public class MakerDropdown : BaseEditableGuiEntry<int>
    {
        private static Transform _dropdownCopy;

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="settingName">Text displayed next to the dropdown</param>
        /// <param name="options">Items for the dropdown menu</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="initialValue">Initially selected item in the dropdown menu</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerDropdown(string settingName, string[] options, MakerCategory category, int initialValue, BaseUnityPlugin owner)
            : base(category, initialValue, owner)
        {
            SettingName = settingName;
            Options = options;
        }

        /// <summary>
        /// List of all options in the dropdown
        /// </summary>
        public string[] Options { get; }

        /// <summary>
        /// Name displayed next to the dropdown
        /// </summary>
        public string SettingName { get; }

        private static Transform DropdownCopy
        {
            get
            {
                if (_dropdownCopy == null)
                    MakeCopy();
                return _dropdownCopy;
            }
        }

        private static void MakeCopy()
        {
#if KKS
            var originalSlider = GetExistingControl("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/tglVisualSettings", "ddRampG");
#else
            var originalSlider = GetExistingControl("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/tglConfig", "ddRamp");
#endif

            _dropdownCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, false);
            _dropdownCopy.gameObject.SetActive(false);
            _dropdownCopy.name = "ddList";

            var dd = _dropdownCopy.GetComponentInChildren<TMP_Dropdown>();
            dd.onValueChanged.RemoveAllListeners();
            dd.ClearOptions();
            dd.template.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 540);

            foreach (var img in dd.GetComponentsInChildren<Image>(true))
                img.raycastTarget = true;

            RemoveLocalisation(_dropdownCopy.gameObject);
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_dropdownCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(DropdownCopy, subCategoryList, false);

            var settingName = tr.Find("textKindTitle").GetComponent<TextMeshProUGUI>();
            settingName.text = SettingName;
            settingName.color = TextColor;

            var dropdown = tr.GetComponentInChildren<TMP_Dropdown>();
            dropdown.options.AddRange(Options.Select(x => new TMP_Dropdown.OptionData(x)));

            dropdown.onValueChanged.AddListener(SetValue);
            BufferedValueChanged.Subscribe(i => dropdown.value = i);

            // Fix box not updating if BufferedValueChanged equals the default dropdown val
            if (Value == dropdown.value)
            {
                dropdown.RefreshShownValue();
                SetValue(dropdown.value);
            }

            var layout = tr.GetComponent<LayoutElement>();
            layout.flexibleWidth = 1;

            return tr.gameObject;
        }
    }
}
