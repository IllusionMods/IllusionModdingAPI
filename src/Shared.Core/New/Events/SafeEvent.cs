using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModdingAPI
{
    public class SafeEvent<T> where T : EventArgs
    {
        protected readonly List<EventHandler<T>> _handlers;
        public readonly ReadOnlyCollection<EventHandler<T>> Handlers;

        public SafeEvent()
        {
            _handlers = new List<EventHandler<T>>();
            Handlers = _handlers.AsReadOnly();
        }

        public void Add(EventHandler<T> handler)
        {
            _handlers.Add(handler);
        }

        public bool Remove(EventHandler<T> handler)
        {
            return _handlers.Remove(handler);
        }

        public void SafeInvoke(object source, T value)//todo internal?
        {
            for (var i = 0; i < _handlers.Count; i++)
            {
                try
                {
                    var handler = _handlers[i];
                    handler.Invoke(source, value);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }
    }

    public sealed class SafeEvent : SafeEvent<EventArgs>
    {
        public void SafeInvoke(object source)
        {
            SafeInvoke(source, EventArgs.Empty);
        }
    }
}