using System;
using System.Collections.Generic;
using KKAPI.Maker.UI;
using UniRx;

namespace KKAPI.Maker
{
    /// <summary>
    /// A wrapper for custom controls used in accessory window (added by using <see cref="MakerAPI.AddAccessoryWindowControl{T}"/>).
    /// It abstracts away switching between accessory slots and provides a simple list of values for each accessory.
    /// </summary>
    /// <typeparam name="T">Control to be wrapped. The control has to be added by using <see cref="MakerAPI.AddAccessoryWindowControl{T}"/> or results will be undefined.</typeparam>
    /// <typeparam name="TVal">Type of the control's value.</typeparam>
    public class AccessoryControlWrapper<T, TVal> where T : BaseEditableGuiEntry<TVal>
    {
        public AccessoryControlWrapper(T control)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));

            Control = control;
            _defaultValue = control.Value;

            control.ValueChanged.Subscribe(val => SetValue(AccessoriesApi.SelectedMakerAccSlot, val));

            AccessoriesApi.SelectedMakerAccSlotChanged += OnSelectedMakerAccSlotChanged;
        }

        private bool _changingValue;

        private void OnSelectedMakerAccSlotChanged(object o, AccessorySlotChangeEventArgs accessorySlotChangeEventArgs)
        {
            if (Control == null) return;

            _changingValue = true;
            Control.Value = GetValue(accessorySlotChangeEventArgs.SlotIndex);
            _changingValue = false;

            VisibleIndexChanged?.Invoke(o, accessorySlotChangeEventArgs);
        }

        /// <summary>
        /// The wrapped control.
        /// </summary>
        public T Control { get; }

        /// <summary>
        /// Get value of the control for the specified accessory.
        /// </summary>
        public TVal GetValue(int accessoryIndex)
        {
            CheckIndexRange(accessoryIndex);

            if (_values.TryGetValue(accessoryIndex, out var result))
                return result;
            return _defaultValue;
        }

        /// <summary>
        /// Get value of the control for the currently selected accessory.
        /// </summary>
        public TVal GetSelectedValue()
        {
            return GetValue(CurrentlySelectedIndex);
        }

        /// <summary>
        /// Set value of the control for the specified accessory.
        /// </summary>
        public void SetValue(int accessoryIndex, TVal value)
        {
            CheckIndexRange(accessoryIndex);

            _values[accessoryIndex] = value;

            if (!_changingValue)
                ValueChanged?.Invoke(this, new AccessoryControlValueChangedEventArgs<TVal>(value, CurrentlySelectedIndex));
        }

        /// <summary>
        /// Set value of the control for the currently selected accessory.
        /// </summary>
        public void SetSelectedValue(TVal value)
        {
            SetValue(CurrentlySelectedIndex, value);
        }

        /// <summary>
        /// Index of the currently selected accessory.
        /// </summary>
        public int CurrentlySelectedIndex => AccessoriesApi.SelectedMakerAccSlot;
        
        private readonly TVal _defaultValue;

        private readonly Dictionary<int, TVal> _values = new Dictionary<int, TVal>();

        /// <summary>
        /// Fired when
        /// </summary>
        public event EventHandler<AccessoryControlValueChangedEventArgs<TVal>> ValueChanged;

        /// <summary>
        /// Fired when the currently visible accessory was changed by the user clicking on one of the slots.
        /// </summary>
        public event EventHandler<AccessorySlotChangeEventArgs> VisibleIndexChanged;

        private static void CheckIndexRange(int accessoryIndex)
        {
            if (accessoryIndex < 0 || accessoryIndex >= AccessoriesApi.GetCvsAccessoryCount())
                throw new IndexOutOfRangeException("accessoryIndex has to be between 0 and AccessoriesApi.GetCvsAccessoryCount() - 1");
        }
    }
}