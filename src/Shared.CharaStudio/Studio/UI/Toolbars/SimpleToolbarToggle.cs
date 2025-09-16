using System;
using BepInEx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable CS1573 // Necessary because inheritdoc doesn't count as having a param tag for the compiler, even though the tag is indeed added

namespace KKAPI.Studio.UI.Toolbars
{
    /// <summary>
    /// Toolbar button that acts as a toggle (on/off).
    /// </summary>
    public class SimpleToolbarToggle : SimpleToolbarButton
    {
        /// <summary>
        /// Observable value representing the toggle state.
        /// </summary>
        public BehaviorSubject<bool> Toggled { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleToolbarToggle"/> class.
        /// </summary>
        /// <inheritdoc />
        /// <param name="initialValue">Initial toggle value.</param>
        /// <param name="onValueChanged">Action to invoke when value changes.</param>
        public SimpleToolbarToggle(string buttonID, string hoverText, Func<Texture2D> iconGetter, bool initialValue, BaseUnityPlugin owner, Action<bool> onValueChanged = null)
            : base(buttonID, hoverText, iconGetter, owner)
        {
            Toggled = new BehaviorSubject<bool>(initialValue);
            Toggled.Subscribe(_ => UpdateVisualState());

            if (onValueChanged != null)
            {
                var firstSkipped = false;
                Toggled.Subscribe(b =>
                {
                    if (firstSkipped) onValueChanged(b);
                    else firstSkipped = true;
                });
            }
        }

        /// <inheritdoc />
        protected internal override void CreateControl()
        {
            if (ButtonObject) return;

            base.CreateControl();

            OnClicked.Subscribe(button =>
            {
                if (button == PointerEventData.InputButton.Left)
                    Toggled.OnNext(!Toggled.Value);
            });

            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(SimpleToolbarToggle));
            var button = ButtonObject;
            if (!button) return;
            var btnIcon = button.image;
            btnIcon.color = Toggled.Value ? Color.green : Color.white;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            Toggled?.Dispose();
        }
    }
}
