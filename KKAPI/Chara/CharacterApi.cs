using BepInEx.Logging;
using ExtensibleSaveFormat;
using Harmony;
using KKAPI.Maker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Logger = BepInEx.Logger;

namespace KKAPI.Chara
{
    /// <summary>
    /// Provides an easy way to add custom logic to all characters in the game and in studio. 
    /// It takes care of all the error-prone plumbing and lets you easily save and load data to the character cards.
    /// </summary>
    public static partial class CharacterApi
    {
        internal static readonly HashSet<ChaControl> ChaControls = new HashSet<ChaControl>();

        private static readonly List<ControllerRegistration> _registeredHandlers = new List<ControllerRegistration>();

        /// <summary>
        /// All currently registered kinds of <see cref="CharaCustomFunctionController"/> controllers.
        /// </summary>
        public static IEnumerable<ControllerRegistration> RegisteredHandlers => _registeredHandlers.AsReadOnly();

        /// <summary>
        /// Override to supply custom extended data copying logic.
        /// By default copies all data under <code>ExtendedDataId</code> by reference.
        /// </summary>
        /// <param name="dst">Copy current character's ext data to this character</param>
        /// <param name="src">Current character to copy the data from</param>
        public delegate void CopyExtendedDataFunc(ChaFile dst, ChaFile src);

        /// <summary>
        /// Get ChaControl that is using the specified ChaFileControl.
        /// </summary>
        public static ChaControl FileControlToChaControl(ChaFileControl fileControl)
        {
            return ChaControls.Single(x => x.chaFile == fileControl);
        }

        /// <summary>
        /// Get all extra behaviours for specified character. If null, returns extra behaviours for all characters.
        /// </summary>
        public static IEnumerable<CharaCustomFunctionController> GetBehaviours(ChaControl character = null)
        {
            if (character == null)
                return _registeredHandlers.SelectMany(x => x.Instances);

            return character.GetComponents<CharaCustomFunctionController>();
        }

