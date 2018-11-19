using System.Linq;
using BepInEx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MakerAPI
{
    public class MakerDropdown : BaseEditableGuiEntry<int>
    {
        private static Transform _dropdownCopy;

        public MakerDropdown(string settingName, string[] options, MakerCategory category, int initialValue, BaseUnityPlugin owner) 
            : base(category, initialValue, owner)
        {
            SettingName = settingName;
            Options = options;
        }

        public string[] Options { get; }
        public string SettingName { get; }

        private static Transform DropdownCopy
        {
            get
            {
                if (_dropdownCopy == null)
                {
                    var originalSlider = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/tglConfig/ConfigTop/ddRamp").transform;

                    _dropdownCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, true);
                    _dropdownCopy.gameObject.SetActive(false);
                    _dropdownCopy.name = "ddList" + GuiApiNameAppendix;

                    var dd = _dropdownCopy.GetComponentInChildren<TMP_Dropdown>();
                    dd.onValueChanged.RemoveAllListeners();
                    dd.ClearOptions();
                    dd.GetComponent<Image>().raycastTarget = true;
                }
                return _dropdownCopy;
            }
        }

        protected internal override void CreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(DropdownCopy, subCategoryList, true);

            var settingName = tr.Find("textKindTitle").GetComponent<TextMeshProUGUI>();
            settingName.text = SettingName;
            settingName.color = TextColor;

            var dropdown = tr.GetComponentInChildren<TMP_Dropdown>();
            dropdown.onValueChanged.AddListener(SetNewValue);
            dropdown.options.AddRange(Options.Select(x => new TMP_Dropdown.OptionData(x)));
            BufferedValueChanged.Subscribe(i => dropdown.value = i);

            tr.gameObject.SetActive(true);
        }
    }
}
