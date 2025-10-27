## `ConfigurationManagerAttributes`

Class that specifies how a setting should be displayed inside the ConfigurationManager settings window.    Usage:  You can use this copy of the class instead of including it in your own plugin.  Make a new instance, assign any fields that you want to override, and pass it as a tag for your setting.    If a field is null (default), it will be ignored and won't change how the setting is displayed.  If a field is non-null (you assigned a value to it), it will override default behavior.
```csharp
public class KKAPI.Utilities.ConfigurationManagerAttributes

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Nullable<Boolean>` | Browsable | Show this setting in the settings screen at all? If false, don't show. | 
| `String` | Category | Category the setting is under. Null to be directly under the plugin. | 
| `Action<ConfigEntryBase>` | CustomDrawer | Custom setting editor (OnGUI code that replaces the default editor provided by ConfigurationManager).  See below for a deeper explanation. Using a custom drawer will cause many of the other fields to do nothing. | 
| `CustomHotkeyDrawerFunc` | CustomHotkeyDrawer | Custom setting editor that allows polling keyboard input with the Input (or UnityInput) class.  Use either CustomDrawer or CustomHotkeyDrawer, using both at the same time leads to undefined behaviour. | 
| `Object` | DefaultValue | If set, a "Default" button will be shown next to the setting to allow resetting to default. | 
| `String` | Description | Optional description shown when hovering over the setting.  Not recommended, provide the description when creating the setting instead. | 
| `String` | DispName | Name of the setting. | 
| `Nullable<Boolean>` | HideDefaultButton | Force the "Reset" button to not be displayed, even if a valid DefaultValue is available. | 
| `Nullable<Boolean>` | HideSettingName | Force the setting name to not be displayed. Should only be used with a `KKAPI.Utilities.ConfigurationManagerAttributes.CustomDrawer` to get more space.  Can be used together with `KKAPI.Utilities.ConfigurationManagerAttributes.HideDefaultButton` to gain even more space. | 
| `Nullable<Boolean>` | IsAdvanced | If true, don't show the setting by default. User has to turn on showing advanced settings or search for it. | 
| `Func<Object, String>` | ObjToStr | Custom converter from setting type to string for the built-in editor textboxes. | 
| `Nullable<Int32>` | Order | Order of the setting on the settings list relative to other settings in a category.  0 by default, higher number is higher on the list. | 
| `Nullable<Boolean>` | ReadOnly | Only show the value, don't allow editing it. | 
| `Nullable<Boolean>` | ShowRangeAsPercent | Should the setting be shown as a percentage (only use with value range settings). | 
| `Func<String, Object>` | StrToObj | Custom converter from string to setting type for the built-in editor textboxes. | 


## `CoroutineUtils`

