using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Maker
{
    /// <summary>
    /// Collection of methods useful for interfacing with character accessories. Has methods both for chara maker and
    /// everywhere else.
    /// Abstracts away MoreAccessories so you don't have to worry if it's installed or not.
    /// </summary>
    public static partial class AccessoriesApi
    {
        private static Object _moreAccessoriesInstance;
        private static Type _moreAccessoriesType;

        private static GameObject _accessoryWindowObject;
        private static Traverse _accCustomEdit;

        /// <summary>
        /// Returns true if the accessory tab in maker is currently selected.
        /// If you want to know if the user can actually see the tab on the screen check <see cref="MakerAPI.IsInterfaceVisible"/>.
        /// </summary>
        public static bool AccessoryCanvasVisible => _accessoryWindowObject != null && _accessoryWindowObject.activeSelf;

        /// <summary>
        /// True if the MoreAccessories mod is installed.
        /// Avoid relying on this and instead use other methods in this class since they will handle this for you.
        /// </summary>
        public static bool MoreAccessoriesInstalled => _moreAccessoriesType != null;

        /// <summary>
        /// Get the index of the currently selected accessory slot under Accessories group in Chara Maker.
        /// If none are selected or chara maker is not opened, returns -1. 0-indexed.
        /// Use <see cref="SelectedMakerAccSlotChanged"/> to get notified when the selected slot changes.
        /// </summary>
        public static int SelectedMakerAccSlot { get; private set; } = -1;
        // if (!MakerAPI.InsideMaker) return -1;
        // return _accCustomEdit.Field<int>("nowTab").Value;

        /// <summary>
        /// Fires whenever the index of the currently selected accessory slot under Accessories group in Chara Maker is changed.
        /// This happens when user click on another slot.
        /// </summary>
        public static event EventHandler<AccessorySlotEventArgs> SelectedMakerAccSlotChanged;

        ///// <summary>
        ///// A new slot was added by MoreAccessories. Adding 10 slots triggers this 10 times.
        ///// </summary>
        //public static event EventHandler<AccessorySlotEventArgs> MakerAccSlotAdded;

        /// <summary>
        /// Fires when user selects a different accessory in the accessory window.
        /// </summary>
        public static event EventHandler<AccessorySlotEventArgs> AccessoryKindChanged;

#if KK 
        /// <summary>
        /// Fires after user copies accessories between coordinates by using the Copy window.
        /// </summary>
        public static event EventHandler<AccessoryCopyEventArgs> AccessoriesCopied;
#endif

        /// <summary>
        /// Fires after user copies an accessory within a single coordinate by using the Transfer window.
        /// </summary>
        public static event EventHandler<AccessoryTransferEventArgs> AccessoryTransferred;

        /// <summary>
        /// Get a list of all accessory objects for this character.
        /// If an accessory slot exists but has no accessory in it, it will appear as null on the list.
        /// </summary>
        public static GameObject[] GetAccessoryObjects(this Human character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));
            return character.accessories.objAcs;
        }

        /// <summary>
        /// Get accessory objects of specified index for this character.
        /// null will be returned if an accessory slot exists but has no accessory in it, or if the slot doesn't exist.
        /// If index is below 0 or the character is null an exception will be thrown.
        /// </summary>
        public static GameObject GetAccessoryObject(this Human character, int index)
        {
            var acs = GetAccessoryObjects(character);
            return acs.Length > index ? acs[index] : null;
        }

        /// <summary>
        /// Get count of the UI entries for accessories (accessory slots).
        /// Returns 0 outside of chara maker.
        /// </summary>
        public static int GetMakerAccessoryCount()
        {
            return GetMakerToggles()?.Length ?? 0;
        }

        private static ToggleButton[] GetMakerToggles()
        {//nowTab
            if (!MakerAPI.InsideMaker || _accCustomEdit == null) return null;
            return _accCustomEdit.Field<ToggleButton[]>("toggles").Value;
        }

        /// <summary>
        /// Get the character that owns this accessory
        /// </summary>
        public static Human GetOwningCharacter(GameObject accessoryRootObject)
        {
            if (accessoryRootObject == null) throw new ArgumentNullException(nameof(accessoryRootObject));
            return accessoryRootObject.GetComponentInParent<Human>();
        }

        /// <summary>
        /// Get index of this accessory, or -1 if it doesn't exist for the specified character.
        /// </summary>
        public static int GetAccessoryIndex(this Human character, GameObject accessoryRootObject)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));
            if (accessoryRootObject == null) throw new ArgumentNullException(nameof(accessoryRootObject));
            return Array.IndexOf(character.GetAccessoryObjects(), accessoryRootObject);
        }

        internal static void Init()
        {
            DetectMoreAccessories();

            BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hooks));

            MakerAPI.InsideMakerChanged += MakerAPI_InsideMakerChanged;
            MakerAPI.MakerFinishedLoading += (sender, args) => OnSelectedMakerSlotChanged(sender, 0);
        }

        private static void DetectMoreAccessories()
        {
            try
            {
                //todo
                _moreAccessoriesType = Type.GetType("MoreAccessoriesKOI.MoreAccessories, MoreAccessories, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

                if (_moreAccessoriesType != null)
                {
                    _moreAccessoriesInstance = Object.FindObjectOfType(_moreAccessoriesType);

                    //var slotAddEvent = _moreAccessoriesType.GetEvent("onCharaMakerSlotAdded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    //if (slotAddEvent != null)
                    //{
                    //    slotAddEvent.AddEventHandler(
                    //        _moreAccessoriesInstance,
                    //        new Action<int, Transform>((i, transform) => OnMakerAccSlotAdded(_moreAccessoriesInstance, i, transform)));
                    //}
                    //else
                    //{
                    //    _moreAccessoriesType = null;
                    //    KoikatuAPI.Logger.LogWarning("WARNING: Your MoreAccesories is outdated! Some features won't work correctly until you update to the latest version.");
                    //}
                }
            }
            catch (Exception e)
            {
                _moreAccessoriesType = null;
                KoikatuAPI.Logger.LogWarning("Failed to detect MoreAccessories!");
                KoikatuAPI.Logger.LogDebug(e);
            }
        }

        private static void MakerAPI_InsideMakerChanged(object sender, EventArgs e)
        {
            if (MakerAPI.InsideMaker)
            {
                var ace = MakerInterfaceCreator.GetAccessoryCustomEdit();
                _accCustomEdit = Traverse.Create(ace);
                _accessoryWindowObject = ((AccessoryCustomEdit)ace).gameObject;
                SelectedMakerAccSlot = 0;
            }
            else
            {
                _accessoryWindowObject = null;
                _accCustomEdit = null;
                SelectedMakerAccSlot = -1;
            }
        }

        private static void OnSelectedMakerSlotChanged(object source, int newSlotIndex)
        {
            if (newSlotIndex == SelectedMakerAccSlot) return;
            SelectedMakerAccSlot = newSlotIndex;

            if (KoikatuAPI.EnableDebugLogging)
                KoikatuAPI.Logger.LogMessage("SelectedMakerAccSlotChanged - slot:" + newSlotIndex);

            if (SelectedMakerAccSlotChanged == null) return;
            try
            {
                SelectedMakerAccSlotChanged(source, new AccessorySlotEventArgs(newSlotIndex));
            }
            catch (Exception ex)
            {
                KoikatuAPI.Logger.LogError("Subscription to SelectedMakerSlot crashed: " + ex);
            }
        }

        //private static void OnMakerAccSlotAdded(object source, int newSlotIndex, Transform newSlotTransform)
        //{
        //    if (KoikatuAPI.EnableDebugLogging)
        //        KoikatuAPI.Logger.LogMessage("MakerAccSlotAdded - slot:" + newSlotIndex);
        //
        //    MakerInterfaceCreator.OnMakerAccSlotAdded(newSlotTransform);
        //
        //    if (MakerAccSlotAdded == null) return;
        //    try
        //    {
        //        MakerAccSlotAdded(source, new AccessorySlotEventArgs(newSlotIndex));
        //    }
        //    catch (Exception ex)
        //    {
        //        KoikatuAPI.Logger.LogError("Subscription to SelectedMakerSlot crashed: " + ex);
        //    }
        //}

        private static void OnAccessoryChanged(object source, int slotNo)
        {
            if (KoikatuAPI.EnableDebugLogging)
                KoikatuAPI.Logger.LogMessage("AccessoryKindChanged - slot:" + slotNo);

            if (AccessoryKindChanged == null) return;
            try
            {
                AccessoryKindChanged(source, new AccessorySlotEventArgs(slotNo));
            }
            catch (Exception ex)
            {
                KoikatuAPI.Logger.LogError("Subscription to AccessoryKindChanged crashed: " + ex);
            }
        }

#if KK
        private static void OnCopyAcs(CvsAccessoryCopy instance)
        {
            if (KoikatuAPI.EnableDebugLogging)
                KoikatuAPI.Logger.LogMessage($"AccessoriesCopied - ids: {string.Join(", ", args.CopiedSlotIndexes.Select(x => x.ToString()).ToArray())}, src:{args.CopySource}, dst:{args.CopyDestination}");

            if (AccessoriesCopied == null) return;
            try
            {
                var selected = instance.GetComponentsInChildren<UnityEngine.UI.Toggle>()
                    .Where(x => x.isOn)
                    .Select(x => x.transform.parent.name)
                    .Select(n => int.Parse(n.Substring("kind".Length)))
                    .ToList();

                var dropdowns = Traverse.Create(instance).Field("ddCoordeType").GetValue<TMP_Dropdown[]>();

                var args = new AccessoryCopyEventArgs(selected, (ChaFileDefine.CoordinateType)dropdowns[1].value, (ChaFileDefine.CoordinateType)dropdowns[0].value);

                AccessoriesCopied(instance, args);
            }
            catch (Exception ex)
            {
                KoikatuAPI.Logger.LogError("Crash in AccessoriesCopied event: " + ex);
            }
        }
#endif

        private static void OnChangeAcs(object instance, int selSrc, int selDst)
        {
            if (KoikatuAPI.EnableDebugLogging)
                KoikatuAPI.Logger.LogMessage($"AccessoryTransferred - srcId:{selSrc}, dstId:{selDst}");

            if (AccessoryTransferred == null) return;
            try
            {
                var args = new AccessoryTransferEventArgs(selSrc, selDst);

                AccessoryTransferred(instance, args);
            }
            catch (Exception ex)
            {
                KoikatuAPI.Logger.LogError("Crash in AccessoryTransferred event: " + ex);
            }
        }
    }
}
