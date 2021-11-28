using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using ActionGame;
using ActionGame.Communication;
using ADV;
using HarmonyLib;
using KKAPI.Studio;
using KKAPI.Utilities;
using Manager;
using UniRx;

namespace KKAPI.MainGame
{
    /// <summary>
    /// Allows adding custom topics to the game. You can use them when talking to characters to trigger your own custom events or other actions.
    /// Topics should be registered while still in the title menu or earlier (before the player loads a save file or starts a new game), but registering them later can work too (make sure to test well).
    /// Only useful in the story mode.
    /// </summary>
    public static class TopicApi
    {
        /// <summary>
        /// Apply results of talking about a custom topic. Fired immediately after the ADV scene ends.
        /// Return value is used to add to basic parameters with a fancy animation. The topic field takes an index of the <see cref="GameAssist.topicDrops"/> list (it has an ID of a topic and a % chance of it dropping, it's better to just do it on your own instead of trying to use this).
        /// </summary>
        /// <param name="scene">Talk scene we are in</param>
        /// <param name="topicNo">ID of the custom topic we are talking about</param>
        /// <param name="personality">Personality of the heroine</param>
        /// <param name="isNpc">Is the heroine an NPC (namely the island girl)</param>
        /// <param name="advVars">Variable dictionary from the ADV scene.
        /// You can use this to show choices in your custom ADV scene for player to click (with the Choice command), then set a variable based on what the player chooses (with the VAR command), and finally look for your variable in this dictionary and decide what to do.
        /// In case you want to give the player a cancel option, you can work around the topic being removed at the end by using <see cref="AddTopicToInventory"/> with popup disabled to give it back.</param>
        public delegate ChangeValueTopicInfo GetTopicADVresult(TalkScene scene, int topicNo, int personality, bool isNpc, Dictionary<string, ValData> advVars);

        /// <summary>
        /// Apply results of talking about a custom topic. Fired immediately after the ADV scene ends.
        /// Return value is used to add to basic parameters with a fancy animation.
        /// </summary>
        /// <param name="scene">Talk scene we are in</param>
        /// <param name="topicNo">ID of the custom topic we are talking about</param>
        /// <param name="personality">Personality of the heroine</param>
        /// <param name="isNpc">Is the heroine an NPC (namely the island girl)</param>
        public delegate List<Program.Transfer> GetTopicADVscript(TalkScene scene, int topicNo, int personality, bool isNpc);

        /// <summary>
        /// Category of your topic. Changes which icon is shown, how it can be obtained and when it is shown.
        /// </summary>
        public enum TopicCategory
        {
            /// <summary>
            /// Used for topics related to playing and lazing around.
            /// Can randomly drop if the rarity is 3 or lower.
            /// Can be traded for in the topic exchange shop.
            /// Shown with an icon of beach sunbed and umbrella.
            /// </summary>
            Leisure = 0,
            /// <summary>
            /// Used for topics related to nature.
            /// Can randomly drop if the rarity is 3 or lower.
            /// Can be traded for in the topic exchange shop.
            /// Shown with an icon of a flower.
            /// </summary>
            Nature = 1,
            /// <summary>
            /// Used for topics related to the sea and sea creatures.
            /// Can randomly drop if the rarity is 3 or lower.
            /// Can be traded for in the topic exchange shop.
            /// Shown with an icon of a shell.
            /// </summary>
            Sealife = 2,
            /// <summary>
            /// Used for topics related to the occult and traditions.
            /// Can randomly drop if the rarity is 3 or lower.
            /// Can be traded for in the topic exchange shop.
            /// Shown with an icon of a weird painted face.
            /// </summary>
            Occult = 3,
            /// <summary>
            /// Used for topics related to love and eros.
            /// Topics with this category will not appear when talking with NPCs (island girl), with virgins, or if the R18 patch is not installed.
            /// Can randomly drop if the rarity is 3 or lower.
            /// Can NOT be traded for in the topic exchange shop. In base game these topics are only obtainable through special means. You can use <see cref="TopicApi.AddTopicToInventory"/> to add them.
            /// Shown with an icon of hearts.
            /// </summary>
            Love = 4,
        }

