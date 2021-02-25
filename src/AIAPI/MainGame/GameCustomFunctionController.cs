﻿using System;
// using ActionGame;
using ExtensibleSaveFormat;
using KKAPI.Maker;
using UnityEngine;
using Manager;
using AIProject;
using AIChara;
using CharaCustom;

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
        private static EnvironmentSimulator _environmentSimulator;

        /// <summary>
        /// Extended save ID used by this function controller
        /// </summary>
        public string ExtendedDataId { get; internal set; }
        
        /// <summary>
        /// Get extended data based on supplied ExtendedDataId. When in chara maker loads data from character that's being loaded. 
        /// </summary>
        [Obsolete("This method is not implemented in AI.  WIll require update to ExtensibleSaveFormat to implement.", true)]
        public PluginData GetExtendedData()
        {
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            throw new NotImplementedException();
            //return ExtendedSave.GetExtendedDataById(Singleton<Map>.Instance.Player.ChaControl.chaFile, ExtendedDataId);
        }

        /// <summary>
        /// Save your custom data to the character card under the ID you specified when registering this controller.
        /// </summary>
        /// <param name="data">Your custom data to be written to the character card. Can be null to remove the data.</param>
        [Obsolete("This method is not implemented in AI.  WIll require update to ExtensibleSaveFormat to implement.", true)]
        public void SetExtendedData(PluginData data)
        {
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            throw new NotImplementedException();
            //ExtendedSave.SetExtendedDataById(Singleton<Map>.Instance.Player.ChaControl.chaFile, ExtendedDataId, data);
        }

        /// <summary>
        /// Triggered when the H scene is ended, but before it is unloaded.
        /// Warning: This is triggered in free H as well!
        /// </summary>
        /// <param name="proc">H scene controller instance</param>
        /// <param name="freeH">If true, the h scene was started from Main menu > Extra > FreeH</param>
        protected internal virtual void OnEndH(HScene proc, bool freeH) { }

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
        /// Warning: This is triggered in free H as well!
        /// </summary>
        /// <param name="proc">H scene controller instance</param>
        /// <param name="freeH">If true, the h scene was started from Main menu > Extra > FreeH</param>
        protected internal virtual void OnStartH(HScene proc, bool freeH) { }

        /// <summary>
        /// Get the current game EnvironmentSimulator (Cycle in KK) object, if it exists.
        /// </summary>
        protected static EnvironmentSimulator GetCycle()
        {
            if (_environmentSimulator == null)
                _environmentSimulator = FindObjectOfType<EnvironmentSimulator>();
            return _environmentSimulator;
        }

        /// <summary>
        /// Triggered when the current day changes in story mode.
        /// </summary>
        protected internal virtual void OnDayChange(int day) { }

        /// <summary>
        /// Triggered when the current time of the day changes in story mode (morning, noon, night).
        /// </summary>
        protected internal virtual void OnPeriodChange(AIProject.TimeZone period) { }

        /// <summary>
        /// Triggered when a new game is started in story mode.
        /// </summary>
        protected internal virtual void OnNewGame() { }
    }
}
