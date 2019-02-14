## `CurrentStateCategory`

```csharp
public class KKAPI.Studio.UI.CurrentStateCategory

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | CategoryName |  | 
| `IEnumerable<CurrentStateCategorySubItemBase>` | SubItems |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateCategory(`GameObject` containerObject) |  | 
| `void` | UpdateInfo(`OCIChar` ociChar) |  | 


## `CurrentStateCategorySubItemBase`

```csharp
public abstract class KKAPI.Studio.UI.CurrentStateCategorySubItemBase

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Name |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateItem(`GameObject` categoryObject) |  | 
| `void` | OnUpdateInfo(`OCIChar` ociChar) |  | 


## `CurrentStateCategoryToggle`

```csharp
public class KKAPI.Studio.UI.CurrentStateCategoryToggle
    : CurrentStateCategorySubItemBase

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `BehaviorSubject<Int32>` | SelectedIndex |  | 
| `Int32` | ToggleCount |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateItem(`GameObject` categoryObject) |  | 
| `void` | OnUpdateInfo(`OCIChar` ociChar) |  | 


