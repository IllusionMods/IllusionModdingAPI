using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using ChaCustom;
using Harmony;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KKAPI.Maker
{
    /// <summary>
    /// Collection of methods useful for interfacing with character accessories. Has methods both for chara maker and everywhere else.
    /// Abstracts away MoreAccessories so you don't have to worry if it's installed or not.
    /// </summary>
    public static class AccessoriesApi
    {
        private static Type _moreAccsType;

        internal static void Init()
        {
            try
            {
                _moreAccsType = Type.GetType("MoreAccessoriesKOI.MoreAccessories, MoreAccessories, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

                if (_moreAccsType != null)
                    _moreaAccessoriesInstance = Object.FindObjectOfType(_moreAccsType);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            MakerAPI.InsideMakerChanged += MakerAPI_InsideMakerChanged;
        }

        private static void MakerAPI_InsideMakerChanged(object sender, EventArgs e)
        {
            if (MakerAPI.InsideMaker)
            {
                var cvsAccessoryField = AccessTools.Field(typeof(CustomAcsParentWindow), "cvsAccessory");
                var cvsAccessories = (CvsAccessory[])cvsAccessoryField.GetValue(Object.FindObjectOfType<CustomAcsParentWindow>());

                var changeSlot = Object.FindObjectOfType<CustomAcsChangeSlot>();
                _accessorySlotCanvasGroup = changeSlot.GetComponent<CanvasGroup>();

                if (MoreAccessoriesInstalled)
                {
                    var m = AccessTools.Method(_moreAccsType, "GetCvsAccessory");
                    _getCvsAccessory = i => i < 20 ? cvsAccessories[i] : (CvsAccessory)m.Invoke(_moreaAccessoriesInstance, new object[] { i });

                    var slots = AccessTools.Field(_moreAccsType, "_additionalCharaMakerSlots");
                    var additionalSlotCollection = (ICollection)slots.GetValue(_moreaAccessoriesInstance);
                    _getAdditionalCvsAccessoryCount = () => additionalSlotCollection.Count;

                    var getIndexMethod = AccessTools.Method(_moreAccsType, "GetSelectedMakerIndex");
                    _getSelectedAccessoryIndex = () => (int)getIndexMethod.Invoke(_moreaAccessoriesInstance, null);

                }
                else
                {
                    _getCvsAccessory = i => cvsAccessories[i];
                    _getSelectedAccessoryIndex = () => Array.FindIndex(changeSlot.items, info => info.tglItem.isOn);
                }
            }
            else
            {
                _getCvsAccessory = null;
                _getAdditionalCvsAccessoryCount = null;
            }
        }

        private static Func<int, CvsAccessory> _getCvsAccessory;
        private static Func<int> _getAdditionalCvsAccessoryCount;
        private static Func<int> _getSelectedAccessoryIndex;
        private static CanvasGroup _accessorySlotCanvasGroup;
        private static Object _moreaAccessoriesInstance;

        /// <summary>
        /// Get accessory UI entry in maker.
        /// Only works inside chara maker.
        /// </summary>
        public static CvsAccessory GetCvsAccessory(int index)
        {
            if (!MakerAPI.InsideMaker) throw new InvalidOperationException("Can only call GetCvsAccessory when inside Chara Maker");
            return _getCvsAccessory(index);
        }

        /// <summary>
        /// Get the index of the currently selected accessory tab under Accessories group in Chara Maker.
        /// If none are selected or chara maker is not opened, returns -1.
        /// </summary>
        public static int GetSelectedAccessoryIndex()
        {
            if (!MakerAPI.InsideMaker) return -1;
            return _getSelectedAccessoryIndex.Invoke();
        }

        /// <summary>
        /// Get count of the UI entries for accessories (accessory slots).
        /// Returns 0 outside of chara maker.
        /// </summary>
        /// <returns></returns>
        public static int GetCvsAccessoryCount()
        {
            if (!MakerAPI.InsideMaker) return 0;
            if (_getAdditionalCvsAccessoryCount == null) return 20;
            return _getAdditionalCvsAccessoryCount.Invoke() + 20;
        }

        /// <summary>
        /// Get the ChaControl that owns this accessory
        /// </summary>
        public static ChaControl GetOwningChaControl(this ChaAccessoryComponent accessoryComponent)
        {
            return accessoryComponent.GetComponentInParent<ChaControl>();
        }

        /// <summary>
        /// Returns true if the accessory tab in maker is currently selected.
        /// </summary>
        public static bool AccessoryCanvasVisible => _accessorySlotCanvasGroup != null && _accessorySlotCanvasGroup.alpha.Equals(1f);

        /// <summary>
        /// True if the MoreAccessories mod is installed.
        /// Avoid relying on this and instead use other methods in this class since they will handle this for you.
        /// </summary>
        public static bool MoreAccessoriesInstalled => _moreAccsType != null;

        /// <summary>
        /// Get slot index of his accessory, useful for referencing to the accesory in extended data.
        /// TODO Not finished, most likely buggy
        /// </summary>
        public static int GetAccessoryIndex(this ChaAccessoryComponent accessoryComponent)
        {
            var chaControl = GetOwningChaControl(accessoryComponent);
            var index = Array.IndexOf(chaControl.cusAcsCmp, accessoryComponent);
            if (index > 0)
                return index;

            // todo ask joan to expose a method for getting this
            if (MoreAccessoriesInstalled)
            {
                try
                {
                    var d = Traverse.Create(_moreaAccessoriesInstance).Field("_accessoriesByChar").GetValue<IDictionary>();
                    var val = d[chaControl.chaFile];
                    var components = Traverse.Create(val).Field("cusAcsCmp").GetValue<List<ChaAccessoryComponent>>();

                    index = components.IndexOf(accessoryComponent);
                    return index < 0 ? index : index + 20;
                }
                catch (Exception ex)
                {
                    BepInEx.Logger.Log(LogLevel.Warning, ex);
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the accessory given a slot index.
        /// TODO Not finished, most likely buggy
        /// </summary>
        public static ChaAccessoryComponent GetAccessory(this ChaControl character, int accessoryIndex)
        {
            if (accessoryIndex < 20)
                return character.cusAcsCmp[accessoryIndex];

            // todo ask joan to expose a method for getting this
            if (MoreAccessoriesInstalled)
            {
                try
                {
                    var d = Traverse.Create(_moreaAccessoriesInstance).Field("_accessoriesByChar").GetValue<IDictionary>();
                    var val = d[character.chaFile];
                    var components = Traverse.Create(val).Field("cusAcsCmp").GetValue<List<ChaAccessoryComponent>>();

                    return components[accessoryIndex + 20];
                }
                catch (Exception ex)
                {
                    BepInEx.Logger.Log(LogLevel.Warning, ex);
                }
            }

            return null;
        }
    }
}
