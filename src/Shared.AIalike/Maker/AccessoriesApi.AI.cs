using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using CharaCustom;
using HarmonyLib;
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

        private static Func<int, ChaFileAccessory.PartsInfo> _getPartsInfo;
        private static Func<int> _getCvsAccessoryCount;
        private static Func<ChaControl, int, CmpAccessory> _getChaAccessoryCmp;
        private static Func<ChaControl, CmpAccessory, int> _getChaAccessoryCmpIndex;

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
        /// Fires when user selects a different accessory in the accessory window.
        /// </summary>
        public static event EventHandler<AccessorySlotEventArgs> AccessoryKindChanged;

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

            if (!MoreAccessoriesInstalled) return character.objAccessory;

            var dict = Traverse.Create(_moreAccessoriesInstance).Field("_charAdditionalData").GetValue();
            var m = dict.GetType().GetMethod("TryGetValue", AccessTools.allDeclared) ?? throw new ArgumentException("TryGetValue not found");
            var parameters = new object[] { character.chaFile, null };
            m.Invoke(dict, parameters);
            //List<MoreAccessories.AdditionalData.AccessoryObject> objects
            var objs = Traverse.Create(parameters[1]).Field<ICollection>("objects").Value;
            var objs2 = objs.Cast<object>().Select(x => Traverse.Create(x).Field<GameObject>("obj").Value);
            return character.objAccessory.Concat(objs2).ToArray();
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
        /// </summary>
        /// <param name="index">Index of the accessory to get the UI for. Use -1 to get the currently opened accessory page</param>
        public static GameObject GetMakerAccessoryPageObject(int index = -1)
        {
            var cvsAccessory = GetCvsAccessory();
            if (cvsAccessory == null) return null;
            if (index >= 0 && cvsAccessory.slotNo != index)
            {
                KoikatuAPI.Logger.LogWarning("GetMakerAccessoryPageObject - Index did not match currently opened accessory page, therefore null was returned. This game only supports getting the currently opened page (since it's reused).");
                return null;
            }

            return cvsAccessory.gameObject;
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
            var accessoryComponent = accessoryRootObject.GetComponent<CmpAccessory>();
            if (accessoryComponent == null) return -1;
            return _getChaAccessoryCmpIndex(character, accessoryComponent);
        }

        /// <summary>
        /// Get the accessory given a slot index.
        /// </summary>
        [Obsolete]
        public static CmpAccessory GetAccessory(this ChaControl character, int accessoryIndex)
        {
            return _getChaAccessoryCmp(character, accessoryIndex);
        }

        /// <summary>
        /// Get slot index of his accessory, useful for referencing to the accesory in extended data.
        /// </summary>
        [Obsolete]
        public static int GetAccessoryIndex(this CmpAccessory accessoryComponent)
        {
            var chaControl = GetOwningChaControl(accessoryComponent);
            return _getChaAccessoryCmpIndex(chaControl, accessoryComponent);
        }

        /// <summary>
        /// Get accessory UI entry in maker.
        /// Only works inside chara maker.
        /// </summary>
        [Obsolete]
        public static CustomAcsCorrectSet GetCvsAccessory()
        {
            if (!MakerAPI.InsideMaker) throw new InvalidOperationException("Can only call GetCvsAccessory when inside Chara Maker");
            return Object.FindObjectOfType<CustomAcsCorrectSet>();
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
            if (_getPartsInfo == null) throw new InvalidOperationException("Can only call GetPartsInfo when inside Chara Maker");
            return _getPartsInfo(index);
        }

        /// <summary>
        /// Get count of the UI entries for accessories (accessory slots).
        /// Returns 0 outside of chara maker.
        /// </summary>
        [Obsolete]
        public static int GetCvsAccessoryCount()
        {
            if (_getCvsAccessoryCount == null) return 0;
            return _getCvsAccessoryCount.Invoke();
        }

        /// <summary>
        /// Get the ChaControl that owns this accessory
        /// </summary>
        [Obsolete]
        public static ChaControl GetOwningChaControl(this CmpAccessory accessoryComponent)
        {
            return accessoryComponent.GetComponentInParent<ChaControl>();
        }

        internal static void Init()
        {
            DetectMoreAccessories();

            BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hooks));

            MakerAPI.InsideMakerChanged += MakerAPI_InsideMakerChanged;
            MakerAPI.MakerFinishedLoading += (sender, args) => OnSelectedMakerSlotChanged(sender, 0);

            NoMoreaccsFallback:
            if (MoreAccessoriesInstalled)
            {
                try
                {
                    var patchesTraverse = Traverse.CreateWithType("MoreAccessoriesAI.Patches.ChaControl_Patches, MoreAccessories");

                    //GetCmpAccessory(ChaControl self, int slotNo)
                    var mGca = patchesTraverse.Method("GetCmpAccessory", new Type[] { typeof(ChaControl), typeof(int) });
                    if (!mGca.MethodExists()) throw new InvalidOperationException("Failed to find MoreAccessoriesAI.Patches.ChaControl_Patches.GetCmpAccessory");
                    _getChaAccessoryCmp = (control, componentIndex) => mGca.GetValue<CmpAccessory>(control, componentIndex);

                    _getChaAccessoryCmpIndex = (control, component) =>
                    {
                        var idx = Array.IndexOf(control.cmpAccessory, component);
                        if (idx >= 0) return idx;

                        // No better way than to iterate the entries until we get an out of range exception
                        idx = 20;
                        try
                        {
                            while (true)
                            {
                                if (_getChaAccessoryCmp(control, idx) == component)
                                    return idx;
                                idx++;
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            return -1;
                        }
                    };

                    //ChaFileAccessory.PartsInfo GetPartsInfo(ChaControl self, int slotNo)
                    var mGpi = patchesTraverse.Method("GetPartsInfo", new Type[] { typeof(ChaControl), typeof(int) });
                    if (!mGpi.MethodExists()) throw new InvalidOperationException("Failed to find MoreAccessoriesAI.Patches.ChaControl_Patches.GetPartsInfo");
                    _getPartsInfo = i => mGpi.GetValue<ChaFileAccessory.PartsInfo>(MakerAPI.GetCharacterControl(), i);
                }
                catch (Exception e)
                {
                    _moreAccessoriesType = null;
                    KoikatuAPI.Logger.LogWarning("Failed to set up MoreAccessories integration!");
                    KoikatuAPI.Logger.LogDebug(e);
                    goto NoMoreaccsFallback;
                }
            }
            else
            {
                _getChaAccessoryCmp = (control, i) => control.cmpAccessory[i];
                _getChaAccessoryCmpIndex = (control, component) => Array.IndexOf(control.cmpAccessory, component);
                _getPartsInfo = i => MakerAPI.GetCharacterControl().nowCoordinate.accessory.parts[i];
            }
        }

        private static void DetectMoreAccessories()
        {
            try
            {
                _moreAccessoriesType = Type.GetType("MoreAccessoriesAI.MoreAccessories, MoreAccessories", false);
                if (_moreAccessoriesType != null)
                    _moreAccessoriesInstance = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(_moreAccessoriesType);
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
                _accessorySlotCanvasGroup = GameObject.Find("SubMenuAccessory").GetComponent<CanvasGroup>();

                _getCvsAccessoryCount = () => 20;

                SelectedMakerAccSlot = 0;
            }
            else
            {
                _accessorySlotCanvasGroup = null;
                _getCvsAccessoryCount = null;

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

        private static void OnAccessoryKindChanged(object source, int slotNo)
        {
            if (!MakerAPI.InsideAndLoaded) return;

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
