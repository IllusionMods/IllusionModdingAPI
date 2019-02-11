namespace MakerAPI
{
    public class RegisterSubCategoriesEvent : RegisterCustomControlsEvent
    {
        public RegisterSubCategoriesEvent(MakerAPI makerApi) : base(makerApi) { }

        /// <summary>
        /// Add custom sub categories. They need to be added before maker starts loading,
        /// or in the RegisterCustomSubCategories event.
        /// </summary>
        public void AddSubCategory(MakerCategory category)
        {
            Api.AddSubCategory(category);
        }
    }
}