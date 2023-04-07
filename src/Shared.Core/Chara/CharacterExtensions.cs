using System;
using System.Collections.Generic;
using System.Linq;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
#if AI || HS2
using AIChara;
#endif

namespace KKAPI.Chara
{
    /// <summary>
    /// Extensions for use with ChaControl, ChaFile and similar
    /// </summary>
    public static class CharacterExtensions
    {
        /// <summary>
        /// Get ChaControl that is using this ChaFile if any exist.
        /// </summary>
        public static ChaControl GetChaControl(this ChaFile chaFile)
        {
            if (chaFile == null) return null;

            var cachedResult = CharacterApi.ChaControls.FirstOrDefault(control => control.chaFile == chaFile);
            if (cachedResult != null) return cachedResult;
#if KK
            if (Manager.Game.instance == null) return null;
            return Manager.Game.instance.Player?.charFile == chaFile ?
                Manager.Game.instance.Player.chaCtrl :
                Manager.Game.instance.HeroineList.FirstOrDefault(h => h.charFile == chaFile)?.chaCtrl;
#elif KKS
            return Manager.Game.Player?.charFile == chaFile ?
                Manager.Game.Player.chaCtrl : 
                Manager.Game.HeroineList.FirstOrDefault(h => h.charFile == chaFile)?.chaCtrl;
#elif EC
            return null;
#elif AI
            if (Manager.Map.instance == null) return null;
            return Manager.Map.instance.Player?.ChaControl?.chaFile == chaFile ?
                Manager.Map.instance.Player.ChaControl :
                Manager.Map.instance.AgentTable.FirstOrDefault(h => h.Value.ChaControl.chaFile == chaFile).Value?.ChaControl;
#elif HS2
            if (Manager.Game.instance == null) return null;
            return Manager.Game.instance.player?.chaFile == chaFile ?
                Manager.Game.instance.player.chaCtrl :
                Manager.Game.instance.heroineList.FirstOrDefault(h => h.chaFile == chaFile)?.chaCtrl;
#endif
        }

        /// <summary>
        /// Get cleaned up full name of this character, including its translation if available.
        /// </summary>
        /// <param name="chaFile">Character to get the name of.</param>
        /// <param name="describeIfEmpty">If the chaFile is null or the name is empty, a string describing this is returned. If set to false, an empty string is returned instead.</param>
        public static string GetFancyCharacterName(this ChaFile chaFile, bool describeIfEmpty = true)
        {
            if (chaFile?.parameter?.fullname == null) return describeIfEmpty ? "[NULL]" : string.Empty;

            var origName = chaFile.parameter.fullname.Trim();
            if (origName.Length == 0) return describeIfEmpty ? "[NO NAME]" : string.Empty;

            TranslationHelper.TryTranslate(origName, out var tl);
            if (tl != null)
            {
                tl = tl.Trim();
                if (tl.Length > 0 && origName != tl)
                    return $"{tl} ({origName})";
            }

            return origName;
        }

        #region Binding

        /// <summary>
        /// Function for pulling data from a controller to be set to a maker control.
        /// </summary>
        /// <typeparam name="TController">Type of a custom character function controller.</typeparam>
        /// <typeparam name="TValue">Type of the value used by the control.</typeparam>
        /// <param name="controller">Controller to pull the value from.</param>
        public delegate TValue GetValueForInterface<in TController, out TValue>(TController controller) where TController : CharaCustomFunctionController;

        /// <summary>
        /// Function for pushing data from a control to be set in a controller.
        /// </summary>
        /// <typeparam name="TController">Type of a custom character function controller.</typeparam>
        /// <typeparam name="TValue">Type of the value used by the control.</typeparam>
        /// <param name="controller">Controller to push the value to.</param>
        /// <param name="value">Value to be pushed to the controller.</param>
        public delegate void SetValueToController<in TController, in TValue>(TController controller, TValue value) where TController : CharaCustomFunctionController;

        /// <summary>
        /// Synchronize this maker control to the state of your custom <see cref="CharaCustomFunctionController"/>.
        /// When the control's value changes the change is sent to the controller, and when the character is reloaded 
        /// the change is automatically pulled from the controller into the control.
        /// </summary>
        /// <param name="guiEntry">Control to bind</param>
        /// <param name="getValue">Function that extracts the new value of the control from the controller.</param>
        /// <param name="setValue">Function that sets the new value from the control on the controller.</param>
        /// <typeparam name="TController">Type of the custom controller to bind the control to.</typeparam>
        /// <typeparam name="TValue">Type of the value used by the control.</typeparam>
        /// <exception cref="InvalidOperationException">The controller has not been registered in CharacterApi.</exception>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public static void BindToFunctionController<TController, TValue>(this BaseEditableGuiEntry<TValue> guiEntry, GetValueForInterface<TController, TValue> getValue, SetValueToController<TController, TValue> setValue) where TController : CharaCustomFunctionController
        {
            if (guiEntry == null) throw new ArgumentNullException(nameof(guiEntry));
            if (getValue == null) throw new ArgumentNullException(nameof(getValue));
            if (setValue == null) throw new ArgumentNullException(nameof(setValue));
            guiEntry.ThrowIfDisposed(nameof(guiEntry));

            var controllerType = typeof(TController);

            if (!CharacterApi.RegisteredHandlers.Select(x => x.ControllerType).Contains(controllerType))
                throw new InvalidOperationException($"The controller {controllerType.FullName} has not been registered in CharacterApi");

            void OnValueChanged(TValue newVal)
            {
                try
                {
                    var controller = (TController)MakerAPI.GetCharacterControl().GetComponent(controllerType);
                    setValue(controller, newVal);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Crash in {nameof(getValue)} function of binding for {guiEntry.GetType().FullName} - {ex}");
                }
            }

            guiEntry.ValueChanged.Subscribe(OnValueChanged);

            void OnReloadInterface(object sender, EventArgs args)
            {
                try
                {
                    var controller = (TController)MakerAPI.GetCharacterControl().GetComponent(controllerType);
                    guiEntry.Value = getValue(controller);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Crash in {nameof(setValue)} function of binding for {guiEntry.GetType().FullName} - {ex}");
                }
            }

            MakerAPI.ReloadCustomInterface += OnReloadInterface;

            void OnCleanup(object sender, EventArgs args)
            {
                MakerAPI.ReloadCustomInterface -= OnReloadInterface;
                MakerAPI.MakerExiting -= OnCleanup;
            }

            MakerAPI.MakerExiting += OnCleanup;
        }

        #endregion

        internal static readonly Dictionary<ChaFile, string> ChaFileFullPathLookup = new Dictionary<ChaFile, string>(); //todo add extension

        /// <summary>
        /// Gets full path to the file where this ChaFile was loaded from. Usually this means the character card,
        /// but can also point to a studio scene or a game save file if the character was contained inside them.
        /// If the ChaFile was loaded from memory or copied, this will most likely return null. Might not work in maker in some games (todo).
        /// </summary>
        public static string GetSourceFilePath(this ChaFile chaFile)
        {
            ChaFileFullPathLookup.TryGetValue(chaFile, out var fullPath);
            return fullPath;
        }
    }
}
