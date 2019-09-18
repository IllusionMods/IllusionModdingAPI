using BepInEx;
using CharaCustom;
using KKAPI.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

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

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(GameObject.Find("CharaCustom/CustomControl/CanvasMain/SettingWindow/WinFace/F_Mole/Setting/Setting02/Scroll View/Viewport/Content/ColorSet"), subCategoryList, true);
            tr.name = "ColorSet";

            var ccs = tr.GetComponent<CustomColorSet>();
            var settingName = ccs.title;
            settingName.text = SettingName;
            settingName.color = TextColor;
            SetTextAutosize(settingName);

            var button = ccs.button;
            button.onClick.ActuallyRemoveAllListeners();
            button.targetGraphic.raycastTarget = true;

            ccs.image.color = Value;

            ccs.actUpdateColor = SetValue;

            return tr.gameObject;
        }
    }
}
