using System;
using Studio;
using UniRx;
using UnityEngine;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Base of custom controls created under CurrentState category
    /// </summary>
    public abstract class CurrentStateCategorySubItemBase
    {
        /// <summary>
        /// Create a new custom CurrentState control
        /// </summary>
        /// <param name="name">Name of the setting displayed on the left</param>
        protected CurrentStateCategorySubItemBase(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            Visible = new BehaviorSubject<bool>(true);
            Visible.Subscribe(
                b =>
                {
                    if (RootGameObject != null)
                        RootGameObject.SetActive(b);
                });
        }

        /// <summary>
        /// Name of the setting, displayed to the left
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Fired when API wants to create the control. Should return the control's root GameObject
        /// </summary>
        /// <param name="categoryObject">Parent object of the control to be created</param>
        protected abstract GameObject CreateItem(GameObject categoryObject);

        internal void CreateItemInt(GameObject categoryObject)
        {
            if (categoryObject == null) throw new ArgumentNullException(nameof(categoryObject));
            if (!StudioAPI.StudioLoaded) throw new InvalidOperationException("Called before studio was loaded");

            if (Created) return;

            var rootGameObject = CreateItem(categoryObject);
            if (rootGameObject == null) throw new ArgumentException("CreateItem has to return a GameObject, check its overload in " + GetType().FullName);
            RootGameObject = rootGameObject;

            rootGameObject.SetActive(Visible.Value);
        }

        /// <summary>
        /// Fired when currently selected character changes and the control need to be updated
        /// </summary>
        /// <param name="ociChar">Newly selected character</param>
        protected internal abstract void OnUpdateInfo(OCIChar ociChar);

        /// <summary>
        /// The control's root gameobject. null if the control was not created yet.
        /// </summary>
        public GameObject RootGameObject { get; private set; }

        /// <summary>
        /// The control was created and still exists.
        /// </summary>
        public bool Created => RootGameObject != null;

        /// <summary>
        /// The control is visible to the user (usually the same as it's GameObject being active).
        /// </summary>
        public BehaviorSubject<bool> Visible { get; }
    }
}
