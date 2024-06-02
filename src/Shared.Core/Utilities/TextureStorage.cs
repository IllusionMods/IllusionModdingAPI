using System;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace KoiSkinOverlayX
{
    /// <summary>
    /// A class for storing textures that should be saved and loaded from extended data's <see cref="PluginData"/> (e.g. to character cards and scenes).
    /// Duplicate textures are automatically handled so that only one copy of the texture is held in memory and saved.
    /// </summary>
    public class TextureStorage : IDisposable
    {
        // Do not change or it will break stuff that used this marker previously
        private const string DataMarker = "_TextureID_";

        private readonly Dictionary<int, TextureHolder> _data = new Dictionary<int, TextureHolder>();
        private readonly TextureFormat _format;

        /// <summary>
        /// Create a new TextureStorage.
        /// </summary>
        /// <param name="format">Format of the loaded textures. It doesn't affect data saved to extended data.</param>
        public TextureStorage(TextureFormat format = TextureFormat.ARGB32)
        {
            _format = format;
        }

        void IDisposable.Dispose()
        {
            lock (_data)
            {
                foreach (var tex in _data) tex.Value?.Dispose();
                _data.Clear();
            }
        }

        /// <summary>
        /// Remove unused textures based on a list of used IDs. Textures with IDs not in the list will be removed.
        /// </summary>
        /// <param name="usedIDs">A list of IDs to be kept if they exist</param>
        public void PurgeUnused(IEnumerable<int> usedIDs)
        {
            if (usedIDs == null) throw new ArgumentNullException(nameof(usedIDs));
            var lookup = new HashSet<int>(usedIDs);

            lock (_data)
            {
                foreach (var kvp in _data.ToList())
                {
                    var contains = lookup.Contains(kvp.Key);
                    if (!contains || kvp.Value?.Data == null)
                    {
                        Console.WriteLine($"Removing {(contains ? "empty" : "unused")} texture with ID {kvp.Key}");
                        kvp.Value?.Dispose();
                        _data.Remove(kvp.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Get IDs of all textures stored in this object.
        /// </summary>
        public int[] GetAllTextureIDs()
        {
            lock (_data)
            {
                return _data.Keys.ToArray();
            }
        }

        /// <summary>
        /// Clear the texture list and optionally destroy all textures.
        /// </summary>
        public void Clear(bool destroy = true)
        {
            lock (_data)
            {
                if (destroy)
                    ((IDisposable)this).Dispose();
                else
                    _data.Clear();
            }
        }

        /// <summary>
        /// Load textures from extended data that were stored with <see cref="Save"/>.
        /// </summary>
        public void Load(PluginData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            lock (_data)
            {
                foreach (var dataPair in data.data.Where(x => x.Key.StartsWith(DataMarker)))
                {
                    var idStr = dataPair.Key.Substring(DataMarker.Length);
                    if (!int.TryParse(idStr, out var id))
                    {
                        KoikatuAPI.Logger.LogDebug($"Invalid ID {idStr} in key {dataPair.Key}");
                        continue;
                    }

                    var value = dataPair.Value as byte[];
                    if (value == null && dataPair.Value != null)
                    {
                        KoikatuAPI.Logger.LogDebug($"Invalid value of ID {id}. Should be of type byte[] but is {dataPair.Value.GetType()}");
                        continue;
                    }

                    _data[id] = new TextureHolder(value, _format);
                }
            }
        }

        /// <summary>
        /// Save textures stored in this object to extended data. Can be loaded later with <see cref="Load"/>.
        /// </summary>
        public void Save(PluginData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            lock (_data)
            {
                foreach (var tex in _data)
                {
                    if (tex.Value == null) continue;
                    data.data[DataMarker + tex.Key] = tex.Value.Data;
                }
            }
        }

        /// <summary>
        /// Store a texture and get an ID representing it. The ID can be used to get the texture with <see cref="GetSharedTexture"/>.
        /// If you try to store a texture that was already stored before, the ID of the previous texture is returned so there are no multiple identical textures stored.
        /// </summary>
        /// <param name="tex">Raw PNG data of the texture. If you reuse a texture make sure you always use the same PNG data or deduplicating won't work.</param>
        public int StoreTexture(byte[] tex)
        {
            if (tex == null) throw new ArgumentNullException(nameof(tex));
            lock (_data)
            {
                var existing = _data.FirstOrDefault(x => x.Value != null && x.Value.Data.SequenceEqualFast(tex));
                if (existing.Value != null)
                {
                    Console.WriteLine("StoreTexture - Texture already exists, reusing it");
                    return existing.Key;
                }

                // Use random ID instaed of sequential to help catch code using IDs that no longer exist
                for (var i = Random.Range(1000, 9990); ; i++)
                {
                    if (!_data.ContainsKey(i))
                    {
                        _data[i] = new TextureHolder(tex, _format);
                        return i;
                    }
                }
            }
        }

        /* todo remove? very slow and potentially not very useful
        public int StoreTexture(Texture2D tex)
        {
            if (tex == null) throw new ArgumentNullException(nameof(tex));
            var rawTextureData = tex.GetRawTextureData();
            lock (_data)
            {
                var existing = _data.FirstOrDefault(x =>
                    x.Value != null && x.Value.Texture.GetRawTextureData().SequenceEqual(rawTextureData));
                if (existing.Value != null) return existing.Key;
                return StoreTexture(tex.EncodeToPNG());
            }
        }*/

        /// <summary>
        /// Get a texture based on texture ID. The same texture is returned every time, so it shouldn't be destroyed.
        /// </summary>
        /// <param name="id">ID of the texture you want to get. You get the ID when using <see cref="StoreTexture"/>.</param>
        /// <returns></returns>
        public Texture2D GetSharedTexture(int id)
        {
            lock (_data)
            {
                if (_data.TryGetValue(id, out var data))
                {
                    if (data == null)
                        return null;
                    if (data.Texture.IsDestroyed())
                        KoikatuAPI.Logger.LogDebug($"Texture ID={id} from TextureStorage was destroyed, recreating");
                    return data.Texture;
                }
            }

            KoikatuAPI.Logger.LogWarning("Tried getting texture with nonexisting ID: " + id);
            return null;
        }

        private sealed class TextureHolder : IDisposable
        {
            private readonly TextureFormat _format;
            private byte[] _data;
            private Texture2D _texture;

            public TextureHolder(byte[] data, TextureFormat format)
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                _format = format;
            }

            public byte[] Data
            {
                get => _data;
                set
                {
                    Dispose();
                    _data = value;
                }
            }

            public Texture2D Texture
            {
                get
                {
                    if (_texture == null && _data != null)
                        _texture = _data.LoadTexture(_format);
                    return _texture;
                }
            }

            public void Dispose()
            {
                if (_texture != null)
                {
                    Object.Destroy(_texture);
                    _texture = null;
                }
            }
        }
    }
}