        /// <summary>
        /// Rarity of the topic. Changes what color background is used and how it can be obtained.
        /// </summary>
        public enum TopicRarity
        {
            /// <summary>
            /// Can drop on the map.
            /// </summary>
            Rarity1 = 0,
            /// <summary>
            /// Can drop on the map.
            /// </summary>
            Rarity2,
            /// <summary>
            /// Can only drop if player has the "good topic drops" prayer effect.
            /// </summary>
            Rarity3,
            /// <summary>
            /// Can't drop. It needs to be added through code or obtained through the topic merge shop.
            /// </summary>
            Rarity4,
            /// <summary>
            /// Can't drop. It needs to be added through code or obtained through the topic merge shop.
            /// </summary>
            Rarity5,
        }

        private static readonly Dictionary<int, CustomTopicInfo> _customTopics = new Dictionary<int, CustomTopicInfo>();

        /// <summary>
        /// Add a custom topic to the game.
        /// Custom topics can not be asked about by heroines when using the "listen" option, and selecting a custom topic as an answer will always result in failing the prompt. You can only use custom topics when talking to the heroine (speech bubble icon).
        /// If you want your topic to not be obtainable outside of giving it to the player through code, you need to set rarity to <see cref="TopicRarity.Rarity4"/> or higher, and category to <see cref="TopicCategory.Love"/>.
        /// </summary>
        /// <param name="topicNo">Unique ID of the topic.
        /// This ID is used in the save file to keep track of owned and used topics, so it has to always be the same between game starts (i.e. use a hardcoded number and never change it).
        /// Topic IDs below 100 are reserved for the base game. For safety it's best to use IDs above 100000. Be careful to not conflict with other plugins!</param>
        /// <param name="topicName">Name of your topic shown on the lists.</param>
        /// <param name="category">Category of your topic. Changes which icon is shown, how it can be obtained and when it is shown.</param>
        /// <param name="rarity">Rarity of your topic. Changes what color background is used and how it can be obtained.</param>
        /// <param name="getAdvScript">Called when player chooses your topic to talk about. It should return a valid ADV event (remember to add Close at the end!) or null if you don't want to display anything. You can use the ADVEditor plugin to create your own scenes. The base game uses different variations of events based on the personality and sometimes relationship of the character you talk to. You can do the same by checking the personality and isNPC parameters.</param>
        /// <param name="getAdvResult">Called after your ADV event from 'getAdvScript' finishes. You can change basic stats with the return value (they will be animated), or do something else (return null if you don't want to change any basic stats).</param>
        /// <returns>Dispose the return value to remove the topic. Warning: This is intended only for development use! This might not remove the topic immediately or fully, and you might need to go back to title menu and load the game again for the changes to take effect. Disposing won't clear the topic from topic inventory or other similar lists.</returns>
        public static IDisposable RegisterTopic(int topicNo, string topicName, TopicCategory category, TopicRarity rarity, GetTopicADVscript getAdvScript, GetTopicADVresult getAdvResult)
        {
            if (StudioAPI.InsideStudio) return Disposable.Empty;

            if (topicNo < 100) throw new ArgumentOutOfRangeException(nameof(topicNo), topicNo, "Topic IDs below 100 are reserved for the base game");
            if (topicName == null) throw new ArgumentNullException(nameof(topicName));
            if (!Enum.IsDefined(typeof(TopicCategory), category)) throw new ArgumentOutOfRangeException(nameof(category), category, "Invalid TopicCategory");
            if (!Enum.IsDefined(typeof(TopicRarity), rarity)) throw new ArgumentOutOfRangeException(nameof(rarity), rarity, "Invalid TopicRarity");

            // As far as I can see RarityName is unused in the game so it doesn't matter what it's set to
            return RegisterTopic(
                new Topic.Param
                { No = topicNo, Name = topicName, Category = (int)category, Rarity = (int)rarity, RarityName = "Modded" },
                getAdvScript, getAdvResult);
        }

