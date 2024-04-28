﻿using System;
using System.Collections;
using KKAPI.Chara;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using KKAPI.Utilities;
using UnityEngine;
#if KK || KKS || EC
using ChaCustom;
#elif AI || HS2
using CharaCustom;
using AIChara;
#endif

namespace KKAPI.Maker
{
    /// <summary>
    /// Provides a way to add custom items to the in-game Character Maker, and gives useful methods for interfacing with the maker.
    /// </summary>
    public static partial class MakerAPI
    {
        /// <summary>
        /// Add custom controls. If you want to use custom sub categories, register them by calling AddSubCategory.
        /// </summary>
        public static T AddControl<T>(T control) where T : BaseGuiEntry
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            control.ThrowIfDisposed(nameof(control));
            MakerInterfaceCreator.AddControl(control);
            return control;
        }

        /// <summary>
        /// Add custom sub categories. They need to be added before maker starts loading,
        /// or in the <see cref="RegisterCustomSubCategories"/> event.
        /// </summary>
        internal static void AddSubCategory(MakerCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            MakerInterfaceCreator.AddSubCategory(category);
        }

        /// <summary>
        /// Add a control to the right sidebar in chara maker (the "Control Panel" where you set eye blinking, mouth expressions etc.)
        /// </summary>
        public static T AddSidebarControl<T>(T control) where T : BaseGuiEntry, ISidebarControl
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            control.ThrowIfDisposed(nameof(control));
            MakerInterfaceCreator.AddSidebarControl(control);
            return control;
        }

        /// <summary>
        /// Add a control to the accessory selection and settings window.
        /// For editable controls that depend on the selected accessory use <see cref="AddEditableAccessoryWindowControl{T,TVal}"/>.
        /// </summary>
        public static T AddAccessoryWindowControl<T>(T control) where T : BaseGuiEntry
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            control.ThrowIfDisposed(nameof(control));
            MakerInterfaceCreator.AddAccessoryWindowControl(control);
            return control;
        }
#if KK || KKS || EC //todo AI and HS2
        /// <summary>
        /// Add a control to the accessory selection and settings window.
        /// For editable controls that depend on the selected accessory use <see cref="AddEditableAccessoryWindowControl{T,TVal}"/>.
        /// To have API control visibility automatically set automate_visible to true.
        /// </summary>
        public static T AddAccessoryWindowControl<T>(T control, bool automate_visible) where T : BaseGuiEntry
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            control.ThrowIfDisposed(nameof(control));
            MakerInterfaceCreator.AddAccessoryWindowControl(control, automate_visible);
            return control;
        }
#endif
        /// <summary>
        /// Add a control to the accessory selection and settings window. The control is wrapped to properly respond to changes in selected accessory slot (has unique values for each slot).
        /// </summary>
        public static AccessoryControlWrapper<T, TVal> AddEditableAccessoryWindowControl<T, TVal>(T control) where T : BaseEditableGuiEntry<TVal>
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            control.ThrowIfDisposed(nameof(control));
            MakerInterfaceCreator.AddAccessoryWindowControl(control);
            return new AccessoryControlWrapper<T, TVal>(control);
        }

#if KK || KKS || EC //todo AI and HS2
        /// <summary>
        /// Add a control to the accessory selection and settings window. The control is wrapped to properly respond to changes in selected accessory slot (has unique values for each slot).
        /// To have API control visibility automatically set automate_visible to true.
        /// </summary>
        public static AccessoryControlWrapper<T, TVal> AddEditableAccessoryWindowControl<T, TVal>(T control, bool automate_visible) where T : BaseEditableGuiEntry<TVal>
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            control.ThrowIfDisposed(nameof(control));
            MakerInterfaceCreator.AddAccessoryWindowControl(control, automate_visible);
            return new AccessoryControlWrapper<T, TVal>(control);
        }
#endif
        /// <summary>
        /// 0 is male, 1 is female
        /// </summary>
        public static int GetMakerSex()
#if KK || KKS || EC


        {
            var res = (int)Hooks._modeSex;
#if DEBUG
            if(GetMakerBase().modeSex != res)
                KoikatuAPI.Logger.LogMessage($"Difference in modesex hooks={res} base={GetMakerBase().modeSex}");
#endif
            return res;
        }
#elif AI || HS2
            => CharaCustom.CharaCustom.modeSex;
#endif

        /// <summary>
        /// Returns current maker logic instance.
        /// Same as <see cref="Singleton{CustomBase}.Instance"/>
        /// </summary>
        public static CustomBase GetMakerBase() => CustomBase.Instance;

        /// <summary>
        /// Get the ChaControl of the character serving as a preview in character maker.
        /// Outside of character maker and early on in maker load process this returns null.
        /// </summary>
        public static ChaControl GetCharacterControl() => InsideMaker ? GetMakerBase()?.chaCtrl : null;

#if KK
        /// <summary>
        /// Check if the maker was loaded from within classroom select screen in main game
        /// </summary>
        public static bool IsInsideClassMaker() => InsideMaker && Manager.Scene.Instance.NowSceneNames.Contains("ClassRoomSelect");
