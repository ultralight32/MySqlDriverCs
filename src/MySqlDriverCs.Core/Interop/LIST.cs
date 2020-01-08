using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
#pragma warning disable 1591
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct LIST
    {
        IntPtr prev;
        IntPtr next;
        IntPtr data;
    }
}