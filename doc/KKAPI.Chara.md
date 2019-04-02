## `CharacterApi`

Provides an easy way to add custom logic to all characters in the game and in studio.  It takes care of all the error-prone plumbing and lets you easily save and load data to the character cards.
```csharp
public static class KKAPI.Chara.CharacterApi

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `HashSet<ChaControl>` | ChaControls |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `IEnumerable<ControllerRegistration>` | RegisteredHandlers | All currently registered kinds of `KKAPI.Chara.CharaCustomFunctionController` controllers. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `ChaControl` | FileControlToChaControl(`ChaFileControl` fileControl) | Get ChaControl that is using the specified ChaFileControl. | 
| `IEnumerable<CharaCustomFunctionController>` | GetBehaviours(`ChaControl` character = null) | Get all extra behaviours for specified character. If null, returns extra behaviours for all characters. | 
| `void` | Init() |  | 
| `void` | RegisterExtraBehaviour(`String` extendedDataId) | Register new functionality that will be automatically added to all characters (where applicable).  Offers easy API for saving and loading extended data, and for running logic to apply it to the characters.  All necessary hooking and event subscribing is done for you. All you have to do is create a type  that inherits from <code>CharaExtraBehaviour</code> (don't make instances, the API will make them for you). | 
| `void` | RegisterExtraBehaviour(`String` extendedDataId, `CopyExtendedDataFunc` customDataCopier) | Register new functionality that will be automatically added to all characters (where applicable).  Offers easy API for saving and loading extended data, and for running logic to apply it to the characters.  All necessary hooking and event subscribing is done for you. All you have to do is create a type  that inherits from <code>CharaExtraBehaviour</code> (don't make instances, the API will make them for you). | 


Static Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler<CharaReloadEventArgs>` | CharacterReloaded | Fired after all CharaCustomFunctionController have updated. | 
| `EventHandler<CoordinateEventArgs>` | CoordinateLoaded | Fired after a coordinate card was loaded and all controllers were updated.  Not filed if the coordinate file was not loaded into a character (so not during list updates). | 
| `EventHandler<CoordinateEventArgs>` | CoordinateSaving | Fired just before a coordinate card is saved, but after all controllers wrote their data. | 


## `CharacterExtensions`

