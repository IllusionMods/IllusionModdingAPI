using System;

namespace MakerAPI
{
    public class RegisterCustomControlsEvent : EventArgs
    {
        /// <summary>
        /// Add custom controls. If you want to use custom sub categories, register them by calling AddSubCategory.
        /// </summary>
        public T AddControl<T>(T control) where T : MakerGuiEntryBase
        {
            return MakerAPI.Instance.AddControl(control);
        }
        
        public MakerAPI Api => MakerAPI.Instance;
    }
}