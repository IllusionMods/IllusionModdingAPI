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
    : SimpleToolbarButton, IDisposable

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
| `Button` | ButtonObject | The Unity UI Button object for this toolbar button. Null until the button is created in the UI. | 
| `Nullable<ToolbarPosition>` | DesiredPosition | Which row and column to place the button in.  For the bottom left toolbar, this is counted from bottom left corner of the screen.  If null, the button position will be chosen automatically. | 
| `String` | HoverText | Text to display when hovering over the button. | 
| `Texture2D` | IconTex | The icon texture for the button. Must be 32x32. | 
| `Boolean` | IsDisposed | True if the button has been removed and needs to be recreated to be used again. | 
| `BaseUnityPlugin` | Owner | The plugin that owns this button. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateControl() | Create the actual button control in the studio UI. | 
| `void` | Dispose() | Destroys the button and cleans up resources. | 
| `void` | ResetDesiredPosition() | Clear the desired position and let the button be positioned automatically. | 
| `void` | SetDesiredPosition(`Int32` row, `Int32` column) | Set the desired position of the button in the toolbar.  If another button has already requested this position, this button may be moved to the right. | 
| `void` | SetDesiredPosition(`ToolbarPosition` position) | Set the desired position of the button in the toolbar.  If another button has already requested this position, this button may be moved to the right. | 


## `ToolbarManager`

Add custom buttons to studio toolbars. Thread-safe.  You can find a button template in "\doc\studio icon template.png"
```csharp
public static class KKAPI.Studio.UI.Toolbars.ToolbarManager

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | AddLeftToolbarControl(`ToolbarControlBase` button) | Adds a custom toolbar toggle button to the left toolbar. | 
| `ToolbarControlBase[]` | GetAllButtons(`Boolean` includeInvisible) | Get an array of all toolbar buttons added so far. | 
| `void` | RemoveControl(`ToolbarControlBase` toolbarControlBase) | Removes the button from the toolbar and destroys it. The button must be recreated to be used again.  If you want to temporarily hide a button, set its Visible property to false instead. | 
| `void` | RequestToolbarRelayout() | Queues an update of the toolbar interface layout, which will be done on the next frame if necessary.  Shouldn't need to be called manually unless button positions are changed externally. | 


## `ToolbarPosition`

Represents the position of a toolbar in a grid layout, defined by its row and column indices.
```csharp
public struct KKAPI.Studio.UI.Toolbars.ToolbarPosition
    : IEquatable<ToolbarPosition>

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | Column | Horizontal position (column index). | 
| `Int32` | Row | Vertical position (row index). | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | Equals(`ToolbarPosition` other) |  | 
| `Boolean` | Equals(`Object` obj) |  | 
| `Int32` | GetHashCode() |  | 


