using System;
using System.Collections.Generic;

namespace KKAPI.Maker
{
    /// <summary>
    /// Useful values from the character maker. Mostly built-in categories for use with registering custom controls.
    /// </summary>
    public static class MakerConstants
    {
        private static readonly List<MakerCategory> _builtInCategories = new List<MakerCategory>
        {
            new MakerCategory("00_FaceTop", "tglAll", 10),
            new MakerCategory("00_FaceTop", "tglEar", 20),
            new MakerCategory("00_FaceTop", "tglChin", 30),
            new MakerCategory("00_FaceTop", "tglCheek", 40),
            new MakerCategory("00_FaceTop", "tglEyebrow", 50),
            new MakerCategory("00_FaceTop", "tglEye01", 60),
            new MakerCategory("00_FaceTop", "tglEye02", 70),
            new MakerCategory("00_FaceTop", "tglNose", 80),
            new MakerCategory("00_FaceTop", "tglMouth", 90),
            new MakerCategory("00_FaceTop", "tglMole", 100),
            new MakerCategory("00_FaceTop", "tglMakeup", 110),
            new MakerCategory("00_FaceTop", "tglShape", 120),

            new MakerCategory("01_BodyTop", "tglAll", 10),
            new MakerCategory("01_BodyTop", "tglBreast", 20),
            new MakerCategory("01_BodyTop", "tglUpper", 30),
            new MakerCategory("01_BodyTop", "tglLower", 40),
            new MakerCategory("01_BodyTop", "tglArm", 50),
            new MakerCategory("01_BodyTop", "tglLeg", 60),
            new MakerCategory("01_BodyTop", "tglNail", 70),
            new MakerCategory("01_BodyTop", "tglUnderhair", 80),
            new MakerCategory("01_BodyTop", "tglSunburn", 90),
            new MakerCategory("01_BodyTop", "tglPaint", 100),
            new MakerCategory("01_BodyTop", "tglShape", 110),

            new MakerCategory("02_HairTop", "common", 10),
            new MakerCategory("02_HairTop", "tglBack", 20),
            new MakerCategory("02_HairTop", "tglFront", 30),
            new MakerCategory("02_HairTop", "tglSide", 40),
            new MakerCategory("02_HairTop", "tglExtension", 50),
            new MakerCategory("02_HairTop", "tglEtc", 60),

            new MakerCategory("03_ClothesTop", "tglTop", 10),
            new MakerCategory("03_ClothesTop", "tglBot", 20),
            new MakerCategory("03_ClothesTop", "tglBra", 30),
            new MakerCategory("03_ClothesTop", "tglShorts", 40),
            new MakerCategory("03_ClothesTop", "tglGloves", 50),
            new MakerCategory("03_ClothesTop", "tglPanst", 60),
            new MakerCategory("03_ClothesTop", "tglSocks", 70),
            new MakerCategory("03_ClothesTop", "tglInnerShoes", 80),
            new MakerCategory("03_ClothesTop", "tglOuterShoes", 90),
            new MakerCategory("03_ClothesTop", "tglCopy", 100),

            new MakerCategory("05_ParameterTop", "tglCharactor", 10),
            new MakerCategory("05_ParameterTop", "tglCharactorEx", 20),
            new MakerCategory("05_ParameterTop", "tglH", 30),
            new MakerCategory("05_ParameterTop", "tglQA", 40),
            new MakerCategory("05_ParameterTop", "tglAttribute", 50),
            new MakerCategory("05_ParameterTop", "tglADK", 60)
        };

        private static Dictionary<string, MakerCategory> _categoryLookup;

        /// <summary>
        /// All ategories that are built-into the character maker by default.
        /// </summary>
        public static IEnumerable<MakerCategory> BuiltInCategories => _builtInCategories;

