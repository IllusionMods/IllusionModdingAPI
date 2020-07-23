## `BaseCurrentStateEditableGuiEntry<T>`

Base class of controls that hold a value.  Subscribe to `KKAPI.Studio.UI.BaseCurrentStateEditableGuiEntry`1.Value` to update your control's state whenever the value changes.
```csharp
public abstract class KKAPI.Studio.UI.BaseCurrentStateEditableGuiEntry<T>
    : CurrentStateCategorySubItemBase

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `BehaviorSubject<T>` | Value | Current value of this control | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | OnUpdateInfo(`OCIChar` ociChar) |  | 


## `CurrentStateCategory`

Category under the Anim &gt; CustomState tab
```csharp
public class KKAPI.Studio.UI.CurrentStateCategory

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | CategoryName | Name of the category. Controls are drawn under it. | 
| `Boolean` | Created | The category was created and still exists. | 
| `IEnumerable<CurrentStateCategorySubItemBase>` | SubItems | All custom controls under this category. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `T` | AddControl(`T` control) | Add new control to this category | 
| `void` | AddControls(`CurrentStateCategorySubItemBase[]` controls) | Add new controls to this category | 
| `void` | CreateCategory(`GameObject` containerObject) | Used by the API to actually create the custom control object | 
| `void` | UpdateInfo(`OCIChar` ociChar) | Fired when currently selected character changes and the controls need to be updated | 


## `CurrentStateCategoryDropdown`

Custom control that draws a dropdown menu in the Chara &gt; CurrentState studio menu.
```csharp
public class KKAPI.Studio.UI.CurrentStateCategoryDropdown
    : BaseCurrentStateEditableGuiEntry<Int32>

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | CreateItem(`GameObject` categoryObject) |  | 


## `CurrentStateCategorySlider`

