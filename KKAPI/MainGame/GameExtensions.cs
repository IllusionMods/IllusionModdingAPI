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

        /// <summary>
        /// Get the persisting heroine object that describes this character.
        /// Returns null if the heroine could not be found. Works only in the main game.
        /// </summary>
        public static SaveData.Heroine GetHeroine(this ChaControl chaControl)
        {
            return Manager.Game.Instance?.HeroineList?.Find(heroine => heroine.chaCtrl == chaControl);
        }

        /// <summary>
        /// Get the persisting heroine object that describes this character.
        /// Returns null if the heroine could not be found. Works only in the main game.
        /// </summary>
        public static SaveData.Heroine GetHeroine(this ChaFileControl chaFile)
        {
            return Manager.Game.Instance?.HeroineList?.Find(heroine => heroine.charFile == chaFile);
        }
    }
}
