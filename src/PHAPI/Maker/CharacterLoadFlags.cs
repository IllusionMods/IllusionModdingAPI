#pragma warning disable 1591
namespace KKAPI.Maker
{
    /// <summary>
    /// Specifies which parts of the character will be loaded when loading a card in character maker.
    /// (It's the toggles at the bottom of load window) Only includes stock toggles.
    /// </summary>
    public sealed class CharacterLoadFlags
    {
        public bool Clothes;
        public bool Face;
        public bool Hair;
        public bool Body;
        //public bool Parameters;
        public bool Accessories;
    }

    /// <summary>
    /// Specifies which parts of the coordinate will be loaded when loading a clothing card in character maker.
    /// </summary>
    public sealed class CoordinateLoadFlags
    {
        public bool Clothes;
        public bool Accessories;
    }
}
