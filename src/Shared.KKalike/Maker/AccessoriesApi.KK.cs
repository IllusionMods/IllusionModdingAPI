using ChaCustom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
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

        private static Func<int, CvsAccessory> _getCvsAccessory;
        private static Func<int, ChaFileAccessory.PartsInfo> _getPartsInfo;
        private static Func<int> _getCvsAccessoryCount;
        private static Func<int> _getPartsCount;
        private static Func<ChaControl, int, ChaAccessoryComponent> _getChaAccessoryCmp;
        private static Func<ChaControl, ChaAccessoryComponent, int> _getChaAccessoryCmpIndex;

        private static object AccessoryByChar;//Cache as it seems to be practically readonly static
        private static MethodInfo AccessoryByCharTryget;

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

        private static object CharAdditionalData(ChaControl character)
        {
            object CharAdditional = null;
            if (!MoreAccessoriesInstalled) return CharAdditional;

            if (MakerAPI.InsideAndLoaded)//can be null at start _getPartsCount has same issue
            {
                return Traverse.Create(_moreAccessoriesInstance).Field("_charaMakerData").GetValue(); //dedicated CharAdditionalData
            }

            var parameters = new object[] { character.chaFile, null };
            AccessoryByCharTryget.Invoke(AccessoryByChar, parameters);
            return parameters[1];
        }

        private static List<ListInfoBase> GetAccessoryListInfoBase(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            var list = new List<ListInfoBase>();

            foreach (var gameObject in character.objAccessory)
            {
                if (gameObject == null)
                {
                    list.Add(null);
                    continue;
                }
                ListInfoComponent component = gameObject.GetComponent<ListInfoComponent>();
                list.Add(component.data);
            }

            if (!MoreAccessoriesInstalled) return list;

            var charadditionaldata = CharAdditionalData(character);

            if (charadditionaldata == null) return list;

            var data = Traverse.Create(charadditionaldata).Field<ICollection<ListInfoBase>>("infoAccessory").Value;

            list.AddRange(data);

            return list;
        }

        private static List<GameObject[]> GetAccessoryobjAcsMove(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            var list = new List<GameObject[]>();

            var array = character.objAcsMove;
            for (int i = 0; i < 20; i++)
            {
                list.Add(new[] { array[i, 0], array[i, 1] });
            }

            if (!MoreAccessoriesInstalled) return list;

            var charadditionaldata = CharAdditionalData(character);

            if (charadditionaldata == null) return list;

            var data = Traverse.Create(charadditionaldata).Field<ICollection<GameObject[]>>("objAcsMove").Value;

            list.AddRange(data);

            return list;
        }

        private static List<ChaAccessoryComponent> GetAccessorycusAcsCmp(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            var list = new List<ChaAccessoryComponent>();

            list.AddRange(character.cusAcsCmp);

            if (!MoreAccessoriesInstalled) return list;

            var charadditionaldata = CharAdditionalData(character);

            if (charadditionaldata == null) return list;

            var data = Traverse.Create(charadditionaldata).Field<ICollection<ChaAccessoryComponent>>("cusAcsCmp").Value;

            list.AddRange(data);

            return list;
        }

        private static List<bool> GetShowAccessory(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            var list = new List<bool>();

            list.AddRange(character.fileStatus.showAccessory);

            if (!MoreAccessoriesInstalled) return list;

            var charadditionaldata = CharAdditionalData(character);

            if (charadditionaldata == null) return list;

            var data = Traverse.Create(charadditionaldata).Field<ICollection<bool>>("showAccessories").Value;

            list.AddRange(data);

            return list;
        }

        private static Dictionary<int, List<ChaFileAccessory.PartsInfo>> GetAllAccessories(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            var dict = new Dictionary<int, List<ChaFileAccessory.PartsInfo>>();

            int outfitnum = 0;

#if KK || KKS
            foreach (var item in character.chaFile.coordinate)
            {
                var list = dict[outfitnum] = new List<ChaFileAccessory.PartsInfo>();
                list.AddRange(item.accessory.parts);
                outfitnum++;
            }
#elif EC
            var list = dict[outfitnum] = new List<ChaFileAccessory.PartsInfo>();
            list.AddRange(character.nowCoordinate.accessory.parts);

#endif
            if (!MoreAccessoriesInstalled) return dict;

            var charadditionaldata = CharAdditionalData(character);

            if (charadditionaldata == null) return dict;

            var data = Traverse.Create(charadditionaldata).Field<Dictionary<int, List<ChaFileAccessory.PartsInfo>>>("rawAccessoriesInfos").Value;

            outfitnum = 0;
#if KK || KKS
            foreach (var item in character.chaFile.coordinate)
            {
                if (dict.TryGetValue(outfitnum, out var list) && data.TryGetValue(outfitnum, out var list2))
                    list.AddRange(list2);
                outfitnum++;
            }
#elif EC
            if (data.TryGetValue(outfitnum, out var list2))
                list.AddRange(list2);
#endif
            return dict;
        }

        private static List<ChaFileAccessory.PartsInfo> GetNowAccessories(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            var list = new List<ChaFileAccessory.PartsInfo>();

            list.AddRange(character.nowCoordinate.accessory.parts);

            if (!MoreAccessoriesInstalled) return list;

            var charadditionaldata = CharAdditionalData(character);

            if (charadditionaldata == null) return list;

            var data = Traverse.Create(charadditionaldata).Field<ICollection<ChaFileAccessory.PartsInfo>>("nowAccessories").Value;

            if (data == null) return list;

            list.AddRange(data);

            return list;
        }

        private static int GetNowAccessoriesCount(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            return character.GetNowAccessories().Count;
        }

