using BepInEx;
using TMPro;
using UnityEngine;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that displays a simple text
    /// </summary>
    public class MakerText : BaseGuiEntry
    {
        private static Transform _textCopy;

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="text">Displayed text</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerText(string text, MakerCategory category, BaseUnityPlugin owner) : base(category, owner)
        {
            Text = text;
        }

        /// <summary>
        /// Displayed text
        /// </summary>
        public string Text { get; }

        private static Transform TextCopy
        {
            get
            {
                if (_textCopy == null)
                    MakeCopy();
                return _textCopy;
            }
        }

        private static void MakeCopy()
        {
            var original = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/tglConfig/ConfigTop/txtExplanation").transform;

            _textCopy = Object.Instantiate(original, GuiCacheTransfrom, true);
            _textCopy.gameObject.SetActive(false);
            _textCopy.name = "txtCustom";
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_textCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        public override void Dispose() { }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(TextCopy, subCategoryList, true);

            var settingName = tr.GetComponentInChildren<TextMeshProUGUI>();
            settingName.text = Text;
            settingName.color = TextColor;

            return tr.gameObject;
        }
    }
}
