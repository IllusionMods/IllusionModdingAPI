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
| `void` | RunImmediately(this `IEnumerator` coroutine) | Fully executes the coroutine synchronously (immediately run all of its code till completion). | 
| `IEnumerator` | StripYields(this `IEnumerator` coroutine, `Boolean` onlyStripNulls = True, `Boolean` flatten = True) | Remove yields from the coroutine, making its code run immediately. | 


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
| `void` | FancyDestroy(this `GameObject` self, `Boolean` useDestroyImmediate = False, `Boolean` detachParent = False) | Destroy this GameObject. Safe to use on null objects. | 
| `AssignedAnotherWeights` | GetAaWeightsBody(this `ChaControl` ctrl) | Get value of the aaWeightsBody field | 
| `String` | GetFullPath(this `GameObject` self) | Get full GameObject "path" to this GameObject.  Example: RootObject\ChildObject1\ChildObject2 | 
| `String` | GetFullPath(this `Component` self) | Get full GameObject "path" to this GameObject.  Example: RootObject\ChildObject1\ChildObject2 | 
| `Transform` | GetTopmostParent(this `Component` src) | Get the topmost parent of Transform that this this Component is attached to. | 
| `Transform` | GetTopmostParent(this `GameObject` src) | Get the topmost parent of Transform that this this Component is attached to. | 
| `Transform` | GetTopmostParent(this `Transform` src) | Get the topmost parent of Transform that this this Component is attached to. | 
| `void` | MarkXuaIgnored(this `Component` target) | Mark GameObject of this Component as ignored by AutoTranslator. Prevents AutoTranslator from trying to translate custom UI elements. | 
| `ReadOnlyDictionary<TKey, TValue>` | ToReadOnlyDictionary(this `IDictionary<TKey, TValue>` original) | Wrap this dictionary in a read-only wrapper that will prevent any changes to it.  Warning: Any reference types inside the dictionary can still be modified. | 


## `HSceneUtils`

Utility methods for working with H Scenes / main game.
```csharp
public static class KKAPI.Utilities.HSceneUtils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Heroine` | GetLeadingHeroine(this `HFlag` hFlag) | Get the heroine that is currently in leading position in the h scene.  In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine. | 
| `Heroine` | GetLeadingHeroine(this `HSprite` hSprite) | Get the heroine that is currently in leading position in the h scene.  In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine. | 
| `Int32` | GetLeadingHeroineId(this `HFlag` hFlag) | Get ID of the heroine that is currently in leading position in the h scene. 0 is the main heroine, 1 is the "tag along".  In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine. | 
| `Int32` | GetLeadingHeroineId(this `HSprite` hSprite) | Get ID of the heroine that is currently in leading position in the h scene. 0 is the main heroine, 1 is the "tag along".  In 3P returns the heroine the cum options affect. Outside of 3P it gets the single heroine. | 
| `Boolean` | IsHoushi(this `HFlag` hFlag) | Is current h mode service? | 
| `Boolean` | IsSonyu(this `HFlag` hFlag) | Is current H mode penetration? | 


## `IMGUIUtils`

Utility methods for working with IMGUI / OnGui.
```csharp
public static class KKAPI.Utilities.IMGUIUtils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | DrawButtonWithShadow(`Rect` r, `GUIContent` content, `GUIStyle` style, `Single` shadowAlpha, `Vector2` direction) |  | 
| `void` | DrawLabelWithOutline(`Rect` rect, `String` text, `GUIStyle` style, `Color` txtColor, `Color` outlineColor, `Int32` outlineThickness) | Draw a label with an outline | 
| `void` | DrawLabelWithShadow(`Rect` rect, `GUIContent` content, `GUIStyle` style, `Color` txtColor, `Color` shadowColor, `Vector2` shadowOffset) | Draw a label with a shadow | 
| `Boolean` | DrawLayoutButtonWithShadow(`GUIContent` content, `GUIStyle` style, `Single` shadowAlpha, `Vector2` direction, `GUILayoutOption[]` options) |  | 
| `void` | DrawLayoutLabelWithShadow(`GUIContent` content, `GUIStyle` style, `Color` txtColor, `Color` shadowColor, `Vector2` direction, `GUILayoutOption[]` options) |  | 
| `void` | DrawSolidBox(`Rect` boxRect) | Draw a gray non-transparent GUI.Box at the specified rect. Use before a GUI.Window or other controls to get rid of  the default transparency and make the GUI easier to read.  <example>  IMGUIUtils.DrawSolidBox(screenRect);  GUILayout.Window(362, screenRect, TreeWindow, "Select character folder");  </example> | 
| `void` | EatInputInRect(`Rect` eatRect) | Block input from going through to the game/canvases if the mouse cursor is within the specified Rect.  Use after a GUI.Window call or the window will not be able to get the inputs either.  <example>  GUILayout.Window(362, screenRect, TreeWindow, "Select character folder");  Utils.EatInputInRect(screenRect);  </example> | 


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
| `IObservable<Unit>` | OnGUIAsObservable(this `Component` component) | Get an observable that triggers on every OnGUI call on this gameObject | 
| `IObservable<Unit>` | OnGUIAsObservable(this `Transform` transform) | Get an observable that triggers on every OnGUI call on this gameObject | 
| `IObservable<Unit>` | OnGUIAsObservable(this `GameObject` gameObject) | Get an observable that triggers on every OnGUI call on this gameObject | 


## `ObservableOnGUITrigger`

Trigger component that implements `KKAPI.Utilities.ObservableExtensions.OnGUIAsObservable(UnityEngine.Component)`
```csharp
public class KKAPI.Utilities.ObservableOnGUITrigger
    : ObservableTriggerBase

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `IObservable<Unit>` | OnGUIAsObservable() | Get observable that triggers every time this component's OnGUI is called | 
| `void` | RaiseOnCompletedOnDestroy() |  | 


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
| `void` | Show(`Action<String[]>` onAccept, `String` title, `String` initialDir, `String` filter, `String` defaultExt, `OpenSaveFileDialgueFlags` flags = OFN_FILEMUSTEXIST, OFN_EXPLORER, OFN_LONGNAMES) | Show windows file open dialog. Doesn't pause the game. | 
| `String[]` | ShowDialog(`String` title, `String` initialDir, `String` filter, `String` defaultExt, `OpenSaveFileDialgueFlags` flags, `IntPtr` owner) | Show windows file open dialog. Blocks the thread until user closes the dialog. Returns list of selected files, or null if user cancelled the action. | 


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


## `TextureUtils`

Utility methods for working with texture objects.
```csharp
public static class KKAPI.Utilities.TextureUtils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Texture2D` | LoadTexture(this `Byte[]` texBytes, `TextureFormat` format = ARGB32, `Boolean` mipMaps = False) | Create texture from an image stored in a byte array, for example a png file read from disk. | 
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


