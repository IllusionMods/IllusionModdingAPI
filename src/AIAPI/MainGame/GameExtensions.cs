using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Get the persisting AgentData (heroine) object that describes this character.
        /// Returns null if the AgentData (heroine) could not be found. Works only in the main game.
        /// </summary>
        public static AgentData GetHeroine(this ChaControl chaControl)
        {
            if (chaControl == null) throw new ArgumentNullException(nameof(chaControl));

            if (!Manager.Map.IsInstance()) return null;

            var agentTable = Manager.Map.Instance.AgentTable;            
            if (agentTable == null || agentTable.Count <= 0) return null;

            foreach (int key in agentTable.Keys)
			{
				if (agentTable.TryGetValue(key, out AgentActor agentActor))
				{
                    if (agentActor?.ChaControl == chaControl) return agentActor.AgentData;
                }
            }           

            return null;
        }

        /// <summary>
        /// Get the persisting AgentData (heroine) object that describes this character.
        /// Returns null if the AgentData (heroine) could not be found. Works only in the main game.
        /// </summary>
        public static AgentData GetHeroine(this ChaFileControl chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));

            if (!Manager.Map.IsInstance()) return null;

            var agentTable = Manager.Map.Instance.AgentTable;            
            if (agentTable == null || agentTable.Count <= 0) return null;

            foreach (int key in agentTable.Keys)
			{
				if (agentTable.TryGetValue(key, out AgentActor agentActor))
				{
                    if (agentActor?.ChaControl?.chaFile == chaFile) return agentActor.AgentData;
                }
            }  
            return null;
        }

        /// <summary>
        /// Get the AgentActor that represents this AgentData (heroine) in the game. Works only in the main game.
        /// If the AgentData (heroine) has not been spawned into the game it returns null.
        /// </summary>
        public static AgentActor GetNPC(this AgentData agentData)
        {
            if (agentData == null) throw new ArgumentNullException(nameof(agentData));
            if (agentData.param == null) return null;

            var agentTable = Manager.Map.Instance.AgentTable;            
            if (agentTable == null || agentTable.Count <= 0) return null;

            foreach (int key in agentTable.Keys)
			{
				if (agentTable.TryGetValue(key, out AgentActor agentActor))
				{
                    if (agentActor == agentData.param?.actor) return agentActor;
                }
            }  

            return null;
        }

        /// <summary>
        /// Get ChaFiles that are related to this AgentData (heroine). Warning: It might not return some copies.
        /// </summary>
        public static IEnumerable<ChaFileControl> GetRelatedChaFiles(this AgentData agentData)
        {
            if (agentData == null) throw new ArgumentNullException(nameof(agentData));

            var results = new HashSet<ChaFileControl>();

            if (agentData.param?.actor?.ChaControl?.chaFile != null)
                results.Add(agentData.param.actor.ChaControl.chaFile);

            var agentActor = agentData.GetNPC();
            if (agentActor?.ChaControl?.chaFile != null)
                results.Add(agentActor.ChaControl.chaFile);

            return results;
        }

        /// <summary>
        /// Get ChaFiles that are related to this AgentActor (heroine). Warning: It might not return some copies.
        /// </summary>
        public static IEnumerable<ChaFileControl> GetRelatedChaFiles(this AgentActor agentActor)
        {
            if (agentActor == null) throw new ArgumentNullException(nameof(agentActor));

            var results = new HashSet<ChaFileControl>();

            if (agentActor.ChaControl?.chaFile != null)
                results.Add(agentActor.ChaControl.chaFile);        

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

            if (player.ChaControl?.chaFile != null)
                results.Add(player.ChaControl.chaFile);

            return results;
        }
    }
}
