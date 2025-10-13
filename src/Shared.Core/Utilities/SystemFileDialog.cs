using System;
using System.Runtime.InteropServices;
using System.Text;

namespace KKAPI.Utilities
{
    public class SystemFileDialog
    {
        [Flags]
        public enum FOS : uint
        {
            OVERWRITEPROMPT = 0x00000002,
            STRICTFILETYPES = 0x00000004,
            NOCHANGEDIR = 0x00000008,
            PICKFOLDERS = 0x00000020,
            FORCEFILESYSTEM = 0x00000040,
            ALLNONSTORAGEITEMS = 0x00000080,
            NOVALIDATE = 0x00000100,
            ALLOWMULTISELECT = 0x00000200,
            PATHMUSTEXIST = 0x00000800,
            FILEMUSTEXIST = 0x00001000,
            CREATEPROMPT = 0x00002000,
            SHAREAWARE = 0x00004000,
            NOREADONLYRETURN = 0x00008000,
            NOTESTFILECREATE = 0x00010000,
            HIDEMRUPLACES = 0x00020000,
            HIDEPINNEDPLACES = 0x00040000,
            NODEREFERENCELINKS = 0x00100000,
            OKBUTTONNEEDSINTERACTION = 0x00200000,
            DONTADDTORECENT = 0x02000000,
            FORCESHOWHIDDEN = 0x10000000,
            DEFAULTNOMINIMODE = 0x20000000,
            FORCEPREVIEWPANEON = 0x40000000,
            SUPPORTSTREAMABLEITEMS = 0x80000000
        }

        [DllImport("Helper.dll", CharSet = CharSet.Unicode)]
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
        /// The file type filter string (e.g., "All Files|*.*") used to restrict the types of files shown in the dialog.
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