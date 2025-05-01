using System;
using System.Runtime.InteropServices;

namespace NuoDb.VisualStudio.DataTools.Editors
{
    public class Win32
    {
        public const int WM_KEYDOWN = 0x0100,
        WM_CHAR = 0x0102,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSCHAR = 0x0106;

        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool IsChild(IntPtr hwndParent, IntPtr hwndChildTest);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndBefore, int x, int y, int cx, int cy, int flags);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
