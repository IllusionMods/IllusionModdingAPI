using System;
using System.Linq;
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
        
        private void MakeCopy()
        {
            _dropdownCopy = Object.Instantiate(GameObject.Find("ddBirthday"), GuiCacheTransfrom, false).transform;
            _dropdownCopy.gameObject.SetActive(false);
            _dropdownCopy.name = "ddList";

            // Setup layout of the group
            var mainle = _dropdownCopy.GetComponent<LayoutElement>();
            mainle.minHeight = 40;
            mainle.preferredHeight = 40;
            mainle.flexibleHeight = 0;
            _dropdownCopy.gameObject.AddComponent<HorizontalLayoutGroup>();

            // Destroy unnecessary objects
            Text text = null;
            Dropdown dropdown = null;
            foreach (var child in _dropdownCopy.transform.Children())
            {
                if (text == null)
                {
                    text = child.GetComponent<Text>();
                    if (text != null) continue;
                }
                else if (dropdown == null)
                {
                    dropdown = child.GetComponent<Dropdown>();
                    if (dropdown != null) continue;
                }
                Object.DestroyImmediate(child.gameObject);
            }
            if(text == null) throw new ArgumentNullException(nameof(text));
            if(dropdown == null) throw new ArgumentNullException(nameof(dropdown));

            // Needed for HorizontalLayoutGroup
            text.gameObject.AddComponent<LayoutElement>();
            var dle = dropdown.gameObject.AddComponent<LayoutElement>();
            dle.minWidth = 230;
            dle.flexibleWidth = 0;

            dropdown.name = "ddListInner";

            text.alignment = TextAnchor.MiddleLeft;
            SetTextAutosize(text);

            dropdown.onValueChanged.ActuallyRemoveAllListeners();
            dropdown.ClearOptions();
            dropdown.GetComponent<Image>().raycastTarget = true;
            dropdown.template.GetComponentInChildren<UI_ToggleEx>().image.raycastTarget = true;
            SetTextAutosize(dropdown.template.GetComponentInChildren<Text>(true));

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
            var tr = Object.Instantiate(_dropdownCopy, subCategoryList, false);

            var settingName = tr.Find("textKindTitle").GetComponent<Text>();
            settingName.text = SettingName;
            settingName.color = TextColor;

            var dropdown = tr.GetComponentInChildren<Dropdown>();
            dropdown.options.AddRange(Options.Select(x => new Dropdown.OptionData(x)));

            dropdown.onValueChanged.AddListener(SetValue);
            BufferedValueChanged.Subscribe(i => dropdown.value = i);

            // Fix box not updating if BufferedValueChanged equals the default dropdown val
            if (Value == dropdown.value)
            {
                dropdown.RefreshShownValue();
                SetValue(dropdown.value);
            }

            return tr.gameObject;
        }
    }
}
