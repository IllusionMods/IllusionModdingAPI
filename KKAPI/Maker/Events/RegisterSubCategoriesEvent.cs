namespace KKAPI.Maker
{
    public class RegisterSubCategoriesEvent : RegisterCustomControlsEvent
    {
        /// <summary>
        /// Add custom sub categories. They need to be added before maker starts loading,
        /// or in the RegisterCustomSubCategories event.
        /// </summary>
        public void AddSubCategory(MakerCategory category)
        {
            MakerAPI.AddSubCategory(category);
        }
    }
}