﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Adds a toggle to the bottom of the coordinate/clothes card load window in character maker.
    /// Use to allow user to not load data related to your mod.
    /// Use with <see cref="AddLoadToggle"/>
    /// </summary>
    [Obsolete("Not implemented")]
    public class MakerCoordinateLoadToggle : BaseEditableGuiEntry<bool>
    {
        private static readonly List<MakerCoordinateLoadToggle> Toggles = new List<MakerCoordinateLoadToggle>();

        /// <summary>
        /// Create a new coordinate load toggle. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/> 
        /// with <see cref="RegisterCustomControlsEvent.AddCoordinateLoadToggle"/>.
        /// </summary>
        /// <param name="text">Text displayed next to the checkbox</param>
        /// <param name="initialValue">Initial value of the toggle</param>
        public MakerCoordinateLoadToggle(string text, bool initialValue = true) : base(null, initialValue, null)
        {
            Text = text;
        }

        /// <summary>
        /// Text displayed next to the toggle
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Check if any of the custom toggles are checked
        /// </summary>
        public static bool AnyEnabled => Toggles.Any(x => x.Value);

        internal static void CreateCustomToggles()
        {
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform loadBoxTransform)
        {
            KoikatuAPI.Logger.LogWarning("MakerCoordinateLoadToggles are not implemented yet");
            Value = true;
            return null;
        }

        /// <inheritdoc />
        protected internal override void Initialize() { }

        internal static MakerCoordinateLoadToggle AddLoadToggle(MakerCoordinateLoadToggle toggle)
        {
            if (toggle == null) throw new ArgumentNullException(nameof(toggle));
            toggle.ThrowIfDisposed(nameof(toggle));

            Toggles.Add(toggle);
            return toggle;
        }

        internal static void Reset()
        {
            foreach (var toggle in Toggles)
                toggle.Dispose();
            Toggles.Clear();
        }

        internal static void Setup()
        {
        }
    }
}
