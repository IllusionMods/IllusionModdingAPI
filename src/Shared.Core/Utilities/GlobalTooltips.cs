using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KKAPI.Utilities
{
    /// <summary>
    /// API for easily displaying tooltips in game and studio.
    /// </summary>
    public static class GlobalTooltips
    {
        private const float TooltipDelay = 0.5f; // seconds

        private static readonly Dictionary<GameObject, Tooltip> _tooltips = new Dictionary<GameObject, Tooltip>();

        private static GUIStyle _tooltipStyle;
        private static GUIContent _tooltipContent;
        private static Texture2D _tooltipBackground;

        private static Tooltip _previouslyHovered;
        private static Tooltip _currentlyDisplayed;
        private static float _hoverStartTime;
        private static Vector2 _mousePosition;

        /// <inheritdoc cref="RegisterTooltip(UnityEngine.GameObject,Tooltip)"/>
        /// <param name="target">GameObject to attach the tooltip to.</param>
        /// <param name="text">The text to display in the tooltip.</param>
        /// <returns>A Tooltip object that can be used to update or remove the tooltip.</returns>
        public static Tooltip RegisterTooltip(GameObject target, string text)
        {
            var tooltip = new Tooltip(text);
            RegisterTooltip(target, tooltip);
            return tooltip;
        }

        /// <summary>
        /// Show a tooltip when user hovers over the target.
        /// Only works for objects that block mouse raycasts (e.g. UI elements, move gizmos).
        /// Tooltip does not show if the object is obstructed by another blocking object or by an IMGUI window.
        /// </summary>
        /// <param name="target">GameObject to attach the tooltip to.</param>
        /// <param name="tooltip">The tooltip to attach.</param>
        public static void RegisterTooltip(GameObject target, Tooltip tooltip)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (tooltip == null) throw new ArgumentNullException(nameof(tooltip));

            if (_tooltips.ContainsKey(target))
                KoikatuAPI.Logger.LogWarning($"A tooltip is already registered for {target.name} - it will be replaced!" + (KoikatuAPI.EnableDebugLogging ? "\n" + new StackTrace() : null));

            _tooltips[target] = tooltip;
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
            var current = EventSystem.current;
            if (current == null) return;

            Tooltip hovered = null;

            // Check if mouse is over any IMGUI window or GUI control that takes mouse input, don't show tooltips in that case
            if (!GUIUtility.mouseUsed)
            {
                // Find out what the mouse is currently over
                // Use PointerInputModule for best compatibility, AI/HS2 have their own implementation that inherits from it
                var im = (PointerInputModule)current.currentInputModule;
                im.GetPointerData(-1, out var pointerEventData, false);

                if (!ReferenceEquals(pointerEventData?.pointerEnter, null))
                {
                    _mousePosition = pointerEventData.position;
                    if (_tooltips.TryGetValue(pointerEventData.pointerEnter, out hovered) && hovered.IsDestroyed)
                    {
                        _tooltips.Remove(pointerEventData.pointerEnter);
                        hovered = null;
                    }
                }
            }

            if (hovered != _previouslyHovered)
            {
                _previouslyHovered = hovered;
                _hoverStartTime = Time.realtimeSinceStartup;
            }

            if (_previouslyHovered != null && Time.realtimeSinceStartup - _hoverStartTime >= TooltipDelay)
                _currentlyDisplayed = _previouslyHovered;
            else
                _currentlyDisplayed = null;
        }

        private static void DrawTooltip()
        {
            if (_currentlyDisplayed == null) return;

            // Calculate tooltip size
            _tooltipContent.text = _currentlyDisplayed.Text;
            _tooltipStyle.CalcMinMaxWidth(_tooltipContent, out var minWidth, out var maxWidth);
            const int margin = 10;
            var width = Mathf.Clamp((int)maxWidth + margin,
                                    Mathf.Max(_currentlyDisplayed.MinWidth, (int)minWidth),
                                    Mathf.Min(_currentlyDisplayed.MaxWidth, Screen.width / 3));
            var height = _tooltipStyle.CalcHeight(_tooltipContent, width) + margin;

            // Calculate tooltip position
            // Event.current.mousePosition doesn't work correctly in AI/HS2, it's not updated and has weird origin point
            var mousePosition = _mousePosition;
            mousePosition.y = Screen.height - mousePosition.y;
            var x = mousePosition.x + width > Screen.width ? Screen.width - width : mousePosition.x;
            var y = mousePosition.y + 25 + height > Screen.height ? mousePosition.y - 2 - height : mousePosition.y + 25;

            GUI.Box(new Rect(x, y, width, height), _tooltipContent, _tooltipStyle);
        }

        /// <summary>
        /// Information required to show a tooltip. Can be destroyed to unregister it.
        /// </summary>
        public sealed class Tooltip : IDisposable
        {
            private string _text;

            /// <summary>
            /// Creates a new Tooltip instance.
            /// </summary>
            /// <param name="text">The tooltip text.</param>
            public Tooltip(string text)
            {
                Text = text ?? throw new ArgumentNullException(nameof(text));
            }

            /// <summary>
            /// Whether this tooltip has been destroyed.
            /// </summary>
            public bool IsDestroyed { get; private set; }

            /// <summary>
            /// The text displayed in the tooltip.
            /// </summary>
            public string Text
            {
                get => _text;
                set => _text = value ?? "";
            }

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
                Unregister();
            }

            /// <summary>
            /// Remove the tooltip from existence. The tooltip cannot be used after this and has to be recreated.
            /// </summary>
            public void Unregister()
            {
                IsDestroyed = true;
            }
        }
    }
}
