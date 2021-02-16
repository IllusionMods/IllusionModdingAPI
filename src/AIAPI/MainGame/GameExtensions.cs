using System;
using System.Collections.Generic;
using System.Linq;
// using ActionGame.Chara;
using AIProject;
using AIProject.SaveData;
using AIChara;

namespace KKAPI.MainGame
{
    /// <summary>
    /// Extensions useful in the main game
    /// </summary>
    public static class GameExtensions
    {

        /// <summary>
        /// Get the persisting heroine object that describes this character.
        /// Returns null if the heroine could not be found. Works only in the main game.
        /// </summary>
        public static Actor GetHeroine(this ChaControl chaControl)
        {
            if (chaControl == null) throw new ArgumentNullException(nameof(chaControl));

            if (!Manager.Map.IsInstance()) return null;
            return Manager.Map.Instance.Actors.Find(heroine => heroine.ChaControl == chaControl);
        }

        /// <summary>
        /// Get the persisting heroine object that describes this character.
        /// Returns null if the heroine could not be found. Works only in the main game.
        /// </summary>
        public static Actor GetHeroine(this ChaFileControl chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));

            if (!Manager.Map.IsInstance()) return null;
            return Manager.Map.Instance.Actors.Find(heroine => heroine.ChaControl?.chaFile == chaFile);
        }

        /// <summary>
        /// Get the NPC that represents this heroine in the game. Works only in the main game.
        /// If the heroine has not been spawned into the game it returns null.
        /// </summary>
        public static Actor GetNPC(this AgentActor heroine)
        {
            if (heroine == null) throw new ArgumentNullException(nameof(heroine));

            if (heroine.transform == null) return null;
            return heroine.transform.GetComponent<Actor>();
        }

        /// <summary>
        /// Get ChaFiles that are related to this heroine. Warning: It might not return some copies.
        /// </summary>
        public static IEnumerable<ChaFileControl> GetRelatedChaFiles(this AgentActor heroine)
        {
            if (heroine == null) throw new ArgumentNullException(nameof(heroine));

            var results = new HashSet<ChaFileControl>();

            if (heroine.ChaControl != null && heroine.ChaControl.chaFile != null)
                results.Add(heroine.ChaControl.chaFile);

            var npc = heroine.GetNPC();
            if (npc != null && npc.ChaControl != null && npc.ChaControl.chaFile != null)
                results.Add(npc.ChaControl.chaFile);

            return results;
        }

        /// <summary>
        /// Get the persisting player object that describes this character.
        /// Returns null if the player could not be found. Works only in the main game.
        /// </summary>
        public static PlayerActor GetPlayer(this ChaControl chaControl)
        {
            if (chaControl == null) throw new ArgumentNullException(nameof(chaControl));

            if (!Manager.Map.IsInstance() || Manager.Map.Instance.Player == null) return null;
            return Manager.Map.Instance.Player.ChaControl == chaControl ? Manager.Map.Instance.Player : null;
        }

        /// <summary>
        /// Get the persisting player object that describes this character.
        /// Returns null if the player could not be found. Works only in the main game.
        /// </summary>
        public static PlayerActor GetPlayer(this ChaFileControl chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));

            if (!Manager.Map.IsInstance() || Manager.Map.Instance.Player == null) return null;
            return Manager.Map.Instance.Player.GetRelatedChaFiles().Contains(chaFile)
                ? Manager.Map.Instance.Player
                : null;
        }

        /// <summary>
        /// Get ChaFiles that are related to this player. Warning: It might not return some copies.
        /// </summary>
        public static IEnumerable<ChaFileControl> GetRelatedChaFiles(this PlayerActor player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            var results = new HashSet<ChaFileControl>();

            if (player.ChaControl != null && player.ChaControl.chaFile != null)
                results.Add(player.ChaControl.chaFile);

            return results;
        }
    }
}
