using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKAPI.Utilities
{
    public delegate void TabletEvent(Packet[] packets);

    public class TabletManager : MonoBehaviour
    {
        private static GameObject _instance;
        private static readonly object _lock = new object();
        private readonly Tablet _tablet = new Tablet();
        private readonly List<TabletEvent> _subscribers = new List<TabletEvent>();
        private bool _isPolling;

        private TabletManager() { }

        private static TabletManager instance
        {
            get
            {
                if (_instance != null)
                    return _instance.GetComponent<TabletManager>();
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GameObject("TabletManager")
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        };
                        _instance.AddComponent<TabletManager>();
                        DontDestroyOnLoad(_instance);
                    }
                }
                return _instance.GetComponent<TabletManager>();
            }
        }

        public static void Subscribe(TabletEvent handler)
        {
            lock (_lock)
            {
                instance._subscribers.Add(handler);
                if (!instance._isPolling && instance._subscribers.Count > 0)
                    instance.StartPolling();
            }
        }

        public static void Unsubscribe(TabletEvent handler)
        {
            lock (_lock)
            {
                instance._subscribers.Remove(handler);
                if (instance._subscribers.Count == 0)
                    instance.StopPolling();
            }
        }

        private void FixedUpdate()
        {
            if (_tablet.IsInitialized && _isPolling)
            {
                PollTablet();
            }
        }

        private void StartPolling()
        {
            lock (_lock)
            {
                if (!_tablet.IsInitialized && !_tablet.Initialize())
                    return;
                _isPolling = true;
            }
        }

        private void StopPolling()
        {
            lock (_lock)
            {
                _isPolling = false;
                _tablet.Dispose();
            }
        }

        private void PollTablet()
        {
            Packet[] packet;
            if (!_tablet.QueryMulti(out packet))
                return;
            lock (_lock)
            {
                foreach (var subscriber in _subscribers)
                {
                    try
                    {
                        subscriber(packet);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Unsubscribe(subscriber);
                        break;
                    }
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_tablet.IsInitialized && !hasFocus)
            {
                _tablet.Dispose();
                return;
            }

            if (!_tablet.IsInitialized && hasFocus)
            {
                _tablet.Initialize();
            }
        }

        public static uint MaxPressure => instance._tablet.MaxPressure;
    }
}