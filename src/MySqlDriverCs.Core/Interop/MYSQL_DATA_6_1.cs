using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
#pragma warning disable 1591
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MYSQL_DATA_6_1
    {
        public IntPtr data;
        public IntPtr embedded_info;
        public MEM_ROOT_6_1 alloc;
        public uint rows;
        public uint fields;
        public System.IntPtr extension;
    }
}