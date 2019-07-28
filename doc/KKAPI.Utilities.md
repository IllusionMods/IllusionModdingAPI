## `Extensions`

```csharp
public static class KKAPI.Utilities.Extensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `ReadOnlyDictionary<TKey, TValue>` | ToReadOnlyDictionary(this `IDictionary<TKey, TValue>` original) | Wrap this dictionary in a read-only wrapper that will prevent any changes to it.  Warning: Any reference types inside the dictionary can still be modified. | 


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


