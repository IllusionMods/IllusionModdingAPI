using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKAPI.Utilities
{
    public delegate void TabletEvent(Packet[] packets);

    /// <summary>
    /// Manages tablet input events and subscriptions.
    /// </summary>
    /// <remarks>
    /// The TabletManager class is responsible for overseeing the handling of tablet input events and enabling
    /// event subscriptions for external components. It ensures a singleton instance exists and facilitates
    /// subscribing and unsubscribing of handlers to process tablet data packets.
    /// </remarks>
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

        /// <summary>
        /// Subscribes a provided event handler to receive tablet input updates.
        /// </summary>
        /// <param name="handler">The event handler to subscribe. If null, no operation is performed.</param>
        public static void Subscribe(TabletEvent handler)
        {
            lock (_lock)
            {
                if (handler != null)
                {
                    instance._subscribers.Add(handler);
                }
                
                if (!instance._isPolling && instance._subscribers.Count > 0)
                    instance.StartPolling();
            }
        }

        /// <summary>
        /// Unsubscribes a previously registered event handler from receiving tablet input updates.
        /// </summary>
        /// <param name="handler">The event handler to unsubscribe. If null, no operation is performed.</param>
        public static void Unsubscribe(TabletEvent handler)
        {
            lock (_lock)
            {
                if (handler != null)
                {
                    instance._subscribers.Remove(handler);
                }
                
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

        /// <summary>
        /// Gets the maximum pressure value supported by the tablet device.
        /// This value indicates the upper limit of pressure sensitivity that the tablet can detect.
        /// </summary>
        public static uint MaxPressure => instance._tablet.MaxPressure;
    }
}