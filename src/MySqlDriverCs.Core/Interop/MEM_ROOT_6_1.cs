using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
#pragma warning disable 1591
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MEM_ROOT_6_1
    {
        public IntPtr free;                  /* blocks with free memory in it */
        public IntPtr used;                  /* blocks almost without free memory */
        public IntPtr pre_alloc;             /* preallocated block */
        /* if block have less memory it will be put in 'used' list */
        public uint min_malloc;
        public uint block_size;         /* initial block size */
        public uint block_num;          /* allocated blocks counter */
        public uint first_block_usage;
        public IntPtr errohandler;
        public IntPtr m_psi_key;
    }
}