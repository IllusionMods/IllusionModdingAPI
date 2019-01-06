using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using ExtensibleSaveFormat;

namespace MakerAPI
{
    internal static partial class CharacterApi
    {
        private static readonly HashSet<ChaControl> ChaControls = new HashSet<ChaControl>();

        private static readonly HashSet<Type> RegisteredHandlers = new HashSet<Type>();

        public static ChaControl FileControlToChaControl(ChaFileControl fileControl)
        {
            return ChaControls.Single(x => x.chaFile == fileControl);
        }

        /// <summary>
        /// Get all extra behaviours for specified character. If null, returns extra behaviours for all characters.
        /// </summary>
        public static IEnumerable<CharaExtraBehaviour> GetBehaviours(ChaControl character = null)
        {
            if (character == null)
                return ChaControls.SelectMany(x => x.GetComponents<CharaExtraBehaviour>());

            return character.GetComponents<CharaExtraBehaviour>();
        }

        /// <summary>
        /// Register new functionality that will be automatically added to all characters (where applicable).
        /// Offers easy API for saving and loading extended data, and for running logic to apply it to the characters.
        /// All necessary hooking and event subscribing is done for you. All you have to do is create a type
        /// that inherits from <code>CharaExtraBehaviour</code> (don't make instances, the API will make them for you).
        /// </summary>
        /// <typeparam name="T">Type with your custom logic to add to a character</typeparam>
        public static void RegisterExtraBehaviour<T>() where T : CharaExtraBehaviour, new()
        {
            RegisteredHandlers.Add(typeof(T));
        }

        internal static void Init()
        {
            Hooks.InstallHook();
            ExtendedSave.CardBeingSaved += OnCardBeingSaved;
        }

        private static void CreateOrAddBehaviours(ChaControl target)
        {
            foreach (var handler in RegisteredHandlers)
            {
                if (target.gameObject.GetComponent(handler) == null)
                    target.gameObject.AddComponent(handler);
            }
        }

        private static void OnCardBeingSaved(ChaFile _)
        {
            if (!MakerAPI.Instance.InsideMaker) return;

            foreach (var behaviour in GetBehaviours(MakerAPI.Instance.GetCharacterControl()))
            {
                try
                {
                    behaviour.OnCardBeingSaved();
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex);
                }
            }
        }

        private static void ReloadChara(ChaControl chaControl = null)
        {
            foreach (var behaviour in GetBehaviours(chaControl))
                behaviour.OnReload();
        }
    }
}
