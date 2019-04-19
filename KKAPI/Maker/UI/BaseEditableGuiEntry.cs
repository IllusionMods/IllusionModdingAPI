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
            set
            {
                if (!Equals(value, _incomingValue.Value))
                {
                    _incomingValue.OnNext(value);

                    // If the control is instantiated it will fire _outgoingValue by itself
                    if (!Exists || !_firingEnabled)
                        _outgoingValue.OnNext(value);
                }
            }
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
        /// Trigger value changed events and set the value
        /// </summary>
        protected void SetNewValue(TValue newValue)
        {
            if (Equals(newValue, _incomingValue.Value))
                return;

            _incomingValue.OnNext(newValue);

            if (_firingEnabled)
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