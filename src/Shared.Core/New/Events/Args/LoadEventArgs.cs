using System;

namespace ModdingAPI
{
    public sealed class LoadEventArgs<T> : EventArgs
    {
        public T LoadedItem { get; }
        public bool MaintainState { get; }

        public LoadEventArgs(T loadedItem, bool maintainState)
        {
            LoadedItem = loadedItem;
            MaintainState = maintainState;
        }
    }
}