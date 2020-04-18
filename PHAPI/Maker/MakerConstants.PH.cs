using UnityEngine;

namespace KKAPI.Maker
{
    /// <summary>
    /// Useful values from the character maker. Mostly built-in categories for use with registering custom controls.
    /// </summary>
    public static class MakerConstants
    {
        /// <summary>
        /// Default text color for maker controls.
        /// </summary>
        public static readonly Color DefaultControlTextColor = Color.white;

#pragma warning disable 1591       
        public static class Hair
        {
            public static string CategoryName { get; } = "Hair";
            public static MakerCategory Set { get; } = new MakerCategory("Hair", "Set", 10);
            public static MakerCategory Back { get; } = new MakerCategory("Hair", "Back", 20);
            public static MakerCategory Side { get; } = new MakerCategory("Hair", "Side", 30);
            public static MakerCategory Front { get; } = new MakerCategory("Hair", "Front", 40);
        }

        public static class Face
        {
            public static string CategoryName { get; } = "Face";
            public static MakerCategory General { get; } = new MakerCategory("Face", "General", 10);
            public static MakerCategory Ear { get; } = new MakerCategory("Face", "Ear", 20);
            public static MakerCategory Eyebrow { get; } = new MakerCategory("Face", "Eyebrow", 30);
            public static MakerCategory Eyelash { get; } = new MakerCategory("Face", "Eyelash", 40);
            public static MakerCategory Orbita { get; } = new MakerCategory("Face", "Orbita", 50);
            public static MakerCategory Eye { get; } = new MakerCategory("Face", "Eye", 60);
            public static MakerCategory Nose { get; } = new MakerCategory("Face", "Nose", 70);
            public static MakerCategory Cheek { get; } = new MakerCategory("Face", "Cheek", 80);
            public static MakerCategory Mouth { get; } = new MakerCategory("Face", "Mouth", 90);
            public static MakerCategory Chin { get; } = new MakerCategory("Face", "Chin", 100);
            public static MakerCategory Mole { get; } = new MakerCategory("Face", "Mole", 110);
            public static MakerCategory Makeup { get; } = new MakerCategory("Face", "Makeup", 120);
            public static MakerCategory Tattoo { get; } = new MakerCategory("Face", "Tattoo", 130);
            public static MakerCategory Beard { get; } = new MakerCategory("Face", "Beard", 140);
        }

        public static class Body
        {
            public static string CategoryName { get; } = "Body";
            public static MakerCategory General { get; } = new MakerCategory("Body", "General", 10);
            public static MakerCategory Bust { get; } = new MakerCategory("Body", "Bust", 20);
            public static MakerCategory Upper { get; } = new MakerCategory("Body", "Upper", 30);
            public static MakerCategory Lower { get; } = new MakerCategory("Body", "Lower", 40);
            public static MakerCategory Arm { get; } = new MakerCategory("Body", "Arm", 50);
            public static MakerCategory Leg { get; } = new MakerCategory("Body", "Leg", 60);
            public static MakerCategory Nail { get; } = new MakerCategory("Body", "Nail", 70);
            public static MakerCategory UnderHair { get; } = new MakerCategory("Body", "UnderHair", 80);
            public static MakerCategory Sunburn { get; } = new MakerCategory("Body", "Sunburn", 90);
            public static MakerCategory Tattoo { get; } = new MakerCategory("Body", "Tattoo", 100);
        }

        public static class Wear
        {
            public static string CategoryName { get; } = "Wear";
            public static MakerCategory SwimTops { get; } = new MakerCategory("Wear", "Swim Tops", 10);
            public static MakerCategory SwimBottoms { get; } = new MakerCategory("Wear", "Swim Bottoms", 20);
            public static MakerCategory SwimWear { get; } = new MakerCategory("Wear", "Swim Wear", 30);
            public static MakerCategory Tops { get; } = new MakerCategory("Wear", "Tops", 40);
            public static MakerCategory Bottoms { get; } = new MakerCategory("Wear", "Bottoms", 50);
            public static MakerCategory Bra { get; } = new MakerCategory("Wear", "Bra", 60);
            public static MakerCategory Shorts { get; } = new MakerCategory("Wear", "Shorts", 70);
            public static MakerCategory Glove { get; } = new MakerCategory("Wear", "Glove", 80);
            public static MakerCategory Panst { get; } = new MakerCategory("Wear", "Panst", 90);
            public static MakerCategory Socks { get; } = new MakerCategory("Wear", "Socks", 100);
            public static MakerCategory Shoes { get; } = new MakerCategory("Wear", "Shoes", 110);
        }

        public static class Accessory
        {
            public static string CategoryName { get; } = "Accessory";
            //public static MakerCategory Slot01 { get; } = new MakerCategory("Accessory", "Slot01", 10);
            //public static MakerCategory Slot02 { get; } = new MakerCategory("Accessory", "Slot02", 20);
            //public static MakerCategory Slot03 { get; } = new MakerCategory("Accessory", "Slot03", 30);
            //public static MakerCategory Slot04 { get; } = new MakerCategory("Accessory", "Slot04", 40);
            //public static MakerCategory Slot05 { get; } = new MakerCategory("Accessory", "Slot05", 50);
            //public static MakerCategory Slot06 { get; } = new MakerCategory("Accessory", "Slot06", 60);
            //public static MakerCategory Slot07 { get; } = new MakerCategory("Accessory", "Slot07", 70);
            //public static MakerCategory Slot08 { get; } = new MakerCategory("Accessory", "Slot08", 80);
            //public static MakerCategory Slot09 { get; } = new MakerCategory("Accessory", "Slot09", 90);
            //public static MakerCategory Slot10 { get; } = new MakerCategory("Accessory", "Slot10", 100);
        }

        public static class File
        {
            public static string CategoryName { get; } = "File";
            public static MakerCategory CharaCapture { get; } = new MakerCategory("File", "CharaCapture", 10);
            public static MakerCategory CharaListUI { get; } = new MakerCategory("File", "CharaListUI", 20);
            public static MakerCategory EquipListUI { get; } = new MakerCategory("File", "EquipListUI", 30);
        }
#pragma warning restore 1591
    }
}