#elif KKS
        /// <summary>
        /// Check if the maker was loaded from within classroom select screen in main game
        /// </summary>
        [Obsolete("Not implemented, always false")]
        public static bool IsInsideClassMaker() => false;
#endif

#if KK || KKS
        /// <summary>
        /// Currently selected maker coordinate
        /// </summary>
        public static ChaFileDefine.CoordinateType GetCurrentCoordinateType() => (ChaFileDefine.CoordinateType)GetMakerBase().chaCtrl.fileStatus.coordinateType;
#endif

        /// <summary>
        /// This event is fired every time the character maker is being loaded, near the very beginning.
        /// This is the only chance to add custom sub categories. Custom controls can be added now on later in <see cref="MakerBaseLoaded"/>.
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker start.
        /// It's recommended to completely clear your GUI state in <see cref="MakerExiting"/> in preparation for loading into maker again.
        /// </summary>
        public static event EventHandler<RegisterSubCategoriesEvent> RegisterCustomSubCategories;

        /// <summary>
        /// Early in the process of maker loading. Most game components are initialized and had their Start methods ran.
        /// Warning: Some components and objects might not be loaded or initialized yet, especially if they are mods.
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
        /// </summary>
        public static event EventHandler<RegisterCustomControlsEvent> MakerStartedLoading;

        /// <summary>
        /// Maker is fully loaded. Use to load mods that rely on something that is loaded late, else use MakerStartedLoading.
        /// This is the last chance to add custom controls!
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
        /// </summary>
        public static event EventHandler<RegisterCustomControlsEvent> MakerBaseLoaded;

        /// <summary>
        /// Maker is fully loaded and the user has control.
        /// Warning: Avoid loading mods or doing anything heavy in this event.
        /// </summary>
        public static event EventHandler MakerFinishedLoading;

        /// <summary>
        /// Fired after the user exits the maker. Use this to clean up any references and resources.
        /// You want to return to the state you were in before maker was loaded.
        /// </summary>
        public static event EventHandler MakerExiting;

        private static void OnRegisterCustomSubCategories()
        {
            MakerInterfaceCreator.InitializeMaker();

            var args = new RegisterSubCategoriesEvent();
            RegisterCustomSubCategories.SafeInvoke(handler => handler.Invoke(KoikatuAPI.Instance, args));
        }

        private static void OnMakerStartedLoading()
        {
            var args = new RegisterCustomControlsEvent();
            MakerStartedLoading.SafeInvoke(handler => handler.Invoke(KoikatuAPI.Instance, args));
        }

        private static void OnMakerFinishedLoading()
        {
            MakerFinishedLoading.SafeInvoke(handler => handler.Invoke(KoikatuAPI.Instance, EventArgs.Empty));
#if KK || KKS ||EC
            AccessoriesApi.AutomaticControlVisibility();
#endif
            _makerLoaded = true;
        }

        private static void OnMakerBaseLoaded()
        {
            var args = new RegisterCustomControlsEvent();
            MakerBaseLoaded.SafeInvoke(handler => handler.Invoke(KoikatuAPI.Instance, args));

            MakerInterfaceCreator.InitializeGuiEntries();
        }

        private static void OnCreateCustomControls()
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnCreateCustomControls), null);

            eLogger.PluginStart();
            MakerInterfaceCreator.CreateCustomControls();
            eLogger.PluginEnd("MakerInterfaceCreator.CreateCustomControls");

            // todo prevent reloads from happening before this point? this needs testing in AIS and HS2
            OnChaFileLoaded(KoikatuAPI.Instance, new ChaFileLoadedEventArgs(null, (byte)GetMakerSex(), true, true, true, true, true, GetCharacterControl().chaFile, LastLoadedChaFile), eLogger);
            OnReloadInterface(KoikatuAPI.Instance, new CharaReloadEventArgs(GetCharacterControl()), eLogger);

            eLogger.End();
        }

        private static void OnMakerExiting()
        {
            MakerExiting.SafeInvoke(handler => handler.Invoke(KoikatuAPI.Instance, EventArgs.Empty));

            MakerInterfaceCreator.RemoveCustomControls();
        }

        /// <summary>
        /// Use to avoid unnecessary processing cards when they are loaded to the character list.
        /// For example, don't load extended data for these characters since it's never used.
        /// </summary>
        public static bool CharaListIsLoading { get; private set; }

        /// <summary>
        /// ChaFile of the character currently opened in maker. Do not use to save extended data, or it will be lost when saving the card.
        /// Use ChaFile from <code>ExtendedSave.CardBeingSaved</code> event to save extended data instead.
        /// </summary>
        public static ChaFile LastLoadedChaFile => InsideMaker ? (Hooks.InternalLastLoadedChaFile ?? GetCharacterControl()?.chaFile) : null;

        /// <summary>
        /// Fired when the current ChaFile in maker is being changed by loading other cards or coordinates.
        /// This event is only fired when inside the character maker.
        /// 
        /// You might need to wait for the next frame with <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/> before handling this.
        /// </summary>
        [Obsolete("Use ReloadCustomInterface instead")]
        public static event EventHandler<ChaFileLoadedEventArgs> ChaFileLoaded;

        private static void OnChaFileLoaded(object sender, ChaFileLoadedEventArgs args, ApiEventExecutionLogger eventLogger)
        {
            ChaFileLoaded.SafeInvokeWithLogging(handler => handler.Invoke(sender, args), nameof(ChaFileLoaded), eventLogger);
        }

        /// <summary>
        /// Fired after character or coordinate is loaded in maker, after all controllers had their events fired.
        /// This event is only fired when inside the character maker. Use this to update values of custom controls.
        /// EventArgs can be either <see cref="CharaReloadEventArgs"/> or <see cref="CoordinateEventArgs"/> depending on why the reload happened.
        /// </summary>
        public static event EventHandler ReloadCustomInterface;

        internal static void OnReloadInterface(object sender, EventArgs args, ApiEventExecutionLogger eventLogger)
        {
            ReloadCustomInterface.SafeInvokeWithLogging(handler => handler.Invoke(sender, args), nameof(ReloadCustomInterface), eventLogger);
#if KK || KKS || EC
            AccessoriesApi.AutomaticControlVisibility();
#endif
        }

