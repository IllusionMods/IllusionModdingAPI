using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKAPI.Utilities
{
    public delegate void TabletEvent(Packet[] packet);

    public class TabletManager
    {
        private static TabletManager _instance;
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
                            _instance = new TabletManager();
                    }
                }
                return _instance;
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
            if (!IsInitialized && !_tablet.Initialize())
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
                    _timer?.Dispose();
                    _timer = null;
                }
                _tablet.Dispose();
            }
        }

        private void PollTablet(object state)
        {
            if (_tablet.IsInitialized && !Application.isFocused)
            {
                _tablet.Dispose();
                return;
            }

            if (!_tablet.IsInitialized && Application.isFocused)
            {
                _tablet.Initialize();
            }

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

        public uint MaxPressure => _tablet.MaxPressure;
        public bool IsInitialized => _tablet.IsInitialized;
    }
}