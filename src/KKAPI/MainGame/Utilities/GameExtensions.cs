using System;
using System.Collections.Generic;
using System.Linq;
using ActionGame.Chara;
using HarmonyLib;
using UnityEngine;

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
            if (heroine.charaBase is NPC npc) return npc;
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

        /// <summary>
        /// Get the persisting player object that describes this character.
        /// Returns null if the player could not be found. Works only in the main game.
        /// </summary>
        public static SaveData.Player GetPlayer(this ChaControl chaControl)
        {
            if (chaControl == null) throw new ArgumentNullException(nameof(chaControl));

            if (!Manager.Game.IsInstance() || Manager.Game.Instance.Player == null) return null;
            return Manager.Game.Instance.Player.chaCtrl == chaControl ? Manager.Game.Instance.Player : null;
        }

        /// <summary>
        /// Get the persisting player object that describes this character.
        /// Returns null if the player could not be found. Works only in the main game.
        /// </summary>
        public static SaveData.Player GetPlayer(this ChaFileControl chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));

            if (!Manager.Game.IsInstance() || Manager.Game.Instance.Player == null) return null;
            return Manager.Game.Instance.Player.GetRelatedChaFiles().Contains(chaFile)
                ? Manager.Game.Instance.Player
                : null;
        }

        /// <summary>
        /// Get ChaFiles that are related to this player. Warning: It might not return some copies.
        /// </summary>
        public static IEnumerable<ChaFileControl> GetRelatedChaFiles(this SaveData.Player player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            var results = new HashSet<ChaFileControl>();

            if (player.charFile != null)
                results.Add(player.charFile);

            if (player.chaCtrl != null && player.chaCtrl.chaFile != null)
                results.Add(player.chaCtrl.chaFile);

            return results;
        }

        /// <summary>
        /// Set the value of isCursorLock (setter is private by default).
        /// Used to regain mouse cursor during roaming mode.
        /// Best used together with setting <see cref="Time.timeScale"/> to 0 to pause the game.
        /// </summary>
        public static void SetIsCursorLock(this ActionScene actScene, bool value)
        {
            var lockField = Traverse.Create(actScene).Field<bool>("_isCursorLock");
            lockField.Value = value;
        }

        /// <summary>
        /// Get a list of free slots in class roster. New heroines can be added to these slots in saveData.heroineList.
        /// <example>
        /// var freeSlot = saveData.GetFreeClassSlots(1).FirstOrDefault();
        /// freeSlot.SetCharFile(fairyCard.charFile);
        /// freeSlot.charFileInitialized = true;
        /// saveData.heroineList.Add(freeSlot);
        /// </example>
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="classNumber">Class to search for free slots. Starts at 0, 1 is player's class, last 'class' is not usable since it's only for NPCs.</param>
        public static IEnumerable<SaveData.Heroine> GetFreeClassSlots(this SaveData saveData, int classNumber)
        {
            return Enumerable.Range(0, GetClassMaxSeatCount(saveData, classNumber))
                .Where(index => IsValidClassSeat(saveData, classNumber, index) && GetHeroineAtSeat(saveData, classNumber, index) == null)
                .Select(index => new SaveData.Heroine(true) { schoolClass = classNumber, schoolClassIndex = index });
        }

        /// <summary>
        /// Get seat count in a class based on game settings.
        /// </summary>
        public static int GetClassMaxSeatCount(this SaveData saveData, int classNumber)
        {
            return classNumber == saveData.player.schoolClass || Manager.Config.AddData.OtherClassRegisterMax
                ? 5 * 5
                : 5;
        }

        /// <summary>
        /// Get heroine at a specified class and seat.
        /// </summary>
        public static SaveData.Heroine GetHeroineAtSeat(this SaveData saveData, int classNumber, int classIndex)
        {
            return saveData.heroineList.Find(h => h.schoolClass == classNumber && h.schoolClassIndex == classIndex);
        }

        /// <summary>
        /// Check if the seat can be used for heroines.
        /// Returns true even if the seats are disabled by config, use <see cref="GetClassMaxSeatCount"/> to check for this case.
        /// </summary>
        public static bool IsValidClassSeat(this SaveData saveData, int classNumber, int classIndex)
        {
            var player = saveData.player;
            return !(player.schoolClass == classNumber && (classIndex == player.schoolClassIndex || classIndex == player.schoolClassIndex + 1) || classIndex > 24 || classIndex < 0);
        }
    }
}
