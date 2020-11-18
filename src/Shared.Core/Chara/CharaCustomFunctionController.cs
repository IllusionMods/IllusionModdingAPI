using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections;
#if KK
using KKAPI.MainGame;
using UniRx;
#elif AI || HS2
using AIChara;
#endif
using ExtensibleSaveFormat;
using UnityEngine;

#pragma warning disable 618

namespace KKAPI.Chara
{
    /// <summary>
    /// Base type for custom character extensions.
    /// It provides many useful methods that abstract away the nasty hooks needed to figure out when
    /// a character is changed or how to save and load your custom data to the character card.
    /// 
    /// This controller is a MonoBehaviour that is added to root gameObjects of ALL characters spawned into the game. 
    /// It's recommended to not use constructors, Awake or Start in controllers. Use <see cref="OnReload(GameMode,bool)"/> instead.
    /// </summary>
    public abstract class CharaCustomFunctionController : MonoBehaviour
    {
        private bool _wasLoaded;

        /// <summary>
        /// ChaControl of the character this controller is attached to. It's on the same gameObject as this controller.
        /// </summary>
        public ChaControl ChaControl { get; private set; }

        /// <summary>
        /// ChaFile of the character this controller is attached to.
        /// </summary>
        public ChaFileControl ChaFileControl => ChaControl.chaFile;

        /// <summary>
        /// ID used for extended data by this controller. It's set when registering the controller
        /// with <see cref="CharacterApi.RegisterExtraBehaviour{T}(string)"/>
        /// </summary>
        public string ExtendedDataId => ControllerRegistration.ExtendedDataId;

        /// <summary>
        /// Definition of this kind of function controllers.
        /// </summary>
        public CharacterApi.ControllerRegistration ControllerRegistration { get; internal set; }

        /// <summary>
        /// True if this controller has been initialized
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// Get extended data based on supplied ExtendedDataId. When in chara maker loads data from character that's being loaded. 
        /// </summary>
        public PluginData GetExtendedData()
        {
            return GetExtendedData(true);
        }

        /// <summary>
        /// Get extended data of the current character by using the ID you specified when registering this controller.
        /// </summary>
        /// <param name="getFromLoadedChara">If true, when in chara maker load data from character that's being loaded. 
        /// When outside maker or false, always grab current character's data.</param>
        public PluginData GetExtendedData(bool getFromLoadedChara)
        {
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            var chaFile = getFromLoadedChara ? MakerAPI.LastLoadedChaFile ?? ChaFileControl : ChaFileControl;
            return ExtendedSave.GetExtendedDataById(chaFile, ExtendedDataId);
        }

        /// <summary>
        /// Save your custom data to the character card under the ID you specified when registering this controller.
        /// </summary>
        /// <param name="data">Your custom data to be written to the character card. Can be null to remove the data.</param>
        public void SetExtendedData(PluginData data)
        {
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            ExtendedSave.SetExtendedDataById(ChaFileControl, ExtendedDataId, data);

#if KK 
            // Needed for propagating changes back to the original charFile since they don't get copied back.
            var heroine = ChaControl.GetHeroine();
            if (heroine != null)
            {
                ExtendedSave.SetExtendedDataById(heroine.charFile, ExtendedDataId, data);

                if (ChaControl != heroine.chaCtrl)
                {
                    ExtendedSave.SetExtendedDataById(heroine.chaCtrl.chaFile, ExtendedDataId, data);

                    // Update other instance to reflect the new ext data
                    var other = heroine.chaCtrl.GetComponent(GetType()) as CharaCustomFunctionController;
                    if (other != null) other.OnReloadInternal(KoikatuAPI.GetCurrentGameMode());
                }

                var npc = heroine.GetNPC();
                if (npc != null && npc.chaCtrl != null && npc.chaCtrl != ChaControl)
                {
                    ExtendedSave.SetExtendedDataById(npc.chaCtrl.chaFile, ExtendedDataId, data);

                    // Update other instance to reflect the new ext data
                    var other = npc.chaCtrl.GetComponent(GetType()) as CharaCustomFunctionController;
                    if (other != null) other.OnReloadInternal(KoikatuAPI.GetCurrentGameMode());
                }
            }

            var player = ChaControl.GetPlayer();
            if (player != null)
            {
                ExtendedSave.SetExtendedDataById(player.charFile, ExtendedDataId, data);

                if (ChaControl != player.chaCtrl)
                {
                    ExtendedSave.SetExtendedDataById(player.chaCtrl.chaFile, ExtendedDataId, data);

                    // Update other instance to reflect the new ext data
                    var other = player.chaCtrl.GetComponent(GetType()) as CharaCustomFunctionController;
                    if (other != null) other.OnReloadInternal(KoikatuAPI.GetCurrentGameMode());
                }
            }

#endif
        }

