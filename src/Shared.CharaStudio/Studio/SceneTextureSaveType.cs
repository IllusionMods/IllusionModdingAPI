namespace KKAPI.Studio
{
    /// <summary>
    /// Options for the type of texture saving that plugins should use in Studio.
    /// </summary>
    public enum SceneTextureSaveType
    {
        /// <summary>
        /// Textures should be bundled with the scene.
        /// </summary>
        Bundled = 0,
        /// <summary>
        /// Textures should be deduped between different Chara and SceneControllers.
        /// </summary>
        Deduped = 1,
        /// <summary>
        /// Textures should be saved separately from the scene in a local folder.
        /// </summary>
        Local = 2,
    }
}
