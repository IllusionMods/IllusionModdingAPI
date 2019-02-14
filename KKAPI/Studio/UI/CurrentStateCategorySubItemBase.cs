using System;
using Studio;
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
        }

        /// <summary>
        /// Name of the setting, displayed to the left
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Fired when API wants to create the control
        /// </summary>
        /// <param name="categoryObject">Parent object of the control to be created</param>
        protected internal abstract void CreateItem(GameObject categoryObject);

        /// <summary>
        /// Fired when currently selected character changes and the control need to be updated
        /// </summary>
        /// <param name="ociChar">Newly selected character</param>
        protected internal abstract void OnUpdateInfo(OCIChar ociChar);
    }
}
