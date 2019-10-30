using System;
using Studio;
using UniRx;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Base class of controls that hold a value. 
    /// Subscribe to <see cref="Value"/> to update your control's state whenever the value changes.
    /// </summary>
    /// <typeparam name="T">Type of the held value</typeparam>
    public abstract class BaseCurrentStateEditableGuiEntry<T> : CurrentStateCategorySubItemBase
    {
        private readonly Func<OCIChar, T> _updateValue;

        /// <summary>
        /// Create a new control that holds a value
        /// </summary>
        /// <param name="name">Name of the control</param>
        /// <param name="updateValue">Function called every time current character changes and the value needs to be updated</param>
        /// <param name="initialValue">Initial value used before first updateValue call</param>
        protected BaseCurrentStateEditableGuiEntry(string name, Func<OCIChar, T> updateValue, T initialValue = default(T)) : base(name)
        {
            _updateValue = updateValue ?? throw new ArgumentNullException(nameof(updateValue));
            Value = new BehaviorSubject<T>(initialValue);
        }

        /// <summary>
        /// Current value of this control
        /// </summary>
        public BehaviorSubject<T> Value { get; }

        /// <inheritdoc />
        protected internal override void OnUpdateInfo(OCIChar ociChar)
        {
            Value.OnNext(_updateValue.Invoke(ociChar));
        }
    }
}
