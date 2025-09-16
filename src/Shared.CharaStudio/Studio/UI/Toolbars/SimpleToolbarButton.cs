using System;
using BepInEx;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable CS1573 // Necessary because inheritdoc doesn't count as having a param tag for the compiler, even though the tag is indeed added

namespace KKAPI.Studio.UI.Toolbars
{
    /// <summary>
    /// Simple toolbar button that triggers an action when clicked.
    /// </summary>
    public class SimpleToolbarButton : ToolbarControlBase
    {
        /// <summary>
        /// Observable triggered when the button is clicked.
        /// </summary>
        public Subject<PointerEventData.InputButton> OnClicked { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleToolbarButton"/> class.
        /// </summary>
        /// <inheritdoc />
        /// <param name="onClicked">Action to invoke when clicked.</param>
        public SimpleToolbarButton(string buttonID, string hoverText, Func<Texture2D> iconGetter, BaseUnityPlugin owner, Action<PointerEventData.InputButton> onClicked = null)
            : base(buttonID, hoverText, iconGetter, owner)
        {
            OnClicked = new Subject<PointerEventData.InputButton>();
            if (onClicked != null) OnClicked.Subscribe(onClicked);
        }

        /// <inheritdoc />
        protected internal override void CreateControl()
        {
            if (ButtonObject) return;
            base.CreateControl();
            // Do this instead of ButtonObject.onClick to support right and middle clicks too
            ButtonObject.image.OnPointerClickAsObservable().Subscribe(data =>
            {
                // Recreate Button.onClick behavior
                if (ButtonObject.IsActive() && ButtonObject.IsInteractable())
                    OnClicked.OnNext(data.button);
            });
        }
    }
}
