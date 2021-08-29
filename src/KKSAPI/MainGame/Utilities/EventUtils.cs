using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActionGame.Chara;
using ADV;
using HarmonyLib;
using KKAPI.Utilities;
using Manager;
using UniRx;
using UnityEngine;

namespace KKAPI.MainGame
{
    /// <summary>
    /// Utilities for creating and playing custom ADV scenes (talking scenes with the text box at the bottom).
    /// Check CommandList.CommandGet in dnSpy to see source for different ADV commands that you can use.
    /// You can use the ADV_Editor plugin to help with creating custom events: https://github.com/ManlyMarco/ADV_Editor
    /// <code>
    /// var list = EventUtils.CreateNewEvent();
    /// list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Hi, what's your hobby?"));
    /// list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I make plugins for KK!"));
    /// list.Add(Program.Transfer.Text(EventUtils.HeroineName, "That got me wet, take me now!"));
    /// list.Add(Program.Transfer.Text(EventUtils.PlayerName, "No time, writing code."));
    /// list.Add(Program.Transfer.Close());
    /// return EventUtils.StartTextSceneEvent(talkScene, list, decreaseTalkTime: true);
    /// </code>
    /// </summary>
    public static class EventUtils
    {
        /// <summary>
        /// Get current save data of the player
        /// </summary>
        public static SaveData.Player GetPlayerData()
        {
            return Game.saveData?.player;
        }

        /// <summary>
        /// Helper for creating a new event command list. Sets up player variables automatically.
        /// </summary>
        /// <param name="waitForSceneFade">Stop processing inputs until any Scene.Fade finishes.</param>
        /// <param name="setPlayerParam">Automatically add any vars of the player to the start of the list (Program.SetParam).</param>
        public static List<Program.Transfer> CreateNewEvent(bool waitForSceneFade = false, bool setPlayerParam = true)
        {
            var list = Program.Transfer.NewList(isSceneRegulate: waitForSceneFade);
            if (setPlayerParam)
            {
                var playerData = GetPlayerData();
                if (playerData == null) throw new ArgumentNullException(nameof(playerData));
                Program.SetParam(playerData, list);
            }

            return list;
        }

        /// <summary>
        /// Use as Text owner to get the neutral white text used to describe the situation
        /// </summary>
        public const string Narrator = "";
        /// <summary>
        /// Returns empty on default character. Not sure what's the intended use, could be used to set text to player's color while not having player name appear
        /// Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work
        /// </summary>
        public const string Player = "[P]";
        /// <summary>
        /// Returns empty on default character. Avoid using, better to use PlayerFullName
        /// Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work
        /// </summary>
        public const string PlayerSurname = "[P姓]";
        /// <summary>
        /// Should be used when indicating who is speaking
        /// Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work
        /// </summary>
        public const string PlayerName = "[P名]";
        /// <summary>
        /// Name + Surname. Returns same as PlayerName on default character since he has no surname
        /// Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work
        /// </summary>
        public const string PlayerFullName = "[P名前]";
        /// <summary>
        /// Nickname. Doesn't seem to work, not set with Program.SetParam?
        /// </summary>
        public const string PlayerNickname = "[Pあだ名]";
        /// <summary>
        /// Avoid using, not sure what's the use
        /// Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work
        /// </summary>
        public const string Heroine = "[H]";
        /// <summary>
        /// Some characters might not have a surname, safer to use HeroineFullName
        /// Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work
        /// </summary>
        public const string HeroineSurname = "[H姓]";
        /// <summary>
        /// Should be used when indicating who is speaking
        /// Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work
        /// </summary>
        public const string HeroineName = "[H名]";
        /// <summary>
        /// Name + Surname
        /// Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work
        /// </summary>
        public const string HeroineFullName = "[H名前]";
        /// <summary>
        /// Nickname. Doesn't seem to work, not set with Program.SetParam?
        /// </summary>
        public const string HeroineNickname = "[Hあだ名]";

