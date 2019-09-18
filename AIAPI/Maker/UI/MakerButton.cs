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
        private static Transform _buttonCopy;

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

            TextColor = new Color(0.090f, 0.118f, 0.141f);
        }

        /// <summary>
        /// Fired when user clicks on the button
        /// </summary>
        public Button.ButtonClickedEvent OnClick { get; }

        /// <summary>
        /// Text displayed on the button
        /// </summary>
        public string Text { get; }

        private static Transform ButtonCopy
        {
            get
            {
                if (_buttonCopy == null)
                    MakeCopy();
                return _buttonCopy;
            }
        }

        private static void MakeCopy()
        {
            var original = GameObject.Find("DefaultColor").transform;

            _buttonCopy = Object.Instantiate(original, GuiCacheTransfrom, false);
            _buttonCopy.gameObject.SetActive(false);
            _buttonCopy.name = "btnCustom";

            var button = _buttonCopy.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.targetGraphic.raycastTarget = true;

            button.GetComponent<RectTransform>().sizeDelta = new Vector2(414, 0);

            RemoveLocalisation(_buttonCopy.gameObject);
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_buttonCopy == null)
                MakeCopy();
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
            var tr = Object.Instantiate(ButtonCopy, subCategoryList, false);

            var button = tr.GetComponentInChildren<Button>();
            button.onClick.AddListener(OnClick.Invoke);

            var text = tr.GetComponentInChildren<Text>();
            text.text = Text;
            text.color = TextColor;
            SetTextAutosize(text);

            return tr.gameObject;
        }
    }
}