        /// <summary>
        /// Add a custom topic to the game. Check the other overloads for more info.
        /// Warning: This overload skips most of the parameter checks! It's best to use the other overload instead.
        /// </summary>
        public static IDisposable RegisterTopic(Topic.Param param, GetTopicADVscript getAdvScript, GetTopicADVresult getAdvResult)
        {
            if (StudioAPI.InsideStudio) return Disposable.Empty;

            if (param == null) throw new ArgumentNullException(nameof(param));
            if (getAdvScript == null) throw new ArgumentNullException(nameof(getAdvScript), "You need to return some sort of ADV script or picking the topic will crash the game. You can return a null if you don't want to show any ADV.");
            if (getAdvResult == null) throw new ArgumentNullException(nameof(getAdvResult), "You need to return some sort of ADV result. You can return a null if you don't want to add any stats.");

            if (param.No < 100) throw new ArgumentOutOfRangeException(nameof(param), param.No, "No has to be above 100");

            TopicHooks.ApplyHooksIfNeeded();

            _customTopics.Add(param.No, new CustomTopicInfo(param, getAdvScript, getAdvResult));

            // All topics get added by a hook when ActionScene is loaded, need to add manually after that
            if (ActionScene.initialized && ActionScene.instance.topicDic != null)
                ActionScene.instance.topicDic.Add(param.No, param);

            return Disposable.Create(() =>
            {
                _customTopics.Remove(param.No);
                ActionScene.instance.topicDic.Remove(param.No);
            });
        }

        /// <summary>
        /// Add topic to player's topic inventory/stock. Does nothing if the topic doesn't exist in <see cref="ActionScene.topicDic"/>.
        /// Returns true if adding was successful, false if there are already 99 of this topic, or if 'amount' is negative and there are 0 of this topic.
        /// </summary>
        /// <param name="topicID">ID of the topic, as seen in ActionScene.instance.topicDic</param>
        /// <param name="showPopup">Should the "topic gained" popup be shown to the player (might not work outside of the roaming map and talk scenes, make sure to test)</param>
        /// <param name="amount">How much of the topic to add. Set to negative number to subtract. Range is -99 to 99 (player can only hold 99 of a single topic).</param>
        public static bool AddTopicToInventory(int topicID, bool showPopup = true, int amount = 1)
        {
            if (StudioAPI.InsideStudio) return false;

            if (amount < -99 || amount > 99) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Value must be between -99 and 99");
            if (!ActionScene.initialized) throw new InvalidOperationException("ActionScene is not initialized");

            if (ActionScene.instance.topicDic.TryGetValue(topicID, out var param))
            {
                if (showPopup && !InformationUI.initialized) showPopup = false;

                var topicStock = Game.Player.topicStock;

                if (!topicStock.TryGetValue(topicID, out var topicState))
                    topicState = topicStock[topicID] = new SaveData.Player.TopicState();

                if (topicState.Add(amount))
                {
                    if (showPopup) InformationUI.SetTopic(param.Name);
                    return true;
                }
                else
                {
                    if (showPopup) InformationUI.SetTopicFull();
                    return false;
                }
            }
            //todo handle missing topics differently?
            KoikatuAPI.Logger.LogWarning("Tried to add nonexisting topic ID=" + topicID);
            return false;
        }

        private sealed class CustomTopicInfo
        {
            public readonly Topic.Param Param;
            public readonly GetTopicADVresult ResultGetter;
            public readonly GetTopicADVscript ScriptGetter;

            public CustomTopicInfo(Topic.Param param, GetTopicADVscript scriptGetter, GetTopicADVresult resultGetter)
            {
                Param = param;
                ScriptGetter = scriptGetter;
                ResultGetter = resultGetter;
            }
        }

        private static class TopicHooks
        {
            private static bool _topichooked;

