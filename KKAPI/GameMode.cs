namespace KKAPI
{
    /// <summary>
    /// Current gameplay mode the game is in
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// Anywhere else, including main menu
        /// </summary>
        Unknown,
        /// <summary>
        /// Inside character maker (can be started from main menu or from class roster)
        /// </summary>
        Maker,
        /// <summary>
        /// Anywhere inside CharaStudio.exe
        /// </summary>
        Studio,
        /// <summary>
        /// Anywhere inside the main game.
        /// Includes everything after starting a new game from title screen and after loading a saved game.
        /// This means this includes story scenes, night menu, roaming around and h scenes inside story mode.
        /// This does not hoverwer include the character maker launched from within the class menu.
        /// </summary>
        MainGame
    }
}