Utility methods for working with coroutines.
```csharp
public static class KKAPI.Utilities.CoroutineUtils

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `WaitForEndOfFrame` | WaitForEndOfFrame | Cached WaitForEndOfFrame. Use instead of creating a new instance every time to reduce garbage production. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `IEnumerator` | AppendCo(this `IEnumerator` baseCoroutine, `IEnumerator` appendCoroutine) | Create a coroutine that calls the appendCoroutine after baseCoroutine finishes | 
| `IEnumerator` | AppendCo(this `IEnumerator` baseCoroutine, `YieldInstruction` yieldInstruction) | Create a coroutine that calls the appendCoroutine after baseCoroutine finishes | 
| `IEnumerator` | AppendCo(this `IEnumerator` baseCoroutine, `Action[]` actions) | Create a coroutine that calls the appendCoroutine after baseCoroutine finishes | 
| `IEnumerator` | AttachToYield(this `IEnumerator` coroutine, `Action` onYieldAction) | Create a coroutine that is the same as the supplied coroutine, except every time it yields the onYieldAction is invoked.  (i.e. onYieldAction is invoked after every yield return in the original coroutine)  If the coroutine returns another coroutine, the action is not called for yields performed by the returned coroutine, only the topmost one. Use FlattenCo if that's an issue. | 
| `IEnumerator` | ComposeCoroutine(`IEnumerator[]` coroutine) | Create a coroutine that calls each of the supplied coroutines in order. | 
| `IEnumerator` | CreateCoroutine(`Action[]` actions) | Create a coroutine that calls each of the action delegates on consecutive frames.  One action is called per frame. First action is called right away. There is no frame skip after the last action. | 
| `IEnumerator` | CreateCoroutine(`YieldInstruction` yieldInstruction, `Action[]` actions) | Create a coroutine that calls each of the action delegates on consecutive frames.  One action is called per frame. First action is called right away. There is no frame skip after the last action. | 
| `IEnumerator` | FlattenCo(this `IEnumerator` coroutine) | Flatten the coroutine to yield all values directly. Any coroutines yield returned by this coroutine will have their values directly returned by this new coroutine (this is recursive).  For example if another coroutine is yielded by this coroutine, the yielded coroutine will not be returned and instead the values that it yields will be returned.  If a yielded coroutine yields yet another coroutine, that second coroutine's values will be returned directly from the flattened coroutine. | 
| `MethodInfo` | GetMoveNext(`MethodBase` targetMethod) | Find the compiler-generated MoveNext method that contains the Coroutine/UniTask code. It can be used to apply transpliers to Coroutines and UniTasks.  Note: When writing transpliers for coroutines you might want to turn off the "Decompiler\Decompile enumerators" setting in DnSpy so that you can see the real code.  UniTasks are considered "async/await" so you need to turn off the "Decompile async methods" setting instead. | 
| `MethodInfo` | PatchMoveNext(this `Harmony` harmonyInstance, `MethodBase` original, `HarmonyMethod` prefix = null, `HarmonyMethod` postfix = null, `HarmonyMethod` transpiler = null, `HarmonyMethod` finalizer = null, `HarmonyMethod` ilmanipulator = null) | Used to patch coroutines/IEnumerator methods and async UniTask methods.  This will method automatically find the compiler-generated MoveNext method that contains the coroutine code and apply patches on that. The method you patch must return an IEnumerator or an UniTask.  Warning: Postfix patches will not work as expected, they might be fired after every iteration. Prefix is practically the same as prefixing the entry method. It's best to only use transpliers with this method.  Note: When writing transpliers you might want to turn off the "Decompiler\Decompile enumerators/async" settings in DnSpy so that you can see the real code. | 
| `IEnumerator` | PreventFromCrashing(this `IEnumerator` coroutine) | Prevent a coroutine from getting stopped by exceptions. Exceptions are caught and logged.  Code after the exception is thrown doesn't run up until the next yield. The coroutine continues after the yield then. | 
| `void` | RunImmediately(this `IEnumerator` coroutine) | Fully executes the coroutine synchronously (immediately run all of its code till completion). | 
| `IEnumerator` | StopCoOnQuit(this `IEnumerator` enumerator) | Create a coroutine that is the same as the supplied coroutine, but will stop early if `KKAPI.KoikatuAPI.IsQuitting` is <c>true</c>.  If the coroutine returns another coroutine, the `KKAPI.KoikatuAPI.IsQuitting` check only runs on the topmost one. Use `KKAPI.Utilities.CoroutineUtils.FlattenCo(System.Collections.IEnumerator)` if that's an issue. | 
| `IEnumerator` | StripYields(this `IEnumerator` coroutine, `Boolean` onlyStripNulls = True, `Boolean` flatten = True) | Remove yields from the coroutine, making its code run immediately. | 


## `Curve`

Provides a comprehensive set of utilities for performing operations on curves, including interpolation via Catmull-Rom splines and Bezier curves.
```csharp
public class KKAPI.Utilities.Curve

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Vector4` | Bezier(`Vector4` p0, `Vector4` p1, `Vector4` p2, `Vector4` p3, `Single` t) | Calculates a position on a cubic Bezier curve for a given interpolation value. | 
| `Vector3` | Bezier(`Vector3` p0, `Vector3` p1, `Vector3` p2, `Vector3` p3, `Single` t) | Calculates a position on a cubic Bezier curve for a given interpolation value. | 
| `Vector2` | Bezier(`Vector2` p0, `Vector2` p1, `Vector2` p2, `Vector2` p3, `Single` t) | Calculates a position on a cubic Bezier curve for a given interpolation value. | 
| `Vector4` | CatmullRom(`Vector4` p0, `Vector4` p1, `Vector4` p2, `Vector4` p3, `Single` t) | Performs Catmull-Rom spline interpolation between four points to calculate a position at a specific interpolation value. | 
| `Vector3` | CatmullRom(`Vector3` p0, `Vector3` p1, `Vector3` p2, `Vector3` p3, `Single` t) | Performs Catmull-Rom spline interpolation between four points to calculate a position at a specific interpolation value. | 
| `Vector2` | CatmullRom(`Vector2` p0, `Vector2` p1, `Vector2` p2, `Vector2` p3, `Single` t) | Performs Catmull-Rom spline interpolation between four points to calculate a position at a specific interpolation value. | 
| `Vector3[]` | ResamplePoly(`Vector3[]` points, `Int32` count, `Boolean` smooth = False, `Func<Vector3, Vector3, Vector3, Vector3, Single, Vector3>` solver = null) | Resamples a polyline represented by a set of 3D points into a specified number of evenly spaced points, with optional smoothing. | 


## `Extensions`

General utility extensions that don't fit in other categories.
```csharp
public static class KKAPI.Utilities.Extensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | ActuallyRemoveAllListeners(this `UnityEventBase` evt) | Same as RemoveAllListeners but also disables all PersistentListeners.  To avoid frustration always use this instead of RemoveAllListeners, unless you want to keep the PersistentListeners. | 
| `IEnumerable<T2>` | Attempt(this `IEnumerable<T>` items, `Func<T, T2>` action) | Attempt to project each element of the sequence into a new form (Select but ignore exceptions).  Exceptions thrown while doing this are ignored and any elements that fail to be converted are silently skipped. | 
| `IEnumerable<T2>` | Attempt(this `IEnumerable<T>` items, `Func<T, T2>` action, `Action<Exception>` onError) | Attempt to project each element of the sequence into a new form (Select but ignore exceptions).  Exceptions thrown while doing this are ignored and any elements that fail to be converted are silently skipped. | 
| `void` | FancyDestroy(this `GameObject` self, `Boolean` useDestroyImmediate = False, `Boolean` detachParent = False) | Destroy this GameObject. Safe to use on null objects. | 
| `AssignedAnotherWeights` | GetAaWeightsBody(this `ChaControl` ctrl) | Get value of the aaWeightsBody field | 
| `Boolean` | GetFieldValue(this `Object` self, `String` name, `Object&` value) | Get value of a field through reflection | 
| `String` | GetFullPath(this `GameObject` self) | Get full GameObject "path" to this GameObject.  Example: RootObject\ChildObject1\ChildObject2 | 
| `String` | GetFullPath(this `Component` self) | Get full GameObject "path" to this GameObject.  Example: RootObject\ChildObject1\ChildObject2 | 
| `Boolean` | GetPropertyValue(this `Object` self, `String` name, `Object&` value) | Get value of a property through reflection | 
| `Rect` | GetScreenRect(this `RectTransform` rectTransform) | Get the screen-space rectangle of this RectTransform. | 
| `Transform` | GetTopmostParent(this `Component` src) | Get the topmost parent of Transform that this this Component is attached to. | 
| `Transform` | GetTopmostParent(this `GameObject` src) | Get the topmost parent of Transform that this this Component is attached to. | 
| `Transform` | GetTopmostParent(this `Transform` src) | Get the topmost parent of Transform that this this Component is attached to. | 
| `Boolean` | IsDestroyed(this `Object` obj) | Return true if the object is a "fake" null (i.e. it was destroyed). | 
| `Boolean` | IsInsideScreenBounds(this `RectTransform` rectTransform, `Int32` margin = 0) | Check if the RectTransform's bounds are fully within the screen bounds. | 
| `void` | MarkXuaIgnored(this `Component` target) | Mark GameObject of this Component as ignored by AutoTranslator. Prevents AutoTranslator from trying to translate custom UI elements. | 
| `Boolean` | SequenceEqualFast(this `Byte[]` a, `Byte[]` b) | This method compares two byte arrays for equality, returning true if they are identical and false otherwise.  It is optimized for high performance and uses unsafe code. | 
| `Boolean` | SetFieldValue(this `Object` self, `String` name, `Object` value) | Set value of a field through reflection | 
| `Boolean` | SetPropertyValue(this `Object` self, `String` name, `Object` value) | Set value of a property through reflection | 
| `ReadOnlyDictionary<TKey, TValue>` | ToReadOnlyDictionary(this `IDictionary<TKey, TValue>` original) | Wrap this dictionary in a read-only wrapper that will prevent any changes to it.  Warning: Any reference types inside the dictionary can still be modified. | 