#if KK || KKS || EC
        /// <summary>
        /// Fired when the visbility state of accessory controls, added by <see cref="AddAccessoryWindowControl{T}"/>, should change state if not managed automatically by the api.
        /// </summary>
        public static event EventHandler<AccessoryContolVisibilityArgs> AccessoryContolVisibility;

        internal static void OnVisibilityTrigger(AccessoryContolVisibilityArgs args)
        {
            AccessoryContolVisibility.SafeInvoke(handler => handler.Invoke(KoikatuAPI.Instance, args));
        }
#endif

        private static bool _insideMaker;
        private static bool _makerLoaded;

        /// <summary>
        /// Firen whenever <see cref="InsideMaker"/> changes. This is the earliest event fired when user starts the character maker.
        /// </summary>
        public static event EventHandler InsideMakerChanged;

        /// <summary>
        /// The maker scene is currently loaded. It might still be loading!
        /// </summary>
        public static bool InsideMaker
        {
            get => _insideMaker;
            private set
            {
                if (_insideMaker != value)
                {
                    _insideMaker = value;

                    InsideMakerChanged.SafeInvoke(handler => handler.Invoke(KoikatuAPI.Instance, EventArgs.Empty));
                }

                if (!_insideMaker)
                    _makerLoaded = false;
            }
        }

        /// <summary>
        /// Maker is fully loaded and running
        /// </summary>
        public static bool InsideAndLoaded => InsideMaker && _makerLoaded;

        /// <summary>
        /// Get values of the default partial load checkboxes present at the bottom of the 
        /// character load window (load face, body, hair, parameters, clothes).
        /// Returns null if the values could not be collected (safe to assume it's the same as being enabled).
        /// </summary>
        public static CharacterLoadFlags GetCharacterLoadFlags()
        {
            if (!InsideAndLoaded) return null;
            return MakerInterfaceCreator.GetCharacterLoadFlags();
        }

        /// <summary>
        /// Get which parts of the coordinate will be loaded when loading a clothing card in character maker.
        /// Returns null if the values could not be collected (safe to assume it's the same as being enabled).
        /// </summary>
        public static CoordinateLoadFlags GetCoordinateLoadFlags()
        {
            if (!InsideAndLoaded) return null;
            return MakerInterfaceCreator.GetCoordinateLoadFlags();
        }

        internal static void Init(bool insideStudio)
        {
            if (insideStudio) return;

            Hooks.Init();
        }

        /// <summary>
        /// Check if maker interface is currently visible and not obscured by settings screen or other things.
        /// Useful for knowing when to display OnGui mod windows in maker.
        /// </summary>
        public static bool IsInterfaceVisible()
        {
            // Check if maker is loaded
            if (!InsideMaker)
                return false;
            var mbase = GetMakerBase();
            if (mbase == null || mbase.chaCtrl == null)
                return false;

            // Check if the loading screen is currently visible
            if (SceneApi.GetIsNowLoadingFade())
                return false;

#if KK || KKS || EC
            // Check if UI is hidden (by pressing space)
            if (mbase.customCtrl.hideFrontUI)
                return false;
#endif

            // Check if settings screen, game exit message box or similar are on top of the maker UI
            // In KK class maker the AddSceneName is set to CustomScene, but in normal maker it's empty
            var addScene = SceneApi.GetAddSceneName();
            if (!string.IsNullOrEmpty(addScene) && addScene != "CustomScene" || SceneApi.GetIsOverlap())
                return false;

#if KK || KKS || EC
            // Exit maker / save character dialog boxes
            if (!ccwGo)
                ccwGo = GameObject.FindObjectOfType<CustomChangeSystemMenu>()?.checkWindow.gameObject;
            else if (ccwGo.activeSelf)
                return false;
#endif

            return true;
        }

        private static GameObject ccwGo;
    }
}
