## `StudioAPI`

Provides a way to add custom menu items to CharaStudio, and gives useful methods for interfacing with the studio.
```csharp
public static class KKAPI.Studio.StudioAPI

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | InsideStudio | True if we are currently inside CharaStudio.exe | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | CreateCurrentStateCategory(`CurrentStateCategory` category) | Add a new custom category to the Anim &gt; CurrentState tab in the studio top-left menu.  Can use this at any point. | 
| `void` | Init(`Boolean` insideStudio) |  | 