        /// <summary>
        /// Start a new ADV event inside of a TalkScene (same as stock game events when talking).
        /// The target heroine is automatically added to the heroine list, and its vars are set at the very start.
        /// </summary>
        /// <param name="talkScene">Currently opened talk scene</param>
        /// <param name="list">List of commands for the event</param>
        /// <param name="endTalkScene">If true, end the talk scene after this event finishes.</param>
        /// <param name="decreaseTalkTime">Should the talk time bar decrease after the event. If the bar runs out then the talk scene will end.</param>
        public static IEnumerator StartTextSceneEvent(TalkScene talkScene, List<Program.Transfer> list, bool endTalkScene = false, bool decreaseTalkTime = false) //todo needs to be changed over to unitask
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (talkScene == null)
                throw new ArgumentNullException(nameof(talkScene), "Has to be ran inside a TalkScene");
            if (talkScene.targetHeroine == null)
                throw new ArgumentNullException(nameof(talkScene.targetHeroine), "Heroine in TalkScene is null somehow?");

            list.Insert(0, Program.Transfer.Create(true, Command.CharaChange, "-2", "true"));

            AccessTools.Field(typeof(TalkScene), "isUpdateCamera").SetValue(talkScene, false);
            AccessTools.Method(typeof(TalkScene), "StartADV", new[] { typeof(List<Program.Transfer>) }).Invoke(talkScene, new object[] { list });

            yield return null;
            yield return Program.Wait("Talk");
            if (decreaseTalkTime)
            {
                GameAssist.Instance.DecreaseTalkTime(talkScene.targetHeroine, 1);
                if (talkScene.targetHeroine.talkTime <= 0) endTalkScene = true;
            }

            if (endTalkScene)
            {
                var talkEndM = AccessTools.Method(typeof(TalkScene), "TalkEnd");
                IEnumerator TalkEnd() => (IEnumerator)talkEndM.Invoke(talkScene, null);
                Observable.FromCoroutine(TalkEnd).Subscribe().AddTo(talkScene);
            }
        }

        /// <summary>
        /// Start a new ADV event. Can be used in roaming mode and some other cases. Do not use in TalkScenes, use <see cref="StartTextSceneEvent"/> instead.
        /// Can get variables set in the commands through actScene.AdvScene.Scenario.Vars
        /// You can use Program.SetParam(player, list) to add all parameters related to player/heroine. Alternatively use the CharaChange command.
        /// </summary>
        /// <param name="list">List of commands for the event</param>
        /// <param name="dialogOnly">If true, only open a dialog box with everything else still running. If false, open a full ADV scene with other NPCs paused and such.</param>
        /// <param name="extraData">Event called when running the scene and fade, also fade type</param>
        /// <param name="camera">Initial position and rotation of the camera</param>
        /// <param name="heroines">List of heroines to set in event context. They can be accessed from commands like CreateCharacter. If null, current Scenario.heroineList is used.</param>
        /// <param name="position">This is the position of the center character, NOT the camera. Camera is controlled separately and by default looks at the character from the front.</param>
        /// <param name="rotation">This is the rotation of the center character, NOT the camera. Camera is controlled separately and by default looks at the character from the front. Should only change Y rotation and keep XZ = 0.</param>
        public static IEnumerator StartAdvEvent(List<Program.Transfer> list, bool dialogOnly, Vector3 position, Quaternion rotation,
            Program.OpenDataProc extraData = null, OpenData.CameraData camera = null,
            List<SaveData.Heroine> heroines = null)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (KoikatuAPI.EnableDebugLogging && list.All(x => x.param.Command != Command.Close))
            {
                list.Add(Program.Transfer.Close());
                UnityEngine.Debug.LogWarning("No Close command in the transfer list! Adding close at end to prevent lockup. Add Program.Transfer.Close() at the end of list to fix this.");
            }

            var game = Singleton<Game>.Instance;
            var actScene = game.actScene;

            // has to be at start of list
            //var player = game.saveData.player;
            //Program.SetParam(player, list);

