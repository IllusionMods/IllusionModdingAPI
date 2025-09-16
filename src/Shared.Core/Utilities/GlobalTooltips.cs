using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// API for easily displaying tooltips in game and studio.
    /// </summary>
    public static class GlobalTooltips
    {
        private const float TooltipDelay = 0.5f; // seconds

        private static readonly List<TooltipData> _tooltips = new List<TooltipData>();

        private static GUIStyle _tooltipStyle;
        private static GUIContent _tooltipContent;
        private static Texture2D _tooltipBackground;

        private static TooltipData _previouslyHovered;
        private static Tooltip _currentlyDisplayedTooltip;
        private static float _hoverStartTime;

        /// <summary>
        /// Show a tooltip when the user hovers over the target RectTransform.
        /// </summary>
        /// <param name="target">The RectTransform to attach the tooltip to.</param>
        /// <param name="text">The text to display in the tooltip.</param>
        /// <returns>A Tooltip object that can be used to update or remove the tooltip.</returns>
        public static Tooltip RegisterTooltip(RectTransform target, string text)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (text == null) throw new ArgumentNullException(nameof(text));
            text = text.Trim();
            if (string.IsNullOrEmpty(text)) throw new ArgumentException("Tooltip text cannot be empty or whitespace.", nameof(text));

            // Check if a tooltip is already registered for this target
            foreach (var existing in _tooltips)
            {
                if (existing.Target == target)
                {
                    KoikatuAPI.Logger.LogWarning($"A tooltip is already registered for the target {target.name}! Updating its text, may cause conflicts.\n" + new StackTrace());
                    existing.Tooltip.Text = text;
                    return existing.Tooltip;
                }
            }

            var tooltip = new Tooltip(target, text);
            var data = new TooltipData { Target = target, Tooltip = tooltip };
            _tooltips.Add(data);

            return tooltip;
        }

        internal static void Update()
        {
            if (_tooltipBackground == null)
            {
                _tooltipBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                _tooltipBackground.SetPixel(0, 0, new Color(0, 0, 0, 0.65f));
                _tooltipBackground.Apply();

                _tooltipStyle = new GUIStyle
                {
                    normal = new GUIStyleState { textColor = Color.white, background = _tooltipBackground },
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };
                _tooltipContent = new GUIContent();
            }

            UpdateTooltipSelection();
        }

        internal static void OnGUI()
        {
            DrawTooltip();
        }

        private static void UpdateTooltipSelection()
        {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y; // Convert to GUI coordinates

            TooltipData hovered = null;
            for (var i = 0; i < _tooltips.Count; i++)
            {
                var data = _tooltips[i];

                if (data.Target == null || data.Tooltip.IsDestroyed)
                {
                    _tooltips.RemoveAt(i);
                    i--;
                    continue;
                }

                if (!data.Target.gameObject.activeInHierarchy) continue;

                var rect = data.Target.GetScreenRect();
                if (rect.Contains(mousePos))
                {
                    hovered = data;
                    break;
                }
            }

            if (hovered != _previouslyHovered)
            {
                _previouslyHovered = hovered;
                _hoverStartTime = Time.realtimeSinceStartup;
            }

            if (_previouslyHovered != null && Time.realtimeSinceStartup - _hoverStartTime >= TooltipDelay)
                _currentlyDisplayedTooltip = _previouslyHovered.Tooltip;
            else
                _currentlyDisplayedTooltip = null;
        }

        private static void DrawTooltip()
        {
            if (_currentlyDisplayedTooltip == null) return;

            // Calculate tooltip size
            _tooltipContent.text = _currentlyDisplayedTooltip.Text;
            _tooltipStyle.CalcMinMaxWidth(_tooltipContent, out var minWidth, out var maxWidth);
            const int margin = 10;
            var width = Mathf.Clamp((int)maxWidth + margin,
                                    Mathf.Max(_currentlyDisplayedTooltip.MinWidth, (int)minWidth),
                                    Mathf.Min(_currentlyDisplayedTooltip.MaxWidth, Screen.width / 3));
            var height = _tooltipStyle.CalcHeight(_tooltipContent, width) + margin;

            // Calculate tooltip position
            var mousePosition = Event.current.mousePosition;
            var x = mousePosition.x + width > Screen.width ? Screen.width - width : mousePosition.x;
            var y = mousePosition.y + 25 + height > Screen.height ? mousePosition.y - height : mousePosition.y + 25;

            GUI.Box(new Rect(x, y, width, height), _tooltipContent, _tooltipStyle);
        }

        private sealed class TooltipData
        {
            public RectTransform Target;
            public Tooltip Tooltip;
        }

        /// <summary>
        /// Represents a registered tooltip. Allows updating the text and destroying the tooltip.
        /// </summary>
        public sealed class Tooltip : IDisposable
        {
            /// <summary>
            /// Creates a new Tooltip instance.
            /// </summary>
            /// <param name="target">The RectTransform to attach to.</param>
            /// <param name="text">The tooltip text.</param>
            internal Tooltip(RectTransform target, string text)
            {
                Target = target;
                Text = text;
            }

            /// <summary>
            /// The RectTransform this tooltip is attached to.
            /// </summary>
            public RectTransform Target { get; }

            /// <summary>
            /// Whether this tooltip has been destroyed.
            /// </summary>
            public bool IsDestroyed { get; private set; }

            /// <summary>
            /// The text displayed in the tooltip.
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Maximum width of the tooltip in pixels. Default is uncapped.
            /// </summary>
            public int MaxWidth { get; set; } = int.MaxValue;

            /// <summary>
            /// Minimum width of the tooltip in pixels. Default is uncapped.
            /// </summary>
            public int MinWidth { get; set; } = 0;

            /// <inheritdoc />
            void IDisposable.Dispose()
            {
                Destroy();
            }

            /// <summary>
            /// Remove the tooltip from existence. The tooltip cannot be used after this and has to be recreated.
            /// </summary>
            public void Destroy()
            {
                IsDestroyed = true;
            }
        }
    }
}
