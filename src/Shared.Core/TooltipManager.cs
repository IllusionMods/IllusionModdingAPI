using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace KKAPI
{
    /// <summary>
    /// API for displaying tooltips in game.
    /// </summary>
    public static class TooltipManager
    {
        private const float TooltipDelay = 0.5f; // seconds

        private static readonly List<TooltipData> _tooltips = new List<TooltipData>();

        private static GUIStyle _tooltipStyle;
        private static GUIContent _tooltipContent;
        private static Texture2D _tooltipBackground;

        private static TooltipData _currentTooltip;
        private static string _currentTooltipText;
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
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

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

        internal static void Update() => UpdateTooltipSelection();
        internal static void OnGUI() => DrawTooltip();

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

                if (data.Tooltip == null) continue;
                if (!data.Target.gameObject.activeInHierarchy) continue;

                var rect = GetScreenRect(data.Target);
                if (rect.Contains(mousePos))
                {
                    hovered = data;
                    break;
                }
            }

            if (hovered != _currentTooltip)
            {
                _currentTooltip = hovered;
                _hoverStartTime = Time.realtimeSinceStartup;
            }

            if (_currentTooltip != null && Time.realtimeSinceStartup - _hoverStartTime >= TooltipDelay)
                _currentTooltipText = _currentTooltip.Tooltip.Text;
            else
                _currentTooltipText = null;
        }

        private static Rect GetScreenRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            var xMin = corners[0].x;
            var yMin = Screen.height - corners[1].y;
            var width = corners[2].x - corners[0].x;
            var height = corners[1].y - corners[0].y;
            return new Rect(xMin, yMin, width, height);
        }

        private static void DrawTooltip()
        {
            if (string.IsNullOrEmpty(_currentTooltipText)) return;

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

            _tooltipContent.text = _currentTooltipText;

            _tooltipStyle.CalcMinMaxWidth(_tooltipContent, out var minWidth, out var width);

            var tooltipWidth = Mathf.Min((int)width + 10, Screen.width / 3);

            var height = _tooltipStyle.CalcHeight(_tooltipContent, tooltipWidth) + 10;
            var currentEvent = Event.current;

            var x = currentEvent.mousePosition.x + tooltipWidth > Screen.width
                ? Screen.width - tooltipWidth
                : currentEvent.mousePosition.x;

            var y = currentEvent.mousePosition.y + 25 + height > Screen.height
                ? currentEvent.mousePosition.y - height
                : currentEvent.mousePosition.y + 25;

            GUI.Box(new Rect(x, y, tooltipWidth, height), _currentTooltipText, _tooltipStyle);
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

            /// <inheritdoc />
            void IDisposable.Dispose()
            {
                Destroy();
            }

            /// <summary>
            /// Remove the tooltip from existence.
            /// </summary>
            public void Destroy()
            {
                IsDestroyed = true;
            }
        }
    }
}