#if KK || KKS
        private static List<ChaFileAccessory.PartsInfo> GetCoordinateAccessories(this ChaControl character, int coordinateindex)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            if (character.chaFile.coordinate.Length >= coordinateindex || coordinateindex < 0) throw new IndexOutOfRangeException($"{nameof(character)} only has coordinate length {character.chaFile.coordinate.Length}, tried index {coordinateindex}");

            var list = new List<ChaFileAccessory.PartsInfo>();

            list.AddRange(character.chaFile.coordinate[coordinateindex].accessory.parts);

            if (!MoreAccessoriesInstalled) return list;

            var charadditionaldata = CharAdditionalData(character);

            if (charadditionaldata == null) return list;

            var data = Traverse.Create(charadditionaldata).Field<Dictionary<int, List<ChaFileAccessory.PartsInfo>>>("rawAccessoriesInfos").Value;

            if (data == null) return list;

            if (data.TryGetValue(coordinateindex, out var list2))
                list.AddRange(list2);

            return list;
        }

        private static int GetCoordinateAccessoryCount(this ChaControl character, int coordinateindex)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            return character.GetCoordinateAccessories(coordinateindex).Count;
        }

        private static int[] GetAllAccessoriesCount(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            var dict = character.GetAllAccessories();
            var array = new int[dict.Count];
            for (int i = 0; i < dict.Count; i++)
            {
                array[i] = dict[i].Count;
            }
            return array;
        }