Custom control that draws a slider in the Chara &gt; CurrentState studio menu.
```csharp
public class KKAPI.Studio.UI.CurrentStateCategorySlider
    : BaseCurrentStateEditableGuiEntry<Single>

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | MaxValue | Maximum value of the slider | 
| `Single` | MinValue | Minimum value of the slider | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | CreateItem(`GameObject` categoryObject) |  | 


## `CurrentStateCategorySubItemBase`

Base of custom controls created under CurrentState category
```csharp
public abstract class KKAPI.Studio.UI.CurrentStateCategorySubItemBase

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | Created | The control was created and still exists. | 
| `String` | Name | Name of the setting, displayed to the left | 
| `GameObject` | RootGameObject | The control's root gameobject. null if the control was not created yet. | 
| `BehaviorSubject<Boolean>` | Visible | The control is visible to the user (usually the same as it's GameObject being active). | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | CreateItem(`GameObject` categoryObject) | Fired when API wants to create the control. Should return the control's root GameObject | 
| `void` | OnUpdateInfo(`OCIChar` ociChar) | Fired when currently selected character changes and the control need to be updated | 


## `CurrentStateCategorySwitch`

Custom control that draws a single, circular button with an on/off state in the Chara &gt; CurrentState studio menu.
```csharp
public class KKAPI.Studio.UI.CurrentStateCategorySwitch
    : BaseCurrentStateEditableGuiEntry<Boolean>

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | CreateItem(`GameObject` categoryObject) |  | 


## `CurrentStateCategoryToggle`

Custom control that draws from 2 to 4 radio buttons (they are drawn like toggles)
```csharp
public class KKAPI.Studio.UI.CurrentStateCategoryToggle
    : BaseCurrentStateEditableGuiEntry<Int32>

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | ToggleCount | Number of the radio buttons, can be 2, 3 or 4 | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | CreateItem(`GameObject` categoryObject) |  | 


## `CustomToolbarButtons`

Add custom buttons to studio toolbars
```csharp
public static class KKAPI.Studio.UI.CustomToolbarButtons

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `ToolbarButton` | AddLeftToolbarButton(`Texture2D` iconTex, `Action` onClicked = null) | Add a simple button to the top of the left studio toolbar. | 
| `ToolbarToggle` | AddLeftToolbarToggle(`Texture2D` iconTex, `Boolean` initialValue = False, `Action<Boolean>` onValueChanged = null) | Add a toggle button to the top of the left studio toolbar.  Clicking on the button will toggle it between being on and off. | 


## `SceneEffectsCategory`

Class that adds a new subcategory to the Scene Effects menu. Create a new instance and then add SliderSets and ToggleSets.
```csharp
public class KKAPI.Studio.UI.SceneEffectsCategory

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `List<SceneEffectsSliderSet>` | Sliders | Sliders that have been added. | 
| `List<SceneEffectsToggleSet>` | Toggles | Toggles that have been added. | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | Content | Element that contains the content of the category. | 
| `GameObject` | Header | Element that contains the header of the category. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `SceneEffectsSliderSet` | AddSliderSet(`String` text, `Action<Single>` setter, `Single` initialValue, `Single` sliderMinimum, `Single` sliderMaximum) | Add a slider with text box to this Screen Effects subcategory. | 
| `SceneEffectsToggleSet` | AddToggleSet(`String` text, `Action<Boolean>` setter, `Boolean` initialValue) | Add a toggle to this Screen Effects subcategory. | 


## `SceneEffectsSliderSet`

A container for the value of a slider, associated label, slider, textbox, and reset button UI elements, and the setter method that triggers on value change.
```csharp
public class KKAPI.Studio.UI.SceneEffectsSliderSet

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Button` | Button | Reset button UI element. | 
| `Boolean` | EnforceSliderMaximum | Whether to enforce the SliderMaximum value. If false, users can type values in to the textbox that exceed the maximum value. | 
| `Boolean` | EnforceSliderMinimum | Whether to enforce the SliderMinimum value. If false, users can type values in to the textbox that exceed the minimum value. | 
| `Single` | InitialValue | Initial state of the toggle. | 
| `InputField` | Input | Input field UI element. | 
| `TextMeshProUGUI` | Label | Label UI element. | 
| `Action<Single>` | Setter | Method called when the value of the toggle is changed. | 
| `Slider` | Slider | Slider UI element. | 
| `Single` | SliderMaximum | Maximum value the slider can slide. Can be overriden by the user typing in the textbox if EnforceSliderMaximum is set to false. | 
| `Single` | SliderMinimum | Minimum value the slider can slide. Can be overriden by the user typing in the textbox if EnforceSliderMinimum is set to false. | 
| `String` | Text | Get or set the text of the label. | 
| `Single` | Value | Get or set the value of the toggle. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | GetValue() | Get the value of the slider set. | 
| `void` | Reset() | Reset the slider set to the initial value, update UI elements, and trigger the Setter method. | 
| `void` | Reset(`Boolean` triggerEvents) | Reset the slider set to the initial value, update UI elements, and trigger the Setter method. | 
| `void` | SetValue(`String` value) | Set the value of the slider set, update the UI elements, and trigger the Setter method. | 
| `void` | SetValue(`String` value, `Boolean` triggerEvents) | Set the value of the slider set, update the UI elements, and trigger the Setter method. | 
| `void` | SetValue(`Single` value) | Set the value of the slider set, update the UI elements, and trigger the Setter method. | 
| `void` | SetValue(`Single` value, `Boolean` triggerEvents) | Set the value of the slider set, update the UI elements, and trigger the Setter method. | 


## `SceneEffectsToggleSet`

A container for the value of a toggle, associated label and toggle UI elements, and the setter method that triggers on value change.
```csharp
public class KKAPI.Studio.UI.SceneEffectsToggleSet

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | InitialValue | Initial state of the toggle. | 
| `TextMeshProUGUI` | Label | Label UI element. | 
| `Action<Boolean>` | Setter | Method called when the value of the toggle is changed. | 
| `String` | Text | Get or set the text of the label. | 
| `Toggle` | Toggle | Toggle UI element. | 
| `Boolean` | Value | Get or set the value of the toggle. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | GetValue() | Get the value of the toggle. | 
| `void` | Reset() | Reset the toggle to the initial value and trigger the Setter method. | 
| `void` | Reset(`Boolean` triggerEvents) | Reset the toggle to the initial value and trigger the Setter method. | 
| `void` | SetValue(`Boolean` value) | Set the value of the toggle and trigger the Setter method. | 
| `void` | SetValue(`Boolean` value, `Boolean` triggerEvents) | Set the value of the toggle and trigger the Setter method. | 


## `ToolbarButton`

Custom toolbar button. Add using `KKAPI.Studio.UI.CustomToolbarButtons.AddLeftToolbarButton(UnityEngine.Texture2D,System.Action)`.
```csharp
public class KKAPI.Studio.UI.ToolbarButton
    : BaseGuiEntry, IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `IObservable<Unit>` | Clicked | Triggered when the button is clicked. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `ToolbarToggle`

Custom toolbar toggle button. Add using `KKAPI.Studio.UI.CustomToolbarButtons.AddLeftToolbarToggle(UnityEngine.Texture2D,System.Boolean,System.Action{System.Boolean})`.
```csharp
public class KKAPI.Studio.UI.ToolbarToggle
    : BaseEditableGuiEntry<Boolean>, IDisposable

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


