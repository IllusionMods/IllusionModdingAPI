using System;
using KKAPI.Maker.UI;
using UniRx;
using UnityEngine;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Custom toolbar toggle button. Add using <see cref="CustomToolbarButtons.AddLeftToolbarToggle"/>.
    /// </summary>
    public class ToolbarToggle : BaseEditableGuiEntry<bool>
    {
        private readonly Texture2D _iconTex;

        internal ToolbarToggle(Texture2D iconTex, bool initialValue) : base(null, initialValue, null)
        {
            _iconTex = iconTex ? iconTex : throw new ArgumentNullException(nameof(iconTex));
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var btn = ToolbarButton.CreateLeftToolbarButton(_iconTex);
            btn.onClick.AddListener(() => Value = !Value);
            BufferedValueChanged.Subscribe(b => btn.image.color = Value ? Color.green : Color.white);
            return btn.gameObject;
        }
    }
}