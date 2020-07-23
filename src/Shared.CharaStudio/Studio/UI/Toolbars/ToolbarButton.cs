using System;
using System.Linq;
using KKAPI.Maker.UI;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Custom toolbar button. Add using <see cref="CustomToolbarButtons.AddLeftToolbarButton"/>.
    /// </summary>
    public class ToolbarButton : BaseGuiEntry
    {
        private static GameObject _existingButton;
        private readonly Subject<Unit> _clicked = new Subject<Unit>();
        private readonly Texture2D _iconTex;

        internal ToolbarButton(Texture2D iconTex) : base(null, null)
        {
            _iconTex = iconTex ? iconTex : throw new ArgumentNullException(nameof(iconTex));
        }

        /// <summary>
        /// Triggered when the button is clicked.
        /// </summary>
        public IObservable<Unit> Clicked => _clicked;

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var btn = CreateLeftToolbarButton(_iconTex);
            btn.onClick.AddListener(() => _clicked.OnNext(Unit.Default));
            return btn.gameObject;
        }

        internal static Button CreateLeftToolbarButton(Texture2D iconTex)
        {
            var btnIconSprite = Sprite.Create(iconTex, new Rect(0f, 0f, 32f, 32f), new Vector2(16f, 16f));

            if (_existingButton == null)
                _existingButton = GameObject.Find("StudioScene/Canvas System Menu/01_Button/Button Center");
            var allButtonParent = _existingButton.transform.parent;

            // Figure out where to place the new button
            var allButtons = allButtonParent.OfType<RectTransform>().ToList();
            var columns = allButtons.GroupBy(rt => Mathf.RoundToInt(rt.anchoredPosition.x));
            // Find the column with the least buttons, favor left column if both are equal
            var lowestColumn = columns.OrderBy(x => x.Max(r => r.anchoredPosition.y)).ThenBy(x => x.Key).First();
            // Get the topmost button of that column
            var highestButtonRt = lowestColumn.OrderByDescending(r => r.anchoredPosition.y).First();

            // Create new button
            var copyButton = Object.Instantiate(_existingButton, allButtonParent, false);
            copyButton.name = "Button Custom (StudioAPI)";
            var copyRt = copyButton.GetComponent<RectTransform>();
            copyRt.localScale = Vector3.one;
            copyRt.anchoredPosition = highestButtonRt.anchoredPosition + new Vector2(0, 40f);

            var copyBtn = copyRt.GetComponent<Button>();
            copyBtn.onClick.ActuallyRemoveAllListeners();
            var btnIcon = copyBtn.image;
            btnIcon.sprite = btnIconSprite;
            btnIcon.color = Color.white;
            return copyBtn;
        }
    }
}