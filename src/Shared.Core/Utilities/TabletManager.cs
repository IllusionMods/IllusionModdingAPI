using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Represents a method that handles tablet input events by processing an array of data packets.
    /// </summary>
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
        /// Registers a handler to receive tablet event notifications.
        /// </summary>
        /// <param name="handler">The delegate to subscribe. If <see langword="null"/>, no action is taken.</param>
        /// <remarks>
        /// <para>
        /// When the first subscriber is added, tablet polling is automatically started.
        /// </para>
        /// <para>
        /// This method is thread-safe. Duplicate subscriptions are allowed and will result
        /// in the handler being invoked multiple times per event.
        /// </para>
        /// </remarks>
        /// <seealso cref="Unsubscribe"/>
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
        /// Removes a handler from tablet event notifications.
        /// </summary>
        /// <param name="handler">The delegate to unsubscribe. If <see langword="null"/>, no action is taken.</param>
        /// <remarks>
        /// <para>
        /// When the last subscriber is removed, tablet polling is automatically stopped to
        /// conserve resources.
        /// </para>
        /// <para>
        /// This method is thread-safe.
        /// </para>
        /// </remarks>
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
                if (!_tablet.IsInitialized && !_tablet.Initialize(new lcOut(0, 0, 5000, 5000)))
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
                        UnityEngine.Debug.LogError($"Subscriber crash! Removing subscriber {subscriber?.GetType().FullName} because of exception: {e}");
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
                _tablet.Initialize(new lcOut(0, 0, 5000, 5000));
            }
        }

        /// <inheritdoc cref="Tablet.MaxPressure"/>
        public static uint MaxPressure => instance._tablet.MaxPressure;

        /// <inheritdoc cref="Tablet.IsInitialized"/>
        public static bool IsAvailable => instance._tablet.IsInitialized;
    }
}