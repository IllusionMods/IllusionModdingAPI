using ExtensibleSaveFormat;
using KKAPI.Maker;
using System;
using System.Collections;
using BepInEx.Logging;
using UniRx;
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
            try
            {
                OnCardBeingSaved(gamemode);
            }
            catch (Exception e)
            {
                BepInEx.Logger.Log(LogLevel.Error, e);
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
        /// Obsolete, use other overloads instead.
        /// </summary>
        [Obsolete("Use the other overloads instead")]
        protected virtual void OnReload(GameMode currentGameMode) { }

        internal void OnReloadInternal(GameMode currentGameMode)
        {
            try
            {
                if (!ControllerRegistration.MaintainState)
                    OnReload(currentGameMode);

                OnReload(currentGameMode, ControllerRegistration.MaintainState);
            }
            catch (Exception e)
            {
                BepInEx.Logger.Log(LogLevel.Error, e);
            }
        }

        /// <summary>
        /// Fired just before current coordinate is saved to a coordinate card. Use <see cref="SetCoordinateExtendedData"/> to save data to it. 
        /// You might need to wait for the next frame with <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/> before handling this.
        /// Use <see cref="CurrentCoordinate"/> to figure out what clothes set your character is wearing right now.
        /// </summary>
        protected virtual void OnCoordinateBeingSaved(ChaFileCoordinate coordinate) { }

        internal void OnCoordinateBeingSavedInternal(ChaFileCoordinate coordinate)
        {
            try
            {
                OnCoordinateBeingSaved(coordinate);
            }
            catch (Exception e)
            {
                BepInEx.Logger.Log(LogLevel.Error, e);
            }
        }

        /// <summary>
        /// Fired just after loading a coordinate card into the current coordinate slot.
        /// Use <see cref="GetCoordinateExtendedData"/> to get save data of the loaded coordinate.
        /// Use <see cref="CurrentCoordinate"/> to figure out what clothes set your character is wearing right now.
        /// </summary>
        /// <param name="coordinate">Coordinate being currently loaded.</param>
        /// <param name="maintainState">If true, the current state should be preserved.
        /// Do not load new extended data, instead reuse what you currently have or do nothing.</param>
        protected virtual void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState) { }

        /// <summary>
        /// Obsolete, use other overloads instead.
        /// </summary>
        [Obsolete("Use the other overloads instead")]
        protected virtual void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate) { }

        internal void OnCoordinateBeingLoadedInternal(ChaFileCoordinate coordinate)
        {
            try
            {
                if (!ControllerRegistration.MaintainCoordinateState)
                    OnCoordinateBeingLoaded(coordinate);

                OnCoordinateBeingLoaded(coordinate, ControllerRegistration.MaintainCoordinateState);
            }
            catch (Exception e)
            {
                BepInEx.Logger.Log(LogLevel.Error, e);
            }
        }

        /// <summary>
        /// Currently selected clothes on this character. Can subscribe to listen for changes.
        /// </summary>
        public BehaviorSubject<ChaFileDefine.CoordinateType> CurrentCoordinate { get; private set; }

        /// <summary>
        /// Warning: When overriding make sure to call the base method at the end of your logic!
        /// </summary>
        protected virtual void Update()
        {
            // TODO change into a separate trigger component?
            var currentCoordinate = (ChaFileDefine.CoordinateType)ChaControl.fileStatus.coordinateType;
            if (currentCoordinate != CurrentCoordinate.Value)
                CurrentCoordinate.OnNext(currentCoordinate);
        }

        /// <summary>
        /// Warning: When overriding make sure to call the base method at the end of your logic!
        /// </summary>
        protected virtual void OnDestroy()
        {
            CurrentCoordinate.Dispose();
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
            CurrentCoordinate = new BehaviorSubject<ChaFileDefine.CoordinateType>((ChaFileDefine.CoordinateType)ChaControl.fileStatus.coordinateType);
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
