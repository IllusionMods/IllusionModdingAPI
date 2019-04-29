using System;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using ChaCustom;
using Harmony;
using TMPro;
using UnityEngine;
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

        private static CanvasGroup _accessorySlotCanvasGroup;

        private static Func<int, CvsAccessory> _getCvsAccessory;
        private static Func<int> _getCvsAccessoryCount;
        private static Func<ChaControl, int, ChaAccessoryComponent> _getChaAccessoryCmp;
        private static Func<ChaControl, ChaAccessoryComponent, int> _getChaAccessoryCmpIndex;

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
        /// Get the accessory given a slot index.
        /// </summary>
        public static ChaAccessoryComponent GetAccessory(this ChaControl character, int accessoryIndex)
        {
            return _getChaAccessoryCmp(character, accessoryIndex);
        }

        /// <summary>
        /// Get slot index of his accessory, useful for referencing to the accesory in extended data.
        /// </summary>
        public static int GetAccessoryIndex(this ChaAccessoryComponent accessoryComponent)
        {
            var chaControl = GetOwningChaControl(accessoryComponent);
            return _getChaAccessoryCmpIndex(chaControl, accessoryComponent);
        }

        /// <summary>
        /// Get accessory UI entry in maker.
        /// Only works inside chara maker.
        /// </summary>
        public static CvsAccessory GetCvsAccessory(int index)
        {
            if (_getCvsAccessory == null) throw new InvalidOperationException("Can only call GetCvsAccessory when inside Chara Maker");
            return _getCvsAccessory(index);
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
        /// Get count of the UI entries for accessories (accessory slots).
        /// Returns 0 outside of chara maker.
        /// </summary>
        public static int GetCvsAccessoryCount()
        {
            if (_getCvsAccessoryCount == null) return 0;
            return _getCvsAccessoryCount.Invoke();
        }

        /// <summary>
        /// Get the ChaControl that owns this accessory
        /// </summary>
        public static ChaControl GetOwningChaControl(this ChaAccessoryComponent accessoryComponent)
        {
            return accessoryComponent.GetComponentInParent<ChaControl>();
        }

        internal static void Init()
        {
            DetectMoreAccessories();

            HarmonyPatcher.PatchAll(typeof(Hooks));

            MakerAPI.InsideMakerChanged += MakerAPI_InsideMakerChanged;
            MakerAPI.MakerFinishedLoading += (sender, args) => OnSelectedMakerSlotChanged(sender, 0);

            if (MoreAccessoriesInstalled)
            {
                var getAccCmpM = AccessTools.Method(_moreAccessoriesType, "GetChaAccessoryComponent");
                _getChaAccessoryCmp = (control, componentIndex) => (ChaAccessoryComponent)getAccCmpM.Invoke(_moreAccessoriesInstance, new object[] { control, componentIndex });

                var getAccCmpIndexM = AccessTools.Method(_moreAccessoriesType, "GetChaAccessoryComponentIndex");
                _getChaAccessoryCmpIndex = (control, component) => (int)getAccCmpIndexM.Invoke(_moreAccessoriesInstance, new object[] { control, component });
            }
            else
            {
                _getChaAccessoryCmp = (control, i) => control.cusAcsCmp[i];
                _getChaAccessoryCmpIndex = (control, component) => Array.IndexOf(control.cusAcsCmp, component);
            }

            if (KoikatuAPI.EnableDebugLogging)
            {
                SelectedMakerAccSlotChanged += (sender, args) => KoikatuAPI.Log(LogLevel.Message,
                    $"SelectedMakerAccSlotChanged - id: {args.SlotIndex}, cvs: {args.CvsAccessory.transform.name}, component: {args.AccessoryComponent?.name ?? "null"}");
#if KK
                AccessoriesCopied += (sender, args) => KoikatuAPI.Log(LogLevel.Message,
                    $"AccessoriesCopied - ids: {string.Join(", ", args.CopiedSlotIndexes.Select(x => x.ToString()).ToArray())}, src:{args.CopySource}, dst:{args.CopyDestination}");
#endif
                AccessoryTransferred += (sender, args) => KoikatuAPI.Log(LogLevel.Message, 
                    $"AccessoryTransferred - srcId:{args.SourceSlotIndex}, dstId:{args.DestinationSlotIndex}");
            }
        }

        private static void DetectMoreAccessories()
        {
            try
            {
                _moreAccessoriesType = Type.GetType("MoreAccessoriesKOI.MoreAccessories, MoreAccessories, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

                if (_moreAccessoriesType != null)
                {
                    _moreAccessoriesInstance = Object.FindObjectOfType(_moreAccessoriesType);

                    var slotAddEvent = _moreAccessoriesType.GetEvent("onCharaMakerSlotAdded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (slotAddEvent != null)
                    {
                        slotAddEvent.AddEventHandler(_moreAccessoriesInstance,
                            new Action<int, Transform>((i, transform) => OnMakerAccSlotAdded(_moreAccessoriesInstance, i, transform)));
                    }
                    else
                    {
                        _moreAccessoriesType = null;
                        KoikatuAPI.Log(LogLevel.Message | LogLevel.Error, "[KKAPI] WARNING: Your MoreAccesories is outdated! Some features won't work correctly until you update to the latest version.");
                    }
                }
            }
            catch (Exception e)
            {
                _moreAccessoriesType = null;
                KoikatuAPI.Log(LogLevel.Error, e);
            }
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
                    var getCvsM = AccessTools.Method(_moreAccessoriesType, "GetCvsAccessory");
                    _getCvsAccessory = i => (CvsAccessory)getCvsM.Invoke(_moreAccessoriesInstance, new object[] { i });

                    var cvsCountM = AccessTools.Method(_moreAccessoriesType, "GetCvsAccessoryCount");
                    _getCvsAccessoryCount = () => (int)cvsCountM.Invoke(_moreAccessoriesInstance, null);
                }
                else
                {
                    _getCvsAccessory = i => cvsAccessories[i];
                    _getCvsAccessoryCount = () => 20;
                }
            }
            else
            {
                _accessorySlotCanvasGroup = null;

                _getCvsAccessory = null;
                _getCvsAccessoryCount = null;

                SelectedMakerAccSlot = -1;
            }
        }

        private static void OnSelectedMakerSlotChanged(object source, int newSlotIndex)
        {
            if (newSlotIndex == SelectedMakerAccSlot) return;
            SelectedMakerAccSlot = newSlotIndex;

            if (KoikatuAPI.EnableDebugLogging)
                KoikatuAPI.Log(LogLevel.Message, "SelectedMakerSlotChanged - slot:" + newSlotIndex);

            if (SelectedMakerAccSlotChanged == null) return;
            try
            {
                SelectedMakerAccSlotChanged(source, new AccessorySlotEventArgs(newSlotIndex));
            }
            catch (Exception ex)
            {
                KoikatuAPI.Log(LogLevel.Error, "Subscription to SelectedMakerSlot crashed: " + ex);
            }
        }

        private static void OnMakerAccSlotAdded(object source, int newSlotIndex, Transform newSlotTransform)
        {
            if (KoikatuAPI.EnableDebugLogging)
                KoikatuAPI.Log(LogLevel.Message, "MakerAccSlotAdded - slot:" + newSlotIndex);

            MakerAPI.OnMakerAccSlotAdded(newSlotTransform);

            if (MakerAccSlotAdded == null) return;
            try
            {
                MakerAccSlotAdded(source, new AccessorySlotEventArgs(newSlotIndex));
            }
            catch (Exception ex)
            {
                KoikatuAPI.Log(LogLevel.Error, "Subscription to SelectedMakerSlot crashed: " + ex);
            }
        }

        private static void OnAccessoryKindChanged(object source, int slotNo)
        {
            if (KoikatuAPI.EnableDebugLogging)
                KoikatuAPI.Log(LogLevel.Message, "AccessoryKindChanged - slot:" + slotNo);

            if (AccessoryKindChanged == null) return;
            try
            {
                AccessoryKindChanged(source, new AccessorySlotEventArgs(slotNo));
            }
            catch (Exception ex)
            {
                KoikatuAPI.Log(LogLevel.Error, "Subscription to AccessoryKindChanged crashed: " + ex);
            }
        }

#if KK
        private static void OnCopyAcs(CvsAccessoryCopy instance)
        {
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
                KoikatuAPI.Log(LogLevel.Error, "Crash in AccessoriesCopied event: " + ex);
            }
        }
#endif

        private static void OnChangeAcs(CvsAccessoryChange instance)
        {
            if (AccessoryTransferred == null) return;

            try
            {
                var traverse = Traverse.Create(instance);
                var selSrc = traverse.Field("selSrc").GetValue<int>();
                var selDst = traverse.Field("selDst").GetValue<int>();

                var args = new AccessoryTransferEventArgs(selSrc, selDst);

                AccessoryTransferred(instance, args);
            }
            catch (Exception ex)
            {
                KoikatuAPI.Log(LogLevel.Error, "Crash in AccessoryTransferred event: " + ex);
            }
        }
    }
}
