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


## `Orientation`

PK_ORIENTATION - Represents the orientation data of a tablet input device.
```csharp
public struct KKAPI.Orientation

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `UInt32` | orAltitude |  | 
| `UInt32` | orAzimuth |  | 
| `UInt32` | orTwist |  | 


## `Packet`

Represents a data packet structure used for interacting with tablet hardware,  containing information about button states, positional coordinates, and  pressure sensitivity. This structure is primarily utilized for processing  input data from digitizing devices.
```csharp
public struct KKAPI.Packet

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `UInt32` | pkButtons |  | 
| `UInt32` | pkChanged |  | 
| `IntPtr` | pkContext |  | 
| `UInt32` | pkCursor |  | 
| `UInt32` | pkNormalPressure |  | 
| `Orientation` | pkOrientation |  | 
| `Rotation` | pkRotation |  | 
| `UInt32` | pkSerialNumber |  | 
| `UInt32` | pkStatus |  | 
| `UInt32` | pkTangentPressure |  | 
| `UInt32` | pkTime |  | 
| `Int32` | pkX |  | 
| `Int32` | pkY |  | 
| `Int32` | pkZ |  | 


## `Rotation`

PK_ROTATION - Represents the rotation data of a tablet input device.
```csharp
public struct KKAPI.Rotation

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `UInt32` | roPitch |  | 
| `UInt32` | roRoll |  | 
| `UInt32` | roYaw |  | 


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


## `UniversalRefObjKey`

ChaReference.RefObjKey alternative that's safe to use in all versions of KK.  Why: Everything below CORRECT_HAND_R changed its int value in darkness vs other versions of the game, so if you  compile with darkness dll you will unexpectedly get a different enum value in games without darkness.  This isn't necessary in KKS, but it does work in case your code targets both KK and KKS.  Only values present in all game versions are provided (the lowest common denominator being KK without Dankness).
```csharp
public static class KKAPI.UniversalRefObjKey

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `RefObjKey` | a_n_ana |  | 
| `RefObjKey` | a_n_ankle_L |  | 
| `RefObjKey` | a_n_ankle_R |  | 
| `RefObjKey` | a_n_arm_L |  | 
| `RefObjKey` | a_n_arm_R |  | 
| `RefObjKey` | a_n_back |  | 
| `RefObjKey` | a_n_back_L |  | 
| `RefObjKey` | a_n_back_R |  | 
| `RefObjKey` | a_n_bust |  | 
| `RefObjKey` | a_n_bust_f |  | 
| `RefObjKey` | a_n_dan |  | 
| `RefObjKey` | a_n_earrings_L |  | 
| `RefObjKey` | a_n_earrings_R |  | 
| `RefObjKey` | a_n_elbo_L |  | 
| `RefObjKey` | a_n_elbo_R |  | 
| `RefObjKey` | a_n_hair_pin |  | 
| `RefObjKey` | a_n_hair_pin_R |  | 
| `RefObjKey` | a_n_hair_pony |  | 
| `RefObjKey` | a_n_hair_twin_L |  | 
| `RefObjKey` | a_n_hair_twin_R |  | 
| `RefObjKey` | a_n_hand_L |  | 
| `RefObjKey` | a_n_hand_R |  | 
| `RefObjKey` | a_n_head |  | 
| `RefObjKey` | a_n_headflont |  | 
| `RefObjKey` | a_n_headside |  | 
| `RefObjKey` | a_n_headtop |  | 
| `RefObjKey` | a_n_heel_L |  | 
| `RefObjKey` | a_n_heel_R |  | 
| `RefObjKey` | a_n_ind_L |  | 
| `RefObjKey` | a_n_ind_R |  | 
| `RefObjKey` | a_n_knee_L |  | 
| `RefObjKey` | a_n_knee_R |  | 
| `RefObjKey` | a_n_kokan |  | 
| `RefObjKey` | a_n_leg_L |  | 
| `RefObjKey` | a_n_leg_R |  | 
| `RefObjKey` | a_n_megane |  | 
| `RefObjKey` | a_n_mid_L |  | 
| `RefObjKey` | a_n_mid_R |  | 
| `RefObjKey` | a_n_mouth |  | 
| `RefObjKey` | a_n_neck |  | 
| `RefObjKey` | a_n_nip_L |  | 
| `RefObjKey` | a_n_nip_R |  | 
| `RefObjKey` | a_n_nose |  | 
| `RefObjKey` | a_n_ring_L |  | 
| `RefObjKey` | a_n_ring_R |  | 
| `RefObjKey` | a_n_shoulder_L |  | 
| `RefObjKey` | a_n_shoulder_R |  | 
| `RefObjKey` | a_n_waist |  | 
| `RefObjKey` | a_n_waist_b |  | 
| `RefObjKey` | a_n_waist_f |  | 
| `RefObjKey` | a_n_waist_L |  | 
| `RefObjKey` | a_n_waist_R |  | 
| `RefObjKey` | a_n_wrist_L |  | 
| `RefObjKey` | a_n_wrist_R |  | 
| `RefObjKey` | A_ROOTBONE |  | 
| `RefObjKey` | BUSTUP_TARGET |  | 
| `RefObjKey` | CORRECT_ARM_L |  | 
| `RefObjKey` | CORRECT_ARM_R |  | 
| `RefObjKey` | CORRECT_HAND_L |  | 
| `RefObjKey` | CORRECT_HAND_R |  | 
| `RefObjKey` | DB_SKIRT_BOT |  | 
| `RefObjKey` | DB_SKIRT_TOP |  | 
| `RefObjKey` | DB_SKIRT_TOPA |  | 
| `RefObjKey` | DB_SKIRT_TOPB |  | 
| `RefObjKey` | F_ADJUSTWIDTHSCALE |  | 
| `RefObjKey` | HairParent |  | 
| `RefObjKey` | HeadParent |  | 
| `RefObjKey` | k_f_handL_00 |  | 
| `RefObjKey` | k_f_handR_00 |  | 
| `RefObjKey` | k_f_shoulderL_00 |  | 
| `RefObjKey` | k_f_shoulderR_00 |  | 
| `RefObjKey` | N_EyeBase |  | 
| `RefObjKey` | N_FaceSpecial |  | 
| `RefObjKey` | N_Gag00 |  | 
| `RefObjKey` | N_Gag01 |  | 
| `RefObjKey` | N_Gag02 |  | 
| `RefObjKey` | N_Hitomi |  | 
| `RefObjKey` | NECK_LOOK_TARGET |  | 
| `RefObjKey` | ObjBody |  | 
| `RefObjKey` | ObjBraDef |  | 
| `RefObjKey` | ObjBraNuge |  | 
| `RefObjKey` | ObjDoubleTooth |  | 
| `RefObjKey` | ObjEyebrow |  | 
| `RefObjKey` | ObjEyeL |  | 
| `RefObjKey` | ObjEyeline |  | 
| `RefObjKey` | ObjEyelineLow |  | 
| `RefObjKey` | ObjEyeR |  | 
| `RefObjKey` | ObjEyeWL |  | 
| `RefObjKey` | ObjEyeWR |  | 
| `RefObjKey` | ObjFace |  | 
| `RefObjKey` | ObjInnerDef |  | 
| `RefObjKey` | ObjInnerNuge |  | 
| `RefObjKey` | ObjNip |  | 
| `RefObjKey` | ObjNoseline |  | 
| `RefObjKey` | S_ANA |  | 
| `RefObjKey` | S_CBOT_B_DEF |  | 
| `RefObjKey` | S_CBOT_B_NUGE |  | 
| `RefObjKey` | S_CBOT_T_DEF |  | 
| `RefObjKey` | S_CBOT_T_NUGE |  | 
| `RefObjKey` | S_CTOP_B_DEF |  | 
| `RefObjKey` | S_CTOP_B_NUGE |  | 
| `RefObjKey` | S_CTOP_T_DEF |  | 
| `RefObjKey` | S_CTOP_T_NUGE |  | 
| `RefObjKey` | S_GOMU |  | 
| `RefObjKey` | S_MNPA |  | 
| `RefObjKey` | S_MNPB |  | 
| `RefObjKey` | S_MOZ_ALL |  | 
| `RefObjKey` | S_PANST_DEF |  | 
| `RefObjKey` | S_PANST_NUGE |  | 
| `RefObjKey` | S_SimpleBody |  | 
| `RefObjKey` | S_SimpleTongue |  | 
| `RefObjKey` | S_SimpleTop |  | 
| `RefObjKey` | S_Son |  | 
| `RefObjKey` | S_TEARS_01 |  | 
| `RefObjKey` | S_TEARS_02 |  | 
| `RefObjKey` | S_TEARS_03 |  | 
| `RefObjKey` | S_TongueB |  | 
| `RefObjKey` | S_TongueF |  | 
| `RefObjKey` | S_TPARTS_00_DEF |  | 
| `RefObjKey` | S_TPARTS_00_NUGE |  | 
| `RefObjKey` | S_TPARTS_01_DEF |  | 
| `RefObjKey` | S_TPARTS_01_NUGE |  | 
| `RefObjKey` | S_TPARTS_02_DEF |  | 
| `RefObjKey` | S_TPARTS_02_NUGE |  | 
| `RefObjKey` | S_UWB_B_DEF |  | 
| `RefObjKey` | S_UWB_B_NUGE |  | 
| `RefObjKey` | S_UWB_B_NUGE2 |  | 
| `RefObjKey` | S_UWB_T_DEF |  | 
| `RefObjKey` | S_UWB_T_NUGE |  | 
| `RefObjKey` | S_UWT_B_DEF |  | 
| `RefObjKey` | S_UWT_B_NUGE |  | 
| `RefObjKey` | S_UWT_T_DEF |  | 
| `RefObjKey` | S_UWT_T_NUGE |  | 


