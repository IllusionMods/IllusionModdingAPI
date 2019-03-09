## `ISidebarControl`

Marks the control as being intended for use on Control Panel sidebar in chara maker
```csharp
public interface KKAPI.Maker.UI.Sidebar.ISidebarControl

```

## `SidebarSeparator`

A separator to be used in the right "Control Panel" sidebar in character maker.  The space is limited so use sparingly.
```csharp
public class KKAPI.Maker.UI.Sidebar.SidebarSeparator
    : BaseGuiEntry, IDisposable, ISidebarControl

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Dispose() |  | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


## `SidebarToggle`

A toggle to be used in the right "Control Panel" sidebar in character maker.  The space is limited so use sparingly.
```csharp
public class KKAPI.Maker.UI.Sidebar.SidebarToggle
    : BaseEditableGuiEntry<Boolean>, IDisposable, ISidebarControl

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Text | Text displayed next to the checkbox | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Initialize() |  | 
| `GameObject` | OnCreateControl(`Transform` subCategoryList) |  | 