        /// <summary>
        /// Get the first controller that was registered with the specified extendedDataId.
        /// </summary>
        public static ControllerRegistration GetRegisteredBehaviour(string extendedDataId)
        {
            return _registeredHandlers.Find(registration => string.Equals(registration.ExtendedDataId, extendedDataId, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get the first controller of the specified type that was registered. The type has to be an exact match.
        /// </summary>
        public static ControllerRegistration GetRegisteredBehaviour(Type controllerType)
        {
            return _registeredHandlers.Find(registration => registration.ControllerType == controllerType);
        }

        /// <summary>
        /// Get the first controller of the specified type that was registered with the specified extendedDataId. The type has to be an exact match.
        /// </summary>
        public static ControllerRegistration GetRegisteredBehaviour(Type controllerType, string extendedDataId)
        {
            return _registeredHandlers.Find(registration => registration.ControllerType == controllerType && string.Equals(registration.ExtendedDataId, extendedDataId, StringComparison.Ordinal));
        }

        /// <summary>
        /// Register new functionality that will be automatically added to all characters (where applicable).
        /// Offers easy API for saving and loading extended data, and for running logic to apply it to the characters.
        /// All necessary hooking and event subscribing is done for you. All you have to do is create a type
        /// that inherits from <code>CharaExtraBehaviour</code> (don't make instances, the API will make them for you).
        /// </summary>
        /// <typeparam name="T">Type with your custom logic to add to a character</typeparam>
        /// <param name="extendedDataId">Extended data ID used by this behaviour. Set to null if not used. Needed to copy the data in some situations.</param>
        public static void RegisterExtraBehaviour<T>(string extendedDataId) where T : CharaCustomFunctionController, new()
        {
            void BasicCopier(ChaFile dst, ChaFile src)
            {
                var extendedData = ExtendedSave.GetExtendedDataById(src, extendedDataId);
                if (extendedData != null && extendedData.data.Any())
                    ExtendedSave.SetExtendedDataById(dst, extendedDataId, extendedData);
            }

            var copier = extendedDataId == null ? (CopyExtendedDataFunc)null : BasicCopier;

            RegisterExtraBehaviour<T>(extendedDataId, copier);
        }

        /// <summary>
        /// Register new functionality that will be automatically added to all characters (where applicable).
        /// Offers easy API for saving and loading extended data, and for running logic to apply it to the characters.
        /// All necessary hooking and event subscribing is done for you. All you have to do is create a type
        /// that inherits from <code>CharaExtraBehaviour</code> (don't make instances, the API will make them for you).
        /// </summary>
        /// <typeparam name="T">Type with your custom logic to add to a character</typeparam>
        /// <param name="extendedDataId">Extended data ID used by this behaviour. Set to null if not used.</param>
        /// <param name="customDataCopier">Override default extended data copy logic</param>
        public static void RegisterExtraBehaviour<T>(string extendedDataId, CopyExtendedDataFunc customDataCopier) where T : CharaCustomFunctionController, new()
        {
            _registeredHandlers.Add(new ControllerRegistration(typeof(T), extendedDataId, customDataCopier));
        }

        internal static void Init()
        {
            HarmonyInstance.Create(typeof(Hooks).FullName).PatchAll(typeof(Hooks));

            // Cards -------------------------
            ExtendedSave.CardBeingSaved += OnCardBeingSaved;
            MakerAPI.ChaFileLoaded += (sender, args) =>
            {
                var chaControl = MakerAPI.GetCharacterControl();
                if (chaControl != null) ReloadChara(chaControl);
            };

            // Coordinates -------------------
            ExtendedSave.CoordinateBeingSaved += file =>
            {
                if (file == null) return;

                // Safe to assume we're in maker
                var character = MakerAPI.GetCharacterControl();
                if (character == null)
                {
                    Logger.Log(LogLevel.Error, "[KKAPI] OnCoordinateBeingSaved fired outside chara maker for " + file.coordinateName);
                    Logger.Log(LogLevel.Info, new StackTrace());
                    return;
                }

                OnCoordinateBeingSaved(character, file);
            };
            ExtendedSave.CoordinateBeingLoaded += file =>
            {
                if (Hooks.ClothesFileControlLoading || file == null) return;

                // Coord cards are loaded by loading into the character's nowCoordinate
                var cf = ChaControls.FirstOrDefault(x => x.nowCoordinate == file);
                if (cf != null)
                    OnCoordinateBeingLoaded(cf, file);
            };

            if(KoikatuAPI.EnableDebugLogging)
                RegisterExtraBehaviour<TestCharaCustomFunctionController>(null);
        }

        private static void CreateOrAddBehaviours(ChaControl target)
        {
            foreach (var handler in _registeredHandlers)
            {
                var existing = target.gameObject.GetComponents(handler.ControllerType)
                    .Cast<CharaCustomFunctionController>()
                    .FirstOrDefault(x => x.ExtendedDataId == handler.ExtendedDataId);

                if (existing == null)
                {
                    try
                    {
                        handler.CreateInstance(target);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(LogLevel.Error, e);
                    }
                }
            }
        }

        private static void OnCardBeingSaved(ChaFile chaFile)
        {
            Logger.Log(LogLevel.Debug, "[KKAPI] Character save: " + chaFile.parameter.fullname);

            var gamemode = KoikatuAPI.GetCurrentGameMode();
            foreach (var behaviour in GetBehaviours(MakerAPI.GetCharacterControl()))
                behaviour.OnCardBeingSavedInternal(gamemode);
        }

        private static void ReloadChara(ChaControl chaControl = null)
        {
            Logger.Log(LogLevel.Debug, "[KKAPI] Character load/reload: " + GetLogName(chaControl));

            var gamemode = KoikatuAPI.GetCurrentGameMode();
            foreach (var behaviour in GetBehaviours(chaControl))
                behaviour.OnReloadInternal(gamemode);

            try
            {
                CharacterReloaded?.Invoke(null, new CharaReloadEventArgs(chaControl));
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }
        }

        private static IEnumerator DelayedReloadChara(ChaControl chaControl)
        {
            yield return null;
            ReloadChara(chaControl);
        }

        private static void OnCoordinateBeingSaved(ChaControl character, ChaFileCoordinate coordinateFile)
        {
            Logger.Log(LogLevel.Debug, $"[KKAPI] Saving coord \"{coordinateFile.coordinateName}\" to chara \"{GetLogName(character)}\"" +
                                       $" / {(ChaFileDefine.CoordinateType)character.fileStatus.coordinateType}");

            foreach (var controller in GetBehaviours(character))
                controller.OnCoordinateBeingSavedInternal(coordinateFile);

            try
            {
                CoordinateSaving?.Invoke(null, new CoordinateEventArgs(character, coordinateFile));
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }
        }

        private static void OnCoordinateBeingLoaded(ChaControl character, ChaFileCoordinate coordinateFile)
        {
            Logger.Log(LogLevel.Debug, $"[KKAPI] Loading coord \"{coordinateFile.coordinateName}\" to chara \"{GetLogName(character)}\"" +
                                       $" / {(ChaFileDefine.CoordinateType)character.fileStatus.coordinateType}");

            foreach (var controller in GetBehaviours(character))
                controller.OnCoordinateBeingLoadedInternal(coordinateFile);

            try
            {
                CoordinateLoaded?.Invoke(null, new CoordinateEventArgs(character, coordinateFile));
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }
        }

        private static string GetLogName(ChaInfo chaInfo)
        {
            if (chaInfo == null) return "NULL";
            return chaInfo.chaFile?.charaFileName ?? chaInfo.fileParam?.fullname ?? "Unknown";
        }

        /// <summary>
        /// Fired after all CharaCustomFunctionController have updated.
        /// </summary>
        public static event EventHandler<CharaReloadEventArgs> CharacterReloaded;

        /// <summary>
        /// Fired after a coordinate card was loaded and all controllers were updated.
        /// Not filed if the coordinate file was not loaded into a character (so not during list updates). 
        /// </summary>
        public static event EventHandler<CoordinateEventArgs> CoordinateLoaded;

        /// <summary>
        /// Fired just before a coordinate card is saved, but after all controllers wrote their data.
        /// </summary>
        public static event EventHandler<CoordinateEventArgs> CoordinateSaving;
    }
}
