## `GameAPI`

```csharp
public static class KKAPI.MainGame.GameAPI

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Init(`Boolean` insideStudio) |  | 
| `void` | RegisterExtraBehaviour(`String` extendedDataId) |  | 


Static Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler` | EndH |  | 
| `EventHandler<GameSaveLoadEventArgs>` | GameLoad |  | 
| `EventHandler<GameSaveLoadEventArgs>` | GameSave |  | 
| `EventHandler` | StartH |  | 


## `GameCustomFunctionController`

```csharp
public abstract class KKAPI.MainGame.GameCustomFunctionController
    : MonoBehaviour

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | OnDayChange(`Week` day) |  | 
| `void` | OnEndH(`HSceneProc` proc, `Boolean` freeH) |  | 
| `void` | OnEnterNightMenu() |  | 
| `void` | OnGameLoad(`GameSaveLoadEventArgs` args) |  | 
| `void` | OnGameSave(`GameSaveLoadEventArgs` args) |  | 
| `void` | OnPeriodChange(`Type` period) |  | 
| `void` | OnStartH(`HSceneProc` proc, `Boolean` freeH) |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Cycle` | GetCycle() |  | 


## `GameExtensions`

```csharp
public static class KKAPI.MainGame.GameExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Heroine` | GetHeroine(this `ChaControl` chaControl) |  | 
| `Heroine` | GetHeroine(this `ChaFileControl` chaFile) |  | 
| `Boolean` | IsShowerPeeping(this `HFlag` hFlag) |  | 


## `GameSaveLoadEventArgs`

```csharp
public class KKAPI.MainGame.GameSaveLoadEventArgs
    : EventArgs

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | FileName |  | 
| `String` | FullFilename |  | 
| `String` | Path |  | 


