using System;
using Studio;
using UnityEngine;

namespace MakerAPI.Studio
{
    public abstract class CurrentStateCategorySubItemBase
    {
        protected CurrentStateCategorySubItemBase(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
        protected internal abstract void CreateItem(GameObject categoryObject);
        protected internal abstract void UpdateInfo(OCIChar ociChar);
    }
}
