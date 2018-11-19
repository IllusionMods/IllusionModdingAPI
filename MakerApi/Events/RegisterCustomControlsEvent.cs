using System;

namespace MakerAPI
{
    public class RegisterCustomControlsEvent : EventArgs
    {
        private readonly MakerAPI _makerApi;

        public RegisterCustomControlsEvent(MakerAPI makerApi)
        {
            _makerApi = makerApi;
        }

        /// <summary>
        /// Add custom controls. If you want to use custom sub categories, register them by calling AddSubCategory.
        /// </summary>
        public T AddControl<T>(T control) where T : BaseGuiEntry
        {
            return _makerApi.AddControl(control);
        }
        
        public MakerAPI Api => _makerApi;
    }
}