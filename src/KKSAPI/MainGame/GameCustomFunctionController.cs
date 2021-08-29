using System;
using ActionGame;
using ExtensibleSaveFormat;
using KKAPI.Maker;
using UnityEngine;

namespace KKAPI.MainGame
{
    /// <summary>
    /// Base type for custom game extensions.
    /// It provides many useful methods that abstract away the nasty hooks needed to figure out when the state of the game
    /// changes.
    /// 
    /// This controller is a MonoBehaviour that is created upon registration in <see cref="GameAPI.RegisterExtraBehaviour{T}"/>.
    /// The controller is created only once. If it's created too late it might miss some events.
    /// It's recommended to register controllers in your Start method.
    /// </summary>
    public abstract class GameCustomFunctionController : MonoBehaviour
    {
        private static Cycle _cycle;

        /// <summary>
        /// Extended save ID used by this function controller
        /// </summary>
        public string ExtendedDataId { get; internal set; }

        /// <summary>
        /// Get extended data based on supplied ExtendedDataId. When in chara maker loads data from character that's being loaded. 
        /// </summary>
        [Obsolete("Not implemented yet")]
        public PluginData GetExtendedData()
        {
            throw new NotImplementedException("Not implemented yet");
            //todo
            //if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            //return ExtendedSave.GetExtendedDataById(Manager.Game.saveData, ExtendedDataId);
        }

        /// <summary>
        /// Save your custom data to the character card under the ID you specified when registering this controller.
        /// </summary>
        /// <param name="data">Your custom data to be written to the character card. Can be null to remove the data.</param>
        [Obsolete("Not implemented yet")]
        public void SetExtendedData(PluginData data)
        {
            throw new NotImplementedException("Not implemented yet");
            //todo
            //if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            //ExtendedSave.SetExtendedDataById(Manager.Game.instance.saveData, ExtendedDataId, data);
        }

        /// <summary>
        /// Triggered when the H scene is ended, but before it is unloaded.
        /// Warning: This is triggered in main game, free H, and in VR module as well!
        /// </summary>
        /// <param name="proc">H scene controller instance. Can be either HSceneProc or VRHScene.</param>
        /// <param name="hFlag">H scene parameters, use this over proc if possible.</param>
        /// <param name="vr">If true, we are inside the VR module</param>
        protected internal virtual void OnEndH(MonoBehaviour proc, HFlag hFlag, bool vr) { }

        /// <summary>
        /// Triggered when the night menu is entered at the end of the day (screen where you can save and load the game).
        /// You can use <see cref="GetCycle"/> to see what day it is as well as other game state.
        /// </summary>
        protected internal virtual void OnEnterHotelMyroomMenu() { }

        /// <summary>
        /// Triggered right after game state was loaded from a file. Some things might still be uninitialized.
        /// </summary>
        protected internal virtual void OnGameLoad(GameSaveLoadEventArgs args) { }

        /// <summary>
        /// Triggered right before game state is saved to a file.
        /// </summary>
        protected internal virtual void OnGameSave(GameSaveLoadEventArgs args) { }

        /// <summary>
        /// Triggered after an H scene is loaded.
        /// Warning: This is triggered in main game, free H, and in VR module as well!
        /// </summary>
        /// <param name="proc">H scene controller instance. Can be either HSceneProc or VRHScene.</param>
        /// <param name="hFlag">H scene parameters, use this over proc if possible.</param>
        /// <param name="vr">If true, we are inside the VR module</param>
        protected internal virtual void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr) { }
        
        /// <summary>
        /// Get the current game Cycle object, if it exists.
        /// </summary>
        protected static Cycle GetCycle()
        {
            if (_cycle == null)
                _cycle = FindObjectOfType<Cycle>();
            return _cycle;
        }

        /// <summary>
        /// Triggered when the current day changes in story mode.
        /// </summary>
        protected internal virtual void OnDayChange(Cycle.Week day) { }

        /// <summary>
        /// Triggered when the current time of the day changes in story mode.
        /// </summary>
        protected internal virtual void OnPeriodChange(Cycle.Type period) { }

        /// <summary>
        /// Triggered when a new game is started in story mode.
        /// </summary>
        protected internal virtual void OnNewGame() { }
    }
}
