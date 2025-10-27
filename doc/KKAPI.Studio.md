## `SceneLocalTextures`

API for global toggling of locally saved textures in Studio.  The module is only activated if Activate is called, SaveType is read / set, or if an action is registered to SaveTypeChangedEvent.
```csharp
public static class KKAPI.Studio.SceneLocalTextures

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler<SceneTextureSaveTypeChangedEventArgs>` | SaveTypeChangedEvent | Fired whenever SaveType changes | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `SceneTextureSaveType` | SaveType | The type of texture saving that plugins should use | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | Activate() | Activates the LocalTextures API | 


## `SceneTextureSaveType`

Options for the type of texture saving that plugins should use in Studio.
```csharp
public enum KKAPI.Studio.SceneTextureSaveType
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Bundled | Textures should be bundled with the scene. | 
| `1` | Deduped | Textures should be deduped between different Chara and SceneControllers. | 
| `2` | Local | Textures should be saved separately from the scene in a local folder. | 


## `SceneTextureSaveTypeChangedEventArgs`

Event argument used for when texture save type for scenes is changed
```csharp
public class KKAPI.Studio.SceneTextureSaveTypeChangedEventArgs
    : EventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `SceneTextureSaveType` | NewSetting | The new state of the setting | 


## `StudioAPI`

Provides a way to add custom menu items to CharaStudio, and gives useful methods for interfacing with the studio.
```csharp
public static class KKAPI.Studio.StudioAPI

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | InsideStudio | True if we are currently inside CharaStudio.exe | 
| `Boolean` | StudioLoaded | True inside studio after it finishes loading the interface (when the starting loading screen finishes),  right before custom controls are created. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `CurrentStateCategory` | GetOrCreateCurrentStateCategory(`String` name) | Add a new custom category to the Anim &gt; CurrentState tab in the studio top-left menu.  Can use this at any point. Always returns null outside of studio.  If the name is empty or null, the Misc/Other category is returned. | 
| `IEnumerable<OCIChar>` | GetSelectedCharacters() | Get all character objects currently selected in Studio's Workspace. | 
| `IEnumerable<T>` | GetSelectedControllers() | Get all instances of this controller that belong to characters that are selected in Studio's Workspace. | 
| `IEnumerable<ObjectCtrlInfo>` | GetSelectedObjects() | Get all objects (all types) currently selected in Studio's Workspace. | 


Static Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler` | StudioLoadedChanged | Fires once after studio finished loading the interface, right before custom controls are created. | 


## `StudioObjectExtensions`

Useful extensions for studio metaobjects
```csharp
public static class KKAPI.Studio.StudioObjectExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `ChaControl` | GetChaControl(this `OCIChar` ociChar) | Get character component for this studio object | 
| `ObjectCtrlInfo` | GetObjectCtrlInfo(this `ObjectInfo` obj) | Get the ObjectCtrlInfo object that uses this ObjectInfo.  If the object was not found in current scene, null is returned. | 
| `OCIChar` | GetOCIChar(this `ChaControl` chaControl) | Get GetOCIChar that is assigned to this character. Only works in CharaStudio, returns null elsewhere. | 
| `Int32` | GetSceneId(this `ObjectCtrlInfo` obj) | Get the ID of this object as used in the currently loaded scene.  If the object was not found in current scene, -1 is returned. | 
| `Int32` | GetSceneId(this `ObjectInfo` obj) | Get the ID of this object as used in the currently loaded scene.  If the object was not found in current scene, -1 is returned. | 
| `Boolean` | TryGetObjectCtrlInfo(this `TreeNodeObject` tno, `ObjectCtrlInfo&` objectCtrlInfo) | Try to get the ObjectCtrlInfo controlled by this TreeNodeObject. | 


