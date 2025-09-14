using System;
using BepInEx;
using UniRx;
using UnityEngine;
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Simple toolbar button that triggers an action when clicked.
    /// </summary>
    public class ToolbarButtonControl : ToolbarControlBase
    {
        /// <summary>
        /// Observable triggered when the button is clicked.
        /// </summary>
        public Subject<Unit> OnClicked { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarButtonControl"/> class.
        /// </summary>
        /// <inheritdoc />
        /// <param name="onClicked">Action to invoke when clicked.</param>
        public ToolbarButtonControl(string buttonID, string hoverText, Func<Texture2D> iconGetter, Action onClicked, BaseUnityPlugin owner)
            : base(buttonID, hoverText, iconGetter, owner)
        {
            OnClicked = new Subject<Unit>();
            if (onClicked != null) OnClicked.Subscribe(_ => onClicked());
        }

        /// <inheritdoc />
        protected internal override void CreateControl()
        {
            if (ButtonObject.Value) return;
            base.CreateControl();
            ButtonObject.Value.onClick.AddListener(() => OnClicked.OnNext(Unit.Default));
        }
    }
}
