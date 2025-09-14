using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Wrapper for stock game toolbar buttons to allow unified handling.
    /// </summary>
    internal sealed class ToolbarControlAdapter : ToolbarControlBase
    {
        public ToolbarControlAdapter(Button btnObject) : base(btnObject.gameObject.name, null, () => null, KoikatuAPI.Instance)
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
