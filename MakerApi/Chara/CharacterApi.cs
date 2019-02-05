using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using ExtensibleSaveFormat;

namespace MakerAPI.Chara
{
    public static partial class CharacterApi
    {
        private static readonly HashSet<ChaControl> ChaControls = new HashSet<ChaControl>();

        private static readonly List<KeyValuePair<Type, string>> RegisteredHandlers = new List<KeyValuePair<Type, string>>();
        private static readonly List<CopyExtendedDataFunc> DataCopiers = new List<CopyExtendedDataFunc>();

        /// <summary>
        /// Override to supply custom extended data copying logic.
        /// By default copies all data under <code>ExtendedDataId</code> by reference.
        /// </summary>
        /// <param name="dst">Copy current character's ext data to this character</param>
        /// <param name="src">Current character to copy the data from</param>
        public delegate void CopyExtendedDataFunc(ChaFile dst, ChaFile src);

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
                return ChaControls.SelectMany(x => x.GetComponents<CharaCustomFunctionController>());

            return character.GetComponents<CharaCustomFunctionController>();
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
            RegisteredHandlers.Add(new KeyValuePair<Type, string>(typeof(T), extendedDataId));
            if (customDataCopier != null) DataCopiers.Add(customDataCopier);
        }

        internal static void Init()
        {
            Hooks.InstallHook();
            ExtendedSave.CardBeingSaved += OnCardBeingSaved;
            MakerAPI.Instance.ChaFileLoaded += (sender, args) =>
            {
                var chaControl = MakerAPI.Instance.GetCharacterControl();
                if (chaControl != null) ReloadChara(chaControl); //chaControl.StartCoroutine(DelayedReloadChara(chaControl));
            };
        }

        private static void CreateOrAddBehaviours(ChaControl target)
        {
            foreach (var handler in RegisteredHandlers)
            {
                var existing = target.gameObject.GetComponents(handler.Key)
                    .Cast<CharaCustomFunctionController>()
                    .FirstOrDefault(x => x.ExtendedDataId == handler.Value);

                if (existing == null)
                {
                    try
                    {
                        var newBehaviour = (CharaCustomFunctionController)target.gameObject.AddComponent(handler.Key);
                        newBehaviour.ExtendedDataId = handler.Value;
                    }
                    catch (Exception e)
                    {
                        Logger.Log(LogLevel.Error, e);
                    }
                }
            }
        }

        private static void OnCardBeingSaved(ChaFile _)
        {
            var gamemode = MakerAPI.Instance.GetCurrentGameMode();
            foreach (var behaviour in GetBehaviours(MakerAPI.Instance.GetCharacterControl()))
            {
                try
                {
                    behaviour.OnCardBeingSaved(gamemode);
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e);
                }
            }
        }

        private static void ReloadChara(ChaControl chaControl = null)
        {
            var gamemode = MakerAPI.Instance.GetCurrentGameMode();
            foreach (var behaviour in GetBehaviours(chaControl))
                behaviour.OnReload(gamemode);
        }

        private static IEnumerator DelayedReloadChara(ChaControl chaControl)
        {
            yield return null;
            ReloadChara(chaControl);
        }
    }
}
