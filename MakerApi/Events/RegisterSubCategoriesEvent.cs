namespace MakerAPI
{
    public class RegisterSubCategoriesEvent : RegisterCustomControlsEvent
    {
        /// <summary>
        /// Add custom sub categories. They need to be added before maker starts loading,
        /// or in the RegisterCustomSubCategories event.
        /// </summary>
        public void AddSubCategory(MakerCategory category)
        {
            MakerAPI.Instance.AddSubCategory(category);
        }
    }
}