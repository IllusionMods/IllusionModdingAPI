#if HS2 || AI
using System;
#endif
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Additions to the UniRx IObservable extension methods
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Get an observable that triggers on every OnGUI call on this gameObject
        /// </summary>
        public static IObservable<Unit> OnGUIAsObservable(this Component component) => component == null ? Observable.Empty<Unit>() : component.GetOrAddComponent<ObservableOnGUITrigger>().OnGUIAsObservable();
        ///<inheritdoc cref="OnGUIAsObservable(UnityEngine.Component)"/>
        public static IObservable<Unit> OnGUIAsObservable(this Transform transform) => transform == null ? Observable.Empty<Unit>() : transform.GetOrAddComponent<ObservableOnGUITrigger>().OnGUIAsObservable();
        ///<inheritdoc cref="OnGUIAsObservable(UnityEngine.Component)"/>
        public static IObservable<Unit> OnGUIAsObservable(this GameObject gameObject) => gameObject == null ? Observable.Empty<Unit>() : gameObject.GetOrAddComponent<ObservableOnGUITrigger>().OnGUIAsObservable();
    }

    /// <summary>
    /// Trigger component that implements <see cref="ObservableExtensions.OnGUIAsObservable(UnityEngine.Component)"/>
    /// </summary>
    [DisallowMultipleComponent]
    public class ObservableOnGUITrigger : ObservableTriggerBase
    {
        private void OnGUI()
        {
            _onGui?.OnNext(Unit.Default);
        }

        /// <summary>
        /// Get observable that triggers every time this component's OnGUI is called
        /// </summary>
        public IObservable<Unit> OnGUIAsObservable()
        {
            return _onGui ?? (_onGui = new Subject<Unit>());
        }

        /// <inheritdoc />
        protected override void RaiseOnCompletedOnDestroy()
        {
            _onGui?.OnCompleted();
        }

        private Subject<Unit> _onGui;
    }
}