#endif

        /// <summary>
        /// Get a list of all accessory objects for this character.
        /// If an accessory slot exists but has no accessory in it, it will appear as null on the list.
        /// </summary>
        public static GameObject[] GetAccessoryObjects(this ChaControl character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));

            if (!MoreAccessoriesInstalled) return character.objAccessory;

            var charadditionaldata = CharAdditionalData(character);

            var objs = Traverse.Create(charadditionaldata).Field<ICollection<GameObject>>("objAccessory").Value;
            return character.objAccessory.Concat(objs).ToArray();
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
            return _getChaAccessoryCmpIndex(character, accessoryComponent);
        }

        /// <summary>
        /// Get the accessory given a slot index.
        /// </summary>
        [Obsolete]
        public static ChaAccessoryComponent GetAccessory(this ChaControl character, int accessoryIndex)
        {
            return _getChaAccessoryCmp(character, accessoryIndex);
        }

        /// <summary>
        /// Get slot index of his accessory, useful for referencing to the accesory in extended data.
        /// </summary>
        [Obsolete]
        public static int GetAccessoryIndex(this ChaAccessoryComponent accessoryComponent)
        {
            var chaControl = GetOwningChaControl(accessoryComponent);
            return _getChaAccessoryCmpIndex(chaControl, accessoryComponent);
        }

        /// <summary>
        /// Get accessory UI entry in maker.
        /// Only works inside chara maker.
        /// </summary>
        [Obsolete]
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

            if (MoreAccessoriesInstalled)
            {
                AccessoryByChar = Traverse.Create(_moreAccessoriesInstance).Field("_accessoriesByChar").GetValue();
                AccessoryByCharTryget = AccessoryByChar.GetType().GetMethod("TryGetValue", AccessTools.allDeclared) ?? throw new ArgumentException("TryGetValue not found");

                var getAccCmpM = AccessTools.Method(_moreAccessoriesType, "GetChaAccessoryComponent");
                _getChaAccessoryCmp = (control, componentIndex) => (ChaAccessoryComponent)getAccCmpM.Invoke(_moreAccessoriesInstance, new object[] { control, componentIndex });

                var getAccCmpIndexM = AccessTools.Method(_moreAccessoriesType, "GetChaAccessoryComponentIndex");
                _getChaAccessoryCmpIndex = (control, component) => (int)getAccCmpIndexM.Invoke(_moreAccessoriesInstance, new object[] { control, component });

                var getPartsInfoM = AccessTools.Method(_moreAccessoriesType, "GetPart");
                _getPartsInfo = i => (ChaFileAccessory.PartsInfo)getPartsInfoM.Invoke(_moreAccessoriesInstance, new object[] { i });

                var getPartsCountM = AccessTools.Method(_moreAccessoriesType, "GetPartsLength");
                _getPartsCount = () =>
                {
                    if (MakerAPI.InsideAndLoaded)
                        return (int)getPartsCountM.Invoke(_moreAccessoriesInstance, null);
                    return 20;
                };
            }
            else
            {
                _getChaAccessoryCmp = (control, i) => control.cusAcsCmp[i];
                _getChaAccessoryCmpIndex = (control, component) => Array.IndexOf(control.cusAcsCmp, component);
                _getPartsInfo = i => MakerAPI.GetCharacterControl().nowCoordinate.accessory.parts[i];
                _getPartsCount = () => 20;
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
                        slotAddEvent.AddEventHandler(
                            _moreAccessoriesInstance,
                            new Action<int, Transform>((i, transform) => OnMakerAccSlotAdded(_moreAccessoriesInstance, i, transform)));
                    }
                    else
                    {
                        _moreAccessoriesType = null;
                        KoikatuAPI.Logger.LogWarning("WARNING: Your MoreAccesories is outdated! Some features won't work correctly until you update to the latest version.");
                    }
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
                    _getCvsAccessoryCount = () => 20; // MakerAPI.GetCharacterControl().GetAccessoryObjects().Length;
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
                var selected = instance.GetComponentsInChildren<UnityEngine.UI.Toggle>()
                    .Where(x => x.isOn)
                    .Select(x => x.transform.parent.name)
                    .Select(n => int.Parse(n.Substring("kind".Length)))
                    .ToList();

                var dropdowns = Traverse.Create(instance).Field("ddCoordeType").GetValue<TMP_Dropdown[]>();

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

        internal static void AutomaticControlVisibility()
        {
            var slot = SelectedMakerAccSlot;
            if (slot < 0 || slot >= _getPartsCount.Invoke())
                return;
            var partsinfo = GetPartsInfo(slot);

            var accessoryExists = partsinfo.type != 120;

            MakerInterfaceCreator.AutomaticAccessoryControlVisibility(accessoryExists);
        }
    }
}