            var prevBgm = string.Empty;
            var prevVolume = 1f;

            yield return new WaitWhile(() => Singleton<Scene>.Instance.IsNowLoadingFade);
            yield return null;

            // Set up full adv scene with disabled NPCs, custom bgm and such
            actScene.Player.isActionNow = true;
            if (!dialogOnly)
            {
                actScene.SetPropertyValue("_isInChargeBGM", true);
                actScene.SetPropertyValue("isEventNow", true);
                actScene.Player.isLesMotionPlay = false;
                actScene.SetPropertyValue("shortcutKey", false);
                yield return Illusion.Game.Utils.Sound.GetBGMandVolume(delegate (string bgm, float volume)
                {
                    prevBgm = bgm;
                    prevVolume = volume;
                });
                yield return null;
                yield return new WaitUntil(() => Singleton<Scene>.Instance.AddSceneName.IsNullOrEmpty());
                yield return Scene.Instance.Fade(SimpleFade.Fade.In);
                actScene.Player.SetActive(false);
                actScene.npcList.ForEach(delegate (NPC p) { p.SetActive(false); });
                actScene.npcList.ForEach(delegate (NPC p) { p.Pause(true); });

                list.Insert(0, Program.Transfer.Create(false, Command.CameraSetFov, "23"));
            }
            else
            {
                if (camera == null)
                {
                    camera = new OpenData.CameraData
                    {
                        position = actScene.cameraTransform.position,
                        rotation = actScene.cameraTransform.rotation
                    };
                }
            }

            if (extraData == null)
            {
                extraData = new Program.OpenDataProc();
                //if (!dialogOnly) extraData.fadeType = Scene.Data.FadeType.InOut; // does nothing by itself
            }
            var isOpenAdv = false;
            extraData.onLoad += () =>
            {
                isOpenAdv = true;
                // Hack to fade back in
                if (!dialogOnly) KoikatuAPI.Instance.StartCoroutine(Scene.Instance.Fade(SimpleFade.Fade.Out));
            };
            var prevNowScene = actScene.AdvScene.nowScene;
            actScene.AdvScene.nowScene = KoikatuAPI.Instance;
            //var position = actScene.Player.position;
            //var rotation = actScene.Player.rotation;
            yield return Program.Open(new Data
            {
                fadeInTime = 0f, // setting to other than 0 breaks everything
                position = position,
                rotation = rotation,
                camera = camera,
                heroineList = heroines ?? actScene.AdvScene.Scenario.heroineList,
                scene = actScene,
                transferList = list
            }, extraData).PreventFromCrashing();
            yield return new WaitUntil(() => isOpenAdv);
            yield return Program.Wait(string.Empty);
            actScene.AdvScene.nowScene = prevNowScene;

            // Restore the game to normal after the full ADV scene
            actScene.Player.isActionNow = false;
            if (!dialogOnly)
            {
                actScene.npcList.ForEach(delegate (NPC p) { p.SetActive(p.mapNo == actScene.Map.no); });
                actScene.npcList.ForEach(delegate (NPC p)
                {
                    p.Pause(false);
                    p.isPopOK = true;
                    p.AI.FirstAction();
                    p.ReStart();
                });
                yield return new WaitUntil(() => actScene.MiniMapAndCameraActive);
                if (Illusion.Game.Utils.Scene.IsFadeOutOK)
                    yield return Singleton<Scene>.Instance.Fade(SimpleFade.Fade.Out); //todo not necessary?
                yield return Illusion.Game.Utils.Sound.GetFadePlayerWhileNull(prevBgm, prevVolume);
                actScene.SetPropertyValue("shortcutKey", true);
                actScene.SetPropertyValue("_isInChargeBGM", false);
                actScene.SetPropertyValue("isEventNow", false);
                actScene.Player.move.isStop = false;
                actScene.Player.isPopOK = true;
                actScene.Player.SetActive(true);
            }
        }
    }
}