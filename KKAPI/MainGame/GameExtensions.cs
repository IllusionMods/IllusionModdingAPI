using System;
using System.Collections.Generic;
using System.Linq;
using ActionGame.Chara;

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
            if (hFlag == null) throw new ArgumentNullException(nameof(hFlag));

            return hFlag.mode == HFlag.EMode.peeping && hFlag.nowAnimationInfo.nameAnimation == "シャワー覗き";
        }

        /// <summary>
        /// Get the persisting heroine object that describes this character.
        /// Returns null if the heroine could not be found. Works only in the main game.
        /// </summary>
        public static SaveData.Heroine GetHeroine(this ChaControl chaControl)
        {
            if (chaControl == null) throw new ArgumentNullException(nameof(chaControl));

            if (!Manager.Game.IsInstance()) return null;
            return Manager.Game.Instance.HeroineList.Find(heroine => heroine.chaCtrl == chaControl);
        }

        /// <summary>
        /// Get the persisting heroine object that describes this character.
        /// Returns null if the heroine could not be found. Works only in the main game.
        /// </summary>
        public static SaveData.Heroine GetHeroine(this ChaFileControl chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));

            if (!Manager.Game.IsInstance()) return null;
            return Manager.Game.Instance.HeroineList.Find(heroine => heroine.GetRelatedChaFiles().Contains(chaFile));
        }

        /// <summary>
        /// Get the NPC that represents this heroine in the game. Works only in the main game.
        /// If the heroine has not been spawned into the game it returns null.
        /// </summary>
        public static NPC GetNPC(this SaveData.Heroine heroine)
        {
            if (heroine == null) throw new ArgumentNullException(nameof(heroine));

            if (heroine.transform == null) return null;
            return heroine.transform.GetComponent<NPC>();
        }

        /// <summary>
        /// Get ChaFiles that are related to this heroine. Warning: It might not return some copies.
        /// </summary>
        public static IEnumerable<ChaFileControl> GetRelatedChaFiles(this SaveData.Heroine heroine)
        {
            if (heroine == null) throw new ArgumentNullException(nameof(heroine));

            var results = new HashSet<ChaFileControl>();

            if (heroine.charFile != null)
                results.Add(heroine.charFile);

            if (heroine.chaCtrl != null && heroine.chaCtrl.chaFile != null)
                results.Add(heroine.chaCtrl.chaFile);

            var npc = heroine.GetNPC();
            if (npc != null && npc.chaCtrl != null && npc.chaCtrl.chaFile != null)
                results.Add(npc.chaCtrl.chaFile);

            return results;
        }
    }
}