## `GlobalTooltips`

API for easily displaying tooltips in game and studio.
```csharp
public static class KKAPI.Utilities.GlobalTooltips

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Tooltip` | RegisterTooltip(`GameObject` target, `String` text) |  | 
| `void` | RegisterTooltip(`GameObject` target, `Tooltip` tooltip) |  | 


## `HSceneUtils`

Utility methods for working with H Scenes / main game.
```csharp
public static class KKAPI.Utilities.HSceneUtils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `???` | GetLeadingHeroine(this ???) | Get the heroine that is currently in leading position in the h scene.  In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine. | 
| `???` | GetLeadingHeroine(this ???) | Get the heroine that is currently in leading position in the h scene.  In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine. | 
| `Int32` | GetLeadingHeroineId(this `HFlag` hFlag) | Get ID of the heroine that is currently in leading position in the h scene. 0 is the main heroine, 1 is the "tag along".  In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine. | 
| `Int32` | GetLeadingHeroineId(this `HSprite` hSprite) | Get ID of the heroine that is currently in leading position in the h scene. 0 is the main heroine, 1 is the "tag along".  In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine. | 
| `Boolean` | IsHoushi(this `HFlag` hFlag) | Is current h mode service? | 
| `Boolean` | IsSonyu(this `HFlag` hFlag) | Is current H mode penetration? | 


## `ImageTypeIdentifier`

Tool for identifying likely file extension for byte arrays representing images.
```csharp
public static class KKAPI.Utilities.ImageTypeIdentifier

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | Identify(`Byte[]` bytes, `String` defReturn = bin) | Identify the file extension of a byte array representing an image.  Returns "bin" by default if the array is shorter than 20 bytes, or identification is unsuccessful. | 


## `ImguiComboBox`

