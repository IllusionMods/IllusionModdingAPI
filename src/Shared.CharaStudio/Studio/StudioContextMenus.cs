using KKAPI.Utilities;
using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KKAPI.Studio
{
    /// <summary>
    /// Provides functionality for adding custom context menu items to studio.
    /// </summary>
    public static class StudioContextMenus
    {
        #region Private implementation

        private sealed class MenuItem : IDisposable
        {
            public StudioContextMenuOrder Order { get; }
            public GlobalContextMenu.Entry Entry { get; }

            public MenuItem(StudioContextMenuOrder order, GlobalContextMenu.Entry entry)
            {
                Order = order;
                Entry = entry;
            }

            public void Dispose()
            {
                _WorkspaceMenuItems.Remove(this);
                _workspaceMenuItemsDirty = true;
            }
        }

        private static readonly List<MenuItem> _WorkspaceMenuItems = new List<MenuItem>();
        private static GlobalContextMenu.Entry[] _workspaceMenuItemsSorted;
        private static bool _workspaceMenuItemsDirty = true;
        private static WorkspaceNodeClickedEventArgs _workspaceCurrentArgs;

        internal static void OnShowWorkspaceContextMenu(TreeNodeObject clickedInstance)
        {
            if (!clickedInstance) return;

            var args = new WorkspaceNodeClickedEventArgs(clickedInstance);
            // If the right clicked node is not in selected list, select it
            if (!args.SelectedInstances.Contains(clickedInstance))
            {
#if PH
                StudioAPI.StudioInstance.treeNodeCtrl.AddSelectNode(clickedInstance);
#else
                StudioAPI.StudioInstance.treeNodeCtrl.AddSelectNode(clickedInstance, false);
#endif
                args = new WorkspaceNodeClickedEventArgs(clickedInstance);
            }

            if (args.SelectedInstances.Count == 0) return;

            _workspaceCurrentArgs = args;

            if (_workspaceMenuItemsDirty)
            {
                // Sort the menu items by their order
                _WorkspaceMenuItems.Sort((item1, item2) => item1.Order.CompareTo(item2.Order));

                // Add separators between groups of menu items with different orders
                var results = new List<GlobalContextMenu.Entry>();
                var previous = (StudioContextMenuOrder)int.MinValue;
                foreach (var menuItem in _WorkspaceMenuItems)
                {
                    if (previous != menuItem.Order && results.Count > 0)
                    {
                        previous = menuItem.Order;
                        results.Add(GlobalContextMenu.Entry.Separator);
                    }

                    results.Add(menuItem.Entry);
                }

                _workspaceMenuItemsSorted = results.ToArray();

                _workspaceMenuItemsDirty = false;
            }

            // BUG: Title bar gets cut off
            var title = args.SelectedInstances.Count <= 1 ? clickedInstance.textName : $"{clickedInstance.textName} + {args.SelectedInstances.Count(x => x != clickedInstance)} selected";
            GlobalContextMenu.Show(title, _workspaceMenuItemsSorted);
        }

        internal static void Init()
        {
            RegisterBasicTreeNodeCommands();
        }

        private static void RegisterBasicTreeNodeCommands()
        {
            // Select children
            AddWorkspaceContextMenuItem(
                "Select children",
                args =>
                {
                    var allNodes = args.SelectedInstances.SelectMany(x => x.Flatten()).ToList();
                    var notSelected = allNodes.Except(args.SelectedInstances).ToList();
                    foreach (var tno in notSelected)
#if !PH
                        StudioAPI.StudioInstance.treeNodeCtrl.AddSelectNode(tno, true);
#else
                        StudioAPI.StudioInstance.treeNodeCtrl.AddSelectNode(tno);
#endif
                },
                StudioContextMenuOrder.Selection,
                args =>
                {
                    var allNodes = args.SelectedInstances.SelectMany(x => x.Flatten()).ToList();
                    var notSelected = allNodes.Except(args.SelectedInstances).ToList();
                    return notSelected.Count > 0 ? GlobalContextMenu.Entry.EntryState.Normal : GlobalContextMenu.Entry.EntryState.Hidden;
                });

            // Parent
            AddWorkspaceContextMenuItem(
                "Parent",
                args => StudioAPI.StudioInstance.m_WorkspaceCtrl.OnClickParent(),
                StudioContextMenuOrder.NodeEdit,
                args => args.SelectedInstances.Count < 2
                    ? GlobalContextMenu.Entry.EntryState.Hidden
                    : args.SelectedInstances.Any(x => x.enableChangeParent)
                        ? GlobalContextMenu.Entry.EntryState.Normal
                        : GlobalContextMenu.Entry.EntryState.Disabled);

            // Unparent
            AddWorkspaceContextMenuItem(
                "Unparent",
                args => StudioAPI.StudioInstance.m_WorkspaceCtrl.OnClickRemove(),
                StudioContextMenuOrder.NodeEdit,
                args => args.SelectedInstances.Any(x => x.isParent) ? GlobalContextMenu.Entry.EntryState.Normal : GlobalContextMenu.Entry.EntryState.Hidden);

            // Delete
            AddWorkspaceContextMenuItem(
                "Delete",
                args => StudioAPI.StudioInstance.m_WorkspaceCtrl.OnClickDelete(),
                StudioContextMenuOrder.NodeEdit,
                args => args.SelectedInstances.Any(x => x.enableDelete) ? GlobalContextMenu.Entry.EntryState.Normal : GlobalContextMenu.Entry.EntryState.Disabled);

            // Duplicate
            AddWorkspaceContextMenuItem(
                "Duplicate",
                args => StudioAPI.StudioInstance.m_WorkspaceCtrl.OnClickDuplicate(),
                StudioContextMenuOrder.NodeEdit,
                args => args.SelectedInstances.Any(x => x.enableCopy) ? GlobalContextMenu.Entry.EntryState.Normal : GlobalContextMenu.Entry.EntryState.Disabled);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Add a menu item to the workspace context menu.
        /// </summary>
        /// <param name="content">Content to display in the menu</param>
        /// <param name="onClick">Action to perform when the menu item is clicked</param>
        /// <param name="order">Where to place this item in the menu. Default is <see cref="StudioContextMenuOrder.Default"/></param>
        /// <param name="checkState">Optional condition to determine if the menu item should be visible. If null, the item is always visible.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to remove the menu item when disposed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> or <paramref name="onClick"/> is null.</exception>
        /// <example>
        /// Simple menu item:
        /// <code>
        /// StudioContextMenus.AddWorkspaceContextMenuItem(
        ///     "My Action",
        ///     args => DoSomething(args.ClickedInstance),
        ///     StudioContextMenuOrder.Actions);
        /// </code>
        /// Conditional menu item (only show for character nodes):
        /// <code>
        /// StudioContextMenus.AddWorkspaceContextMenuItem(
        ///     "Edit Character",
        ///     args => EditCharacter(args.ClickedInstance),
        ///     StudioContextMenuOrder.SuggestedActions,
        ///     args => args.GetSelectedObjects().OfType&lt;OCIChar&gt;().Any());
        /// </code>
        /// </example>
        public static IDisposable AddWorkspaceContextMenuItem(
            string content,
            Action<WorkspaceNodeClickedEventArgs> onClick,
            StudioContextMenuOrder order = StudioContextMenuOrder.Default,
            Func<WorkspaceNodeClickedEventArgs, GlobalContextMenu.Entry.EntryState> checkState = null) => AddWorkspaceContextMenuItem(new GUIContent(content), onClick, order, checkState);

        /// <inheritdoc cref="AddWorkspaceContextMenuItem(string, Action{WorkspaceNodeClickedEventArgs}, StudioContextMenuOrder, Func{WorkspaceNodeClickedEventArgs, GlobalContextMenu.Entry.EntryState})"/>
        public static IDisposable AddWorkspaceContextMenuItem(
            GUIContent content,
            Action<WorkspaceNodeClickedEventArgs> onClick,
            StudioContextMenuOrder order = StudioContextMenuOrder.Default,
            Func<WorkspaceNodeClickedEventArgs, GlobalContextMenu.Entry.EntryState> checkState = null)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (onClick == null) throw new ArgumentNullException(nameof(onClick));

            var entry = GlobalContextMenu.Entry.Create(content,
                                                       () => onClick(_workspaceCurrentArgs),
                                                       checkState != null ? () => checkState(_workspaceCurrentArgs) : (Func<GlobalContextMenu.Entry.EntryState>)null);
            return AddWorkspaceContextMenuItem(entry, order);
        }

        /// <inheritdoc cref="AddWorkspaceContextMenuItem(string, Action{WorkspaceNodeClickedEventArgs}, StudioContextMenuOrder, Func{WorkspaceNodeClickedEventArgs, GlobalContextMenu.Entry.EntryState})"/>
        public static IDisposable AddWorkspaceContextMenuItem(GlobalContextMenu.Entry content, StudioContextMenuOrder order)
        {
            var item = new MenuItem(order, content);
            _WorkspaceMenuItems.Add(item);
            _workspaceMenuItemsDirty = true;
            return item;
        }

        #endregion
    }
}
