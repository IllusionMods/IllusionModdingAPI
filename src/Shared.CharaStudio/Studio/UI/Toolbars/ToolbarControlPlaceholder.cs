using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Wrapper for stock game toolbar buttons to allow unified handling.
    /// </summary>
    internal sealed class ToolbarControlPlaceholder : CustomToolbarControlBase
    {
        public ToolbarControlPlaceholder(Button btnObject) : base(btnObject.gameObject.name, string.Empty, () => null, KoikatuAPI.Instance)
        {
            ButtonObject.OnNext(btnObject);
            RectTransform = (RectTransform)btnObject.transform;
            GetActualPosition(out var row, out var col);
            DesiredRow = row;
            DesiredColumn = col;
            Visible.OnNext(btnObject.gameObject.activeSelf);
        }
        /// <inheritdoc />
        protected internal override void CreateControl() { }
        /// <inheritdoc />
        public override void Dispose() { }
    }
}
