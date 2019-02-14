## `CurrentStateCategory`

Category under the Anim &gt; CustomState tab
```csharp
public class KKAPI.Studio.UI.CurrentStateCategory

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | CategoryName | Name of the category. Controls are drawn under it. | 
| `IEnumerable<CurrentStateCategorySubItemBase>` | SubItems | All custom controls under this category. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateCategory(`GameObject` containerObject) | Used by the API to actually create the custom control object | 
| `void` | UpdateInfo(`OCIChar` ociChar) | Fired when currently selected character changes and the controls need to be updated | 


## `CurrentStateCategorySubItemBase`

Base of custom controls created under CurrentState category
```csharp
public abstract class KKAPI.Studio.UI.CurrentStateCategorySubItemBase

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Name | Name of the setting, displayed to the left | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateItem(`GameObject` categoryObject) | Fired when API wants to create the control | 
| `void` | OnUpdateInfo(`OCIChar` ociChar) | Fired when currently selected character changes and the control need to be updated | 


## `CurrentStateCategoryToggle`

Custom control that draws from 2 to 4 radio buttons (they are drawn like toggles)
```csharp
public class KKAPI.Studio.UI.CurrentStateCategoryToggle
    : CurrentStateCategorySubItemBase

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `BehaviorSubject<Int32>` | SelectedIndex | Currently selected button (starts at 0) | 
| `Int32` | ToggleCount | Number of the radio buttons, can be 2, 3 or 4 | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateItem(`GameObject` categoryObject) |  | 
| `void` | OnUpdateInfo(`OCIChar` ociChar) |  | 


