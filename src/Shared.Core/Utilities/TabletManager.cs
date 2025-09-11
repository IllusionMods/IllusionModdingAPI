using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKAPI.Utilities
{
    public delegate void TabletEvent(Packet[] packet);

    public class TabletManager : MonoBehaviour
    {
        private static GameObject _instance;
        private static readonly object _lock = new object();
        private readonly Tablet _tablet = new Tablet();
        private readonly List<TabletEvent> _subscribers = new List<TabletEvent>();
        private bool _isPolling;

        private TabletManager() { }

        public static TabletManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new GameObject("TabletManager");
                            _instance.hideFlags = HideFlags.HideAndDontSave;
                            _instance.AddComponent<TabletManager>();
                            DontDestroyOnLoad(_instance);
                        }
                    }
                }
                return _instance.GetComponent<TabletManager>();
            }
        }

        public void Subscribe(TabletEvent handler)
        {
            lock (_lock)
            {
                _subscribers.Add(handler);
                if (!_isPolling && _subscribers.Count > 0)
                    StartPolling();
            }
        }

        public void Unsubscribe(TabletEvent handler)
        {
            lock (_lock)
            {
                _subscribers.Remove(handler);
                if (_subscribers.Count == 0)
                    StopPolling();
            }
        }

        private System.Threading.Timer _timer;

        private void StartPolling()
        {
            if (!_tablet.IsInitialized && !_tablet.Initialize())
                return;

            _isPolling = true;
            _timer = new System.Threading.Timer(PollTablet, null, 0, 16);
        }

        private void StopPolling()
        {
            lock (_lock)
            {
                _isPolling = false;
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
                _tablet.Dispose();
            }
        }

        private void PollTablet(object state)
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

        public uint MaxPressure => _tablet.MaxPressure;
    }
}