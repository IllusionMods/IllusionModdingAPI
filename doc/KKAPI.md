## `GameMode`

Current gameplay mode the game is in
```csharp
public enum KKAPI.GameMode
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Unknown | Anywhere else, including main menu | 
| `1` | Maker | Inside character maker (can be started from main menu or from class roster) | 
| `2` | Studio | Anywhere inside CharaStudio.exe | 
| `3` | MainGame | Anywhere inside the main game.  Includes everything after starting a new game from title screen and after loading a saved game.  This means this includes story scenes, night menu, roaming around and h scenes inside story mode.  This does not hoverwer include the character maker launched from within the class menu. | 


## `KoikatuAPI`

Provides overall information about the game and the API itself, and provides some useful tools.  More information is available in project wiki at https://github.com/ManlyMarco/KKAPI/wiki
```csharp
public class KKAPI.KoikatuAPI
    : BaseUnityPlugin

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | GUID | GUID of this plugin, use for checking dependancies with `BepInEx.BepInDependency` and `KKAPI.KoikatuAPI.CheckRequiredPlugin(BepInEx.BaseUnityPlugin,System.String,System.Version,BepInEx.Logging.LogLevel)` | 
| `String` | VersionConst | Version of this assembly/plugin.  WARNING: This is a const field, therefore it will be copied to your assembly!  Use this field to check if the installed version of the plugin is up to date by adding this attribute to your plugin class:  <code>[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]</code>  THIS VALUE WILL NOT BE READ FROM THE INSTALLED VERSION, YOU WILL READ THE VALUE FROM THIS VERSION THAT YOU COMPILE YOUR PLUGIN AGAINST!  More info: https://stackoverflow.com/questions/55984/what-is-the-difference-between-const-and-readonly | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | EnableDebugLogging | Enables display of additional log messages when certain events are triggered within KKAPI.  Useful for plugin devs to understand when controller messages are fired. | 
| `KoikatuAPI` | Instance |  | 
| `ManualLogSource` | Logger |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameMode` | GetCurrentGameMode() | Get current game mode. | 
| `Version` | GetGameVersion() | Get current version of the game. | 


