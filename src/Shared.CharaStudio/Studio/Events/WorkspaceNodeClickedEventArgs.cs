using System;
using System.Collections.Generic;
using Studio;

#pragma warning disable 1591

namespace KKAPI.Studio
{
    /// <summary>
    /// Event arguments for workspace tree node context menu events.
    /// </summary>
    public sealed class WorkspaceNodeClickedEventArgs : EventArgs
    {
        /// <summary>
        /// The tree node that was right-clicked to open the context menu.
        /// </summary>
        public TreeNodeObject ClickedInstance { get; }

        /// <summary>
        /// All tree nodes currently selected in the workspace.
        /// </summary>
        public ICollection<TreeNodeObject> SelectedInstances { get; }

        /// <summary>
        /// Get all objects currently selected in the workspace.
        /// </summary>
        public IEnumerable<ObjectCtrlInfo> GetSelectedObjects() => StudioAPI.GetSelectedObjects();

        public WorkspaceNodeClickedEventArgs(TreeNodeObject clickedInstance)
        {
            ClickedInstance = clickedInstance;
            SelectedInstances = StudioAPI.GetSelectedTreeNodes();
        }
    }
}
