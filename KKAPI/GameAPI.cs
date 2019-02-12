using System;
using BepInEx;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;
using Manager;
using UnityEngine;

namespace KKAPI
{
    [BepInPlugin(GUID, "Modding API for Koikatsu!", Version)]
    public class GameAPI : BaseUnityPlugin
    {
        internal const string Version = "1.4";
        public const string GUID = "marco.kkapi";

        public GameAPI()
        {
            Instance = this;
        }

        internal static GameAPI Instance { get; private set; }

        private void Start()
        {
            var insideStudio = Application.productName == "CharaStudio";

            MakerAPI.Init(insideStudio);
            StudioAPI.Init(insideStudio);
            CharacterApi.Init();
        }

        public static GameMode GetCurrentGameMode()
        {
            if (StudioAPI.InsideStudio) return GameMode.Studio;
            if (MakerAPI.InsideMaker) return GameMode.Maker;
            if (Game.Instance != null) return GameMode.MainGame;
            return GameMode.Unknown;
        }

        #region Synchronization

        private readonly object _invokeLock = new object();
        private Action _invokeList;

        /// <summary>
        /// Invoke the Action on the main unity thread. Use to synchronize your threads.
        /// </summary>
        public void SynchronizedInvoke(Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            lock (_invokeLock) _invokeList += callback;
        }

        private void Update()
        {
            // Safe to do outside of lock because nothing can remove callbacks, at worst we execute with 1 frame delay
            if (_invokeList == null) return;

            Action toRun;
            lock (_invokeLock)
            {
                toRun = _invokeList;
                _invokeList = null;
            }

            // Need to execute outside of the lock in case the callback itself calls Invoke we could deadlock
            // The invocation would also block any threads that call Invoke
            toRun();
        }
        
        #endregion
    }
}
