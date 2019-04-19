## `BaseEditableGuiEntry<TValue>`

Base of custom controls that have a value that can be changed and watched for changes.
```csharp
public abstract class KKAPI.Maker.UI.BaseEditableGuiEntry<TValue>
    : BaseGuiEntry, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `IObservable<TValue>` | BufferedValueChanged | Use to get value changes for controls. Fired by external value set and by SetNewValue. | 
| `TValue` | Value | Buttons 1, 2, 3 are values 0, 1, 2 | 
| `IObservable<TValue>` | ValueChanged | Fired every time the value is changed, and once when the control is created.  Buttons 1, 2, 3 are values 0, 1, 2 | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateControl(`Transform` subCategoryList) |  | 
| `void` | Dispose() |  | 
| `void` | SetNewValue(`TValue` newValue) | Trigger value changed events and set the value | 


## `BaseGuiEntry`

Base of all custom character maker controls.
```csharp
public abstract class KKAPI.Maker.UI.BaseGuiEntry
    : IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `MakerCategory` | Category | Category and subcategory that this control is inside of. | 
| `GameObject` | ControlObject | GameObject of the control. Populated once instantiated.  If there are multiple objects, returns one of them. Use `KKAPI.Maker.UI.BaseGuiEntry.ControlObjects` in that case. | 
| `IEnumerable<GameObject>` | ControlObjects | GameObject(s) of the control. Populated once instantiated.  Contains 1 item in most cases, can contain multiple in case of accessory window controls. | 
| `Boolean` | Exists | True if the control is currently instantiated in the scene | 
| `Boolean` | IsDisposed | If true, the control has been disposed and can no longer be used, likely because the character maker exited.  A new control has to be created to be used again. | 
| `BaseUnityPlugin` | Owner | The plugin that owns this custom control. | 
| `Color` | TextColor | Text color of the control's description text (usually on the left).  Can only set this before the control is created. | 
| `BehaviorSubject<Boolean>` | Visible | The control is visible to the user (usually the same as it's GameObject being active). | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateControl(`Transform` subCategoryList) |  | 
| `void` | Dispose() | Remove the control. Called when maker is quitting. | 
| `void` | Initialize() | Called before OnCreateControl to setup the object before instantiating the control. | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) | Used by the API to actually create the custom control.  Should return main GameObject of the control | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | GuiApiNameAppendix | Added to the end of most custom controls to mark them as being created by this API. | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Transform` | GuiCacheTransfrom | Parent transform that holds temporary gui entries used to instantiate custom controls. | 


## `MakerButton`

Custom control that draws a simple blue button.
```csharp
public class KKAPI.Maker.UI.MakerButton
    : BaseGuiEntry, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ButtonClickedEvent` | OnClick | Fired when user clicks on the button | 
| `String` | Text | Text displayed on the button | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Dispose() |  | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerColor`

Control that allows user to change a `UnityEngine.Color` in a separate color selector window
```csharp
public class KKAPI.Maker.UI.MakerColor
    : BaseEditableGuiEntry<Color>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | ColorBoxWidth | Width of the color box. Can adjust this to allow for longer label text.  Default width is 276 and might need to get lowered to allow longer labels.  The default color boxes in accessory window are 230 wide. | 
| `String` | SettingName | Name of the setting | 
| `Boolean` | UseAlpha | If true, the color selector will allow the user to change alpha of the color.  If false, no color slider is shown and alpha is always 1f. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerCoordinateLoadToggle`

Adds a toggle to the bottom of the coordinate/clothes card load window in character maker.  Use to allow user to not load data related to your mod.  Use with `KKAPI.Maker.UI.MakerCoordinateLoadToggle.AddLoadToggle(KKAPI.Maker.UI.MakerCoordinateLoadToggle)`
```csharp
public class KKAPI.Maker.UI.MakerCoordinateLoadToggle
    : BaseEditableGuiEntry<Boolean>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Text | Text displayed next to the toggle | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` loadBoxTransform) |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | AnyEnabled | Check if any of the custom toggles are checked | 
| `Button` | LoadButton |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `MakerCoordinateLoadToggle` | AddLoadToggle(`MakerCoordinateLoadToggle` toggle) |  | 
| `void` | CreateCustomToggles() |  | 
| `void` | Reset() |  | 
| `void` | Setup() |  | 


## `MakerDropdown`

Custom control that draws a dropdown list
```csharp
public class KKAPI.Maker.UI.MakerDropdown
    : BaseEditableGuiEntry<Int32>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String[]` | Options | List of all options in the dropdown | 
| `String` | SettingName | Name displayed next to the dropdown | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerImage`

Custom control that displays a texture in a small preview thumbnail
```csharp
public class KKAPI.Maker.UI.MakerImage
    : BaseGuiEntry, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | Height | Height of the texture preview | 
| `Texture` | Texture | Texture to display in the preview | 
| `Int32` | Width | Width of the texture preview | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerLoadToggle`

Adds a toggle to the bottom of the character card load window in character maker.  Use to allow user to not load data related to your mod.  Use with `KKAPI.Maker.UI.MakerLoadToggle.AddLoadToggle(KKAPI.Maker.UI.MakerLoadToggle)`
```csharp
public class KKAPI.Maker.UI.MakerLoadToggle
    : BaseEditableGuiEntry<Boolean>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Text | Text displayed next to the toggle | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` loadBoxTransform) |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | AnyEnabled | Check if any of the custom toggles are checked | 
| `Button` | LoadButton |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `MakerLoadToggle` | AddLoadToggle(`MakerLoadToggle` toggle) |  | 
| `void` | CreateCustomToggles() |  | 
| `void` | Reset() |  | 
| `void` | Setup() |  | 


## `MakerRadioButtons`

Custom control that displays multiple radio buttons
```csharp
public class KKAPI.Maker.UI.MakerRadioButtons
    : BaseEditableGuiEntry<Int32>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ReadOnlyCollection<Toggle>` | Buttons | Objects of all of the radio buttons | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerSeparator`

Custom control that draws a simple horizontal separator
```csharp
public class KKAPI.Maker.UI.MakerSeparator
    : BaseGuiEntry, IDisposable

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerSlider`

Custom control that draws a slider and a text box (both are used to edit the same value)
```csharp
public class KKAPI.Maker.UI.MakerSlider
    : BaseEditableGuiEntry<Single>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Func<String, Single>` | StringToValue | Custom converter from text in the textbox to the slider value.  If not set, <code>float.Parse(txt) / 100f</code> is used. | 
| `Func<Single, String>` | ValueToString | Custom converter from the slider value to what's displayed in the textbox.  If not set, <code>Mathf.RoundToInt(f * 100).ToString()</code> is used. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerText`

Custom control that displays a simple text
```csharp
public class KKAPI.Maker.UI.MakerText
    : BaseGuiEntry, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Text | Displayed text | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerToggle`

Custom control that displays a toggle
```csharp
public class KKAPI.Maker.UI.MakerToggle
    : BaseEditableGuiEntry<Boolean>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | DisplayName | Text shown next to the checkbox | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


