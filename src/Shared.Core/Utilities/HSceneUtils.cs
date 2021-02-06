using System;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Utility methods for working with H Scenes / main game.
    /// </summary>
    public static class HSceneUtils
    {
#if KK
        /// <summary>
        /// Get the heroine that is currently in leading position in the h scene. 
        /// In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine.
        /// </summary>
        public static SaveData.Heroine GetLeadingHeroine(this HFlag hFlag)
        {
            if (hFlag == null) throw new ArgumentNullException(nameof(hFlag));
            return hFlag.lstHeroine[GetLeadingHeroineId(hFlag)];
        }

        /// <summary>
        /// Get the heroine that is currently in leading position in the h scene. 
        /// In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine.
        /// </summary>
        public static SaveData.Heroine GetLeadingHeroine(this HSprite hSprite)
        {
            if (hSprite == null) throw new ArgumentNullException(nameof(hSprite));
            return GetLeadingHeroine(hSprite.flags);
        }

        /// <summary>
        /// Get ID of the heroine that is currently in leading position in the h scene. 0 is the main heroine, 1 is the "tag along".
        /// In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine.
        /// </summary>
        public static int GetLeadingHeroineId(this HFlag hFlag)
        {
            if (hFlag == null) throw new ArgumentNullException(nameof(hFlag));
            return hFlag.mode == HFlag.EMode.houshi3P || hFlag.mode == HFlag.EMode.sonyu3P ? hFlag.nowAnimationInfo.id % 2 : 0;
        }

        /// <summary>
        /// Get ID of the heroine that is currently in leading position in the h scene. 0 is the main heroine, 1 is the "tag along".
        /// In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine.
        /// </summary>
        public static int GetLeadingHeroineId(this HSprite hSprite)
        {
            if (hSprite == null) throw new ArgumentNullException(nameof(hSprite));
            return GetLeadingHeroineId(hSprite.flags);
        }
        
        /// <summary>
        /// Is current H mode penetration?
        /// </summary>
        public static bool IsSonyu(this HFlag hFlag)
        {
            return hFlag.mode == HFlag.EMode.sonyu || hFlag.mode == HFlag.EMode.sonyu3P || hFlag.mode == HFlag.EMode.sonyu3PMMF;
        }

        /// <summary>
        /// Is current h mode service?
        /// </summary>
        public static bool IsHoushi(this HFlag hFlag)
        {
            return hFlag.mode == HFlag.EMode.houshi || hFlag.mode == HFlag.EMode.houshi3P || hFlag.mode == HFlag.EMode.houshi3PMMF;
        }
#endif
    }
}
