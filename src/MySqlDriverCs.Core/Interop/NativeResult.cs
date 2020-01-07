using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
    public  class NativeResult : IDisposable
    {
        private IntPtr result;

        public NativeResult(IntPtr result)
        {
            this.result = result;
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_num_fields")]
        public static extern uint mysql_num_fields_linux(IntPtr result);
        [DllImport("libmysql.dll", EntryPoint = "mysql_num_fields")]
        public static extern uint mysql_num_fields_win32(IntPtr result);

        public uint mysql_num_fields()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_num_fields_win32(result);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_num_fields_linux(result);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_free_result")]
        public static extern void mysql_free_result_linux(IntPtr result);
        [DllImport("libmysql.dll", EntryPoint = "mysql_free_result")]
        public static extern void mysql_free_result_win32(IntPtr result);

        void mysql_free_result(IntPtr result)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                mysql_free_result_win32(result);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mysql_free_result_linux(result);
            else throw new NotSupportedException();
        }
        [DllImport("libmysqlclient.so", EntryPoint = "mysql_num_rows")]
        public static extern uint mysql_num_rows_linux(IntPtr result);
        [DllImport("libmysql.dll", EntryPoint = "mysql_num_rows")]
        public static extern uint mysql_num_rows_win32(IntPtr result);

        public uint mysql_num_rows( )
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_num_rows_win32(result);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_num_rows_linux(result);
            else throw new NotSupportedException();
        }
        public void Dispose()
        {
            if (result == IntPtr.Zero) return;
            mysql_free_result(result);
            result=IntPtr.Zero;
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_fetch_row")]
        public static extern IntPtr mysql_fetch_row_linux(IntPtr result);
        [DllImport("libmysql.dll", EntryPoint = "mysql_fetch_row")]
        public static extern IntPtr mysql_fetch_row_win32(IntPtr result);

        public IntPtr mysql_fetch_row()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_fetch_row_win32(result);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_fetch_row_linux(result);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_fetch_field_direct")]
        public static extern IntPtr mysql_fetch_field_direct_linux(IntPtr result, uint fieldnr);
        [DllImport("libmysql.dll", EntryPoint = "mysql_fetch_field_direct")]
        public static extern IntPtr mysql_fetch_field_direct_win32(IntPtr result, uint fieldnr);

        public IntPtr mysql_fetch_field_direct( uint fieldnr)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_fetch_field_direct_win32(result, fieldnr);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_fetch_field_direct_linux(result, fieldnr);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_fetch_lengths")]
        public static extern  IntPtr mysql_fetch_lengths_linux(IntPtr result);
        [DllImport("libmysql.dll", EntryPoint = "mysql_fetch_lengths")]
        public static extern  IntPtr mysql_fetch_lengths_win32(IntPtr result);

        public int[] mysql_fetch_lengths(int arrayLength)
        {
            IntPtr p;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                p= mysql_fetch_lengths_win32(result);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                p= mysql_fetch_lengths_linux(result);
            else throw new NotSupportedException();

            int[] r = new int[arrayLength];
            Marshal.Copy(p, r, 0, arrayLength);

            return r;
        }
    }
}