        /// <summary>
        /// Get extended data of the specified coordinate by using the ID you specified when registering this controller.
        /// </summary>
        /// <param name="coordinate">Coordinate you want to get the data from</param>
        public PluginData GetCoordinateExtendedData(ChaFileCoordinate coordinate)
        {
            if (coordinate == null) throw new ArgumentNullException(nameof(coordinate));
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            return ExtendedSave.GetExtendedDataById(coordinate, ExtendedDataId);
        }

        /// <summary>
        /// Set extended data to the specified coordinate by using the ID you specified when registering this controller.
        /// </summary>
        /// <param name="coordinate">Coordinate you want to set the data to</param>
        /// <param name="data">Your custom data to be saved to the coordinate card</param>
        public void SetCoordinateExtendedData(ChaFileCoordinate coordinate, PluginData data)
        {
            if (coordinate == null) throw new ArgumentNullException(nameof(coordinate));
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            ExtendedSave.SetExtendedDataById(coordinate, ExtendedDataId, data);
        }

        /// <summary>
        /// Fired when the character information is being saved.
        /// It handles all types of saving (to character card, to a scene etc.)
        /// Write any of your extended data in this method by using <see cref="SetExtendedData"/>.
        /// Avoid reusing old PluginData since we might no longer be pointed to the same character.
        /// </summary>
        protected abstract void OnCardBeingSaved(GameMode currentGameMode);

        internal void OnCardBeingSavedInternal(GameMode gamemode)
        {
            if(!_wasLoaded)
            {
                KoikatuAPI.Logger.LogWarning("Tried to save card before it was loaded - " + ChaFileControl.charaFileName);
                return;
            }

            try
            {
                OnCardBeingSaved(gamemode);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
            }
        }

        /// <summary>
        /// OnReload is fired whenever the character's state needs to be updated.
        /// This might be beacuse the character was just loaded into the game, 
        /// was replaced with a different character, etc.
        /// Use this method instead of Awake and Start. It will always get called
        /// before other methods, but after the character is in a usable state.
        /// WARNING: Make sure to completely reset your state in this method!
        ///          Assume that all of your variables are no longer valid!
        /// </summary>
        /// <param name="currentGameMode">Game mode we are currently in</param>
        /// <param name="maintainState">If true, the current state should be preserved.
        /// Do not load new extended data, instead reuse what you currently have or do nothing.</param>
        protected virtual void OnReload(GameMode currentGameMode, bool maintainState) { }

        /// <summary>
        /// OnReload is fired whenever the character's state needs to be updated.
        /// This might be beacuse the character was just loaded into the game, 
        /// was replaced with a different character, etc.
        /// Use this method instead of Awake and Start. It will always get called
        /// before other methods, but after the character is in a usable state.
        /// WARNING: Make sure to completely reset your state in this method!
        ///          Assume that all of your variables are no longer valid!
        /// WARNING: Will not get fired if disabled in <see cref="CharacterApi.RegisteredHandlers"/>, 
        /// use overloads with maintainState parameter in that case.
        /// </summary>
        /// <param name="currentGameMode">Game mode we are currently in</param>
        protected virtual void OnReload(GameMode currentGameMode) { }

        internal void OnReloadInternal(GameMode currentGameMode)
        {
            try
            {
                if (!ControllerRegistration.MaintainState)
                    OnReload(currentGameMode);

                OnReload(currentGameMode, ControllerRegistration.MaintainState);

                _wasLoaded = true;
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
            }
        }

