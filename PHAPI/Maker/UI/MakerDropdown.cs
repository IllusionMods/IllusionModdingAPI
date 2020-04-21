using System.Linq;
using BepInEx;
using HarmonyLib;
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

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var dd = MakerAPI.GetMakerBase().CreateDropDownUI(subCategoryList.gameObject, SettingName, Options.Select(x => new Dropdown.OptionData(x)).ToList(), SetValue);
            BufferedValueChanged.Subscribe(dd.SetValue);
            var text = Traverse.Create(dd).Field<Text>("title").Value;
            text.color = TextColor;
            foreach (var txt in dd.GetComponentsInChildren<Text>(true))
                SetTextAutosize(txt);
            return dd.gameObject;
        }
    }
}
