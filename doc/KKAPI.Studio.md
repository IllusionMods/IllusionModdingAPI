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
| `TreeNodeObject[]` | GetSelectedTreeNodes() | Get all tree nodes currently selected in Studio's Workspace. Returns an empty array if called outside of studio or before studio finishes loading. | 


Static Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler` | StudioLoadedChanged | Fires once after studio finished loading the interface, right before custom controls are created. | 


## `StudioContextMenuOrder`

Defines the order in which menu items appear in right click context menus.  Menu items with the same StudioContextMenuOrder value are grouped together. Different groups are separated by a separator.  Any custom value between -100 and 100 can be used. Lower values are called first and appear higher in the menu.
```csharp
public enum KKAPI.Studio.StudioContextMenuOrder
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `-100` | Topmost | Always be at the top of the menu, above all other items. Use only if absolutely necessary. | 
| `-70` | SuggestedActions | Actions that are most likely to be used by the user, e.g. "Edit text..." on a text node. | 
| `-50` | Actions | Actions to perform on the node, e.g. "Set Vanilla+ shaders". | 
| `0` | Default | Default order for menu items, consider using a different value. Appears in the middle of the menu. | 
| `40` | Selection | Commands that change selection. | 
| `50` | NodeEdit | Basic node editing like deleting or duplicating. | 
| `80` | Properties | Opening various properties windows. | 
| `100` | Bottommost | Always be at the bottom of the menu, below all other items. Use only if absolutely necessary. | 


## `StudioContextMenus`

Provides functionality for adding custom context menu items to studio.
```csharp
public static class KKAPI.Studio.StudioContextMenus

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `IDisposable` | AddWorkspaceContextMenuItem(`String` content, `Action<WorkspaceNodeClickedEventArgs>` onClick, `StudioContextMenuOrder` order = Default, `Func<WorkspaceNodeClickedEventArgs, EntryState>` checkState = null) | Add a menu item to the workspace context menu. | 
| `IDisposable` | AddWorkspaceContextMenuItem(`GUIContent` content, `Action<WorkspaceNodeClickedEventArgs>` onClick, `StudioContextMenuOrder` order = Default, `Func<WorkspaceNodeClickedEventArgs, EntryState>` checkState = null) | Add a menu item to the workspace context menu. | 
| `IDisposable` | AddWorkspaceContextMenuItem(`Entry` content, `StudioContextMenuOrder` order) | Add a menu item to the workspace context menu. | 


## `StudioObjectExtensions`

Useful extensions for studio metaobjects
```csharp
public static class KKAPI.Studio.StudioObjectExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `IEnumerable<TreeNodeObject>` | Flatten(this `TreeNodeObject` treeNodeObject) | Recursively flatten this TreeNodeObject and all of its children into a single collection of TreeNodeObjects. | 
| `ChaControl` | GetChaControl(this `OCIChar` ociChar) | Get character component for this studio object | 
| `ObjectCtrlInfo` | GetObjectCtrlInfo(this `ObjectInfo` obj) | Get the ObjectCtrlInfo object that uses this ObjectInfo.  If the object was not found in current scene, null is returned. | 
| `OCIChar` | GetOCIChar(this `ChaControl` chaControl) | Get GetOCIChar that is assigned to this character. Only works in CharaStudio, returns null elsewhere. | 
| `Int32` | GetSceneId(this `ObjectCtrlInfo` obj) | Get the ID of this object as used in the currently loaded scene.  If the object was not found in current scene, -1 is returned. | 
| `Int32` | GetSceneId(this `ObjectInfo` obj) | Get the ID of this object as used in the currently loaded scene.  If the object was not found in current scene, -1 is returned. | 
| `Boolean` | TryGetObjectCtrlInfo(this `TreeNodeObject` tno, `ObjectCtrlInfo&` objectCtrlInfo) | Try to get the ObjectCtrlInfo controlled by this TreeNodeObject. Returns null if the object is not found or if called outside of studio. | 


## `WorkspaceNodeClickedEventArgs`

Event arguments for workspace tree node context menu events.
```csharp
public class KKAPI.Studio.WorkspaceNodeClickedEventArgs
    : EventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `TreeNodeObject` | ClickedInstance | The tree node that was right-clicked to open the context menu. | 
| `ICollection<TreeNodeObject>` | SelectedInstances | All tree nodes currently selected in the workspace. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `IEnumerable<ObjectCtrlInfo>` | GetSelectedObjects() | Get all objects currently selected in the workspace. | 