Extensions for use with ChaControl, ChaFile and similar
```csharp
public static class KKAPI.Chara.CharacterExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `ChaControl` | GetChaControl(this `ChaFile` chaFile) | Get ChaControl that is using this ChaFile if any exist. | 


## `CharaCustomFunctionController`

Base type for custom character extensions.  It provides many useful methods that abstract away the nasty hooks needed to figure out when  a character is changed or how to save and load your custom data to the character card.    This controller is a MonoBehaviour that is added to root gameObjects of ALL characters spawned into the game.  It's recommended to not use constructors, Awake or Start in controllers. Use `KKAPI.Chara.CharaCustomFunctionController.OnReload(KKAPI.GameMode,System.Boolean)` instead.
```csharp
public abstract class KKAPI.Chara.CharaCustomFunctionController
    : MonoBehaviour

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ChaControl` | ChaControl | ChaControl of the character this controller is attached to. It's on the same gameObject as this controller. | 
| `ChaFileControl` | ChaFileControl | ChaFile of the character this controller is attached to. | 
| `ControllerRegistration` | ControllerRegistration | Definition of this kind of function controllers. | 
| `BehaviorSubject<CoordinateType>` | CurrentCoordinate | Currently selected clothes on this character. Can subscribe to listen for changes. | 
| `String` | ExtendedDataId | ID used for extended data by this controller. It's set when registering the controller  with `KKAPI.Chara.CharacterApi.RegisterExtraBehaviour``1(System.String)` | 
| `Boolean` | Started | True if this controller has been initialized | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Awake() | Warning: When overriding make sure to call the base method at the end of your logic! | 
| `PluginData` | GetCoordinateExtendedData(`ChaFileCoordinate` coordinate) | Get extended data of the specified coordinate by using the ID you specified when registering this controller. | 
| `PluginData` | GetExtendedData() | Get extended data based on supplied ExtendedDataId. When in chara maker loads data from character that's being loaded. | 
| `PluginData` | GetExtendedData(`Boolean` getFromLoadedChara) | Get extended data based on supplied ExtendedDataId. When in chara maker loads data from character that's being loaded. | 
| `void` | OnCardBeingSaved(`GameMode` currentGameMode) | Fired when the character information is being saved.  It handles all types of saving (to character card, to a scene etc.)  Write any of your extended data in this method by using `KKAPI.Chara.CharaCustomFunctionController.SetExtendedData(ExtensibleSaveFormat.PluginData)`.  Avoid reusing old PluginData since we might no longer be pointed to the same character. | 
| `void` | OnCardBeingSavedInternal(`GameMode` gamemode) |  | 
| `void` | OnCoordinateBeingLoaded(`ChaFileCoordinate` coordinate, `Boolean` maintainState) | Fired just after loading a coordinate card into the current coordinate slot.  Use `KKAPI.Chara.CharaCustomFunctionController.GetCoordinateExtendedData(ChaFileCoordinate)` to get save data of the loaded coordinate.  Use `KKAPI.Chara.CharaCustomFunctionController.CurrentCoordinate` to figure out what clothes set your character is wearing right now. | 
| `void` | OnCoordinateBeingLoadedInternal(`ChaFileCoordinate` coordinate) |  | 
| `void` | OnCoordinateBeingSaved(`ChaFileCoordinate` coordinate) | Fired just before current coordinate is saved to a coordinate card. Use `KKAPI.Chara.CharaCustomFunctionController.SetCoordinateExtendedData(ChaFileCoordinate,ExtensibleSaveFormat.PluginData)` to save data to it.  You might need to wait for the next frame with `UnityEngine.MonoBehaviour.StartCoroutine(System.Collections.IEnumerator)` before handling this.  Use `KKAPI.Chara.CharaCustomFunctionController.CurrentCoordinate` to figure out what clothes set your character is wearing right now. | 
| `void` | OnCoordinateBeingSavedInternal(`ChaFileCoordinate` coordinate) |  | 
| `void` | OnDestroy() | Warning: When overriding make sure to call the base method at the end of your logic! | 
| `void` | OnEnable() | Warning: When overriding make sure to call the base method at the end of your logic! | 
| `void` | OnReload(`GameMode` currentGameMode, `Boolean` maintainState) | OnReload is fired whenever the character's state needs to be updated.  This might be beacuse the character was just loaded into the game,  was replaced with a different character, etc.  Use this method instead of Awake and Start. It will always get called  before other methods, but after the character is in a usable state.  WARNING: Make sure to completely reset your state in this method!  Assume that all of your variables are no longer valid! | 
| `void` | OnReloadInternal(`GameMode` currentGameMode) |  | 
| `void` | SetCoordinateExtendedData(`ChaFileCoordinate` coordinate, `PluginData` data) | Set extended data to the specified coordinate by using the ID you specified when registering this controller. | 
| `void` | SetExtendedData(`PluginData` data) | Save your custom data to the character card under the ID you specified when registering this controller. | 
| `void` | Start() | Warning: When overriding make sure to call the base method at the end of your logic! | 
| `void` | Update() | Warning: When overriding make sure to call the base method at the end of your logic! | 


## `CharaReloadEventArgs`

Event arguments used by character reload events
```csharp
public class KKAPI.Chara.CharaReloadEventArgs
    : EventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ChaControl` | ReloadedCharacter | Can be null when all characters in a scene are reloaded | 


## `CoordinateEventArgs`

Fired in events that deal with coordinate cards
```csharp
public class KKAPI.Chara.CoordinateEventArgs
    : EventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ChaControl` | Character | Character the coordinate was loaded to | 
| `ChaFileCoordinate` | LoadedCoordinate | The loaded coordinate | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Dictionary<String, PluginData>` | GetCoordinateExtData() | Get all exrtended data assigned to this coordinate card | 
| `void` | SetCoordinateExtData(`String` dataId, `PluginData` data) | Set extended data for this coordinate card | 


