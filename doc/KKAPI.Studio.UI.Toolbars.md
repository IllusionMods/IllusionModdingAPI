## `SimpleToolbarButton`

Simple toolbar button that triggers an action when clicked.
```csharp
public class KKAPI.Studio.UI.Toolbars.SimpleToolbarButton
    : ToolbarControlBase, IDisposable

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateControl() |  | 


## `SimpleToolbarToggle`

Toolbar button that acts as a toggle (on/off).
```csharp
public class KKAPI.Studio.UI.Toolbars.SimpleToolbarToggle
    : ToolbarControlBase, IDisposable

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateControl() |  | 
| `void` | Dispose() |  | 


## `ToolbarControlBase`

Base class for custom toolbar buttons in the studio UI.
```csharp
public abstract class KKAPI.Studio.UI.Toolbars.ToolbarControlBase
    : IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | ButtonID | Unique identifier for the button. | 
| `Int32` | DesiredColumn | Which column to place the button in.  For the bottom left toolbar, this is counted from bottom left corner of the screen.  Set to -1 to automatically add to the end of the toolbar. | 
| `Int32` | DesiredRow | Which row to place the button in.  For the bottom left toolbar, this is counted from bottom left corner of the screen.  Set to -1 to automatically add to the end of the toolbar. | 
| `String` | HoverText | Text to display when hovering over the button. | 
| `Texture2D` | IconTex | The icon texture for the button. Must be 32x32. | 
| `Boolean` | IsDisposed | True if the button has been removed and needs to be recreated to be used again. | 
| `BaseUnityPlugin` | Owner | The plugin that owns this button. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateControl() | Create the actual button control in the studio UI. | 
| `void` | Dispose() | Destroys the button and cleans up resources. | 
| `void` | SetDesiredPosition(`Int32` row, `Int32` column) | Set the desired position of the button in the toolbar.  If another button has already requested this position, this button will be moved to the right. | 


## `ToolbarManager`

Add custom buttons to studio toolbars.  You can find a button template here https://github.com/IllusionMods/IllusionModdingAPI/blob/master/doc/studio%20icon%20template.png
```csharp
public static class KKAPI.Studio.UI.Toolbars.ToolbarManager

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | AddLeftToolbarControl(`ToolbarControlBase` button) | Adds a custom toolbar toggle button to the left toolbar. | 
| `void` | RequestToolbarRelayout() | Queues an update of the toolbar interface if necessary. | 


