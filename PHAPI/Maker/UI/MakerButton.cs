using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that draws a simple blue button.
    /// </summary>
    public class MakerButton : BaseGuiEntry
    {
        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="text">Text displayed on the button</param>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerButton(string text, MakerCategory category, BaseUnityPlugin owner) : base(category, owner)
        {
            Text = text;
            OnClick = new Button.ButtonClickedEvent();
        }

        /// <summary>
        /// Fired when user clicks on the button
        /// </summary>
        public Button.ButtonClickedEvent OnClick { get; }

        /// <summary>
        /// Text displayed on the button
        /// </summary>
        public string Text { get; }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            OnClick.RemoveAllListeners();
            base.Dispose();
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var button = MakerAPI.GetMakerBase().CreateButton(subCategoryList.gameObject, Text, OnClick.Invoke);
            SetTextAutosize(button.GetComponentInChildren<Text>());
            return button.gameObject;
        }
    }
}
