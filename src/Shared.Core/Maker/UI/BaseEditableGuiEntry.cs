#if AI || HS2 || KKS
using System;
#endif
using BepInEx;
using UniRx;
using UnityEngine;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Base of custom controls that have a value that can be changed and watched for changes.
    /// </summary>
    public abstract class BaseEditableGuiEntry<TValue> : BaseGuiEntry
    {
        private readonly BehaviorSubject<TValue> _incomingValue;
        private readonly Subject<TValue> _outgoingValue;

        /// <inheritdoc />
        protected BaseEditableGuiEntry(MakerCategory category, TValue initialValue, BaseUnityPlugin owner) : base(category, owner)
        {
            _incomingValue = new BehaviorSubject<TValue>(initialValue);
            _outgoingValue = new Subject<TValue>();
        }

        /// <summary>
        /// Buttons 1, 2, 3 are values 0, 1, 2
        /// </summary>
        public TValue Value
        {
            get => _incomingValue.Value;
            set => SetValue(value);
        }

        /// <summary>
        /// Fired every time the value is changed, and once when the control is created.
        /// Buttons 1, 2, 3 are values 0, 1, 2
        /// </summary>
        public IObservable<TValue> ValueChanged => _outgoingValue;

        /// <summary>
        /// Use to get value changes for controls. Fired by external value set and by SetNewValue.
        /// </summary>
        protected IObservable<TValue> BufferedValueChanged => _incomingValue;

        /// <summary>
        /// Set the new value and trigger the <see cref="ValueChanged"/> event if the control has been created and the value actually changed.
        /// </summary>
        /// <param name="newValue">Value to set</param>
        public void SetValue(TValue newValue)
        {
            SetValue(newValue, true);
        }

        /// <summary>
        /// Set the new value and optionally trigger the <see cref="ValueChanged"/> event if the control has been created.
        /// </summary>
        /// <param name="newValue">Value to set</param>
        /// <param name="fireEvents">Fire the <see cref="ValueChanged"/> event if the value actually changed.</param>
        public void SetValue(TValue newValue, bool fireEvents)
        {
            if (Equals(newValue, _incomingValue.Value))
                return;

            _incomingValue.OnNext(newValue);

            if (_firingEnabled && fireEvents)
                _outgoingValue.OnNext(newValue);
        }

        private bool _firingEnabled;

        internal override void CreateControl(Transform subCategoryList)
        {
            var wasCreated = Exists;

            _firingEnabled = false;
            base.CreateControl(subCategoryList);
            _firingEnabled = true;

            // Trigger value changed events after the control is created to make sure everything updates its state
            // Make sure this only happens with the 1st copy of the control, so it's not fired for every accessory slot
            if (!wasCreated)
            {
                _incomingValue.OnNext(_incomingValue.Value);
                if (!MakerAPI.InsideAndLoaded)
                    _outgoingValue.OnNext(_incomingValue.Value);
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _incomingValue.Dispose();
            _outgoingValue.Dispose();
            base.Dispose();
        }
    }
}
