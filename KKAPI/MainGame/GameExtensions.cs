namespace KKAPI.MainGame
{
    /// <summary>
    /// Extensions useful in the main game
    /// </summary>
    public static class GameExtensions
    {
        /// <summary>
        /// Returns true if the H scene is peeping in the shower.
        /// Use <see cref="HFlag.mode"/> to get info on what mode the H scene is in.
        /// </summary>
        public static bool IsShowerPeeping(this HFlag hFlag)
        {
            return hFlag.mode == HFlag.EMode.peeping && hFlag.nowAnimationInfo.nameAnimation == "シャワー覗き";
        }
    }
}
