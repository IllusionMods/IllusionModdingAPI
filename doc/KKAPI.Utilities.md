## `CoroutineUtils`

Utility methods for working with coroutines.
```csharp
public static class KKAPI.Utilities.CoroutineUtils

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `IEnumerator` | AppendCo(this `IEnumerator` baseCoroutine, `IEnumerator` appendCoroutine) | Create a coroutine that calls the appendCoroutine after baseCoroutine finishes | 
| `IEnumerator` | AppendCo(this `IEnumerator` baseCoroutine, `YieldInstruction` yieldInstruction) | Create a coroutine that calls the appendCoroutine after baseCoroutine finishes | 
| `IEnumerator` | AppendCo(this `IEnumerator` baseCoroutine, `Action[]` actions) | Create a coroutine that calls the appendCoroutine after baseCoroutine finishes | 
| `IEnumerator` | ComposeCoroutine(`IEnumerator[]` coroutine) | Create a coroutine that calls each of the supplied coroutines in order. | 
| `IEnumerator` | CreateCoroutine(`Action[]` actions) | Create a coroutine that calls each of the action delegates on consecutive frames.  One action is called per frame. First action is called right away. There is no frame skip after the last action. | 


## `Extensions`

General utility extensions that don't fit in other categories.
```csharp
public static class KKAPI.Utilities.Extensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | ActuallyRemoveAllListeners(this `UnityEventBase` evt) | Same as RemoveAllListeners but also disables all PersistentListeners.  To avoid frustration always use this instead of RemoveAllListeners, unless you want to keep the PersistentListeners. | 
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
| `void` | DrawSolidBox(`Rect` boxRect) | Draw a gray non-transparent GUI.Box at the specified rect. Use before a window or other controls to get rid of  the default transparency and make the GUI easier to read. | 


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


## `ThreadingHelper`

Provides methods for running code on other threads and synchronizing with the main thread.
```csharp
public class KKAPI.Utilities.ThreadingHelper
    : MonoBehaviour

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | StartAsyncInvoke(`Func<Action>` action) | Queue the delegate to be invoked on a background thread. Use this to run slow tasks without affecting the game.  NOTE: Most of Unity API can not be accessed while running on another thread! | 
| `void` | StartSyncInvoke(`Action` action) | Queue the delegate to be invoked on the main unity thread. Use to synchronize your threads. | 


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


