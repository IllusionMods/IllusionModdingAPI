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
| `String` | GameProcessName | The game process name for use with `BepInEx.BepInProcess` attributes.  This is for the jp release. In almost all cases should be used together with the steam version. | 
| `String` | GameProcessNameSteam | The game process name for use with `BepInEx.BepInProcess` attributes.  This is for the steam release. In almost all cases should be used together with the jp version. | 
| `String` | GUID | GUID of this plugin, use for checking dependancies with `BepInEx.BepInDependency`."/&gt; | 
| `String` | StudioProcessName | The studio process name for use with `BepInEx.BepInProcess` attributes. | 
| `String` | VersionConst | Version of this assembly/plugin.  WARNING: This is a const field, therefore it will be copied to your assembly!  Use this field to check if the installed version of the plugin is up to date by adding this attribute to your plugin class:  <code>[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]</code>  THIS VALUE WILL NOT BE READ FROM THE INSTALLED VERSION, YOU WILL READ THE VALUE FROM THIS VERSION THAT YOU COMPILE YOUR PLUGIN AGAINST!  More info: https://stackoverflow.com/questions/55984/what-is-the-difference-between-const-and-readonly | 
| `String` | VRProcessName | The VR module process name for use with `BepInEx.BepInProcess` attributes.  This is for the jp release. In almost all cases should be used together with the steam version. | 
| `String` | VRProcessNameSteam | The VR module process name for use with `BepInEx.BepInProcess` attributes.  This is for the steam release. In almost all cases should be used together with the jp version. | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | EnableDebugLogging | Enables display of additional log messages when certain events are triggered within KKAPI.  Useful for plugin devs to understand when controller messages are fired. | 
| `Boolean` | IsQuitting | Can be used to detect if application is currently quitting. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `GameMode` | GetCurrentGameMode() | Get current game mode. | 
| `Version` | GetGameVersion() | Get current version of the game. | 
| `Boolean` | IsDarkness() | Check if the game is running the Darkness version  <remarks>It's best to not rely on this and instead make the same code works either way (if possible).</remarks> | 
| `Boolean` | IsSteamRelease() | Check if the game is the Steam release instead of the original Japanese release.  <remarks>It's best to not rely on this and instead make the same code work in both versions (if possible).</remarks> | 
| `Boolean` | IsVR() | Check if this is the official VR module. Main game VR mods are ignored (returns false). | 


Static Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler` | Quitting | Occurs when application is quitting.  Plugins can use this to do things like write config files and caches, or stop outstanding coroutines to prevent shutdown delays.  Note: This event might not fire if the game isn't closed cleanly (hard crashes, killed process, closing the console window, etc.). | 


## `SceneApi`

Game-agnostic version of Manager.Scene. It allows using the same code in all games without any #if directives.
```csharp
public static class KKAPI.SceneApi

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | GetAddSceneName() | Get name of the currently loaded overlay scene (eg. exit game box, config, confirmation dialogs). | 
| `Boolean` | GetIsFadeNow() | True if screen is currently fading in or out. | 
| `Boolean` | GetIsNowLoading() | True if loading screen is being displayed. | 
| `Boolean` | GetIsNowLoadingFade() | True if loading screen is being displayed, or if screen is currently fading in or out. | 
| `Boolean` | GetIsOverlap() | True if a dialog box or some other overlapping menu is shown (e.g. exit dialog after pressing esc). | 
| `String` | GetLoadSceneName() | Get name of the currently loaded game scene (eg. maker, h, adv). | 


