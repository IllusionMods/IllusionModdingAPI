namespace KKAPI
{
    /// <summary>
    /// Current mode the game is in.
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// Anywhere else, including main menu.
        /// </summary>
        Unknown,
        /// <summary>
        /// Inside character maker started from main menu or from class roster.
        /// </summary>
        Maker,
        /// <summary>
        /// Inside ChaStudio.exe
        /// </summary>
        Studio,
        /// <summary>
        /// Refers to anything after starting a new game from title screen, or loading a saved game.
        /// This means this includes story scenes, night menu, roaming around and h scenes inside story mode.
        /// </summary>
        MainGame
    }
}