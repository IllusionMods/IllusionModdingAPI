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
    /// <typeparam name="T">Type of the control to be wrapped. The control has to be added by using <see cref="MakerAPI.AddAccessoryWindowControl{T}"/> or results will be undefined.</typeparam>
    /// <typeparam name="TVal">Type of the control's value.</typeparam>
    public class AccessoryControlWrapper<T, TVal> where T : BaseEditableGuiEntry<TVal>
    {
        /// <summary>
        /// Create a new wrapper.
        /// </summary>
        /// <param name="control">Control to be wrapped. The control has to be added by using <see cref="MakerAPI.AddAccessoryWindowControl{T}"/> or results will be undefined.</param>
        public AccessoryControlWrapper(T control)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));

            Control = control;
            _defaultValue = control.Value;

            control.ValueChanged.Subscribe(val => SetValue(AccessoriesApi.SelectedMakerAccSlot, val));

            AccessoriesApi.SelectedMakerAccSlotChanged += OnSelectedMakerAccSlotChanged;
            AccessoriesApi.AccessoryKindChanged += OnAccessoryKindChanged;
#if KK
            AccessoriesApi.AccessoriesCopied += OnAccessoriesCopied;
#endif
            AccessoriesApi.AccessoryTransferred += OnAccessoryTransferred;
        }

        private void OnAccessoryTransferred(object sender, AccessoryTransferEventArgs e)
        {
            if (CheckDisposed()) return;
            AccessoryTransferred?.Invoke(sender, e);
        }

#if KK
        private void OnAccessoriesCopied(object sender, AccessoryCopyEventArgs e)
        {
            if (CheckDisposed()) return;
            AccessoriesCopied?.Invoke(sender, e);
        }
#endif

        private void OnAccessoryKindChanged(object sender, AccessorySlotEventArgs accessorySlotEventArgs)
        {
            if (CheckDisposed()) return;
            AccessoryKindChanged?.Invoke(sender, accessorySlotEventArgs);
        }

        private void OnSelectedMakerAccSlotChanged(object o, AccessorySlotEventArgs accessorySlotEventArgs)
        {
            if (CheckDisposed()) return;

            Control.SetValue(GetValue(accessorySlotEventArgs.SlotIndex), false);

            VisibleIndexChanged?.Invoke(o, accessorySlotEventArgs);
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
            CheckDisposedThrow();
            CheckIndexRangeThrow(accessoryIndex);

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
        /// <param name="accessoryIndex">Index of the accessory to set the value for</param>
        /// <param name="value">Value to set</param>
        public void SetValue(int accessoryIndex, TVal value)
        {
            SetValue(accessoryIndex, value, true);
        }

        /// <summary>
        /// Set value of the control for the specified accessory.
        /// </summary>
        /// <param name="accessoryIndex">Index of the accessory to set the value for</param>
        /// <param name="value">Value to set</param>
        /// <param name="fireEvents">Fire the <see cref="ValueChanged"/> event if the value actually changed.</param>
        public void SetValue(int accessoryIndex, TVal value, bool fireEvents)
        {
            CheckDisposedThrow();
            CheckIndexRangeThrow(accessoryIndex);

            _values[accessoryIndex] = value;

            if (AccessoriesApi.SelectedMakerAccSlot == accessoryIndex)
                Control.SetValue(value, false);

            if (fireEvents)
                ValueChanged?.Invoke(this, new AccessoryWindowControlValueChangedEventArgs<TVal>(value, CurrentlySelectedIndex));
        }

        /// <summary>
        /// Set value of the control for the currently selected accessory.
        /// </summary>
        /// <param name="value">Value to set</param>
        public void SetSelectedValue(TVal value)
        {
            SetSelectedValue(value, false);
        }

        /// <summary>
        /// Set value of the control for the currently selected accessory.
        /// </summary>
        /// <param name="value">Value to set</param>
        /// <param name="fireEvents">Fire the <see cref="ValueChanged"/> event if the value actually changed.</param>
        public void SetSelectedValue(TVal value, bool fireEvents)
        {
            SetValue(CurrentlySelectedIndex, value, fireEvents);
        }

        /// <summary>
        /// Index of the currently selected accessory.
        /// </summary>
        public int CurrentlySelectedIndex => AccessoriesApi.SelectedMakerAccSlot;

        private readonly TVal _defaultValue;

        private readonly Dictionary<int, TVal> _values = new Dictionary<int, TVal>();
        private bool _isDisposed;

        /// <summary>
        /// Fired when the value of this control changes for any of the accessories.
        /// </summary>
        public event EventHandler<AccessoryWindowControlValueChangedEventArgs<TVal>> ValueChanged;

        /// <summary>
        /// Fired when the currently visible accessory was changed by the user clicking on one of the slots.
        /// </summary>
        public event EventHandler<AccessorySlotEventArgs> VisibleIndexChanged;

        /// <summary>
        /// Fires when user selects a different accessory in the accessory window.
        /// </summary>
        public event EventHandler<AccessorySlotEventArgs> AccessoryKindChanged;

#if KK
        /// <summary>
        /// Fires after user copies accessories between coordinates by using the Copy window.
        /// </summary>
        public static event EventHandler<AccessoryCopyEventArgs> AccessoriesCopied;
#endif

        /// <summary>
        /// Fires after user copies an accessory within a single coordinate by using the Transfer window.
        /// </summary>
        public static event EventHandler<AccessoryTransferEventArgs> AccessoryTransferred;

        private static void CheckIndexRangeThrow(int accessoryIndex)
        {
            if (accessoryIndex < 0 || accessoryIndex >= AccessoriesApi.GetCvsAccessoryCount())
                throw new IndexOutOfRangeException("accessoryIndex has to be between 0 and AccessoriesApi.GetCvsAccessoryCount() - 1");
        }

        private void CheckDisposedThrow()
        {
            if (CheckDisposed())
                throw new ObjectDisposedException("The control has been disposed. Controls only live in Maker, and you need to create a new one every time maker starts");
        }

        private bool CheckDisposed()
        {
            if (_isDisposed) return true;

            if (Control.IsDisposed)
            {
                _isDisposed = true;

                _values.Clear();

                ValueChanged = null;
                VisibleIndexChanged = null;
                AccessoryKindChanged = null;
                AccessoryTransferred = null;

                AccessoriesApi.SelectedMakerAccSlotChanged -= OnSelectedMakerAccSlotChanged;
                AccessoriesApi.AccessoryKindChanged -= OnAccessoryKindChanged;
                AccessoriesApi.AccessoryTransferred -= OnAccessoryTransferred;

#if KK
                AccessoriesCopied = null;
                AccessoriesApi.AccessoriesCopied -= OnAccessoriesCopied;
#endif

                return true;
            }

            return false;
        }

        /// <summary>
        /// If true, the control has been disposed and can no longer be used, likely because the character maker exited.
        /// A new control has to be created to be used again.
        /// </summary>
        public bool IsDisposed => CheckDisposed();
    }
}