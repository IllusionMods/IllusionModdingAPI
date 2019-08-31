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
        /// <summary>
        /// Light gray color best used for text explaining another setting
        /// </summary>
        public static Color ExplanationGray => new Color(0.7f, 0.7f, 0.7f);

        private static Transform _textCopy;

        private string _text;
        private TextMeshProUGUI _instance;

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
        public string Text
        {
            get => _text;
            set
            {
                _text = value;

                if (_instance != null)
                    _instance.text = value;
            }
        }

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
            var original = GetExistingControl("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/tglConfig", "txtExplanation");

            _textCopy = Object.Instantiate(original, GuiCacheTransfrom, true);
            _textCopy.gameObject.SetActive(false);
            _textCopy.name = "txtCustom";

            RemoveLocalisation(_textCopy.gameObject);
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_textCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(TextCopy, subCategoryList, true);

            _instance = tr.GetComponentInChildren<TextMeshProUGUI>();
            _instance.text = Text;
            _instance.color = TextColor;

            return tr.gameObject;
        }
    }
}