Dropdown control for use in GUILayout areas and windows. Keep the instance and call Show on it to draw it inside OnGUI.  Remember to call `DrawDropdownIfOpen` at the very end of the OnGUI area/window to actually display the dropdown list if it's open.  Only one dropdown list can be open globally. If a new dropdown is opened, all others are closed without changing the selected index.
```csharp
public class KKAPI.Utilities.ImguiComboBox

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | DrawDropdownIfOpen() | Draws the dropdown list on top of all other window controls if it is open.  This should always be called at the very end of area/window that `Show` was called in.  Returns true if the dropdown list was opened and subsequently drawn. | 
| `Int32` | Show(`Int32` selectedIndex, `GUIContent[]` listContent, `Int32` windowYmax = 2147483647, `GUIStyle` listStyle = null) | Show a button that when clicked opens a dropdown list. Returns new index if user selected a different option, or the old index.  Warning: The list itself is not drawn here, you have to call DrawDropdownIfOpen at the end of your GUILayout area/window. | 
| `void` | Show(`GUIContent` selectedContent, `Func<GUIContent[]>` getListContent, `Action<Int32>` onIndexChanged, `Int32` windowYmax = 2147483647, `GUIStyle` listStyle = null) | Show a button that when clicked opens a dropdown list. Returns new index if user selected a different option, or the old index.  Warning: The list itself is not drawn here, you have to call DrawDropdownIfOpen at the end of your GUILayout area/window. | 


## `IMGUIUtils`

Utility methods for working with IMGUI / OnGui.
```csharp
public static class KKAPI.Utilities.IMGUIUtils

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `GUILayoutOption[]` | EmptyLayoutOptions | Empty GUILayoutOption array. You can use this instead of nothing to avoid allocations.  For example: <code>GUILayout.Label("Hello world!", IMGUIUtils.EmptyLayoutOptions);</code>  At that point you might also want to use GUIContent (created once and stored) instead of string. | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `GUISkin` | SolidBackgroundGuiSkin | A custom GUISkin with a solid background, sharper edges and less padding.  The skin background color is adjusted to the game (if its color filter affects imgui layer).  Warning: Only use inside OnGUI or things might break. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Rect` | DragResizeEatWindow(`Int32` windowId, `Rect` windowRect) | Handle both dragging and resizing of OnGUI windows, as well as eat mouse inputs when cursor is over the window.  Use this instead of `UnityEngine.GUI.DragWindow(UnityEngine.Rect)` and `KKAPI.Utilities.IMGUIUtils.EatInputInRect(UnityEngine.Rect)`. Don't use these methods at the same time as DragResizeEatWindow.  To use, place this at the end of your Window method: _windowRect = IMGUIUtils.DragResizeEatWindow(windowId, _windowRect); | 
| `Rect` | DragResizeWindow(`Int32` windowId, `Rect` windowRect) | Handle both dragging and resizing of OnGUI windows.  Use this instead of GUI.DragWindow(), don't use both at the same time.  To use, place this at the end of your Window method: _windowRect = IMGUIUtils.DragResizeWindow(windowId, _windowRect); | 
| `Boolean` | DrawButtonWithShadow(`Rect` r, `GUIContent` content, `GUIStyle` style, `Single` shadowAlpha, `Vector2` direction) |  | 
| `void` | DrawLabelWithOutline(`Rect` rect, `String` text, `GUIStyle` style, `Color` txtColor, `Color` outlineColor, `Int32` outlineThickness) | Draw a label with an outline | 
| `void` | DrawLabelWithShadow(`Rect` rect, `GUIContent` content, `GUIStyle` style, `Color` txtColor, `Color` shadowColor, `Vector2` shadowOffset) | Draw a label with a shadow | 
| `Boolean` | DrawLayoutButtonWithShadow(`GUIContent` content, `GUIStyle` style, `Single` shadowAlpha, `Vector2` direction, `GUILayoutOption[]` options) |  | 
| `void` | DrawLayoutLabelWithShadow(`GUIContent` content, `GUIStyle` style, `Color` txtColor, `Color` shadowColor, `Vector2` direction, `GUILayoutOption[]` options) |  | 
| `void` | DrawSolidBox(`Rect` boxRect) | Draw a gray non-transparent GUI.Box at the specified rect. Use before a GUI.Window or other controls to get rid of  the default transparency and make the GUI easier to read.  <example>  IMGUIUtils.DrawSolidBox(screenRect);  GUILayout.Window(362, screenRect, TreeWindow, "Select character folder");  </example> | 
| `void` | DrawTooltip(`Rect` area, `Int32` tooltipWidth = 400) | Display a tooltip for any GUIContent with the tootlip property set in a given window.  To use, place this at the end of your Window method: IMGUIUtils.DrawTooltip(_windowRect); | 
| `void` | EatInputInRect(`Rect` eatRect) | Block input from going through to the game/canvases if the mouse cursor is within the specified Rect.  Use after a GUI.Window call or the window will not be able to get the inputs either.  <example>  GUILayout.Window(362, screenRect, TreeWindow, "Select character folder");  Utils.EatInputInRect(screenRect);  </example> | 


## `ImguiWindow<T>`

Base class for IMGUI windows that are implemented as full MonoBehaviours.  Instantiate to add the window, only one instance should ever exist.  Turn drawing the window on and off by setting the enable property (off by default).
```csharp
public abstract class KKAPI.Utilities.ImguiWindow<T>
    : MonoBehaviour

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Vector2` | MinimumSize | Minimum size of the window. | 
| `String` | Title | Title of the window. | 
| `Int32` | WindowId | ID of the window, set to a random number by default. | 
| `Rect` | WindowRect | Position and size of the window. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | DrawContents() | Draw contents of the IMGUI window (this is inside of the GUILayout.Window func).  Use GUILayout instead of GUI, and expect the window size to change during runtime. | 
| `Rect` | GetDefaultWindowRect(`Rect` screenRect) | Should return the initial desired size of the window, adjusted to fit inside the screen space. | 
| `void` | OnEnable() | Make sure to call base.OnEnable when overriding! | 
| `void` | OnGUI() | Make sure to call base.OnGUI when overriding! | 
| `void` | ResetWindowRect() | Reset the window rect (position and size) to its default value. | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `T` | Instance | Instance of the window. Null if none were created yet. | 


## `MemoryInfo`

Provides information about system memory status
```csharp
public static class KKAPI.Utilities.MemoryInfo

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `MEMORYSTATUSEX` | GetCurrentStatus() | Can return null if the call fails for whatever reason | 


## `ObservableExtensions`

Additions to the UniRx IObservable extension methods
```csharp
public static class KKAPI.Utilities.ObservableExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `???` | OnGUIAsObservable(this ???) | Get an observable that triggers on every OnGUI call on this gameObject | 
| `???` | OnGUIAsObservable(this ???) | Get an observable that triggers on every OnGUI call on this gameObject | 
| `???` | OnGUIAsObservable(this ???) | Get an observable that triggers on every OnGUI call on this gameObject | 


## `OpenFileDialog`

Gives access to the Windows open file dialog.  http://www.pinvoke.net/default.aspx/comdlg32/GetOpenFileName.html  http://www.pinvoke.net/default.aspx/Structures/OpenFileName.html  http://www.pinvoke.net/default.aspx/Enums/OpenSaveFileDialgueFlags.html  https://social.msdn.microsoft.com/Forums/en-US/2f4dd95e-5c7b-4f48-adfc-44956b350f38/getopenfilename-for-multiple-files?forum=csharpgeneral
```csharp
public class KKAPI.Utilities.OpenFileDialog

