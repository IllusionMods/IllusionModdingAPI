namespace KKAPI.Maker
{
    /// <summary>
    /// Event fired when character maker is starting and plugins are given an opportunity to register custom categories and controls
    /// </summary>
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