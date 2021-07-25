using System;

namespace ModdingAPI
{
    public sealed class NewValueEventArgs<T> : EventArgs
    {
        public T NewValue { get; }

        public NewValueEventArgs(T newValue)
        {
            NewValue = newValue;
        }
    }
}