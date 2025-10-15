using System;
using System.Runtime.InteropServices;
using System.Text;

namespace KKAPI.Utilities
{
    public class SystemFileDialog
    {
        /// <summary>
        /// Specifies options for the file open and save dialogs.
        /// This enumeration is used to modify the behavior and appearance of the dialogs.
        /// </summary>
        [Flags]
        public enum FOS : uint
        {
            /// <summary>
            /// Prompts the user for confirmation if attempting to overwrite an existing file.
            /// </summary>
            OVERWRITEPROMPT = 0x00000002,
            /// <summary>
            /// Specifies that the file dialog restricts the selectable file types
            /// strictly to those explicitly specified by the filter.
            /// </summary>
            STRICTFILETYPES = 0x00000004,
            /// <summary>
            /// Prevents the file dialog from changing the current working directory to the directory containing the selected file.
            /// </summary>
            NOCHANGEDIR = 0x00000008,
            /// <summary>
            /// Configures the file dialog to allow users to select folders instead of files.
            /// </summary>
            PICKFOLDERS = 0x00000020,
            /// <summary>
            /// Ensures that only file system items are displayed in the dialog, even if the default behavior allows non-file system items.
            /// </summary>
            FORCEFILESYSTEM = 0x00000040,
            /// <summary>
            /// Includes all non-storage items, such as virtual or non-file objects, in the selection scope.
            /// </summary>
            ALLNONSTORAGEITEMS = 0x00000080,
            /// <summary>
            /// Disables file or folder validation. Allows the user to select items that may not pass standard validation checks.
            /// </summary>
            NOVALIDATE = 0x00000100,
            /// <summary>
            /// Allows the selection of multiple files or folders in the dialog.
            /// </summary>
            ALLOWMULTISELECT = 0x00000200,
            /// <summary>
            /// Ensures that the specified path exists before allowing the operation to proceed.
            /// </summary>
            PATHMUSTEXIST = 0x00000800,
            /// <summary>
            /// Ensures that the file selected by the user in the dialog box exists.
            /// If the specified file is not found, the dialog box will display an error message.
            /// </summary>
            FILEMUSTEXIST = 0x00001000,
            /// <summary>
            /// Prompts the user for confirmation to create a file if the specified file does not exist.
            /// </summary>
            CREATEPROMPT = 0x00002000,
            /// <summary>
            /// Indicates support for showing network share-aware options in the file dialog.
            /// </summary>
            SHAREAWARE = 0x00004000,
            /// <summary>
            /// Ensures that the file returned from the dialog cannot have the read-only attribute set.
            /// </summary>
            NOREADONLYRETURN = 0x00008000,
            /// <summary>
            /// Prevents creation of a test file during the file dialog operation.
            /// </summary>
            NOTESTFILECREATE = 0x00010000,
            /// <summary>
            /// Hides the "Recent Places" from the navigation pane in the file dialog.
            /// </summary>
            HIDEMRUPLACES = 0x00020000,
            /// <summary>
            /// Hides the pinned places section in the file dialog.
            /// </summary>
            HIDEPINNEDPLACES = 0x00040000,
            /// <summary>
            /// Specifies that the file dialog should not return shortcuts or links to files, but instead directly resolve and return the actual target file or folder.
            /// </summary>
            NODEREFERENCELINKS = 0x00100000,
            /// <summary>
            /// Indicates that the OK button in the file dialog will not be enabled until
            /// the user interacts with a required element within the dialog.
            /// </summary>
            OKBUTTONNEEDSINTERACTION = 0x00200000,
            /// <summary>
            /// Prevents the item from being added to the recent documents list.
            /// </summary>
            DONTADDTORECENT = 0x02000000,
            /// <summary>
            /// Forces the file dialog to display hidden and system files, overriding the user's folder option preferences.
            /// </summary>
            FORCESHOWHIDDEN = 0x10000000,
            /// <summary>
            /// Enables the default non-mini mode appearance for the file dialog, overriding a compact or simplified interface.
            /// </summary>
            DEFAULTNOMINIMODE = 0x20000000,
            /// <summary>
            /// Forces the file dialog to display the preview pane, preventing the user from turning it off.
            /// </summary>
            FORCEPREVIEWPANEON = 0x40000000,
            /// <summary>
            /// Specifies that the file dialog should support selecting items that can be streamed.
            /// </summary>
            SUPPORTSTREAMABLEITEMS = 0x80000000
        }

        [DllImport(Constants2.NativeHelperFilename, CharSet = CharSet.Unicode)]
        private static extern bool ShowDialog(
            [MarshalAs(UnmanagedType.LPWStr)] string title,
            [MarshalAs(UnmanagedType.LPWStr)] string initialPath,
            StringBuilder outPath,
            uint fos,
            [MarshalAs(UnmanagedType.LPWStr)] string filter
        );

        /// Displays a system file dialog to allow the user to select a file or folder.
        /// <param name="title">
        /// The title of the dialog. If null or empty, a default title will be used based on the specified flags.
        /// </param>
        /// <param name="path">
        /// The initial directory or file path that the dialog will display when opened.
        /// </param>
        /// <param name="result">
        /// Output parameter receiving the path to the selected file or folder.
        /// </param>
        /// <param name="fos">
        /// Optional flags from the <see cref="FOS"/> enumeration that determine the behavior and appearance of the dialog.
        /// </param>
        /// <param name="filter">
        /// The file type filter string (e.g., "All Files|*.*" or "All Images|*.jpg;*.png;*.jpeg;*.bmp|Text Files|*.txt") used to restrict the types of files shown in the dialog.
        /// Defaults to "All Files|*.*".
        /// </param>
        /// <returns>
        /// True if the user selected a file or folder, otherwise false.
        /// </returns>
        public static bool ShowDialog(string title, string path, out string result, FOS fos = default, string filter = "All Files|*.*")
        {
            if (string.IsNullOrEmpty(title))
            {
                if ((fos & FOS.PICKFOLDERS) > 0)
                {
                    title = "Select Folder...";
                }
                else if ((fos & FOS.OVERWRITEPROMPT) > 0 || (fos & FOS.CREATEPROMPT) > 0)
                {
                    title = "Save as...";
                }
                else
                {
                    title = "Open File...";
                }
            }
            StringBuilder sb = new StringBuilder(32767);
            bool success = ShowDialog(title, path, sb, (uint)fos, filter);
            result = sb.ToString();
            return success;
        }
    }
}