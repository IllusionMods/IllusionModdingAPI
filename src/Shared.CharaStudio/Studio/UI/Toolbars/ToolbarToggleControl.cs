using System;
using BepInEx;
using UniRx;
using UnityEngine;
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Toolbar button that acts as a toggle (on/off).
    /// </summary>
    public class ToolbarToggleControl : CustomToolbarControlBase
    {
        /// <summary>
        /// Observable value representing the toggle state.
        /// </summary>
        public BehaviorSubject<bool> Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarToggleControl"/> class.
        /// </summary>
        /// <inheritdoc />
        /// <param name="initialValue">Initial toggle value.</param>
        /// <param name="onValueChanged">Action to invoke when value changes.</param>
        public ToolbarToggleControl(string buttonID, string hoverText, Func<Texture2D> iconGetter, bool initialValue, Action<bool> onValueChanged, BaseUnityPlugin owner)
            : base(buttonID, hoverText, iconGetter, owner)
        {
            Value = new BehaviorSubject<bool>(initialValue);
            Value.Subscribe(_ => UpdateVisualState());
            if (onValueChanged != null)
            {
                var firstSkipped = false;
                Value.Subscribe(b =>
                {
                    if (firstSkipped) onValueChanged(b);
                    else firstSkipped = true;
                });
            }
        }

        /// <inheritdoc />
        protected internal override void CreateControl()
        {
            if (ButtonObject.Value) return;

            base.CreateControl();
            ButtonObject.Value.onClick.AddListener(() => Value.OnNext(!Value.Value));
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ToolbarToggleControl));
            var button = ButtonObject.Value;
            if (!button) return;
            var btnIcon = button.image;
            btnIcon.color = Value.Value ? Color.green : Color.white;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            Value?.Dispose();
        }
    }
}
