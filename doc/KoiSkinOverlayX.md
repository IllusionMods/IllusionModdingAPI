## `TextureStorage`

A class for storing textures that should be saved and loaded from extended data's `ExtensibleSaveFormat.PluginData` (e.g. to character cards and scenes).  Duplicate textures are automatically handled so that only one copy of the texture is held in memory and saved.
```csharp
public class KoiSkinOverlayX.TextureStorage
    : IDisposable

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Clear(`Boolean` destroy = True) | Clear the texture list and optionally destroy all textures. | 
| `Int32[]` | GetAllTextureIDs() | Get IDs of all textures stored in this object. | 
| `Texture2D` | GetSharedTexture(`Int32` id) | Get a texture based on texture ID. The same texture is returned every time, so it shouldn't be destroyed. | 
| `void` | Load(`PluginData` data) | Load textures from extended data that were stored with `KoiSkinOverlayX.TextureStorage.Save(ExtensibleSaveFormat.PluginData)`. | 
| `void` | PurgeUnused(`IEnumerable<Int32>` usedIDs) | Remove unused textures based on a list of used IDs. Textures with IDs not in the list will be removed. | 
| `void` | Save(`PluginData` data) | Save textures stored in this object to extended data. Can be loaded later with `KoiSkinOverlayX.TextureStorage.Load(ExtensibleSaveFormat.PluginData)`. | 
| `Int32` | StoreTexture(`Byte[]` tex) | Store a texture and get an ID representing it. The ID can be used to get the texture with `KoiSkinOverlayX.TextureStorage.GetSharedTexture(System.Int32)`.  If you try to store a texture that was already stored before, the ID of the previous texture is returned so there are no multiple identical textures stored. | 
| `void` | System.IDisposable.Dispose() |  | 


