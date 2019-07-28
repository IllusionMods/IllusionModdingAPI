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
| `void` | Init(`Boolean` insideStudio) |  | 


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
| `ObjectCtrlInfo` | GetObjectCtrlInfo(this `ObjectInfo` obj) | Get the ObjectCtrlInfo object that uses this ObjectInfo.  If the object was not found in current scene, null is returned. | 
| `OCIChar` | GetOCIChar(this `ChaControl` chaControl) | Get GetOCIChar that is assigned to this character. Only works in CharaStudio, returns null elsewhere. | 
| `Int32` | GetSceneId(this `ObjectCtrlInfo` obj) | Get the ID of this object as used in the currently loaded scene.  If the object was not found in current scene, -1 is returned. | 
| `Int32` | GetSceneId(this `ObjectInfo` obj) | Get the ID of this object as used in the currently loaded scene.  If the object was not found in current scene, -1 is returned. | 


