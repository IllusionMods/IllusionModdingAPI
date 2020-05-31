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
        public static readonly Color DefaultControlTextColor = new Color(0.922f, 0.886f, 0.843f);

#pragma warning disable 1591
        public static class Face
        {
            public static string CategoryName { get; } = "Face";
            public static MakerCategory FaceType { get; } = new MakerCategory("Face", "FaceType");
            public static MakerCategory All { get; } = new MakerCategory("Face", "All");
            public static MakerCategory Chin { get; } = new MakerCategory("Face", "Chin");
            public static MakerCategory Cheek { get; } = new MakerCategory("Face", "Cheek");
            public static MakerCategory Eyebrow { get; } = new MakerCategory("Face", "Eyebrow");
            public static MakerCategory Eyes { get; } = new MakerCategory("Face", "Eyes");
            public static MakerCategory Nose { get; } = new MakerCategory("Face", "Nose");
            public static MakerCategory Mouth { get; } = new MakerCategory("Face", "Mouth");
            public static MakerCategory Ear { get; } = new MakerCategory("Face", "Ear");
            public static MakerCategory Mole { get; } = new MakerCategory("Face", "Mole");
            public static MakerCategory EyeL { get; } = new MakerCategory("Face", "EyeL");
            public static MakerCategory EyeR { get; } = new MakerCategory("Face", "EyeR");
            public static MakerCategory Etc { get; } = new MakerCategory("Face", "Etc");
            public static MakerCategory HL { get; } = new MakerCategory("Face", "HL");
            //public static MakerCategory Eyebrow {get;} = new MakerCategory("Face", "Eyebrow");
            public static MakerCategory Eyelashes { get; } = new MakerCategory("Face", "Eyelashes");
            public static MakerCategory Eyeshadow { get; } = new MakerCategory("Face", "Eyeshadow");
            //public static MakerCategory Cheek {get;} = new MakerCategory("Face", "Cheek");
            public static MakerCategory Lip { get; } = new MakerCategory("Face", "Lip");
            public static MakerCategory Paint01 { get; } = new MakerCategory("Face", "Paint01");
            public static MakerCategory Paint02 { get; } = new MakerCategory("Face", "Paint02");
        }

        public static class Body
        {
            public static string CategoryName { get; } = "Body";
            public static MakerCategory All { get; } = new MakerCategory("Body", "All");
            public static MakerCategory Breast { get; } = new MakerCategory("Body", "Breast");
            public static MakerCategory Upper { get; } = new MakerCategory("Body", "Upper");
            public static MakerCategory Lower { get; } = new MakerCategory("Body", "Lower");
            public static MakerCategory Arm { get; } = new MakerCategory("Body", "Arm");
            public static MakerCategory Leg { get; } = new MakerCategory("Body", "Leg");
            public static MakerCategory SkinType { get; } = new MakerCategory("Body", "SkinType");
            public static MakerCategory Sunburn { get; } = new MakerCategory("Body", "Sunburn");
            public static MakerCategory Nip { get; } = new MakerCategory("Body", "Nip");
            public static MakerCategory Underhair { get; } = new MakerCategory("Body", "Underhair");
            public static MakerCategory Nails { get; } = new MakerCategory("Body", "Nails");
            public static MakerCategory Paint01 { get; } = new MakerCategory("Body", "Paint01");
            public static MakerCategory Paint02 { get; } = new MakerCategory("Body", "Paint02");
        }

        public static class Hair
        {
            public static string CategoryName { get; } = "Hair";
        }

        public static class Clothes
        {
            public static string CategoryName { get; } = "Clothes";
        }

        public static class Parameter
        {
            public static string CategoryName { get; } = "Option";
            public static MakerCategory Name { get; } = new MakerCategory("Option", "Name");
            public static MakerCategory Type { get; } = new MakerCategory("Option", "Type");
            public static MakerCategory Status { get; } = new MakerCategory("Option", "Status");
        }
#pragma warning restore 1591
    }
}
