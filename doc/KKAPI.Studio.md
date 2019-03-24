## `StudioAPI`

Provides a way to add custom menu items to CharaStudio, and gives useful methods for interfacing with the studio.
```csharp
public static class KKAPI.Studio.StudioAPI

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | InsideStudio | True if we are currently inside CharaStudio.exe | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateCurrentStateCategory(`CurrentStateCategory` category) | Add a new custom category to the Anim &gt; CurrentState tab in the studio top-left menu.  Can use this at any point. | 
| `void` | Init(`Boolean` insideStudio) |  | 


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


