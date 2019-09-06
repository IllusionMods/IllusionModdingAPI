namespace KKAPI.Maker
{
    /// <summary>
    /// Useful values from the character maker. Mostly built-in categories for use with registering custom controls.
    /// </summary>
    public static class MakerConstants
    {
#pragma warning disable 1591
        public static class Face
        {
            public static string CategoryName => "Face";
            public static MakerCategory FaceType => new MakerCategory("Face", "FaceType");
            public static MakerCategory All => new MakerCategory("Face", "All");
            public static MakerCategory Chin => new MakerCategory("Face", "Chin");
            public static MakerCategory Cheek => new MakerCategory("Face", "Cheek");
            public static MakerCategory Eyebrow => new MakerCategory("Face", "Eyebrow");
            public static MakerCategory Eyes => new MakerCategory("Face", "Eyes");
            public static MakerCategory Nose => new MakerCategory("Face", "Nose");
            public static MakerCategory Mouth => new MakerCategory("Face", "Mouth");
            public static MakerCategory Ear => new MakerCategory("Face", "Ear");
            public static MakerCategory Mole => new MakerCategory("Face", "Mole");
            public static MakerCategory EyeL => new MakerCategory("Face", "EyeL");
            public static MakerCategory EyeR => new MakerCategory("Face", "EyeR");
            public static MakerCategory Etc => new MakerCategory("Face", "Etc");
            public static MakerCategory HL => new MakerCategory("Face", "HL");
            //public static MakerCategory Eyebrow => new MakerCategory("Face", "Eyebrow");
            public static MakerCategory Eyelashes => new MakerCategory("Face", "Eyelashes");
            public static MakerCategory Eyeshadow => new MakerCategory("Face", "Eyeshadow");
            //public static MakerCategory Cheek => new MakerCategory("Face", "Cheek");
            public static MakerCategory Lip => new MakerCategory("Face", "Lip");
            public static MakerCategory Paint01 => new MakerCategory("Face", "Paint01");
            public static MakerCategory Paint02 => new MakerCategory("Face", "Paint02");
        }

        public static class Body
        {
            public static string CategoryName => "Body";
            public static MakerCategory All => new MakerCategory("Body", "All");
            public static MakerCategory Breast => new MakerCategory("Body", "Breast");
            public static MakerCategory Upper => new MakerCategory("Body", "Upper");
            public static MakerCategory Lower => new MakerCategory("Body", "Lower");
            public static MakerCategory Arm => new MakerCategory("Body", "Arm");
            public static MakerCategory Leg => new MakerCategory("Body", "Leg");
            public static MakerCategory SkinType => new MakerCategory("Body", "SkinType");
            public static MakerCategory Sunburn => new MakerCategory("Body", "Sunburn");
            public static MakerCategory Nip => new MakerCategory("Body", "Nip");
            public static MakerCategory Underhair => new MakerCategory("Body", "Underhair");
            public static MakerCategory Nails => new MakerCategory("Body", "Nails");
            public static MakerCategory Paint01 => new MakerCategory("Body", "Paint01");
            public static MakerCategory Paint02 => new MakerCategory("Body", "Paint02");
        }

        public static class Hair
        {
            public static string CategoryName => "Hair";
        }

        public static class Clothes
        {
            public static string CategoryName => "Clothes";
        }

        public static class Parameter
        {
            public static string CategoryName => "Option";
            public static MakerCategory Name => new MakerCategory("Option", "Name");
            public static MakerCategory Type => new MakerCategory("Option", "Type");
            public static MakerCategory Status => new MakerCategory("Option", "Status");
        }
#pragma warning restore 1591
    }
}
