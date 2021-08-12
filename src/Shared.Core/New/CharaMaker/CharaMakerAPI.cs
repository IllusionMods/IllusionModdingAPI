using System;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
#if KK || KKS || EC
using ChaCustom;
#elif AI || HS2
using CharaCustom;
using AIChara;
#endif

namespace ModdingAPI.CharaMaker
{
    /// <summary>
    /// Provides a way to add custom items to the in-game Character Maker, and gives useful methods for interfacing with the maker.
    /// </summary>
    public abstract class CharaMakerAPI
    {
        /// <summary>
        /// Add custom controls. If you want to use custom sub categories, register them by calling AddSubCategory.
        /// </summary>
        internal abstract T AddControl<T>(T control) where T : BaseGuiEntry;

        /// <summary>
        /// Add custom sub categories. They need to be added before maker starts loading,
        /// or in the <see cref="RegisterCustomSubCategories"/> event.
        /// </summary>
        internal abstract void AddSubCategory(MakerCategory category);

        /// <summary>
        /// Add a control to the right sidebar in chara maker (the "Control Panel" where you set eye blinking, mouth expressions etc.)
        /// </summary>
        internal abstract T AddSidebarControl<T>(T control) where T : BaseGuiEntry, ISidebarControl;

        /// <summary>
        /// Add a control to the accessory selection and settings window.
        /// For editable controls that depend on the selected accessory use <see cref="AddEditableAccessoryWindowControl{T,TVal}"/>.
        /// </summary>
        internal abstract T AddAccessoryWindowControl<T>(T control) where T : BaseGuiEntry;

        /// <summary>
        /// Add a control to the accessory selection and settings window. The control is wrapped to properly respond to changes in selected accessory slot (has unique values for each slot).
        /// </summary>
        internal abstract AccessoryControlWrapper<T, TVal> AddEditableAccessoryWindowControl<T, TVal>(T control) where T : BaseEditableGuiEntry<TVal>;


        /// <summary>
        /// 0 is male, 1 is female
        /// </summary>
        public abstract int GetMakerSex();

        /// <summary>
        /// Returns current maker logic instance.
        /// </summary>
        public abstract CustomBase GetMakerBase();

        /// <summary>
        /// Get the ChaControl of the character serving as a preview in character maker.
        /// Outside of character maker and early on in maker load process this returns null.
        /// </summary>
        public abstract ChaControl GetMainCharacter(); //todo return a wrapper?

        /// <summary>
        /// Check if the maker was loaded from within classroom select screen in main game
        /// If false, maker was opened from main menu
        /// </summary>
        public abstract bool IsInsideMainGameMaker();

        /// <summary>
        /// Index of currently selected maker coordinate
        /// </summary>
        public abstract int GetCurrentCoordinateType();

        /// <summary>
        /// This event is fired every time the character maker is being loaded, near the very beginning.
        /// This is the only chance to add custom sub categories. Custom controls can be added now on later in <see cref="MakerBaseLoaded"/>.
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker start.
        /// It's recommended to completely clear your GUI state in <see cref="MakerExiting"/> in preparation for loading into maker again.
        /// </summary>
        public virtual event EventHandler<RegisterSubCategoriesEvent> RegisterCustomSubCategories; //todo switch to safeevent

        /// <summary>
        /// Early in the process of maker loading. Most game components are initialized and had their Start methods ran.
        /// Warning: Some components and objects might not be loaded or initialized yet, especially if they are mods.
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
        /// </summary>
        public virtual event EventHandler<RegisterCustomControlsEvent> MakerStartedLoading;

        /// <summary>
        /// Maker is fully loaded. Use to load mods that rely on something that is loaded late, else use MakerStartedLoading.
        /// This is the last chance to add custom controls!
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
        /// </summary>
        public virtual event EventHandler<RegisterCustomControlsEvent> MakerBaseLoaded;

        /// <summary>
        /// Maker is fully loaded and the user has control.
        /// Warning: Avoid loading mods or doing anything heavy in this event.
        /// </summary>
        public virtual event EventHandler MakerFinishedLoading;

        /// <summary>
        /// Fired after the user exits the maker. Use this to clean up any references and resources.
        /// You want to return to the state you were in before maker was loaded.
        /// </summary>
        public virtual event EventHandler MakerExiting;

        /// <summary>
        /// ChaFile of the character currently opened in maker. Do not use to save extended data, or it will be lost when saving the card.
        /// Use ChaFile from <code>ExtendedSave.CardBeingSaved</code> event to save extended data instead.
        /// </summary>
        public abstract ChaFile LastLoadedChaFile { get; }

        /// <summary>
        /// Fired after character or coordinate is loaded in maker, after all controllers had their events fired.
        /// This event is only fired when inside the character maker. Use this to update values of custom controls.
        /// EventArgs can be either <see cref="CharaReloadEventArgs"/> or <see cref="CoordinateEventArgs"/> depending on why the reload happened.
        /// </summary>
        public virtual event EventHandler ReloadCustomInterface;

        /// <summary>
        /// Firen whenever <see cref="InsideMaker"/> changes. This is the earliest event fired when user starts the character maker.
        /// </summary>
        public virtual event EventHandler InsideMakerChanged;

        /// <summary>
        /// The maker scene is currently loaded. It might still be loading!
        /// </summary>
        public abstract bool InsideMaker { get; }

        /// <summary>
        /// Maker is fully loaded and running
        /// </summary>
        public abstract bool InsideAndLoaded { get; }

        /// <summary>
        /// Get values of the default partial load checkboxes present at the bottom of the 
        /// character load window (load face, body, hair, parameters, clothes).
        /// Returns null if the values could not be collected (safe to assume it's the same as being enabled).
        /// </summary>
        public virtual CharacterLoadFlags GetCharacterLoadFlags()
        {
            if (!InsideAndLoaded) return null;
            return MakerInterfaceCreator.GetCharacterLoadFlags();
        }

        /// <summary>
        /// Get which parts of the coordinate will be loaded when loading a clothing card in character maker.
        /// Returns null if the values could not be collected (safe to assume it's the same as being enabled).
        /// </summary>
        public virtual CoordinateLoadFlags GetCoordinateLoadFlags()
        {
            if (!InsideAndLoaded) return null;
            return MakerInterfaceCreator.GetCoordinateLoadFlags();
        }

        /// <summary>
        /// Check if maker interface is currently visible and not covered by settings screen or other things.
        /// Useful for knowing when to display OnGui mod windows in maker.
        /// </summary>
        public abstract bool IsInterfaceUnobstructed();
    }
}
