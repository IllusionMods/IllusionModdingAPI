using System;
using System.Linq;
using Character;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Studio;
using UniRx;
using UnityEngine;

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
        public static Human GetChaControl(this CustomParameter chaFile)
        {
            return CharacterApi.ChaControls.FirstOrDefault(x => x.customParam == chaFile);
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

        public static string GetCharacterName(this Human h)
        {
            if (StudioAPI.InsideStudio)
            {
                var charaName = StudioAPI.GetSelectedCharacters().FirstOrDefault(x => x.charInfo.human == h)?.charStatus.name;
                if (!string.IsNullOrEmpty(charaName)) return charaName;
            }
            //return h is Female f ? f.HeroineID.ToString() : ((Male) h).MaleID.ToString();
            return h is Female f
                ? Female.HeroineName(f.HeroineID)
                : Male.MaleName(((Male)h).MaleID);
        }
    }
}
