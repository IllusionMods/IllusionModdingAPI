using BepInEx.Logging;
using ChaCustom;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
#pragma warning disable 612

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

        private static CanvasGroup _accessorySlotCanvasGroup;
        internal static CustomAcsChangeSlot CustomAcs { get; private set; }

        /// <summary>
        /// Returns true if the accessory tab in maker is currently selected.
        /// If you want to know if the user can actually see the tab on the screen check <see cref="MakerAPI.IsInterfaceVisible"/>.
        /// </summary>
        public static bool AccessoryCanvasVisible => _accessorySlotCanvasGroup != null && _accessorySlotCanvasGroup.alpha.Equals(1f);

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

        /// <summary>
        /// Fires whenever the index of the currently selected accessory slot under Accessories group in Chara Maker is changed.
        /// This happens when user click on another slot.
        /// </summary>
        public static event EventHandler<AccessorySlotEventArgs> SelectedMakerAccSlotChanged;

        /// <summary>
        /// A new slot was added by MoreAccessories. Adding 10 slots triggers this 10 times.
        /// </summary>
        public static event EventHandler<AccessorySlotEventArgs> MakerAccSlotAdded;

        /// <summary>
        /// Fires when user selects a different accessory in the accessory window.
        /// </summary>
        public static event EventHandler<AccessorySlotEventArgs> AccessoryKindChanged;

