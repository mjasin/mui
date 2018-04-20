using System.Runtime.InteropServices;

namespace ModernUI.Win32
{
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable S101 // Types should be named in camel case
    struct RECT
#pragma warning restore S101 // Types should be named in camel case
    {
        public int left, top, right, bottom;
    }
}