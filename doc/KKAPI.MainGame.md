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
| `Heroine` | GetHeroine(this `ChaControl` chaControl) | Get the persisting heroine object that describes this character.  Returns null if the heroine could not be found. Works only in the main game. | 
| `Heroine` | GetHeroine(this `ChaFileControl` chaFile) | Get the persisting heroine object that describes this character.  Returns null if the heroine could not be found. Works only in the main game. | 
| `NPC` | GetNPC(this `Heroine` heroine) | Get the NPC that represents this heroine in the game. Works only in the main game.  If the heroine has not been spawned into the game it returns null. | 
| `Player` | GetPlayer(this `ChaControl` chaControl) | Get the persisting player object that describes this character.  Returns null if the player could not be found. Works only in the main game. | 
| `Player` | GetPlayer(this `ChaFileControl` chaFile) | Get the persisting player object that describes this character.  Returns null if the player could not be found. Works only in the main game. | 
| `IEnumerable<ChaFileControl>` | GetRelatedChaFiles(this `Heroine` heroine) | Get ChaFiles that are related to this heroine. Warning: It might not return some copies. | 
| `IEnumerable<ChaFileControl>` | GetRelatedChaFiles(this `Player` player) | Get ChaFiles that are related to this heroine. Warning: It might not return some copies. | 
| `Boolean` | IsShowerPeeping(this `HFlag` hFlag) | Returns true if the H scene is peeping in the shower.  Use `HFlag.mode` to get info on what mode the H scene is in. | 


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


