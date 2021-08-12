using KKAPI.Maker.UI;
using System;
using BepInEx;
using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using UnityEngine;

namespace ModdingAPI.CharaMaker
{
    public abstract class ControlFactory
    {
        public MakerCategory Category { get; set; }
        public BaseUnityPlugin Owner { get; set; }

        internal ControlFactory(MakerCategory category, BaseUnityPlugin owner)
        {
            Category = category;
            Owner = owner;
        }
    }
    public abstract class MakerControlFactory : ControlFactory
    {
        internal MakerControlFactory(MakerCategory category, BaseUnityPlugin owner) : base(category, owner)
        {
        }

        public abstract MakerButton CreateButton(string text);
        public abstract MakerColor CreateColor(string settingName, Color initialValue, bool useAlpha);
        public abstract MakerDropdown CreateDropdown(string settingName, int initialValue, params string[] options);
        public abstract MakerImage CreateImage(Texture texture);
        public abstract MakerRadioButtons CreateRadioButtons(string settingName, int initialValue, params string[] buttons);
        public abstract MakerSeparator CreateSeparator();
        public abstract MakerText CreateText(string text);
        public abstract MakerTextbox CreateTextbox(string settingName, string defaultValue);
        public abstract MakerToggle CreateToggle(string settingName, bool initialValue);

        public abstract SidebarToggle CreateSidebarToggle(string text, bool initialValue);
        public abstract SidebarSeparator CreateSidebarSeparator();
    }

    public class MakerControlFactory_KKS : MakerControlFactory
    {
        internal MakerControlFactory_KKS(MakerCategory category, BaseUnityPlugin owner) : base(category, owner)
        {
        }
        
        public override MakerButton CreateButton(string text) => new MakerButton(text, Category, Owner);
        public override MakerColor CreateColor(string settingName, Color initialValue, bool useAlpha) => new MakerColor(settingName, useAlpha, Category, initialValue, Owner);
        public override MakerDropdown CreateDropdown(string settingName, int initialValue, params string[] options) => new MakerDropdown(settingName, options, Category, initialValue, Owner);
        public override MakerImage CreateImage(Texture texture) => new MakerImage(texture, Category, Owner);
        public override MakerRadioButtons CreateRadioButtons(string settingName, int initialValue, params string[] buttons) => new MakerRadioButtons(Category, Owner, settingName, initialValue, buttons);
        public override MakerSeparator CreateSeparator() => new MakerSeparator(Category, Owner);
        public override MakerText CreateText(string text) => new MakerText(text, Category, Owner);
        public override MakerTextbox CreateTextbox(string settingName, string defaultValue) => new MakerTextbox(Category, settingName, defaultValue, Owner);
        public override MakerToggle CreateToggle(string settingName, bool initialValue) => new MakerToggle(Category, settingName, initialValue, Owner);
        
        public override SidebarToggle CreateSidebarToggle(string text, bool initialValue) => new SidebarToggle(text, initialValue, Owner);
        public override SidebarSeparator CreateSidebarSeparator() => new SidebarSeparator(Owner);
    }
    

    /// <summary>
    /// Event fired when character maker is starting and plugins are given an opportunity to register custom controls
    /// </summary>
    public class RegisterCustomControlsEvent : EventArgs
    {
        //todo overridable methods for creating / adding each type of control? make a separate class for it under api

        public MakerControlFactory GetControlFactory(MakerCategory controlCategory, BaseUnityPlugin owner) =>
            GetControlFactoryGameSpecific(controlCategory,owner);
        
        public MakerControlFactory_KKS GetControlFactoryGameSpecific(MakerCategory controlCategory, BaseUnityPlugin owner) =>
            new MakerControlFactory_KKS(controlCategory, owner);
        
        /// <summary>
        /// Add custom controls. If you want to use custom sub categories, register them by calling AddSubCategory.
        /// </summary>
        public T AddControl<T>(T control) where T : BaseGuiEntry
        {
            return API.Maker.AddControl<T>(control);
        }
        
        /// <summary>
        /// Add a control to the right sidebar in chara maker (the "Control Panel" where you set eye blinking, mouth expressions etc.)
        /// </summary>
        public T AddSidebarControl<T>(T control) where T : BaseGuiEntry, ISidebarControl
        {   
            return API.Maker.AddSidebarControl<T>(control);
        }

        /// <summary>
        /// Add a toggle to the bottom of the "Load character" window that allows for partial loading of characters.
        /// </summary>
        public MakerLoadToggle AddLoadToggle(string text, bool initialValue = true)
        {
            return MakerLoadToggle.AddLoadToggle(new MakerLoadToggle(text, initialValue));
        }

        /// <summary>
        /// Add a toggle to the bottom of the "Load coordinate/clothes" window that allows for partial loading of coordinate cards.
        /// </summary>
        public MakerCoordinateLoadToggle AddCoordinateLoadToggle(string text, bool initialValue = true)
        {
            return MakerCoordinateLoadToggle.AddLoadToggle(new MakerCoordinateLoadToggle(text, initialValue));
        }
    }
}
