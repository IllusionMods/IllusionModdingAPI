using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections;
#if KK || KKS
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

        #region Ext data

        /// <summary>
        /// Get extended data based on supplied ExtendedDataId. When in chara maker loads data from character that's being loaded. 
        /// This should be used inside the <see cref="OnReload(KKAPI.GameMode,bool)"/> event.
        /// Consider using one of the other "Get___ExtData" and "Set___ExtData" methods instead since they are more reliable and handle copying and transferring outfits and they conform to built in maker load toggles.
        /// </summary>
        public PluginData GetExtendedData()
        {
            return GetExtendedData(true);
        }

        /// <summary>
        /// Get extended data of the current character by using the ID you specified when registering this controller.
        /// This should be used inside the <see cref="OnReload(KKAPI.GameMode,bool)"/> event.
        /// Consider using one of the other "Get___ExtData" and "Set___ExtData" methods instead since they are more reliable and handle copying and transferring outfits and they conform to built in maker load toggles.
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
        /// This should be used inside the <see cref="OnCardBeingSaved"/> event.
        /// Consider using one of the other "Get___ExtData" and "Set___ExtData" methods instead since they are more reliable and handle copying and transferring outfits and they conform to built in maker load toggles.
        /// </summary>
        /// <param name="data">Your custom data to be written to the character card. Can be null to remove the data.</param>
        public void SetExtendedData(PluginData data)
        {
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            ExtendedSave.SetExtendedDataById(ChaFileControl, ExtendedDataId, data);

#if KK || KKS
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.MainGame)
            {
                // In main game store ext data for the character inside of the main chaFile object (the one that gets saved to game saves).
                // This allows saving ext data inside talk scenes and H scenes without losing it after exiting to main map.
                var heroine = ChaControl.GetHeroine();
                if (heroine != null)
                {
                    ExtendedSave.SetExtendedDataById(heroine.charFile, ExtendedDataId, data);

                    if (ChaControl != heroine.chaCtrl)
                    {
                        ExtendedSave.SetExtendedDataById(heroine.chaCtrl.chaFile, ExtendedDataId, data);
                        // Update other instance to reflect the new ext data
                        CharacterApi.Hooks.SetDirty(heroine, true);

                    }

                    var npc = heroine.GetNPC();
                    if (npc != null && npc.chaCtrl != null && npc.chaCtrl != ChaControl && npc.chaCtrl != heroine.chaCtrl)
                    {
                        ExtendedSave.SetExtendedDataById(npc.chaCtrl.chaFile, ExtendedDataId, data);
                        // Update other instance to reflect the new ext data
                        CharacterApi.Hooks.SetDirty(heroine, true);
                    }
                }
                else
                {
                    var player = ChaControl.GetPlayer();
                    if (player != null)
                    {
                        ExtendedSave.SetExtendedDataById(player.charFile, ExtendedDataId, data);

                        if (ChaControl != player.chaCtrl)
                        {
                            ExtendedSave.SetExtendedDataById(player.chaCtrl.chaFile, ExtendedDataId, data);
                            // Update other instance to reflect the new ext data
                            CharacterApi.Hooks.SetDirty(player, true);
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Get extended data of the specified coordinate by using the ID you specified when registering this controller.
        /// This should be used inside the <see cref="OnCoordinateBeingLoaded(ChaFileCoordinate,bool)"/> event.
        /// Consider using one of the other "Get___ExtData" and "Set___ExtData" methods instead since they are more reliable and handle copying and transferring outfits and they conform to built in maker load toggles.
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
        /// This should be used inside the <see cref="OnCoordinateBeingSaved"/> event.
        /// Consider using one of the other "Get___ExtData" and "Set___ExtData" methods instead since they are more reliable and handle copying and transferring outfits and they conform to built in maker load toggles.
        /// </summary>
        /// <param name="coordinate">Coordinate you want to set the data to</param>
        /// <param name="data">Your custom data to be saved to the coordinate card</param>
        public void SetCoordinateExtendedData(ChaFileCoordinate coordinate, PluginData data)
        {
            if (coordinate == null) throw new ArgumentNullException(nameof(coordinate));
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            ExtendedSave.SetExtendedDataById(coordinate, ExtendedDataId, data);
        }

        #endregion

        #region Events

        /// <summary>
        /// Fired when the character information is being saved.
        /// It handles all types of saving (to character card, to a scene etc.)
        /// Write any of your extended data in this method by using <see cref="SetExtendedData"/>.
        /// Avoid reusing old PluginData since we might no longer be pointed to the same character.
        /// </summary>
        protected abstract void OnCardBeingSaved(GameMode currentGameMode);

        internal void OnCardBeingSavedInternal() => OnCardBeingSavedInternal(KoikatuAPI.GetCurrentGameMode());
        internal void OnCardBeingSavedInternal(GameMode gamemode)
        {
            if (!_wasLoaded)
            {
                KoikatuAPI.Logger.LogWarning("Tried to save card before it was loaded - " + ChaFileControl.GetFancyCharacterName());
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

        internal void OnReloadInternal() => OnReloadInternal(KoikatuAPI.GetCurrentGameMode());
        internal void OnReloadInternal(GameMode currentGameMode)
        {
#if KK || KKS
            if (currentGameMode == GameMode.MainGame)
                CharacterApi.Hooks.SetDirty(ChaControl.GetHeroine(), false);
#endif
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

        // issue with stopallcoroutines
        // todo hook startcoroutine and check if active, if not then show stack trace
        ///// <summary>
        ///// Hides base StartCoroutine and runs it on the plugin instance
        ///// </summary>
        //public new Coroutine StartCoroutine(IEnumerator routine)
        //{
        //    return KKAPI.KoikatuAPI.Instance.StartCoroutine(routine);
        //}

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

#if KK || KKS
        /// <summary>
        /// Currently selected clothes on this character. Can subscribe to listen for changes.
        /// </summary>
        public BehaviorSubject<ChaFileDefine.CoordinateType> CurrentCoordinate { get; private set; }
#endif

        #endregion

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
#if KK || KKS
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
#if KK || KKS
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

        #region New ExtData

        private ChaFileCoordinate GetCoordinate(int coordinateId)
        {
            // Get coord from the current ChaControl since it can be temporarily changed and would mess up ext data of clothes inside heroine.chaCtrl if we saved there
            KoikatuAPI.Assert(ChaControl.nowCoordinate != null, "ChaControl.nowCoordinate != null");
#if KK || KKS
            return (coordinateId < 0 ? ChaControl.nowCoordinate : ChaFileControl.coordinate[coordinateId]);
#elif EC || AI || HS2
            if (coordinateId > 0) KoikatuAPI.Logger.LogWarning("This game doesn't support multiple coordinates, nowCoordinate will be used!\n" + new System.Diagnostics.StackTrace());
            return ChaControl.nowCoordinate;
#endif
        }

        private ChaFile GetExtDataTargetChaFile(bool setDirty)
        {
#if KK || KKS
            // In main game store ext data for the character inside of the main chaFile object (the one that gets saved to game saves) instead of its copies.
            // This allows saving ext data inside talk scenes and H scenes without losing it after exiting to main map.
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.MainGame)
            {
                var heroine = ChaControl.GetHeroine();
                if (heroine != null)
                {
                    if (setDirty)
                        CharacterApi.Hooks.SetDirty(heroine, true);
                    return heroine.charFile;
                }

                var player = ChaControl.GetPlayer();
                if (player != null)
                {
                    if (setDirty)
                        CharacterApi.Hooks.SetDirty(player, true);
                    return player.charFile;
                }
            }
#endif
            return ChaFileControl;
        }

        /// <summary>
        /// Get extended data for specific clothes.
        /// Do not store this data because it might change without notice, for example when clothing is copied. Always call Get at the point where you need the data, not earlier.
        /// If you change any of the data, remember to call the corresponding Set method or the change might not be saved.
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// If no extended data of this plugin was set yet, this method will return null.
        /// In maker, you can update controls that use this data in the <see cref="MakerAPI.ReloadCustomInterface"/> event.
        /// </summary>
        /// <param name="coordinateId">The coordinate number to open extened data for if the game supports multiple coords (0-indexed). -1 will use the current coordinate.</param>
        public PluginData GetClothesExtData(int coordinateId = -1)
        {
            var coord = GetCoordinate(coordinateId);
            KoikatuAPI.Assert(coord != null, nameof(coord) + " != null");
            KoikatuAPI.Assert(coord.clothes != null, "coord.clothes != null");
            var clothes = coord.clothes;
            clothes.TryGetExtendedDataById(ExtendedDataId, out var data);
            return data;
        }
        /// <summary>
        /// Get extended data for a specific accessory.
        /// Do not store this data because it might change without notice, for example when clothing is copied. Always call Get at the point where you need the data, not earlier.
        /// If you change any of the data, remember to call the corresponding Set method or the change might not be saved.
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// If no extended data of this plugin was set yet, this method will return null.
        /// In maker, you can update controls that use this data in the <see cref="MakerAPI.ReloadCustomInterface"/> event.
        /// </summary>
        /// <param name="accessoryPartId">The accessory part number to open extened data for (0-indexed).</param>
        /// <param name="coordinateId">The coordinate number to open extened data for if the game supports multiple coords (0-indexed). -1 will use the current coordinate.</param>
        public PluginData GetAccessoryExtData(int accessoryPartId, int coordinateId = -1)
        {
            var coord = GetCoordinate(coordinateId);
            KoikatuAPI.Assert(coord != null, nameof(coord) + " != null");
            KoikatuAPI.Assert(coord.accessory != null, "coord.accessory != null");
            var accessoryPart = coord.accessory.parts[accessoryPartId];
            accessoryPart.TryGetExtendedDataById(ExtendedDataId, out var data);
            return data;
        }
        /// <summary>
        /// Get extended data for character's body (body sliders, tattoos).
        /// Do not store this data because it might change without notice, for example when clothing is copied. Always call Get at the point where you need the data, not earlier.
        /// If you change any of the data, remember to call the corresponding Set method or the change might not be saved.
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// If no extended data of this plugin was set yet, this method will return null.
        /// In maker, you can update controls that use this data in the <see cref="MakerAPI.ReloadCustomInterface"/> event.
        /// </summary>
        public PluginData GetBodyExtData()
        {
            var chafile = GetExtDataTargetChaFile(false);
            KoikatuAPI.Assert(chafile.custom != null, "chafile.custom != null");
            KoikatuAPI.Assert(chafile.custom.body != null, "chafile.custom.body != null");
            chafile.custom.body.TryGetExtendedDataById(ExtendedDataId, out var data);
            return data;
        }
        /// <summary>
        /// Get extended data for character's face (face sliders, eye settings).
        /// Do not store this data because it might change without notice, for example when clothing is copied. Always call Get at the point where you need the data, not earlier.
        /// If you change any of the data, remember to call the corresponding Set method or the change might not be saved.
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// If no extended data of this plugin was set yet, this method will return null.
        /// In maker, you can update controls that use this data in the <see cref="MakerAPI.ReloadCustomInterface"/> event.
        /// </summary>
        public PluginData GetFaceExtData()
        {
            var chafile = GetExtDataTargetChaFile(false);
            KoikatuAPI.Assert(chafile.custom != null, "chafile.custom != null");
            KoikatuAPI.Assert(chafile.custom.face != null, "chafile.custom.face != null");
            chafile.custom.face.TryGetExtendedDataById(ExtendedDataId, out var data);
            return data;
        }
        /// <summary>
        /// Get extended data for character's parameters (personality, preferences, traits).
        /// Do not store this data because it might change without notice, for example when clothing is copied. Always call Get at the point where you need the data, not earlier.
        /// If you change any of the data, remember to call the corresponding Set method or the change might not be saved.
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// If no extended data of this plugin was set yet, this method will return null.
        /// In maker, you can update controls that use this data in the <see cref="MakerAPI.ReloadCustomInterface"/> event.
        /// </summary>
        public PluginData GetParameterExtData()
        {
            var chafile = GetExtDataTargetChaFile(false);
            KoikatuAPI.Assert(chafile.parameter != null, "chafile.parameter != null");
            chafile.parameter.TryGetExtendedDataById(ExtendedDataId, out var data);
            return data;
        }


        /// <summary>
        /// Set extended data for specific clothes.
        /// Always call Set right after changing any of the data, or the change might not be saved if the data is changed for whatever reason (clothing change, reload, etc.)
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// </summary>
        /// <param name="data">Extended data to save.</param>
        /// <param name="coordinateId">The coordinate number to open extened data for if the game supports multiple coords (0-indexed). -1 will use the current coordinate.</param>
        public void SetClothesExtData(PluginData data, int coordinateId = -1)
        {
            var coord = GetCoordinate(coordinateId);
            KoikatuAPI.Assert(coord != null, nameof(coord) + " != null");
            KoikatuAPI.Assert(coord.clothes != null, "coord.clothes != null");
            var clothes = coord.clothes;
            clothes.SetExtendedDataById(ExtendedDataId, data);
        }
        /// <summary>
        /// Set extended data for a specific accessory.
        /// Always call Set right after changing any of the data, or the change might not be saved if the data is changed for whatever reason (clothing change, reload, etc.)
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// </summary>
        /// <param name="data">Extended data to save.</param>
        /// <param name="accessoryPartId">The accessory part number to open extened data for (0-indexed).</param>
        /// <param name="coordinateId">The coordinate number to open extened data for if the game supports multiple coords (0-indexed). -1 will use the current coordinate.</param>
        public void SetAccessoryExtData(PluginData data, int accessoryPartId, int coordinateId = -1)
        {
            var coord = GetCoordinate(coordinateId);
            KoikatuAPI.Assert(coord != null, nameof(coord) + " != null");
            KoikatuAPI.Assert(coord.accessory != null, "coord.accessory != null");
            var accessoryPart = coord.accessory.parts[accessoryPartId];
            accessoryPart.SetExtendedDataById(ExtendedDataId, data);
        }
        /// <summary>
        /// Set extended data for character's body (body sliders, tattoos).
        /// Always call Set right after changing any of the data, or the change might not be saved if the data is changed for whatever reason (clothing change, reload, etc.)
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// </summary>
        /// <param name="data">Extended data to save.</param>
        public void SetBodyExtData(PluginData data)
        {
            var chafile = GetExtDataTargetChaFile(true);
            KoikatuAPI.Assert(chafile.custom != null, "chafile.custom != null");
            KoikatuAPI.Assert(chafile.custom.body != null, "chafile.custom.body != null");
            chafile.custom.body.SetExtendedDataById(ExtendedDataId, data);
            // Save both to the main chafile and to the current instance in case it gets saved by something
            if (chafile != ChaFileControl)
                ChaFileControl.custom.body.SetExtendedDataById(ExtendedDataId, data);
        }
        /// <summary>
        /// Set extended data for character's face (face sliders, eye settings).
        /// Always call Set right after changing any of the data, or the change might not be saved if the data is changed for whatever reason (clothing change, reload, etc.)
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// </summary>
        /// <param name="data">Extended data to save.</param>
        public void SetFaceExtData(PluginData data)
        {
            var chafile = GetExtDataTargetChaFile(true);
            KoikatuAPI.Assert(chafile.custom != null, "chafile.custom != null");
            KoikatuAPI.Assert(chafile.custom.face != null, "chafile.custom.face != null");
            chafile.custom.face.SetExtendedDataById(ExtendedDataId, data);
            // Save both to the main chafile and to the current instance in case it gets saved by something
            if (chafile != ChaFileControl)
                ChaFileControl.custom.face.SetExtendedDataById(ExtendedDataId, data);
        }
        /// <summary>
        /// Set extended data for character's parameters (personality, preferences, traits).
        /// Always call Set right after changing any of the data, or the change might not be saved if the data is changed for whatever reason (clothing change, reload, etc.)
        /// This data is saved alongside game data, which means it is automatically copied and moved as necessary.
        /// </summary>
        /// <param name="data">Extended data to save.</param>
        public void SetParameterExtData(PluginData data)
        {
            var chafile = GetExtDataTargetChaFile(true);
            KoikatuAPI.Assert(chafile.parameter != null, "chafile.parameter != null");
            chafile.parameter.SetExtendedDataById(ExtendedDataId, data);
            // Save both to the main chafile and to the current instance in case it gets saved by something
            if (chafile != ChaFileControl)
                ChaFileControl.parameter.SetExtendedDataById(ExtendedDataId, data);
        }

        #endregion
    }
}
