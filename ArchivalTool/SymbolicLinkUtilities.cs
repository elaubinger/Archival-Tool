using System;
using System.Runtime.InteropServices;

namespace ArchivalTool
{
    public static class SymbolicLinkUtilities
    {

        [DllImport("kernel32.dll")]
        public static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        [Flags]
        public enum LinkFlags
        {
            File = 0x0,
            Directory = 0x1,
            AllowUnprivilegedCreate = 0x2
        }
    }
}
