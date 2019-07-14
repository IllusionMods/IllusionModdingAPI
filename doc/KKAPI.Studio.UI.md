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


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameObject` | CreateItem(`GameObject` categoryObject) | Fired when API wants to create the control. Should return the control's root GameObject | 
| `void` | CreateItemInt(`GameObject` categoryObject) |  | 
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


