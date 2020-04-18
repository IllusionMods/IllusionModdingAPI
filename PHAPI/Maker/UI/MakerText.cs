using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

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
        
        private string _text;
        private Text _instance;

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
        
        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            _instance = MakerAPI.GetMakerBase().CreateLabel(subCategoryList.gameObject, Text);
            SetTextAutosize(_instance.GetComponentInChildren<Text>());
            return _instance.gameObject;
        }
    }
}
