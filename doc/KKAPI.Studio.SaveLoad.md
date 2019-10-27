## `ObjectsCopiedEventArgs`

Arguments used in objects copied events
```csharp
public class KKAPI.Studio.SaveLoad.ObjectsCopiedEventArgs
    : EventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ReadOnlyDictionary<Int32, ObjectCtrlInfo>` | LoadedObjects | Objects copied by the event and their original IDs | 


## `SceneCustomFunctionController`

Base type for custom scene/studio extensions.  It provides many useful methods that abstract away the nasty hooks needed to figure out when  a scene is loaded or imported, or how to save and load your custom data to the scene file.    This controller is a MonoBehaviour that is created upon registration in `KKAPI.Studio.SaveLoad.StudioSaveLoadApi.RegisterExtraBehaviour``1(System.String)`.  The controller is created only once. If it's created too late it might miss some scene load events.  It's recommended to register controllers in your Start method.
```csharp
public abstract class KKAPI.Studio.SaveLoad.SceneCustomFunctionController
    : MonoBehaviour

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | ExtendedDataId | ID used for extended data by this controller. It's set when registering the controller  with `KKAPI.Studio.SaveLoad.StudioSaveLoadApi.RegisterExtraBehaviour``1(System.String)` | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `PluginData` | GetExtendedData() | Get extended data of the last loaded scene by using the ID you specified when registering this controller. | 
| `Studio` | GetStudio() | Get the instance of the Studio game manager object. | 
| `void` | OnObjectsCopied(`ReadOnlyDictionary<Int32, ObjectCtrlInfo>` copiedItems) | Fired when objects are copied. | 
| `void` | OnSceneLoad(`SceneOperationKind` operation, `ReadOnlyDictionary<Int32, ObjectCtrlInfo>` loadedItems) | Fired when a scene is successfully changed, either by loading, importing or resetting. | 
| `void` | OnSceneSave() | Fired when a scene is about to be saved and any exteneded data needs to be written. | 
| `void` | SetExtendedData(`PluginData` data) | Save your custom data to the scene under the ID you specified when registering this controller. | 


## `SceneLoadEventArgs`

Arguments used in scene loaded/imported events
```csharp
public class KKAPI.Studio.SaveLoad.SceneLoadEventArgs
    : EventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ReadOnlyDictionary<Int32, ObjectCtrlInfo>` | LoadedObjects | Objects loaded by the event and their original IDs (from the time the scene was saved) | 
| `SceneOperationKind` | Operation | Operation that caused the event | 


## `SceneOperationKind`

Scene load/change operations
```csharp
public enum KKAPI.Studio.SaveLoad.SceneOperationKind
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Load | Scene is being loaded and will replace what's currently loaded. | 
| `1` | Import | Scene is being loaded and will be added to what's currently loaded.  <remarks>IDs in the scene will be different from the IDs in the file of the scene being imported,  use `KKAPI.Studio.SaveLoad.SceneCustomFunctionController` to get IDs from the scene file.</remarks> | 
| `2` | Clear | Scene is being cleared of all state (by default, only user clicking the "Reset" button can trigger this).  This is not triggered when studio starts. | 


## `StudioSaveLoadApi`

Provides API for loading and saving scenes, as well as a convenient way for registering custom studio functions.
```csharp
public static class KKAPI.Studio.SaveLoad.StudioSaveLoadApi

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | ImportInProgress | A scene is currently being imported | 
| `Boolean` | LoadInProgress | A scene is currently being loaded (not imported or cleared) | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Init() |  | 
| `void` | RegisterExtraBehaviour(`String` extendedDataId) | Register new functionality that will be added to studio. Offers easy API for saving and loading extended data.  All necessary hooking and event subscribing is done for you. Importing scenes is also handled for you.  All you have to do is create a type that inherits from `KKAPI.Studio.SaveLoad.SceneCustomFunctionController`&gt;  (don't make instances, the API will make them for you). Warning: The custom controller is immediately  created when it's registered, but its OnSceneLoad method is not called until a scene actually loads.  This might mean that if the registration happens too late you will potentially miss some load events. | 


Static Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler<ObjectsCopiedEventArgs>` | ObjectsCopied | Fired when objects in the scene are copied | 
| `EventHandler<SceneLoadEventArgs>` | SceneLoad | Fired right after a scene is succesfully imported, loaded or cleared.  Runs immediately after all `KKAPI.Studio.SaveLoad.SceneCustomFunctionController` objects trigger their events. | 
| `EventHandler` | SceneSave | Fired right before a scene is saved to file.  Runs immediately after all `KKAPI.Studio.SaveLoad.SceneCustomFunctionController` objects trigger their events. | 


