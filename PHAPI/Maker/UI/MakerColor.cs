using System;
using BepInEx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Control that allows user to change a <see cref="Color"/> in a separate color selector window
    /// </summary>
    public class MakerColor : BaseEditableGuiEntry<Color>
    {
        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="settingName">Text displayed next to the control</param>
        /// <param name="useAlpha">
        /// If true, the color selector will allow the user to change alpha of the color.
        /// If false, no color slider is shown and alpha is always 1f.
        /// </param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="initialValue">Color set to the control when it is created</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerColor(string settingName, bool useAlpha, MakerCategory category, Color initialValue, BaseUnityPlugin owner) : base(category, initialValue, owner)
        {
            SettingName = settingName;
            UseAlpha = useAlpha;
        }

        /// <summary>
        /// Name of the setting
        /// </summary>
        public string SettingName { get; }

        /// <summary>
        /// If true, the color selector will allow the user to change alpha of the color.
        /// If false, no color slider is shown and alpha is always 1f.
        /// </summary>
        public bool UseAlpha { get; }

        /// <summary>
        /// Width of the color box. Can adjust this to allow for longer label text.
        /// Default width is 276 and might need to get lowered to allow longer labels.
        /// The default color boxes in accessory window are 230 wide.
        /// </summary>
        [Obsolete]
        public int ColorBoxWidth { get; set; }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var dd = MakerAPI.GetMakerBase().CreateColorChangeButton(subCategoryList.gameObject, SettingName, Value, UseAlpha, SetValue);
            BufferedValueChanged.Subscribe(dd.SetColor);
            var text = dd.GetComponentInChildren<Text>();
            text.color = TextColor;
            SetTextAutosize(text);
            return dd.gameObject;
        }
    }
}
