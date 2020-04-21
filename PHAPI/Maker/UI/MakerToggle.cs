using BepInEx;
using HarmonyLib;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that displays a toggle
    /// </summary>
    public class MakerToggle : BaseEditableGuiEntry<bool>
    {
        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="displayName">Text shown next to the checkbox</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerToggle(MakerCategory category, string displayName, BaseUnityPlugin owner) : this(category, displayName, false, owner) { }

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="displayName">Text shown next to the checkbox</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="initialValue">Initial value of the toggle</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerToggle(MakerCategory category, string displayName, bool initialValue, BaseUnityPlugin owner) : base(category, initialValue, owner)
        {
            DisplayName = displayName;
        }

        /// <summary>
        /// Text shown next to the checkbox
        /// </summary>
        public string DisplayName { get; }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var sw = MakerAPI.GetMakerBase().CreateSwitchUI(subCategoryList.gameObject, DisplayName, Value, SetValue);
            BufferedValueChanged.Subscribe(sw.SetValue);
            var text = Traverse.Create(sw).Field<Text>("title").Value;
            text.color = TextColor;
            SetTextAutosize(text);
            return sw.gameObject;
        }
    }
}