        /// <summary>
        /// Quick search for a built-in category. If you know what category you want to use at 
        /// compile time you can use the shortcuts instead, e.g. <see cref="Face.Ear"/> 
        /// </summary>
        public static MakerCategory GetBuiltInCategory(string category, string subCategory)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));
            if (subCategory == null)
                throw new ArgumentNullException(nameof(subCategory));

            string MakeKey(string catName, string subCatName)
            {
                return $"{catName}|||{subCatName}";
            }

            if (_categoryLookup == null)
            {
                _categoryLookup = new Dictionary<string, MakerCategory>();
                foreach (var makerCategory in _builtInCategories)
                    _categoryLookup.Add(MakeKey(makerCategory.CategoryName, makerCategory.SubCategoryName), makerCategory);
            }

            return _categoryLookup.TryGetValue(MakeKey(category, subCategory), out var value) ? value : null;
        }

        public static class Face
        {
            public static MakerCategory All => GetBuiltInCategory("00_FaceTop", "tglAll");
            public static MakerCategory Ear => GetBuiltInCategory("00_FaceTop", "tglEar");
            public static MakerCategory Chin => GetBuiltInCategory("00_FaceTop", "tglChin");
            public static MakerCategory Cheek => GetBuiltInCategory("00_FaceTop", "tglCheek");
            public static MakerCategory Eyebrow => GetBuiltInCategory("00_FaceTop", "tglEyebrow");
            public static MakerCategory Eye => GetBuiltInCategory("00_FaceTop", "tglEye01");
            public static MakerCategory Iris => GetBuiltInCategory("00_FaceTop", "tglEye02");
            public static MakerCategory Nose => GetBuiltInCategory("00_FaceTop", "tglNose");
            public static MakerCategory Mouth => GetBuiltInCategory("00_FaceTop", "tglMouth");
            public static MakerCategory Mole => GetBuiltInCategory("00_FaceTop", "tglMole");
            public static MakerCategory Makeup => GetBuiltInCategory("00_FaceTop", "tglMakeup");
            public static MakerCategory Shape => GetBuiltInCategory("00_FaceTop", "tglShape");
        }

        public static class Body
        {
            public static MakerCategory All => GetBuiltInCategory("01_BodyTop", "tglAll");
            public static MakerCategory Breast => GetBuiltInCategory("01_BodyTop", "tglBreast");
            public static MakerCategory Upper => GetBuiltInCategory("01_BodyTop", "tglUpper");
            public static MakerCategory Lower => GetBuiltInCategory("01_BodyTop", "tglLower");
            public static MakerCategory Arm => GetBuiltInCategory("01_BodyTop", "tglArm");
            public static MakerCategory Leg => GetBuiltInCategory("01_BodyTop", "tglLeg");
            public static MakerCategory Nail => GetBuiltInCategory("01_BodyTop", "tglNail");
            public static MakerCategory Underhair => GetBuiltInCategory("01_BodyTop", "tglUnderhair");
            public static MakerCategory Sunburn => GetBuiltInCategory("01_BodyTop", "tglSunburn");
            public static MakerCategory Paint => GetBuiltInCategory("01_BodyTop", "tglPaint");
            public static MakerCategory Shape => GetBuiltInCategory("01_BodyTop", "tglShape");
        }

        public static class Hair
        {
            public static MakerCategory Common => GetBuiltInCategory("02_HairTop", "common");
            public static MakerCategory Back => GetBuiltInCategory("02_HairTop", "tglBack");
            public static MakerCategory Front => GetBuiltInCategory("02_HairTop", "tglFront");
            public static MakerCategory Side => GetBuiltInCategory("02_HairTop", "tglSide");
            public static MakerCategory Extension => GetBuiltInCategory("02_HairTop", "tglExtension");
            public static MakerCategory Etc => GetBuiltInCategory("02_HairTop", "tglEtc");
        }

        public static class Clothes
        {
            public static MakerCategory Top => GetBuiltInCategory("03_ClothesTop", "tglTop");
            public static MakerCategory Bottom => GetBuiltInCategory("03_ClothesTop", "tglBot");
            public static MakerCategory Bra => GetBuiltInCategory("03_ClothesTop", "tglBra");
            public static MakerCategory Shorts => GetBuiltInCategory("03_ClothesTop", "tglShorts");
            public static MakerCategory Gloves => GetBuiltInCategory("03_ClothesTop", "tglGloves");
            public static MakerCategory Panst => GetBuiltInCategory("03_ClothesTop", "tglPanst");
            public static MakerCategory Socks => GetBuiltInCategory("03_ClothesTop", "tglSocks");
            public static MakerCategory InnerShoes => GetBuiltInCategory("03_ClothesTop", "tglInnerShoes");
            public static MakerCategory OuterShoes => GetBuiltInCategory("03_ClothesTop", "tglOuterShoes");
            public static MakerCategory Copy => GetBuiltInCategory("03_ClothesTop", "tglCopy");
        }

        public static class Parameter
        {
            public static MakerCategory Character => GetBuiltInCategory("05_ParameterTop", "tglCharactor");
            public static MakerCategory CharacterEx => GetBuiltInCategory("05_ParameterTop", "tglCharactorEx");
            public static MakerCategory H => GetBuiltInCategory("03_ClothesTop", "tglH");
            public static MakerCategory QA => GetBuiltInCategory("05_ParameterTop", "tglQA");
            public static MakerCategory Attribute => GetBuiltInCategory("05_ParameterTop", "tglAttribute");
            public static MakerCategory ADK => GetBuiltInCategory("05_ParameterTop", "tglADK");
        }
    }
}