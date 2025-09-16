using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Studio.UI.Toolbars
{
    /// <summary>
    /// Wrapper for stock game toolbar buttons to allow unified handling.
    /// </summary>
    internal sealed class ToolbarControlAdapter : ToolbarControlBase
    {
        public ToolbarControlAdapter(Button btnObject) : base(btnObject.gameObject.name.Replace("Button ", ""), TryGetTooltip(btnObject.gameObject.name), () => null, KoikatuAPI.Instance)
        {
            ButtonObject.OnNext(btnObject);
            RectTransform = (RectTransform)btnObject.transform;
            GetActualPosition(out var row, out var col);
            DesiredRow = row;
            DesiredColumn = col;

            // BUG: These don't update if the button is changed by game code
            Interactable.OnNext(btnObject.interactable);
            Interactable.Subscribe(b => btnObject.interactable = b);
            Visible.OnNext(btnObject.gameObject.activeSelf);
            Visible.Subscribe(b =>
            {
                if (btnObject.gameObject.activeSelf != b)
                {
                    btnObject.gameObject.SetActive(b);
                    ToolbarManager.RequestToolbarRelayout();
                }
            });

            DragHelper.SetUpDragging(this, btnObject.gameObject);
        }

        private static string TryGetTooltip(string originalName)
        {
            _BaseGameTooltips.TryGetValue(originalName, out var value);
            return value;
        }

        private static readonly Dictionary<string, string> _BaseGameTooltips = new Dictionary<string, string>
        {
            // "Button Target" is not implemented so no tooltip
            {"Button Camera", "Switch between free and locked camera.\nTo adjust the free camera hold Left/Right/Both mouse buttons and move."},
            {"Button Center", "Toggle showing the camera center point when moving the camera."},
            {"Button Object", "Open Move Controller.\nA tool for making fine adjustments to object positions and rotations."},
            {"Button Map", "Open Map Controller.\nA tool for moving and rotating the currently set Map.\nIt can change time of day if the map supports this feature.\nOnly works with maps added through the 'add -> Map' menu."},
            //TODO What does this do exactly? Only in KKS. {"Button Gimmick", "Toggle display of some gimmick gizmos."},
            {"Button Axis", "Toggle display of movement/rotation gizmo and\nselection circles if objects are set to 'Show all'.\nHotkey: Q (press W / E / R to switch modes)"},
            {"Button Axis Trans", "Toggle display of translation gizmos\nin the movement/rotation gizmo.\nHotkey: J"},
            {"Button Axis Center", "Toggle display of the origin point\nof the currently selected object.\nHotkey: K"},
            {"Button Undo", "Undo last action."},
            {"Button Redo", "Redo last undone action."},
        };

        /// <inheritdoc />
        protected internal override void CreateControl() { }
        /// <inheritdoc />
        public override void Dispose() { }
    }
}