            public static void ApplyHooksIfNeeded()
            {
                if (_topichooked) return;
                _topichooked = true;

                KoikatuAPI.Logger.LogWarning("TopicHooks.ApplyHooks");

                var h = new Harmony(nameof(TopicApi) + "_Hooks");
                h.Patch(AccessTools.PropertySetter(typeof(ActionScene), nameof(ActionScene._topicDic)), postfix: new HarmonyMethod(typeof(TopicHooks), nameof(TopicHooks.TopicDicSetHook)));
                h.PatchMoveNext(AccessTools.Method(typeof(TalkScene), nameof(TalkScene.CommandFunc)), transpiler: new HarmonyMethod(typeof(TopicHooks), nameof(TryPlayCustomTopicAdvTpl)));
            }

            public static void TopicDicSetHook(ActionScene __instance)
            {
                foreach (var customTopic in _customTopics)
                {
                    var no = customTopic.Key;
                    if (__instance._topicDic.ContainsKey(no))
                        KoikatuAPI.Logger.LogWarning($"Overwriting existing topic No={no} Name={__instance._topicDic[no].Name} with Name={customTopic.Value.Param.Name}");
                    __instance._topicDic[no] = customTopic.Value.Param;
                }
            }

            private static IEnumerable<CodeInstruction> TryPlayCustomTopicAdvTpl(IEnumerable<CodeInstruction> instructions)
            {
                var matcher = new CodeMatcher(instructions)
                    .MatchForward(true, new CodeMatch(OpCodes.Ldstr, "話題を振る"))
                    .MatchForward(true, new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(TalkScene), nameof(TalkScene.isNPC))))
                    .Advance(1)
                    .ThrowIfNotMatch("Brtrue not found", new CodeMatch(OpCodes.Brtrue));

                var startPos = matcher.Pos;

                // Figure out where the if else ends so we can jump there if we want to skip it
                // First go to the start of else, then step back and get the label at the end of if that skips over the else
                var elseLabel = ((Label)matcher.Operand);
                matcher.MatchForward(false, new CodeMatch(instruction => instruction.labels.Contains(elseLabel)))
                    .Advance(-1)
                    .ThrowIfNotMatch("Br not found", new CodeMatch(OpCodes.Br));
                var skipIfelseLabel = matcher.Operand;

                // Go back to the start of the if
                matcher.Advance(startPos - matcher.Pos).ThrowIfNotMatch("Brtrue not found 2", new CodeMatch(OpCodes.Brtrue));

                // Go back to the first if, then insert our if before it
                // Copy the `this` load instead of hardcoding it just in case (isNPC takes it)
                matcher.Advance(-2);
                var loadInstrCopy = new CodeInstruction(matcher.Opcode, matcher.Operand);
                matcher.Advance(1).Insert(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TopicHooks), nameof(TryPlayCustomTopicAdvHook))),
                    new CodeInstruction(OpCodes.Brtrue, skipIfelseLabel),
                    loadInstrCopy);

                return matcher.Instructions();
            }

            private static bool TryPlayCustomTopicAdvHook(TalkScene scene)
            {
                var topic = scene.topics[scene.selectTopic];
                var topicNo = topic.No;

                _customTopics.TryGetValue(topicNo, out var topicInfo);
                if (topicInfo != null)
                {
                    KoikatuAPI.Logger.LogDebug("Handling custom topic No=" + topicNo);
                    var script = topicInfo.ScriptGetter(scene, topicNo, scene.targetHeroine.personality, scene.isNPC);
                    if (script != null)
                    {
                        KoikatuAPI.Logger.LogDebug("Playing ADV scene for the topic");

                        scene.StartADV(script, CancellationToken.None).GetAwaiter().GetResult(); //todo handle cancelling

                        var vars = ActionScene.initialized ? ActionScene.instance.AdvScene.Scenario.Vars : SceneParameter.advScene.Scenario.Vars;

                        var result = topicInfo.ResultGetter(scene, topicNo, scene.targetHeroine.personality, scene.isNPC, vars);
                        scene.m_CVInfo = result ?? new ChangeValueTopicInfo();
                    }
                    return true;
                }

                return false;
            }
        }
    }
}