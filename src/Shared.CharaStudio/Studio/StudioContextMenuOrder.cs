namespace KKAPI.Studio
{
    /// <summary>
    /// Defines the order in which menu items appear in right click context menus.
    /// Menu items with the same StudioContextMenuOrder value are grouped together. Different groups are separated by a separator.
    /// Any custom value between -100 and 100 can be used. Lower values are called first and appear higher in the menu.
    /// </summary>
    public enum StudioContextMenuOrder
    {
        /// <summary>
        /// Always be at the top of the menu, above all other items. Use only if absolutely necessary.
        /// </summary>
        Topmost = -100,
        /// <summary>
        /// Actions that are most likely to be used by the user, e.g. "Edit text..." on a text node.
        /// </summary>
        SuggestedActions = -70,
        /// <summary>
        /// Actions to perform on the node, e.g. "Set Vanilla+ shaders".
        /// </summary>
        Actions = -50,
        /// <summary>
        /// Default order for menu items, consider using a different value. Appears in the middle of the menu.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Commands that change selection.
        /// </summary>
        Selection = 40,
        /// <summary>
        /// Basic node editing like deleting or duplicating.
        /// </summary>
        NodeEdit = 50,
        /// <summary>
        /// Opening various properties windows.
        /// </summary>
        Properties = 80,
        /// <summary>
        /// Always be at the bottom of the menu, below all other items. Use only if absolutely necessary.
        /// </summary>
        Bottommost = 100
    }
}
