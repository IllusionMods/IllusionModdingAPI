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
    }
}
