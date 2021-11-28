using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ActionGame;
using HarmonyLib;
using KKAPI.Studio;
using KKAPI.Utilities;
using Manager;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KKAPI.MainGame
{
    /// <summary>
    /// Allows adding custom items to the main game shop.
    /// Items have to be registered while still in the title menu or earlier (before the player loads a save file or starts a new game).
    /// Only useful in the story mode.
    /// </summary>
    public static class StoreApi
    {
        /// <summary>
        /// Background color of the item (shows the rough type of the item: normal / unlock / lewd).
        /// </summary>
        public enum ShopBackground
        {
            /// <summary>
            /// Used for normal items that usually restock after day ends (e.g. conversation topics).
            /// </summary>
            Blue = 0,
            /// <summary>
            /// Used for special items that usually don't restock after day ends (they might permanently unlock actions on the map or other features).
            /// </summary>
            Yellow = 0,
            /// <summary>
            /// Used for lewd items that may be permanent unlocks (e.g. new touch options), or single use and restock after day ends (e.g. pills).
            /// </summary>
            Pink = 0,
        }

        /// <summary>
        /// Which shop tab to show the item in (day/night).
        /// </summary>
        public enum ShopType
        {
            /// <summary>
            /// Normal - sold in day shop / points store
            /// </summary>
            Normal = 0,
            /// <summary>
            /// Lewd - sold in night shop / night only store
            /// </summary>
            NightOnly = 1
        }

        private const int CustomCategoryIndexOffset = 1000000;
        private const int MinimumItemId = 1200;

        private static readonly List<ShopInfo.Param> _customShopItems = new List<ShopInfo.Param>();
        private static readonly Dictionary<int, Action<ShopItem>> _customShopBuyActions = new Dictionary<int, Action<ShopItem>>();

        private static readonly List<Texture2D> _customShopCategoryIcons = new List<Texture2D>();

        /// <summary>
        /// Register a new category of shop items (needed for adding custom icons to your shop items).
        /// After registering, use the return value of this method as your item's <see cref="ShopInfo.Param.Category"/>.
        /// </summary>
        public static int RegisterShopItemCategory(Texture2D categoryIcon)
        {
            _customShopCategoryIcons.Add(categoryIcon);
            return _customShopCategoryIcons.Count - 1 + CustomCategoryIndexOffset;
        }

        /// <summary>
        /// Registers a custom shop item.
        /// </summary>
        /// <param name="itemId">Unique ID of the shop item.
        /// This ID is used in the save file to keep track of purchases, so it has to always be the same between game starts (i.e. use a hardcoded number and never change it).
        /// Values below 1200 are reserved for the base game items. It's best to use values above 100000 just to be safe (be mindful of other mods, ID collisions are not handled).
        /// </param>
        /// <param name="itemName">Name of the item shown on the shop list.</param>
        /// <param name="explaination">Description of the item when hovering over it.</param>
        /// <param name="shopType">Which shop tab to show the item in (day/night).</param>
        /// <param name="itemBackground">Background color of the item (shows the rough type of the item: normal / unlock / lewd).</param>
        /// <param name="itemCategory">Category of the item, changes which icon is used.
        /// Default icons:
        /// 0 - Speech buble
        /// 1 - Seeds
        /// 2 - Egg in a nest
        /// 3 - Question mark
        /// 4 - Massager
        /// 5 - Vibe
        /// 6 - Onahole
        /// 7 - Perfume
        /// 8 - Drugs
        /// 9 - Book
        /// You can add custom icons by using <see cref="RegisterShopItemCategory"/>
        /// </param>
        /// <param name="stock">How many of this item are available to be bought.
        /// Number of bought items is stored in <see cref="SaveData.Player.buyNumTable"/>, and if the stored value is equal or higher than this parameter then the item can't be bought any more.</param>
        /// <param name="resetsDaily">If true, item is rectocked daily. If false, it can only be bought once.
        /// If the item is set to restock every day:
        /// 1 - It will get its owned count reset to 0 at the start of a new day when you click the "Go out" button in your room. You will be able to buy the item again since you don't own any of it any more.
        /// 2 - For the duration of that day it will be placed in the <see cref="SaveData.WorldData.shopItemEffect"/> dictionary (key = ID of the item; value = number of the items).
        /// Basically, on the day the item was bought it will be counted in <see cref="SaveData.Player.buyNumTable"/> which this method uses, and on the next day it will be counted in <see cref="SaveData.WorldData.shopItemEffect"/>, until it is completely removed on the day after that.
        /// </param>
        /// <param name="cost">How many koikatsu points this item costs to buy.</param>
        /// <param name="sort">Number used to sort items on the list. Larger number is lower on the list. Value of default items range from 0 to 200.</param>
        /// <param name="numText">Override the "You can buy x more of this item today" text shown below the name of the item. The string has to contain `{0}` in it (gets replaced with the number left). If null, the default game string is used (differs based on the value of resetsDaily).</param>
        /// <param name="onBought">Action to run right after the item was bought by the player (similar to how it pops up the "new topic" message right after buying topics in shop). You can use it to apply effects, or you can check if the item was bought later with <see cref="GetItemAmountBought"/>. This is fired for each item bought (can't buy multiple items at once).</param>
        /// <returns>Dispose of the return value to remove the item. Warning: This is intended only for development use! This might not remove the item immediately or fully, and you might need to go back to title menu and load the game again for the changes to take effect. Disposing won't clear the item from inventory lists.</returns>
        public static IDisposable RegisterShopItem(
            int itemId, string itemName, string explaination,
            ShopType shopType, ShopBackground itemBackground, int itemCategory,
            int stock, bool resetsDaily, int cost, int sort = 300,
            string numText = null, Action<ShopItem> onBought = null)
        {
            if (StudioAPI.InsideStudio) return Disposable.Empty;

            if (itemId < MinimumItemId) throw new ArgumentOutOfRangeException(nameof(itemId), itemId, "Values below 1200 are reserved for the base game items");
            if (itemName == null) throw new ArgumentNullException(nameof(itemName));
            if (explaination == null) throw new ArgumentNullException(nameof(explaination));
            if (!Enum.IsDefined(typeof(ShopType), shopType)) throw new ArgumentOutOfRangeException(nameof(shopType), shopType, "Invalid ShopType");
            if (!Enum.IsDefined(typeof(ShopBackground), itemBackground)) throw new ArgumentOutOfRangeException(nameof(itemBackground), itemBackground, "Invalid ShopBackground");
            if (stock < 0) throw new ArgumentOutOfRangeException(nameof(stock), stock, "Value can't be negative");
            if (cost < 0) throw new ArgumentOutOfRangeException(nameof(cost), cost, "Value can't be negative");
            if (itemCategory < 0 || (itemCategory > 9 && itemCategory < CustomCategoryIndexOffset) || itemCategory >= CustomCategoryIndexOffset + _customShopCategoryIcons.Count)
                throw new ArgumentOutOfRangeException(nameof(itemCategory), itemCategory, "itemCategory has to be in range of 0-9 for default categories, or it has to use an index returned from RegisterShopItemCategory.");

            if (numText == null)
            {
                // Use the same strings as stock game items so they get translated as needed.
                // {0} items left for today : {0} left in stock
                numText = resetsDaily ? "本日残り{0}回" : "残り{0}個";
            }

            var param = new ShopInfo.Param
            {
                ID = itemId,
                Name = itemName,
                Explan = explaination,
                Type = (int)shopType,
                BackGround = (int)itemBackground,
                Category = itemCategory,
                Num = stock,
                NumText = numText,
                // todo make sure it's correct, maybe set it a default together with some other settings
                InitType = resetsDaily ? 0 : -1,
                Pt = cost,
                Sort = sort,
            };

            var token = RegisterShopItem(param);
            if (onBought != null) _customShopBuyActions.Add(itemId, onBought);
            return token;
        }

        /// <summary>
        /// Registers a new shop item. Check other RegisterShopItem overloads for more info.
        /// Warning: This overload skips most of the parameter checks! It's best to use the other overload instead.
        /// </summary>
        public static IDisposable RegisterShopItem(ShopInfo.Param itemParams)
        {
            if (StudioAPI.InsideStudio) return Disposable.Empty;

            // TODO handle IDs somehow, needed for data persistance, gets used in .buyList and .buyNumTable so it has to be the same between game reloads even if other things change
            // maybe require some sort of guid and then save this data to game save? the plugin has to deal with making it unique for now.

            var existing = _customShopItems.Find(x => x.ID == itemParams.ID);
            if (existing != null)
                throw new ArgumentException($"Could not add new shop item. The ID={itemParams.ID} is already being used by a custom item with Name={existing.Name}");
            if (SingletonInitializer<ActionScene>.initialized && SingletonInitializer<ActionScene>.instance.shopInfoTable?.ContainsKey(itemParams.ID) == true)
                KoikatuAPI.Logger.LogWarning($"Added item ID={itemParams.ID} is already being used by a base game item with Name={SingletonInitializer<ActionScene>.instance.shopInfoTable[itemParams.ID].Name}");

            _customShopItems.Add(itemParams);

            StoreHooks.ApplyHooksIfNeeded();

            return Disposable.Create(() => _customShopItems.Remove(itemParams));
        }

        /// <summary>
        /// Check if store item with this ID was ever bought in the game. This is saved globally and will stay true after first time the item is bought (can work as a permanent global unlock, it works in Free H).
        /// </summary>
        public static bool WasItemBoughtGlobal(int itemId)
        {
            if (StudioAPI.InsideStudio) return false;

            return Game.globalData?.buyList?.Contains(itemId) ?? false;
        }

        /// <summary>
        /// Set if store item with this ID was ever bought in the game. This is saved globally and will stay true after first time the item is bought (can work as a permanent global unlock, it works in Free H).
        /// Returns true if the previous value was different than wasBought, false if it was the same.
        /// </summary>
        public static bool SetItemBoughtGlobal(int itemId, bool wasBought)
        {
            if (StudioAPI.InsideStudio) return false;

            return wasBought ? Game.globalData.buyList.Add(itemId) : Game.globalData.buyList.Remove(itemId);
        }

        /// <summary>
        /// Check how many store items with this ID were bought on this save file.
        /// This value can be manually changed with <see cref="SetItemAmountBought"/>.
        /// This can be used as an inventory, just be careful to avoid ID conflicts with real existing items.
        /// If the item is set to restock every day it will get its count reset to 0 at the start of a new day when you click the "Go out" button in your room.
        /// </summary>
        public static int GetItemAmountBought(int itemId)
        {
            if (StudioAPI.InsideStudio) return 0;

            if (Game.Player != null &&
                Game.Player.buyNumTable != null &&
                Game.Player.buyNumTable.TryGetValue(itemId, out var num))
                return num;

            return 0;
        }

        /// <summary>
        /// Set how many store items with this id were bought. This is used by the store to keep track of how many of each item is still available for purchase.
        /// This value can be read with <see cref="GetItemAmountBought"/>.
        /// This be used as an inventory, just be careful to avoid ID conflicts with real existing items.
        /// </summary>
        public static void SetItemAmountBought(int itemId, int amount)
        {
            if (StudioAPI.InsideStudio) return;

            var buyNumTable = Game.Player.buyNumTable;
            buyNumTable[itemId] = amount;
        }

        private static class StoreHooks
        {
            private static bool _storehooked;

            public static void ApplyHooksIfNeeded()
            {
                if (_storehooked) return;
                _storehooked = true;

                KoikatuAPI.Logger.LogDebug("StoreHooks.ApplyHooks");

                var h = new Harmony(nameof(StoreApi) + "_Hooks");
                h.PatchMoveNext(AccessTools.Method(typeof(ShopView), nameof(ShopView.Start)), transpiler: new HarmonyMethod(typeof(StoreHooks), nameof(ShopInitTpl)));
                h.PatchMoveNext(AccessTools.Method(typeof(ShopView), nameof(ShopView.Buy)), transpiler: new HarmonyMethod(typeof(StoreHooks), nameof(OnBuyTpl)));
            }

            public static IEnumerable<CodeInstruction> ShopInitTpl(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(SingletonInitializer<ActionScene>), nameof(SingletonInitializer<ActionScene>.instance))),
                        new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ActionScene), nameof(ActionScene.shopInfoTable))),
                        new CodeMatch(instruction => instruction.opcode == OpCodes.Callvirt && ((MethodInfo)instruction.operand).Name == "GetEnumerator"))
                    .ThrowIfInvalid("foreach not found")
                    .Advance(1)
                    .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StoreHooks), nameof(ShopInitHook))))
                    .Instructions();
            }

            private static void ShopInitHook()
            {
                var instance = Object.FindObjectOfType<ShopView>();

                for (var i = 0; i < _customShopCategoryIcons.Count; i++)
                {
                    var icon = _customShopCategoryIcons[i];
                    var catIndex = i + CustomCategoryIndexOffset;
                    instance.iconTable[catIndex] = icon;
                }

                var shopInfoTable = SingletonInitializer<ActionScene>.instance.shopInfoTable;
                foreach (var customShopItem in _customShopItems)
                {
                    KoikatuAPI.Logger.LogDebug($"Adding custom item {customShopItem.Name} to the shop");

                    if (shopInfoTable.ContainsKey(customShopItem.ID))
                        KoikatuAPI.Logger.LogWarning($"Overwriting existing item ID={customShopItem.ID} with Name={customShopItem.Name}");

                    shopInfoTable[customShopItem.ID] = customShopItem;
                }
            }

            private static IEnumerable<CodeInstruction> OnBuyTpl(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ShopView), nameof(ShopView.SetInteractable))))
                    .ThrowIfInvalid("SetInteractable not found")
                    .Insert(new CodeInstruction(OpCodes.Dup), CodeInstruction.Call(typeof(StoreHooks), nameof(OnBuyHook)))
                    .Instructions();
            }

            private static void OnBuyHook(ShopItem itemBought)
            {
                KoikatuAPI.Logger.LogDebug($"Bought item {itemBought.name} in shop");
                if (_customShopBuyActions.TryGetValue(itemBought.param.ID, out var action) && action != null)
                {
                    try
                    {
                        action(itemBought);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                }
            }
        }
    }
}