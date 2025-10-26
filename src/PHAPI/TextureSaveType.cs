namespace KKAPI
{
    /// <summary>
    /// Options for the type of texture saving that plugins should use.
    /// </summary>
    public enum TextureSaveType
    {
        /// <summary>
        /// Textures should be bundled with the card / scene.
        /// </summary>
        Bundled = 0,
        /// <summary>
        /// Textures should be deduped between different Chara and SceneControllers.
        /// Should only be used for Studio scenes.
        /// </summary>
        Deduped = 1,
        /// <summary>
        /// Textures should be saved separately from the card in a local folder.
        /// </summary>
        Local = 2,
    }
}
