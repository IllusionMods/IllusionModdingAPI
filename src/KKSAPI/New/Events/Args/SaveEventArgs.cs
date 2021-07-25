using System;

namespace ModdingAPI
{
    public sealed class SaveEventArgs<T> : EventArgs
    {
        public T SavedItem { get; }

        public SaveEventArgs(T savedItem)
        {
            SavedItem = savedItem;
        }
    }
}