namespace KKAPI.Maker
{
    /// <summary>
    /// Options for the type of texture saving that plugins should use in Maker.
    /// </summary>
    public enum CharaTextureSaveType
    {
        /// <summary>
        /// Textures should be bundled with the card.
        /// </summary>
        Bundled = 0,
        /// <summary>
        /// Textures should be saved separately from the card in a local folder.
        /// </summary>
        Local = 2,
    }
}
