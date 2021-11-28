using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ADV;
using Cysharp.Threading.Tasks;
using Manager;
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
    public static class EventApi
    {
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
        /// Get current save data of the player
        /// </summary>
        public static SaveData.Player GetPlayerData()
        {
            return Game.saveData?.player;
        }

        /// <summary>
        /// Helper for creating a new event command list. Sets up player variables automatically.
        /// </summary>
        /// <param name="waitForSceneFade">Stop processing inputs until any Scene.Fade finishes (<see cref="Command.SceneFadeRegulate"/>, <see cref="Command.SceneFade"/>). If true, game might lock up in some cases.</param>
        /// <param name="setPlayerParam">Automatically add any vars of the player to the start of the list (Program.SetParam). You still need to add any Heroine params either through code or by selecting the character with the <see cref="Command.CharaChange"/> command.</param>
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
        /// Start a new ADV event inside of a TalkScene (same as stock game events when talking).
        /// The target heroine is automatically added to the heroine list, and their vars are set at the very start.
        /// Warning: You have to have a Close command or the scene will softlock after reaching the end!
        /// </summary>
        /// <param name="talkScene">Currently opened talk scene</param>
        /// <param name="commands">List of commands for the event</param>
        /// <param name="endTalkScene">If true, end the talk scene after this event finishes.</param>
        /// <param name="decreaseTalkTime">Should the talk time bar decrease after the event. If the bar runs out then the talk scene will end.</param>
        public static IEnumerator StartTextSceneEvent(TalkScene talkScene, IEnumerable<Program.Transfer> commands, bool decreaseTalkTime, bool endTalkScene = false)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (talkScene == null || !talkScene.isActive)
                throw new ArgumentNullException(nameof(talkScene), "Has to be ran inside of a TalkScene");
            if (talkScene.targetHeroine == null)
                throw new ArgumentNullException(nameof(talkScene.targetHeroine), "Heroine in TalkScene is null somehow?");

            var list = commands.ToList();

            // Auto load all parameters of the TalkScene heroine
            list.Insert(0, Program.Transfer.Create(true, Command.CharaChange, "-2", "true"));

            // todo handle this properly? Used when going back to title screen from F1 menu and possibly other places. Possibly not a problem since we run as a coroutine?
            var token = CancellationToken.None;

            talkScene.canvas.enabled = false;
            talkScene.raycast.enabled = false;
            yield return talkScene.StartADV(list, token).ToCoroutine(UnityEngine.Debug.LogException);

            if (decreaseTalkTime)
            {
                GameAssist.Instance.DecreaseTalkTime(talkScene.targetHeroine, 1);
                if (talkScene.targetHeroine.talkTime <= 0) endTalkScene = true;
            }

            if (endTalkScene)
                yield return talkScene.TalkEnd(token).ToCoroutine(UnityEngine.Debug.LogException);

            talkScene.LeaveAloneCancel();
            talkScene.lstColDisposable.ForEach(x => x.End());
            talkScene.TouchCancel();

            if (talkScene.transVoice != null)
            {
                Voice.Stop(talkScene.transVoice);
                talkScene.transVoice = null;
            }

            if (!token.IsCancellationRequested)
                talkScene.OnClickMain(-1);

            if (!endTalkScene)
                yield return talkScene.CommandWait(token).ToCoroutine(UnityEngine.Debug.LogException);
        }

        /// <summary>
        /// Start a new full ADV scene with fade in, full control over camera and map, multiple characters and such. Can be used in roaming mode and some other cases.
        /// Best to use this for custom events where player talks to other characters, or characters other than player talk (if player talks with himself consider using <see cref="StartMonologueEvent"/> instead).
        /// Do not use in TalkScenes, use <see cref="StartTextSceneEvent"/> instead.
        /// You can get variables set in the commands through actScene.AdvScene.Scenario.Vars.
        /// You can specify heroines you want to use in this script and use the CharaChange command to load their parameters. You can also use Program.SetParam to hard code the parameters in the list. 
        /// Warning: By default you need to have ScreenFadeRegulate and SceneFade commands at the start of your script, check fadeIn parameter description for more info.
        /// Warning: You have to have a Close command or the scene will softlock after reaching the end!
        /// </summary>
        /// <param name="commands">List of commands for the event</param>
        /// <param name="fadeIn">Should the scene fade in and out (otherwise it instantly pops in and out of existence, in that case consider using <see cref="StartMonologueEvent"/> instead).
        /// Important: With this turned on you have to have `ScreenFadeRegulate "false"` as first command or your script will never start (it's automatically added by <see cref="CreateNewEvent"/>).
        /// You also need to add a `SceneFade "out"` command at the start to fade out the effect and show the dialog (spawn characters and do all initialization before this step so it's not visible).
        /// At the end of the scene if you want to fade out then add a `SceneFade "in"` command at the end of your script (after closing it will fade back to the game automatically).</param>
        /// <param name="extraData">Event called when running the scene and fade, also fade type. Can be safely ignored in most cases.</param>
        /// <param name="camera">Initial position and rotation of the camera. By default the camera starts facing the center character. Can be safely ignored in most cases.</param>
        /// <param name="heroines">List of heroines to set in event context. They can be accessed from commands like <see cref="Command.CharaCreate"/> and <see cref="Command.CharaChange"/>. If null, current Scenario.heroineList is used.</param>
        /// <param name="position">This is the position of the center character, NOT the camera. Camera is controlled separately and by default looks at the character from the front.
        /// You can use <see cref="Command.CameraPositionSet"/> and <see cref="Command.CharaPositionSet"/> to change this from your script instead.</param>
        /// <param name="rotation">This is the rotation of the center character, NOT the camera. Camera is controlled separately and by default looks at the character from the front. Should only change Y rotation and keep XZ = 0.
        /// You can use <see cref="Command.CameraRotationSet"/> to change the camera rotation from your script.</param>
        public static IEnumerator StartAdvEvent(IEnumerable<Program.Transfer> commands, Vector3 position, Quaternion rotation, bool fadeIn = true,
            Program.OpenDataProc extraData = null, OpenData.CameraData camera = null, List<SaveData.Heroine> heroines = null)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            var list = commands.ToList();

            if (list.All(x => x.param.Command != Command.Close))
            {
                list.Add(Program.Transfer.Close());
                UnityEngine.Debug.LogWarning("No Close command in the transfer list! Adding close at end to prevent lockup. Add Program.Transfer.Close() at the end of list to fix this.");
            }

            // todo handle this properly? Used when going back to title screen from F1 menu and possibly other places. Possibly not a problem since we run as a coroutine?
            var token = CancellationToken.None;

            var actScene = ActionScene.instance;

            // Stop player input and some game functions
            actScene.Player.isActionNow = true;

            var prevBGM = string.Empty;
            var prevVolume = 1f;
            yield return Illusion.Game.Utils.Sound.GetBGMandVolume(x =>
            {
                prevBGM = x.Item1;
                prevVolume = x.Item2;
            }).ToCoroutine(UnityEngine.Debug.LogException);

            if (fadeIn)
            {
                // Sanity checks
                if (list.Take(4).All(x => x.param.Command != Command.SceneFadeRegulate))
                {
                    list.Insert(0, Program.Transfer.Create(true, Command.SceneFadeRegulate, "false"));
                    UnityEngine.Debug.LogWarning("No 'SceneFadeRegulate false' command at the start of the transfer list when fadeIn=true! Adding SceneFadeRegulate at start to prevent lockup. Add 'SceneFadeRegulate false' at the start of list to fix this.");
                }
                if (list.All(x => x.param.Command != Command.SceneFade && x.param.Args.FirstOrDefault()?.ToLower() != "out"))
                {
                    list.Insert(1, Program.Transfer.Create(false, Command.SceneFade, "out"));
                    UnityEngine.Debug.LogWarning("No 'SceneFade out' command in the transfer list! Adding 'SceneFade out' at start to prevent lockup. Add 'SceneFade out' after you are done with initializing the scene and before first text to fix this.");
                }
                if (list.All(x => x.param.Command != Command.SceneFade && x.param.Args.FirstOrDefault()?.ToLower() != "in"))
                {
                    UnityEngine.Debug.LogWarning("No 'SceneFade in' command in the transfer list! The scene won't fade out back to gameplay, instead it will instantly jump at the end until you add a 'SceneFade in' command before the Close command. Warning: Make sure that 'SceneFadeRegulate false' command has been ran before you run SceneFade or it will lock up.");
                }

                // Fade out of the gameplay into a loading screen
                //Scene.sceneFadeCanvas.SetColor(Color.black);
                yield return Scene.sceneFadeCanvas.StartFadeAysnc(FadeCanvas.Fade.In).ToCoroutine(UnityEngine.Debug.LogException);
            }

            list.Insert(1, Program.Transfer.Create(true, Command.CameraSetFov, Program.BASE_FOV));

            // Disable all characters to make sure nothing weird happens while the scene runs
            actScene.Player.SetActive(false);
            actScene.npcList.ForEach(p => p.SetActive(false));
            actScene.npcList.ForEach(p => p.Pause(true));
            if (actScene.fixChara != null) actScene.fixChara.SetActive(false);

            bool isOpenADV = false;

            if (extraData == null) extraData = new Program.OpenDataProc();
            extraData.onLoad += () => isOpenADV = true;

            // Run the actual scene
            yield return Program.Open(new Data
            {
                fadeInTime = 0f,
                position = position,
                rotation = rotation,
                camera = camera,
                heroineList = heroines ?? actScene.AdvScene.Scenario.heroineList,
                scene = SingletonInitializer<ActionScene>.instance,
                transferList = list
            }, token, extraData).ToCoroutine(UnityEngine.Debug.LogException);

            // Wait until the scene is done
            yield return UniTask.WaitUntil(() => isOpenADV, PlayerLoopTiming.Update, token).ToCoroutine(UnityEngine.Debug.LogException);
            yield return Program.Wait(string.Empty, token).ToCoroutine(UnityEngine.Debug.LogException);

            // Reenable all characters
            actScene.Player.SetActive(true);
            actScene.npcList.ForEach(p => p.SetActive(p.mapNo == actScene.Map.no));
            actScene.npcList.ForEach(p =>
            {
                p.Pause(false);
                // No longer needed in KKS?
                //p.isPopOK = true;
                //p.AI.FirstAction();
                //p.ReStart();
            });
            if (actScene.fixChara != null) actScene.fixChara.SetActive(actScene.fixChara.mapNo == actScene.Map.no);

            // Fade back to gameplay, assuming the script faded to loading screen, does nothing otherwise
            yield return Scene.sceneFadeCanvas.StartFadeAysnc(FadeCanvas.Fade.Out).ToCoroutine(UnityEngine.Debug.LogException);
            Scene.sceneFadeCanvas.DefaultColor();
            yield return Illusion.Game.Utils.Sound.GetFadePlayerWhileNull(prevBGM, prevVolume).ToCoroutine(UnityEngine.Debug.LogException);

            //actScene.Cycle.AddTimer(0.25f);

            // Reenable player controls
            actScene.Player.isActionNow = false;
        }

        /// <summary>
        /// Start a new "light" ADV scene that only pauses player input and shows a dialog box. Can be used in roaming mode and some other cases.
        /// Best to use this for events where player thinks or talks to himself with no other characters participating, for example after picking up an item on the map.
        /// Do not use in TalkScenes, use <see cref="StartTextSceneEvent"/> instead.
        /// You can get variables set in the commands through actScene.AdvScene.Scenario.Vars
        /// You need to use Program.SetParam if you want to add parameters related to a heroine. Alternatively use the CharaChange command with the global heroine syntax.
        /// Warning: You have to have a Close command or the scene will softlock after reaching the end!
        /// </summary>
        /// <param name="commands">List of commands for the event</param>
        public static IEnumerator StartMonologueEvent(IEnumerable<Program.Transfer> commands)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            var list = commands.ToList();

            if (list.All(x => x.param.Command != Command.Close))
            {
                list.Add(Program.Transfer.Close());
                UnityEngine.Debug.LogWarning("No Close command in the transfer list! Adding close at end to prevent lockup. Add Program.Transfer.Close() at the end of list to fix this.");
            }

            // todo handle this properly? Used when going back to title screen from F1 menu and possibly other places. Possibly not a problem since we run as a coroutine?
            var token = CancellationToken.None;

            var actScene = ActionScene.instance;

            // Stop player input and some game functions
            actScene.Player.isActionNow = true;

            var prevBGM = string.Empty;
            var prevVolume = 1f;
            yield return Illusion.Game.Utils.Sound.GetBGMandVolume(x =>
            {
                prevBGM = x.Item1;
                prevVolume = x.Item2;
            }).ToCoroutine(UnityEngine.Debug.LogException);

            ActionScene instance = SingletonInitializer<ActionScene>.instance;
            Transform transform = Camera.main.transform;

            bool isOpenADV = false;

            // Run the actual scene
            yield return Program.Open(new Data
            {
                position = instance.Player.position,
                rotation = instance.Player.rotation,
                scene = instance,
                camera = new OpenData.CameraData
                {
                    position = transform.position,
                    rotation = transform.rotation
                },
                transferList = list
            }, token, new Program.OpenDataProc
            {
                onLoad = delegate
                {
                    isOpenADV = true;
                }
            }).ToCoroutine(UnityEngine.Debug.LogException);

            // Wait until the scene is done
            yield return UniTask.WaitUntil(() => isOpenADV, PlayerLoopTiming.Update, token).ToCoroutine(UnityEngine.Debug.LogException);
            yield return Program.ADVProcessingCheck(token).ToCoroutine(UnityEngine.Debug.LogException);

            // Fade back to gameplay, assuming the script faded to loading screen, does nothing otherwise
            yield return Illusion.Game.Utils.Sound.GetFadePlayerWhileNull(prevBGM, prevVolume).ToCoroutine(UnityEngine.Debug.LogException);
            Scene.sceneFadeCanvas.DefaultColor();

            // Reenable player controls
            actScene.Player.isActionNow = false;
        }
    }
}