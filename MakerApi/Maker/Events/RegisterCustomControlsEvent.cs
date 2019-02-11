using System;

namespace MakerAPI
{
    public class RegisterCustomControlsEvent : EventArgs
    {
        public RegisterCustomControlsEvent(MakerAPI makerApi)
        {
            Api = makerApi;
        }

        /// <summary>
        /// Add custom controls. If you want to use custom sub categories, register them by calling AddSubCategory.
        /// </summary>
        public T AddControl<T>(T control) where T : BaseGuiEntry
        {
            return Api.AddControl(control);
        }

        /// <summary>
        /// Add a toggle to the bottom of the "Load character" window that allows for partial loading of characters.
        /// </summary>
        public MakerLoadToggle AddLoadToggle(MakerLoadToggle toggle)
        {
            return MakerLoadToggle.AddLoadToggle(toggle);
        }

        public MakerCoordinateLoadToggle AddCoordinateLoadToggle(MakerCoordinateLoadToggle toggle)
        {
            return MakerCoordinateLoadToggle.AddLoadToggle(toggle);
        }

        public MakerAPI Api { get; }
    }
}