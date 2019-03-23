using System.Linq;
using KKAPI.Studio;
using Studio;

namespace KKAPI.Chara
{
    /// <summary>
    /// Extensions for use with ChaControl, ChaFile and similar
    /// </summary>
    public static class CharacterExtensions
    {
        /// <summary>
        /// Get ChaControl that is using this ChaFile if any exist.
        /// </summary>
        public static ChaControl GetChaControl(this ChaFile chaFile)
        {
            return CharacterApi.ChaControls.FirstOrDefault(x => x.chaFile == chaFile);
        }

        /// <summary>
        /// Get GetOCIChar that is assigned to this character. Only works in CharaStudio, returns null elsewhere.
        /// </summary>
        public static OCIChar GetOCIChar(this ChaControl chaControl)
        {
            if (!StudioAPI.InsideStudio) return null;
            var infos = global::Studio.Studio.Instance.dicInfo;
            var charas = infos.Values.OfType<OCIChar>();
            return charas.FirstOrDefault(x => x.charInfo == chaControl);
        }
    }
}