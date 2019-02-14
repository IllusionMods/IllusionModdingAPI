namespace KKAPI.Maker
{
    /// <summary>
    /// Specifies a category inside character maker.
    /// </summary>
    public sealed class MakerCategory
    {
        private bool Equals(MakerCategory other)
        {
            return string.Equals(CategoryName, other.CategoryName) && string.Equals(SubCategoryName, other.SubCategoryName);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MakerCategory mc && Equals(mc);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((CategoryName != null ? CategoryName.GetHashCode() : 0) * 397) ^ (SubCategoryName != null ? SubCategoryName.GetHashCode() : 0);
            }
        }
        
        /// <summary>
        /// Make a new custom subcategory. 
        /// </summary>
        public MakerCategory(string categoryName, string subCategoryName, 
            int position = int.MaxValue, string displayName = null)
        {
            CategoryName = categoryName;
            SubCategoryName = subCategoryName;
            Position = position;
            DisplayName = displayName;
        }

        /// <summary>
        /// Main category gameObject name. Main categories are the square buttons at the top-left edge of the screen.
        /// They contain multiple subcategories (tabs on the left edge of the screen).
        /// </summary>
        public string CategoryName { get; }
        /// <summary>
        /// Sub category gameObject name. Sub categories are the named tabs on the left edge of the screen.
        /// They contain the actual controls (inside the window on the right of the tabs).
        /// </summary>
        public string SubCategoryName { get; }
        /// <summary>
        /// Numeric position of the subcategory.
        /// When making new subcategories you can set this value to be in-between stock subcategories.
        /// </summary>
        public int Position { get; }
        /// <summary>
        /// The text displayed on the subcategory tab on the left edge of the screen.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Get combined name for logging etc.
        /// </summary>
        public override string ToString()
        {
            return $"{CategoryName} / {SubCategoryName}";
        }
    }
}