```

Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `OpenSaveFileDialgueFlags` | MultiFileFlags | Arguments used for opening multiple files | 
| `OpenSaveFileDialgueFlags` | SingleFileFlags | Arguments used for opening a single file | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Show(`Action<String[]>` onAccept, `String` title, `String` initialDir, `String` filter, `String` defaultExt, `OpenSaveFileDialgueFlags` flags = OFN_NOCHANGEDIR, OFN_FILEMUSTEXIST, OFN_EXPLORER, OFN_LONGNAMES) | Show windows file open dialog. Doesn't pause the game. | 


## `ReadOnlyDictionary<TKey, TValue>`

Read-only dictionary wrapper. Will protect the base dictionary from being changed.  Warning: Any reference types inside the dictionary can still be modified.
```csharp
public class KKAPI.Utilities.ReadOnlyDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | Count |  | 
| `Boolean` | IsReadOnly |  | 
| `TValue` | Item |  | 
| `ICollection<TKey>` | Keys |  | 
| `ICollection<TValue>` | Values |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | Contains(`KeyValuePair<TKey, TValue>` item) |  | 
| `Boolean` | ContainsKey(`TKey` key) |  | 
| `void` | CopyTo(`KeyValuePair`2[]` array, `Int32` arrayIndex) |  | 
| `IEnumerator<KeyValuePair<TKey, TValue>>` | GetEnumerator() |  | 
| `void` | System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.Add(`KeyValuePair<TKey, TValue>` item) |  | 
| `void` | System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.Clear() |  | 
| `Boolean` | System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.Remove(`KeyValuePair<TKey, TValue>` item) |  | 
| `void` | System.Collections.Generic.IDictionary<TKey,TValue>.Add(`TKey` key, `TValue` value) |  | 
| `Boolean` | System.Collections.Generic.IDictionary<TKey,TValue>.Remove(`TKey` key) |  | 
| `IEnumerator` | System.Collections.IEnumerable.GetEnumerator() |  | 
| `Boolean` | TryGetValue(`TKey` key, `TValue&` value) |  | 


## `RecycleBinUtil`

Allows to move files to recycle bin instead of completely removing them.  https://stackoverflow.com/questions/3282418/send-a-file-to-the-recycle-bin?answertab=votes#tab-top
```csharp
public static class KKAPI.Utilities.RecycleBinUtil

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | MoveToRecycleBin(`String` path, `FileOperationFlags` flags) | Send file to recycle bin | 
| `Boolean` | MoveToRecycleBin(`String` path) | Send file to recycle bin | 


## `ResourceUtils`

Utility methods for working with embedded resources.
```csharp
public static class KKAPI.Utilities.ResourceUtils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Byte[]` | GetEmbeddedResource(`String` resourceFileName, `Assembly` containingAssembly = null) | Get a file set as "Embedded Resource" from the assembly that is calling this code, or optionally from a specified assembly.  The filename is matched to the end of the resource path, no need to give the full path.  If 0 or more than 1 resources match the provided filename, an exception is thrown.  For example if you have a file "ProjectRoot\Resources\icon.png" set as "Embedded Resource", you can use this to load it by  doing <code>GetEmbeddedResource("icon.png"), assuming that no other embedded files have the same name.</code> | 
| `Byte[]` | ReadAllBytes(this `Stream` input) | Read all bytes starting at current position and ending at the end of the stream. | 


## `SmartRect`

Represents a smart rectangle that provides advanced manipulation of a rectangle's dimensions and position.
```csharp
public class KKAPI.Utilities.SmartRect

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | DefaultHeight | The initial height of the rectangle.  Used to `KKAPI.Utilities.SmartRect.Reset` the rectangle back to its original dimensions. | 
| `Single` | DefaultWidth | The initial width of the rectangle.  Used to `KKAPI.Utilities.SmartRect.Reset` the rectangle back to its original dimensions. | 
| `Single` | DefaultX | The initial X offset of the rectangle. | 
| `Single` | DefaultY | The initial Y offset of the rectangle. | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | Height | Gets or sets the height of the rectangle. | 
| `Single` | OffsetX | Gets the horizontal offset applied to the rectangle. | 
| `Single` | OffsetY | Gets the vertical offset applied to the rectangle. | 
| `Single` | TotalHeight | Gets the total height of the rectangle including the vertical offset. | 
| `Single` | TotalWidth | Gets the total width of the rectangle including the horizontal offset. | 
| `Single` | Width | Gets or sets the width of the rectangle. | 
| `Single` | X | Gets or sets the X position of the rectangle. | 
| `Single` | Y | Gets or sets the Y position of the rectangle. | 


Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `EventHandler` | OnAnimationComplete | Occurs when the animation completes. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `SmartRect` | BeginHorizontal(`Int32` elementCount) | Divides the current width of the rectangle into equal segments for horizontal layout.  Adjusts the width of each segment based on the total number of elements and specified horizontal offsets. | 
| `SmartRect` | Col(`Int32` col) | Creates a new `KKAPI.Utilities.SmartRect` instance by adjusting the X-coordinate based on the specified column index. | 
| `SmartRect` | EndHorizontal() | Synonymous to `KKAPI.Utilities.SmartRect.ResetX(System.Boolean)` | 
| `SmartRect` | HeightToEnd(`Single` y) | Sets the height of the rectangle such that its bottom edge aligns with the specified y-coordinate. | 
| `SmartRect` | Move(`Vector2` vec) | Moves the SmartRect by the specified vector values. | 
| `SmartRect` | Move(`Int32` x, `Int32` y) | Moves the SmartRect by the specified vector values. | 
| `SmartRect` | MoveOffsetX(`Single` off) | Moves the rectangle's X position by the specified offset and adjusts its width accordingly. | 
| `SmartRect` | MoveOffsetY(`Single` off) | Adjusts the Y position and height of the smart rectangle by the specified offset. | 
| `SmartRect` | MoveToEndX(`Rect` box, `Single` width) | Adjusts the X position of the current rectangle to align with the right edge of the specified rectangle, taking into account the given width. | 
| `SmartRect` | MoveToEndY(`Rect` box, `Single` height) | Moves the Y position of the rectangle represented by the current `KKAPI.Utilities.SmartRect` instance  to the bottom of the specified 'box' plus the specified 'height'. | 
| `SmartRect` | MoveX() | Moves the rectangle horizontally by a predefined offset and returns the updated `KKAPI.Utilities.SmartRect` instance. | 
| `SmartRect` | MoveX(`Single` off, `Boolean` considerWidth = False) | Moves the rectangle horizontally by a predefined offset and returns the updated `KKAPI.Utilities.SmartRect` instance. | 
| `SmartRect` | MoveY() | Moves the <seealso cref="T:KKAPI.Utilities.SmartRect" /> by it's own height. | 
| `SmartRect` | MoveY(`Single` offset, `Boolean` considerHeight = False) | Moves the <seealso cref="T:KKAPI.Utilities.SmartRect" /> by it's own height. | 
| `SmartRect` | NextColumn() | Moves to the next column by shifting the rectangle horizontally by a predefined offset. | 
| `SmartRect` | NextRow(`Boolean` resetColumn = True) | Moves the rectangle to the next row, optionally resetting the column position. | 
| `SmartRect` | Reset() | Resets the `KKAPI.Utilities.SmartRect` to its default position and dimensions. | 
| `SmartRect` | ResetX(`Boolean` includeWidth = True) | Resets the x-coordinate of the rectangle to its default value.  Optionally resets the width to its default value. | 
| `SmartRect` | ResetY(`Boolean` includeHeight = False) | Resets the y-coordinate of the rectangle to its default value.  Optionally resets the height to its default value. | 
| `SmartRect` | Row(`Int32` row) | Creates a new `KKAPI.Utilities.SmartRect` instance representing a specific row, offset vertically by the specified row index. | 
| `SmartRect` | SetAnimateTo(`Rect` to, `Single` duration) | Sets the target rectangle and duration for an animation. | 
| `SmartRect` | SetAnimation(`Rect` from, `Rect` to, `Single` duration) | Sets the starting and target rectangles, along with the duration for an animation. | 
| `SmartRect` | SetHeight(`Single` height) | Sets the height of the rectangle. | 
| `SmartRect` | SetWidth(`Single` width) | Sets the width of the rectangle. | 
| `Rect` | ToRect() | Converts the current `KKAPI.Utilities.SmartRect` instance into a Rect object. | 
| `Boolean` | UpdateAnimationIndependent(`Func<Single, Vector2>` solver) | Updates the animation of the rectangle independently using a provided solver function based on progress. | 
| `SmartRect` | WidthToEnd(`Single` x) | Sets the width of the rectangle such that its right edge aligns with the specified x-coordinate. | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Single` | DefaultOffsetX | Default horizontal offset value. Used if not specified in constructor. | 
| `Single` | DefaultOffsetY | Default vertical offset value. Used if not specified in constructor. | 


## `SystemFileDialog`

Provides functionality for displaying system file dialogs to allow users to select files or folders.
```csharp
public class KKAPI.Utilities.SystemFileDialog

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | ShowDialog(`String` title, `String` path, `String&` result, `FOS` fos = 0, `String` filter = All Files|*.*) | Displays a system file dialog to allow the user to select a file or folder. | 


## `TabletManager`

Manages tablet input events and subscriptions.
```csharp
public class KKAPI.Utilities.TabletManager
    : MonoBehaviour

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | IsAvailable |  | 
| `UInt32` | MaxPressure |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Subscribe(`TabletEvent` handler) | Subscribes a provided event handler to receive tablet input updates. | 
| `void` | Unsubscribe(`TabletEvent` handler) | Unsubscribes a previously registered event handler from receiving tablet input updates. | 


## `TextureUtils`

Utility methods for working with texture objects.
```csharp
public static class KKAPI.Utilities.TextureUtils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Texture2D` | GetVisibleTexture(this `Sprite` spr) | Gets texture as it is shown by this sprite. If it's not packed then returns the original texture.  If it's packed then this tries to crop out the part that the sprite is supposed to show and return only that. | 
| `Texture2D` | LoadTexture(this `Byte[]` texBytes, `TextureFormat` format = ARGB32, `Boolean` mipMaps = False) | Create texture from an image stored in a byte array, for example a png file read from disk. | 
| `Texture2D` | ResizeTexture(this `Texture2D` pSource, `ImageFilterMode` pFilterMode, `Single` pScale) | Create a resized copy of this texture.  http://blog.collectivemass.com/2014/03/resizing-textures-in-unity/ | 
| `Sprite` | ToSprite(this `Texture2D` texture) | Create a sprite based on this texture. | 
| `Texture2D` | ToTexture2D(this `Texture` tex, `TextureFormat` format = ARGB32, `Boolean` mipMaps = False) | Copy this texture inside a new editable Texture2D. | 


## `TextUtils`

Utility methods for working with text.
```csharp
public static class KKAPI.Utilities.TextUtils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | PascalCaseToSentenceCase(this `String` str) | Convert PascalCase to Sentence case. | 


## `TimelineCompatibility`

API for adding Timeline support to other plugins. Support is done by registering interpolable models that appear in the interpolables list in Timeline.
```csharp
public static class KKAPI.Utilities.TimelineCompatibility

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | AddCharaFunctionInterpolable(`String` owner, `String` id, `String` name, `InterpolableCharaDelegate<TValue, TController>` interpolateBefore, `InterpolableCharaDelegate<TValue, TController>` interpolateAfter, `Func<OCIChar, TController, TValue>` getValue, `Func<TController, XmlNode, TValue>` readValueFromXml = null, `Action<TController, XmlTextWriter, TValue>` writeValueToXml = null, `Func<OCIChar, TController, TValue, TValue, Boolean>` checkIntegrity = null, `Boolean` useOciInHash = True, `Func<String, OCIChar, TController, String>` getFinalName = null, `Func<OCIChar, TController, Boolean>` shouldShow = null) | Adds an interpolableModel that targets CharaCustomFunctionControllers on characters to the available interpolables list. This interpolable can then be instantiated by the user by adding a keyframe. | 
| `void` | AddInterpolableModelDynamic(`String` owner, `String` id, `String` name, `InterpolableDelegate<TValue, TParameter>` interpolateBefore, `InterpolableDelegate<TValue, TParameter>` interpolateAfter, `Func<ObjectCtrlInfo, Boolean>` isCompatibleWithTarget, `Func<ObjectCtrlInfo, TParameter, TValue>` getValue, `Func<TParameter, XmlNode, TValue>` readValueFromXml = null, `Action<TParameter, XmlTextWriter, TValue>` writeValueToXml = null, `Func<ObjectCtrlInfo, TParameter>` getParameter = null, `Func<ObjectCtrlInfo, XmlNode, TParameter>` readParameterFromXml = null, `Action<ObjectCtrlInfo, XmlTextWriter, TParameter>` writeParameterToXml = null, `Func<ObjectCtrlInfo, TParameter, TValue, TValue, Boolean>` checkIntegrity = null, `Boolean` useOciInHash = True, `Func<String, ObjectCtrlInfo, TParameter, String>` getFinalName = null, `Func<ObjectCtrlInfo, TParameter, Boolean>` shouldShow = null) | Adds an interpolableModel with a dynamic parameter to the available interpolables list. This interpolable can then be instantiated by the user by adding a keyframe.  The dynamic parameter means that every time user creates this interpolable, you can provide a different parameter - this allows having multiple instances of this interpolable for a single studio object. The instances are differentiated by the parameter, which must be unique and have a unique hash code (if the parameters have identical hashes they are treated as the same thing).  Examples where this is useful: editing individual character bone positions/scales, changing color of individual accessories.  If you want to make a global interpolable you can use AddInterpolableModelStatic instead.  Try to keep the callbacks as light and self-contained as possible since they can be called on every frame during playback or UI usage (except the xml callbacks). If you need to find some object to later use in the callbacks, consider using it as the parameter (that way it will only be computed once and you can use it later for free, just make sure the parameter is always the same since if you have different parameters will create separate instances). | 
| `void` | AddInterpolableModelDynamic(`String` owner, `String` id, `String` name, `InterpolableDelegate` interpolateBefore, `InterpolableDelegate` interpolateAfter, `Func<ObjectCtrlInfo, Boolean>` isCompatibleWithTarget, `Func<ObjectCtrlInfo, Object, Object>` getValue, `Func<Object, XmlNode, Object>` readValueFromXml, `Action<Object, XmlTextWriter, Object>` writeValueToXml, `Func<ObjectCtrlInfo, Object>` getParameter, `Func<ObjectCtrlInfo, XmlNode, Object>` readParameterFromXml = null, `Action<ObjectCtrlInfo, XmlTextWriter, Object>` writeParameterToXml = null, `Func<ObjectCtrlInfo, Object, Object, Object, Boolean>` checkIntegrity = null, `Boolean` useOciInHash = True, `Func<String, ObjectCtrlInfo, Object, String>` getFinalName = null, `Func<ObjectCtrlInfo, Object, Boolean>` shouldShow = null) | Adds an interpolableModel with a dynamic parameter to the available interpolables list. This interpolable can then be instantiated by the user by adding a keyframe.  The dynamic parameter means that every time user creates this interpolable, you can provide a different parameter - this allows having multiple instances of this interpolable for a single studio object. The instances are differentiated by the parameter, which must be unique and have a unique hash code (if the parameters have identical hashes they are treated as the same thing).  Examples where this is useful: editing individual character bone positions/scales, changing color of individual accessories.  If you want to make a global interpolable you can use AddInterpolableModelStatic instead.  Try to keep the callbacks as light and self-contained as possible since they can be called on every frame during playback or UI usage (except the xml callbacks). If you need to find some object to later use in the callbacks, consider using it as the parameter (that way it will only be computed once and you can use it later for free, just make sure the parameter is always the same since if you have different parameters will create separate instances). | 
| `void` | AddInterpolableModelStatic(`String` owner, `String` id, `TParameter` parameter, `String` name, `InterpolableDelegate<TValue, TParameter>` interpolateBefore, `InterpolableDelegate<TValue, TParameter>` interpolateAfter, `Func<ObjectCtrlInfo, Boolean>` isCompatibleWithTarget, `Func<ObjectCtrlInfo, TParameter, TValue>` getValue, `Func<TParameter, XmlNode, TValue>` readValueFromXml = null, `Action<TParameter, XmlTextWriter, TValue>` writeValueToXml = null, `Func<ObjectCtrlInfo, XmlNode, TParameter>` readParameterFromXml = null, `Action<ObjectCtrlInfo, XmlTextWriter, TParameter>` writeParameterToXml = null, `Func<ObjectCtrlInfo, TParameter, TValue, TValue, Boolean>` checkIntegrity = null, `Boolean` useOciInHash = True, `Func<String, ObjectCtrlInfo, TParameter, String>` getFinalName = null, `Func<ObjectCtrlInfo, TParameter, Boolean>` shouldShow = null) | Adds an interpolableModel to the available interpolables list. This interpolable can then be instantiated by the user by adding a keyframe.  User can create a single instance of this interpolable for each studio object (or single instance globally if `` is <c>false</c>). If you want to have multiple instances of this interpolable for a single studio object use AddInterpolableModelDynamic instead.  Try to keep the callbacks as light and self-contained as possible since they can be called on every frame during playback or UI usage (except the xml callbacks). | 
| `void` | AddInterpolableModelStatic(`String` owner, `String` id, `Object` parameter, `String` name, `InterpolableDelegate` interpolateBefore, `InterpolableDelegate` interpolateAfter, `Func<ObjectCtrlInfo, Boolean>` isCompatibleWithTarget, `Func<ObjectCtrlInfo, Object, Object>` getValue, `Func<Object, XmlNode, Object>` readValueFromXml, `Action<Object, XmlTextWriter, Object>` writeValueToXml, `Func<ObjectCtrlInfo, XmlNode, Object>` readParameterFromXml = null, `Action<ObjectCtrlInfo, XmlTextWriter, Object>` writeParameterToXml = null, `Func<ObjectCtrlInfo, Object, Object, Object, Boolean>` checkIntegrity = null, `Boolean` useOciInHash = True, `Func<String, ObjectCtrlInfo, Object, String>` getFinalName = null, `Func<ObjectCtrlInfo, Object, Boolean>` shouldShow = null) | Adds an interpolableModel to the available interpolables list. This interpolable can then be instantiated by the user by adding a keyframe.  User can create a single instance of this interpolable for each studio object (or single instance globally if `` is <c>false</c>). If you want to have multiple instances of this interpolable for a single studio object use AddInterpolableModelDynamic instead.  Try to keep the callbacks as light and self-contained as possible since they can be called on every frame during playback or UI usage (except the xml callbacks). | 
| `Single` | GetDuration() | Gets the total duration in seconds. | 
| `Boolean` | GetIsPlaying() | Check if timeline is currently playing. | 
| `Single` | GetPlaybackTime() | Gets current playback location in seconds. | 
| `Boolean` | IsTimelineAvailable() | Check if Timeline is loaded and available to be used. If false, other methods in this class will throw.  This must be called after all plugins finish loadeding (in Start/Main instead of constructors/Awake).  It always returns false outside of studio. | 
| `void` | Play() | Play/Pause timeline playback (toggles between the two). | 
| `void` | RefreshInterpolablesList() | Refreshes the list of displayed interpolables. This function is quite heavy as it must go through each InterpolableModel and check if it's compatible with the current target.  It is called automatically by Timeline when selecting another Workspace object or GuideObject.  This triggers visibility checks of all interpolables and can be used to force update all instances of your interpolables if something important changes and Timeline doesn't notice it. | 


## `TranslationHelper`

Class that abstracts away AutoTranslator. It lets you translate text to current language.
```csharp
public static class KKAPI.Utilities.TranslationHelper

```

Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | AutoTranslatorInstalled | True if a reasonably recent version of AutoTranslator is installed.  It might return false for some very old versions that don't have the necessary APIs to make this class work. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | TranslateAsync(`String` untranslatedText, `Action<String>` onCompleted) | Queries AutoTranslator to provide a translated text for the untranslated text.  If the translation cannot be found in the cache, it will make a request to the translator selected by the user.  If AutoTranslator is not installed, this will do nothing. | 
| `Boolean` | TryTranslate(`String` untranslatedText, `String&` translatedText) | Queries the plugin to provide a translated text for the untranslated text.  If the translation cannot be found in the cache, the method returns false  and returns null as the untranslated text. | 


## `WindowsStringComparer`

String comparer that is equivalent to the one used by Windows Explorer to sort files (e.g. 2 will go before 10, unlike normal compare).
```csharp
public class KKAPI.Utilities.WindowsStringComparer
    : IComparer<String>

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | Compare(`String` x, `String` y) | Compare two strings with rules used by Windows Explorer to logically sort files. | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | LogicalCompare(`String` x, `String` y) | Compare two strings with rules used by Windows Explorer to logically sort files. | 


