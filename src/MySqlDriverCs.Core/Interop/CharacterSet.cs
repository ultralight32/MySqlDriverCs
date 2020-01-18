using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CharacterSet
    {
        public uint number;     /* character set number              */
        public uint state;      /* character set state               */
        public IntPtr csname;    /* collation name                    */
        public IntPtr name;      /* character set name                */
        public IntPtr comment;   /* comment                           */
        public IntPtr dir;       /* character set directory           */
        public uint mbminlen;   /* min. length for multibyte strings */
        public uint mbmaxlen;   /* max. length for multibyte strings */
    }
}