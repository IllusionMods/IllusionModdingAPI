## `EventUtils`

Utilities for creating and playing custom ADV scenes (talking scenes with the text box at the bottom).  <code>  var list = EventUtils.CreateNewEvent();  list.Add(Program.Transfer.Text(EventUtils.HeroineName, "Hi, what's your hobby?"));  list.Add(Program.Transfer.Text(EventUtils.PlayerName, "I make plugins for KK!"));  list.Add(Program.Transfer.Text(EventUtils.HeroineName, "That got me wet, take me now!"));  list.Add(Program.Transfer.Text(EventUtils.PlayerName, "No time, writing code."));  list.Add(Program.Transfer.Close());  return EventUtils.StartTextSceneEvent(talkScene, list, decreaseTalkTime: true);  </code>
```csharp
public static class KKAPI.MainGame.EventUtils

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Heroine | Avoid using, not sure what's the use  Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work | 
| `String` | HeroineFullName | Name + Surname  Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work | 
| `String` | HeroineName | Should be used when indicating who is speaking  Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work | 
| `String` | HeroineNickname | Nickname. Doesn't seem to work, not set with Program.SetParam? | 
| `String` | HeroineSurname | Some characters might not have a surname, safer to use HeroineFullName  Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work | 
| `String` | Narrator | Use as Text owner to get the neutral white text used to describe the situation | 
| `String` | Player | Returns empty on default character. Not sure what's the intended use, could be used to set text to player's color while not having player name appear  Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work | 
| `String` | PlayerFullName | Name + Surname. Returns same as PlayerName on default character since he has no surname  Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work | 
| `String` | PlayerName | Should be used when indicating who is speaking  Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work | 
| `String` | PlayerNickname | Nickname. Doesn't seem to work, not set with Program.SetParam? | 
| `String` | PlayerSurname | Returns empty on default character. Avoid using, better to use PlayerFullName  Need to do Program.SetParam and/or Command.CharaChange on player/heroine for this to work | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `List<Transfer>` | CreateNewEvent(`Boolean` waitForSceneFade = False, `Boolean` setPlayerParam = True) | Helper for creating a new event command list. Sets up player variables automatically. | 
| `Player` | GetPlayerData() | Get current save data of the player | 
| `IEnumerator` | StartAdvEvent(`List<Transfer>` list, `Boolean` dialogOnly, `Vector3` position, `Quaternion` rotation, `OpenDataProc` extraData = null, `CameraData` camera = null, `List<Heroine>` heroines = null) | Start a new ADV event. Can be used in roaming mode and some other cases. Do not use in TalkScenes, use `KKAPI.MainGame.EventUtils.StartTextSceneEvent(TalkScene,System.Collections.Generic.List{ADV.Program.Transfer},System.Boolean,System.Boolean)` instead.  Can get variables set in the commands through actScene.AdvScene.Scenario.Vars  You can use Program.SetParam(player, list) to add all parameters related to player/heroine. Alternatively use the CharaChange command. | 
| `IEnumerator` | StartTextSceneEvent(`TalkScene` talkScene, `List<Transfer>` list, `Boolean` endTalkScene = False, `Boolean` decreaseTalkTime = False) | Start a new ADV event inside of a TalkScene (same as stock game events when talking).  The target heroine is automatically added to the heroine list, and its vars are set at the very start. | 


## `GameAPI`

Provides API for interfacing with the main game. It is useful mostly in the actual game, but some  functions will work outside of it (for example in FreeH).
```csharp
public static class KKAPI.MainGame.GameAPI

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | InsideHScene | True if any sort of H scene is currently loaded. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `IDisposable` | AddActionIcon(`Int32` mapNo, `Vector3` position, `Sprite` iconOn, `Sprite` iconOff, `Action` onOpen, `Action<TriggerEnterExitEvent>` onCreated = null, `Boolean` delayed = True, `Boolean` immediate = False) | Register a new action icon in roaming mode (like the icons for training/studying, club report screen, peeping).  Dispose the return value to remove the icon.  Icon templates can be found here https://github.com/IllusionMods/IllusionModdingAPI/tree/master/src/KKAPI/MainGame/ActionIcons | 
| `IDisposable` | AddTouchIcon(`Sprite` icon, `Action<Button>` onCreated, `Int32` row = 1, `Int32` order = 0) | Register a new touch icon in talk scenes in roaming mode (like the touch and look buttons on top right when talking to a character).  Dispose the return value to remove the icon.  Icon templates can be found here https://github.com/IllusionMods/IllusionModdingAPI/tree/master/src/KKAPI/MainGame/TouchIcons  By default this functions as a simple button. If you want to turn this into a toggle you have to manually switch button.image.sprite as needed. | 
| `IEnumerable<GameCustomFunctionController>` | GetBehaviours() | Get all registered behaviours for the game. | 
| `GameCustomFunctionController` | GetRegisteredBehaviour(`String` extendedDataId) | Get the first controller that was registered with the specified extendedDataId. | 
| `GameCustomFunctionController` | GetRegisteredBehaviour(`Type` controllerType) | Get the first controller that was registered with the specified extendedDataId. | 
| `GameCustomFunctionController` | GetRegisteredBehaviour(`Type` controllerType, `String` extendedDataId) | Get the first controller that was registered with the specified extendedDataId. | 
| `void` | RegisterExtraBehaviour(`String` extendedDataId) | Register new functionality that will be added to main game. Offers easy API for custom main game logic.  All you have to do is create a type that inherits from `KKAPI.MainGame.GameCustomFunctionController`&gt;  (don't make instances, the API will make them for you). Warning: The custom controller is immediately  created when it's registered, but its OnGameLoad method is not called until a game actually loads.  This might mean that if the registration happens too late you will potentially miss some load events. | 


Static Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler` | EndH | Fired after an H scene is ended, but before it is unloaded. Can be both in the main game and in free h.  Runs immediately after all `KKAPI.MainGame.GameCustomFunctionController` objects trigger their events. | 
| `EventHandler<GameSaveLoadEventArgs>` | GameLoad | Fired right after a game save is succesfully loaded.  Runs immediately after all `KKAPI.MainGame.GameCustomFunctionController` objects trigger their events. | 
| `EventHandler<GameSaveLoadEventArgs>` | GameSave | Fired right before the game state is saved to file.  Runs immediately after all `KKAPI.MainGame.GameCustomFunctionController` objects trigger their events. | 
| `EventHandler` | StartH | Fired after an H scene is loaded. Can be both in the main game and in free h.  Runs immediately after all `KKAPI.MainGame.GameCustomFunctionController` objects trigger their events. | 


## `GameCustomFunctionController`

Base type for custom game extensions.  It provides many useful methods that abstract away the nasty hooks needed to figure out when the state of the game  changes.    This controller is a MonoBehaviour that is created upon registration in `KKAPI.MainGame.GameAPI.RegisterExtraBehaviour``1(System.String)`.  The controller is created only once. If it's created too late it might miss some events.  It's recommended to register controllers in your Start method.
```csharp
public abstract class KKAPI.MainGame.GameCustomFunctionController
    : MonoBehaviour

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | ExtendedDataId | Extended save ID used by this function controller | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `PluginData` | GetExtendedData() | Get extended data based on supplied ExtendedDataId. When in chara maker loads data from character that's being loaded. | 
| `void` | OnDayChange(`Week` day) | Triggered when the current day changes in story mode. | 
| `void` | OnEndH(`HSceneProc` proc, `Boolean` freeH) | Triggered when the H scene is ended, but before it is unloaded.  Warning: This is triggered in free H as well! | 
| `void` | OnEnterNightMenu() | Triggered when the night menu is entered at the end of the day (screen where you can save and load the game).  You can use `KKAPI.MainGame.GameCustomFunctionController.GetCycle` to see what day it is as well as other game state. | 
| `void` | OnGameLoad(`GameSaveLoadEventArgs` args) | Triggered right after game state was loaded from a file. Some things might still be uninitialized. | 
| `void` | OnGameSave(`GameSaveLoadEventArgs` args) | Triggered right before game state is saved to a file. | 
| `void` | OnNewGame() | Triggered when a new game is started in story mode. | 
| `void` | OnPeriodChange(`Type` period) | Triggered when the current time of the day changes in story mode. | 
| `void` | OnStartH(`HSceneProc` proc, `Boolean` freeH) | Triggered after an H scene is loaded.  Warning: This is triggered in free H as well! | 
| `void` | SetExtendedData(`PluginData` data) | Save your custom data to the character card under the ID you specified when registering this controller. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Cycle` | GetCycle() | Get the current game Cycle object, if it exists. | 


## `GameExtensions`

Extensions useful in the main game
```csharp
public static class KKAPI.MainGame.GameExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | GetClassMaxSeatCount(this `SaveData` saveData, `Int32` classNumber) | Get seat count in a class based on game settings. | 
| `IEnumerable<Heroine>` | GetFreeClassSlots(this `SaveData` saveData, `Int32` classNumber) | Get a list of free slots in class roster. New heroines can be added to these slots in saveData.heroineList.  <example>  var freeSlot = saveData.GetFreeClassSlots(1).FirstOrDefault();  freeSlot.SetCharFile(fairyCard.charFile);  freeSlot.charFileInitialized = true;  saveData.heroineList.Add(freeSlot);  </example> | 
| `Heroine` | GetHeroine(this `ChaControl` chaControl) | Get the persisting heroine object that describes this character.  Returns null if the heroine could not be found. Works only in the main game. | 
| `Heroine` | GetHeroine(this `ChaFileControl` chaFile) | Get the persisting heroine object that describes this character.  Returns null if the heroine could not be found. Works only in the main game. | 
| `Heroine` | GetHeroineAtSeat(this `SaveData` saveData, `Int32` classNumber, `Int32` classIndex) | Get heroine at a specified class and seat. | 
| `NPC` | GetNPC(this `Heroine` heroine) | Get the NPC that represents this heroine in the game. Works only in the main game.  If the heroine has not been spawned into the game it returns null. | 
| `Player` | GetPlayer(this `ChaControl` chaControl) | Get the persisting player object that describes this character.  Returns null if the player could not be found. Works only in the main game. | 
| `Player` | GetPlayer(this `ChaFileControl` chaFile) | Get the persisting player object that describes this character.  Returns null if the player could not be found. Works only in the main game. | 
| `IEnumerable<ChaFileControl>` | GetRelatedChaFiles(this `Heroine` heroine) | Get ChaFiles that are related to this heroine. Warning: It might not return some copies. | 
| `IEnumerable<ChaFileControl>` | GetRelatedChaFiles(this `Player` player) | Get ChaFiles that are related to this heroine. Warning: It might not return some copies. | 
| `Boolean` | IsShowerPeeping(this `HFlag` hFlag) | Returns true if the H scene is peeping in the shower.  Use `HFlag.mode` to get info on what mode the H scene is in. | 
| `Boolean` | IsValidClassSeat(this `SaveData` saveData, `Int32` classNumber, `Int32` classIndex) | Check if the seat can be used for heroines.  Returns true even if the seats are disabled by config, use `KKAPI.MainGame.GameExtensions.GetClassMaxSeatCount(SaveData,System.Int32)` to check for this case. | 
| `void` | SetIsCursorLock(this `ActionScene` actScene, `Boolean` value) | Set the value of isCursorLock (setter is private by default).  Used to regain mouse cursor during roaming mode.  Best used together with setting `UnityEngine.Time.timeScale` to 0 to pause the game. | 


## `GameSaveLoadEventArgs`

Arguments used with main game save/load events.
```csharp
public class KKAPI.MainGame.GameSaveLoadEventArgs
    : EventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | FileName | Name of the safe file. | 
| `String` | FullFilename | Full filename of the save file. | 
| `String` | Path | Path to which the save file will be written. | 


## `IgnoreCaseEqualityComparer`

An equality comparer that uses StringComparison.OrdinalIgnoreCase rule
```csharp
public class KKAPI.MainGame.IgnoreCaseEqualityComparer
    : IEqualityComparer<String>

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | Equals(`String` x, `String` y) |  | 
| `Int32` | GetHashCode(`String` obj) |  | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `IgnoreCaseEqualityComparer` | Instance | Instance of the comparer for use in linq and such | 


## `TalkSceneUtils`

Utility methods for use with TalkScenes in main game roaming mode
```csharp
public static class KKAPI.MainGame.TalkSceneUtils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `List<Param>` | GetSenarioData(`Heroine` girl, `String` asset) | Get scenario data for a specified girl. The data is inside abdata\adv\scenario. | 
| `void` | Touch(this `TalkScene` talkScene, `TouchLocation` touchLocation, `TouchKind` touchKind, `Vector3` touchPosition = null) | Simulate touching the character in TalkScene with mouse. | 


