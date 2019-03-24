namespace KKAPI.Studio.SaveLoad
{
    /// <summary>
    /// Scene load/change operations
    /// </summary>
    public enum SceneOperationKind
    {
        /// <summary>
        /// Scene is being loaded and will replace what's currently loaded.
        /// </summary>
        Load,
        /// <summary>
        /// Scene is being loaded and will be added to what's currently loaded. 
        /// <remarks>IDs in the scene will be different from the IDs in the file of the scene being imported, 
        /// use <see cref="SceneCustomFunctionController"/> to get IDs from the scene file.</remarks>
        /// </summary>
        Import,
        /// <summary>
        /// Scene is being cleared of all state (by default, only user clicking the "Reset" button can trigger this).
        /// This is not triggered when studio starts.
        /// </summary>
        Clear
    }
}