#if KK || KKS 
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
        public static GameObject[] GetAccessoryObjects(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            return character.objAccessory;
        }

        /// <summary>
        /// Get accessory objects of specified index for this character.
        /// null will be returned if an accessory slot exists but has no accessory in it, or if the slot doesn't exist.
        /// If index is below 0 or the character is null an exception will be thrown.
        /// </summary>
        public static GameObject GetAccessoryObject(this ChaControl character, int index)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));
            return GetAccessory(character, index)?.gameObject;
        }

        /// <summary>
        /// Get count of the UI entries for accessories (accessory slots).
        /// Returns 0 outside of chara maker.
        /// </summary>
        public static int GetMakerAccessoryCount()
        {
            return GetCvsAccessoryCount();
        }

        /// <summary>
        /// Get the root gameobject of the maker UI page for the specified accessory.
        /// Returns null if the page doesn't exist or can't be accessed (e.g. when outside maker).
        /// </summary>
        /// <param name="index">Index of the accessory to get the UI for. Use -1 to get the currently opened accessory page</param>
        public static GameObject GetMakerAccessoryPageObject(int index = -1)
        {
            if (index < 0) index = SelectedMakerAccSlot;
            return GetCvsAccessory(index)?.gameObject;
        }

        /// <summary>
        /// Get the character that owns this accessory
        /// </summary>
        public static ChaControl GetOwningCharacter(GameObject accessoryRootObject)
        {
            if (accessoryRootObject == null) throw new ArgumentNullException(nameof(accessoryRootObject));
            return accessoryRootObject.GetComponentInParent<ChaControl>();
        }

        /// <summary>
        /// Get index of this accessory, or -1 if it doesn't exist for the specified character.
        /// </summary>
        public static int GetAccessoryIndex(this ChaControl character, GameObject accessoryRootObject)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));
            if (accessoryRootObject == null) throw new ArgumentNullException(nameof(accessoryRootObject));
            var accessoryComponent = accessoryRootObject.GetComponent<ChaAccessoryComponent>();
            if (accessoryComponent == null) return -1;
            return Array.IndexOf(character.cusAcsCmp, accessoryComponent);
        }

        /// <summary>
        /// Get the accessory given a slot index.
        /// </summary>
        [Obsolete]
        public static ChaAccessoryComponent GetAccessory(this ChaControl character, int accessoryIndex)
        {
            if (accessoryIndex < 0 || accessoryIndex >= character.cusAcsCmp.Length) return null;
            return character.cusAcsCmp[accessoryIndex];
        }

        /// <summary>
        /// Get slot index of his accessory, useful for referencing to the accesory in extended data.
        /// </summary>
        [Obsolete]
        public static int GetAccessoryIndex(this ChaAccessoryComponent accessoryComponent)
        {
            var chaControl = GetOwningChaControl(accessoryComponent);
            return Array.IndexOf(chaControl.cusAcsCmp, accessoryComponent);
        }

        /// <summary>
        /// Get accessory UI entry in maker.
        /// Only works inside chara maker.
        /// </summary>
        [Obsolete]
        public static CvsAccessory GetCvsAccessory(int index)
        {
            if (CustomAcs == null) throw new InvalidOperationException("Can only call GetCvsAccessory when inside Chara Maker");
            return CustomAcs.cvsAccessory[index];
        }

        /*/// <summary>
        /// Get the index of the currently selected accessory slot under Accessories group in Chara Maker.
        /// If none are selected or chara maker is not opened, returns -1.
        /// Use <see cref="SelectedMakerSlotObservable"/> to get notified when the selected slot changes.
        /// </summary>
        public static int GetSelectedAccessoryIndex()
        {
            if (!MakerAPI.InsideMaker) return -1;
            return _getSelectedAccessoryIndex.Invoke();
        }*/

        /// <summary>
        /// Get accessory PartsInfo entry in maker.
        /// Only works inside chara maker.
        /// </summary>
        [Obsolete]
        public static ChaFileAccessory.PartsInfo GetPartsInfo(int index)
        {
            if (!CustomBase.IsInstance()) throw new InvalidOperationException("Can only call GetPartsInfo when inside Chara Maker");
            var control = CustomBase.instance.chaCtrl;
            if (control == null) throw new InvalidOperationException("Chara Maker Not Ready");
            var parts = control.nowCoordinate.accessory.parts;
            if (parts.Length <= index) return new ChaFileAccessory.PartsInfo();
            return parts[index];
        }

        /// <summary>
        /// Get count of the UI entries for accessories (accessory slots).
        /// Returns 0 outside of chara maker.
        /// </summary>
        [Obsolete]
        public static int GetCvsAccessoryCount()
        {
            if (CustomAcs == null) return 0;
            return CustomAcs.cvsAccessory.Length;
        }

        /// <summary>
        /// Get the ChaControl that owns this accessory
        /// </summary>
        [Obsolete]
        public static ChaControl GetOwningChaControl(this ChaAccessoryComponent accessoryComponent)
        {
            return accessoryComponent.GetComponentInParent<ChaControl>();
        }

        internal static void Init()
        {
            DetectMoreAccessories();

            Harmony.CreateAndPatchAll(typeof(Hooks));

            MakerAPI.InsideMakerChanged += MakerAPI_InsideMakerChanged;
            MakerAPI.MakerFinishedLoading += (sender, args) => OnSelectedMakerSlotChanged(sender, 0);
        }

        private static void DetectMoreAccessories()
        {
            try
            {
                _moreAccessoriesType = Type.GetType("MoreAccessoriesKOI.MoreAccessories, MoreAccessories, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true);

                if (_moreAccessoriesType != null)
                {
                    _moreAccessoriesInstance = Object.FindObjectOfType(_moreAccessoriesType);

                    var version = ((BepInEx.BaseUnityPlugin)_moreAccessoriesInstance).Info.Metadata.Version;
                    var minVersion = new Version(2, 0, 6);
                    if (version >= minVersion)
                    {
                        var slotAddEvent = _moreAccessoriesType.GetEvent("onCharaMakerSlotAdded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (slotAddEvent != null)
                        {
                            slotAddEvent.AddEventHandler(
                                _moreAccessoriesInstance,
                                new Action<int, Transform>((i, transform) => OnMakerAccSlotAdded(_moreAccessoriesInstance, i, transform)));
                            return;
                        }
                    }

                    _moreAccessoriesType = null;
                    KoikatuAPI.Logger.Log(LogLevel.Message | LogLevel.Warning, $"WARNING: MoreAccesories is outdated! Some features won't work until you update to v{minVersion} or later (Current verion: v{version}).");
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
                var changeSlot = Object.FindObjectOfType<CustomAcsChangeSlot>();
                _accessorySlotCanvasGroup = changeSlot.GetComponent<CanvasGroup>();
            }
            else
            {
                _accessorySlotCanvasGroup = null;
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

            AutomaticControlVisibility();
        }

        private static void OnMakerAccSlotAdded(object source, int newSlotIndex, Transform newSlotTransform)
        {
            if (KoikatuAPI.EnableDebugLogging)
                KoikatuAPI.Logger.LogMessage("MakerAccSlotAdded - slot:" + newSlotIndex);

            MakerInterfaceCreator.OnMakerAccSlotAdded(newSlotTransform);

            if (MakerAccSlotAdded == null) return;
            try
            {
                MakerAccSlotAdded(source, new AccessorySlotEventArgs(newSlotIndex));
            }
            catch (Exception ex)
            {
                KoikatuAPI.Logger.LogError("Subscription to SelectedMakerSlot crashed: " + ex);
            }
        }

        private static void OnAccessoryKindChanged(object source, int slotNo)
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

#if KK || KKS
        private static void OnCopyAcs(CvsAccessoryCopy instance)
        {
            if (AccessoriesCopied == null && !KoikatuAPI.EnableDebugLogging) return;
            try
            {
                var toggles = instance.tglKind;
                var selected = new System.Collections.Generic.List<int>();
                for (var i = 0; i < toggles.Length; i++)
                {
                    if (toggles[i].isOn)
                    {
                        selected.Add(i);
                    }
                }

                var dropdowns = instance.ddCoordeType;

                var args = new AccessoryCopyEventArgs(selected, (ChaFileDefine.CoordinateType)dropdowns[1].value, (ChaFileDefine.CoordinateType)dropdowns[0].value);

                if (KoikatuAPI.EnableDebugLogging)
                    KoikatuAPI.Logger.LogMessage($"AccessoriesCopied - ids: {string.Join(", ", args.CopiedSlotIndexes.Select(x => x.ToString()).ToArray())}, src:{args.CopySource}, dst:{args.CopyDestination}");

                if (AccessoriesCopied != null)
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

        /// <summary>
        /// Used to tell non-automated plugins that accessory kind has changed
        /// </summary>
        internal static void AutomaticControlVisibility()
        {
            var slot = SelectedMakerAccSlot;
            if (slot < 0)
                return;
            var partsinfo = GetPartsInfo(slot);

            var accessoryExists = partsinfo.type != 120;

            MakerInterfaceCreator.AutomaticAccessoryControlVisibility(accessoryExists);
        }
    }
}
