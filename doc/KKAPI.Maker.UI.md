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
| `GameObject` | ControlObject | GameObject of the control. Populated once instantiated | 
| `Boolean` | Exists | True if the control is currently instantiated in the scene | 
| `BaseUnityPlugin` | Owner | The plugin that owns this custom control. | 
| `Color` | TextColor | Text color of the control's description text (usually on the left).  Can only set this before the control is created. | 
| `BehaviorSubject<Boolean>` | Visible | The control is visible to the user (usually the same as it's GameObject being active). | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateControl(`Transform` subCategoryList) |  | 
| `void` | Dispose() | Remove the control. Called when maker is quitting. | 
| `void` | Initialize() | Called before OnCreateControl to setup the object before instantiating the control. | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) | Should return main GameObject of the control | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | GuiApiNameAppendix | Added to the end of most custom controls to mark them as being created by this API. | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Transform` | GuiCacheTransfrom | Parent transform that holds temporary gui entries used to instantiate custom controls. | 


## `MakerButton`

```csharp
public class KKAPI.Maker.UI.MakerButton
    : BaseGuiEntry, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ButtonClickedEvent` | OnClick |  | 
| `String` | Text |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Dispose() |  | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerColor`

```csharp
public class KKAPI.Maker.UI.MakerColor
    : BaseEditableGuiEntry<Color>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | SettingName |  | 
| `Boolean` | UseAlpha |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Dispose() |  | 
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
| `String` | Text |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` loadBoxTransform) |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | AnyEnabled |  | 
| `Button` | LoadButton |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `MakerCoordinateLoadToggle` | AddLoadToggle(`MakerCoordinateLoadToggle` toggle) |  | 
| `void` | CreateCustomToggles() |  | 
| `void` | Reset() |  | 
| `void` | Setup() |  | 


## `MakerDropdown`

```csharp
public class KKAPI.Maker.UI.MakerDropdown
    : BaseEditableGuiEntry<Int32>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String[]` | Options |  | 
| `String` | SettingName |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerImage`

```csharp
public class KKAPI.Maker.UI.MakerImage
    : BaseGuiEntry, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | Height |  | 
| `Texture` | Texture |  | 
| `Int32` | Width |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Dispose() |  | 
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
| `String` | Text |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` loadBoxTransform) |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | AnyEnabled |  | 
| `Button` | LoadButton |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `MakerLoadToggle` | AddLoadToggle(`MakerLoadToggle` toggle) |  | 
| `void` | CreateCustomToggles() |  | 
| `void` | Reset() |  | 
| `void` | Setup() |  | 


## `MakerRadioButtons`

```csharp
public class KKAPI.Maker.UI.MakerRadioButtons
    : BaseEditableGuiEntry<Int32>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ReadOnlyCollection<Toggle>` | Buttons |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerSeparator`

```csharp
public class KKAPI.Maker.UI.MakerSeparator
    : BaseGuiEntry, IDisposable

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Dispose() |  | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerSlider`

```csharp
public class KKAPI.Maker.UI.MakerSlider
    : BaseEditableGuiEntry<Single>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Func<String, Single>` | StringToValue |  | 
| `Func<Single, String>` | ValueToString |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerText`

```csharp
public class KKAPI.Maker.UI.MakerText
    : BaseGuiEntry, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Text |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Dispose() |  | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `MakerToggle`

```csharp
public class KKAPI.Maker.UI.MakerToggle
    : BaseEditableGuiEntry<Boolean>, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | DisplayName |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Transform` | ToggleCopy |  | 