        /// <summary>
        /// Fired just before current coordinate is saved to a coordinate card. Use <see cref="SetCoordinateExtendedData"/> to save data to it. 
        /// You might need to wait for the next frame with <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/> before handling this.
        /// </summary>
        protected virtual void OnCoordinateBeingSaved(ChaFileCoordinate coordinate) { }

        internal void OnCoordinateBeingSavedInternal(ChaFileCoordinate coordinate)
        {
            if (!_wasLoaded)
            {
                KoikatuAPI.Logger.LogWarning("Tried to save coordinate before the character was loaded - " + ChaFileControl.charaFileName);
            }

            try
            {
                OnCoordinateBeingSaved(coordinate);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
            }
        }

        /// <summary>
        /// Fired just after loading a coordinate card into the current coordinate slot.
        /// Use <see cref="GetCoordinateExtendedData"/> to get save data of the loaded coordinate.
        /// </summary>
        /// <param name="coordinate">Coordinate being currently loaded.</param>
        /// <param name="maintainState">If true, the current state should be preserved.
        /// Do not load new extended data, instead reuse what you currently have or do nothing.</param>
        protected virtual void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState) { }

        /// <summary>
        /// Fired just after loading a coordinate card into the current coordinate slot.
        /// Use <see cref="GetCoordinateExtendedData"/> to get save data of the loaded coordinate.
        /// Will not get fired if disabled in <see cref="CharacterApi.RegisteredHandlers"/>, 
        /// use overloads with maintainState parameter in that case.
        /// </summary>
        /// <param name="coordinate">Coordinate being currently loaded.</param>
        protected virtual void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate) { }

        internal void OnCoordinateBeingLoadedInternal(ChaFileCoordinate coordinate)
        {
            // If the controller didn't load yet then delay the coordinate load event until after onreload to avoid losing data from coordinate
            if (!_wasLoaded)
            {
                StartCoroutine(new WaitUntil(() => _wasLoaded).AppendCo(() => OnCoordinateBeingLoadedInternal(coordinate)));
                return;
            }

            try
            {
                if (!ControllerRegistration.MaintainCoordinateState)
                    OnCoordinateBeingLoaded(coordinate);

                OnCoordinateBeingLoaded(coordinate, ControllerRegistration.MaintainCoordinateState);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
            }
        }

#if KK
        /// <summary>
        /// Currently selected clothes on this character. Can subscribe to listen for changes.
        /// </summary>
        public BehaviorSubject<ChaFileDefine.CoordinateType> CurrentCoordinate { get; private set; }
#endif

        /// <summary>
        /// Warning: When overriding make sure to call the base method at the end of your logic!
        /// </summary>
        protected virtual void Update()
        {
            // Can't remove for bacwards compatibility
        }

        /// <summary>
        /// Warning: When overriding make sure to call the base method at the end of your logic!
        /// </summary>
        protected virtual void OnDestroy()
        {
#if KK
            CurrentCoordinate.Dispose();
#endif
        }

        /// <summary>
        /// Warning: When overriding make sure to call the base method at the end of your logic!
        /// </summary>
        protected virtual void OnEnable()
        {
            // Order is Awake - OnEnable - Start, so need to check if we started yet
            if (Started)
                OnReloadInternal(KoikatuAPI.GetCurrentGameMode());
        }

        /// <summary>
        /// Warning: When overriding make sure to call the base method at the end of your logic!
        /// </summary>
        protected virtual void Awake()
        {
            ChaControl = GetComponent<ChaControl>();
#if KK
            CurrentCoordinate = new BehaviorSubject<ChaFileDefine.CoordinateType>((ChaFileDefine.CoordinateType)ChaControl.fileStatus.coordinateType);
#endif
        }

        /// <summary>
        /// Warning: When overriding make sure to call the base method at the end of your logic!
        /// </summary>
        protected virtual void Start()
        {
            Started = true;
            OnReloadInternal(KoikatuAPI.GetCurrentGameMode());
        }
    }
}
