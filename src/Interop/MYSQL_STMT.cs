using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
#pragma warning disable 1591
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MYSQL_STMT
    {
        public MEM_ROOT mem_root;             /* root allocations */
        public LIST list;                 /* list to keep track of all stmts */
        public IntPtr mysql;               /* connection handle */
        public IntPtr paramss;              /* input parameters */
        public IntPtr bind;                /* output parameters */
        public IntPtr fields;              /* result set metadata */
        public MYSQL_DATA result;               /* cached result set */
        public IntPtr data_cursor;         /* current row in cached result */
        public IntPtr read_row_func;
        /* copy of mysql->affected_rows after statement execution */
        public uint affected_rows;
        public uint insert_id;            /* copy of mysql->insert_id */
        public uint stmt_id;           /* Id for prepared statement */
        public uint flags;                /* i.e. type of cursor to open */
        public uint prefetch_rows;        /* number of rows per one COM_FETCH */
        public uint server_status;
        public uint last_errno;        /* error code */
        public uint param_count;          /* input parameter count */
        public uint field_count;          /* number of columns in result set */
        public /*enum_mysql_stmt_state*/uint state;    /* statement state */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public char[] last_error; /* error message */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5 + 1)]
        public char[] sqlstate;
        public byte send_types_to_server;
        public byte bind_param_done;      /* input buffers were supplied */
        public byte bind_result_done;     /* output buffers were supplied */
        /* mysql_stmt_close() had to cancel this result */
        public byte unbuffered_fetch_cancelled;
        public byte update_max_length;

    }
}