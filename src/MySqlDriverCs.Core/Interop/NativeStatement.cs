using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
    public class NativeStatement:IDisposable
    {
        private readonly NativeConnection _nativeConnection;
        internal IntPtr stmt;
        public NativeStatement(NativeConnection nativeConnection)
        {
            _nativeConnection = nativeConnection;
            stmt = mysql_stmt_init(_nativeConnection.handle);
            if (stmt == IntPtr.Zero)
                throw new MySqlException(_nativeConnection);
        }

        //Functions for Prepared Statements

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_close")]
        public static extern sbyte mysql_stmt_close_linux(IntPtr handle);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_close")]
        public static extern sbyte mysql_stmt_close_win32(IntPtr handle);

        sbyte mysql_stmt_close(IntPtr handle)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_close_win32(handle);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_close_linux(handle);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_prepare")]
        public static extern int mysql_stmt_prepare_linux(IntPtr stmt, string query, int length);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_prepare")]
        public static extern int mysql_stmt_prepare_win32(IntPtr stmt, string query, int length);

        public int mysql_stmt_prepare(string query, int length)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_prepare_win32(stmt, query, length);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_prepare_linux(stmt, query, length);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_init")]
        public static extern IntPtr mysql_stmt_init_linux(IntPtr mysql);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_init")]
        public static extern IntPtr mysql_stmt_init_win32(IntPtr mysql);

        IntPtr mysql_stmt_init(IntPtr mysql)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_init_win32(mysql);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_init_linux(mysql);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_attr_set")]
        public static extern sbyte mysql_stmt_attr_set_linux(IntPtr stmt, StmtAttrTypes option, IntPtr value);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_attr_set")]
        public static extern sbyte mysql_stmt_attr_set_win32(IntPtr stmt, StmtAttrTypes option, IntPtr value);

        public sbyte mysql_stmt_attr_set(StmtAttrTypes option, IntPtr value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_attr_set_win32(stmt, option, value);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_attr_set_linux(stmt, option, value);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_execute")]
        public static extern int mysql_stmt_execute_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_execute")]
        public static extern int mysql_stmt_execute_win32(IntPtr stmt);

        public int mysql_stmt_execute()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_execute_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_execute_linux(stmt);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_fetch")]
        public static extern int mysql_stmt_fetch_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_fetch")]
        public static extern int mysql_stmt_fetch_win32(IntPtr stmt);

        public int mysql_stmt_fetch()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_fetch_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_fetch_linux(stmt);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_bind_result")]
        public static extern sbyte mysql_stmt_bind_result64_linux(IntPtr stmt, MYSQL_BIND_64[] result);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_bind_result")]
        public static extern sbyte mysql_stmt_bind_result64_win32(IntPtr stmt, MYSQL_BIND_64[] result);

        public sbyte mysql_stmt_bind_result64(MYSQL_BIND_64[] result)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_bind_result64_win32(stmt, result);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_bind_result64_linux(stmt, result);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_bind_param")]
        public static extern sbyte mysql_stmt_bind_param64_linux(IntPtr stmt, MYSQL_BIND_64[] parms);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_bind_param")]
        public static extern sbyte mysql_stmt_bind_param64_win32(IntPtr stmt, MYSQL_BIND_64[] parms);

        public sbyte mysql_stmt_bind_param64(MYSQL_BIND_64[] parms)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_bind_param64_win32(stmt, parms);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_bind_param64_linux(stmt, parms);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_bind_result")]
        public static extern sbyte mysql_stmt_bind_result32_linux(IntPtr stmt, MYSQL_BIND_32[] result);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_bind_result")]
        public static extern sbyte mysql_stmt_bind_result32_win32(IntPtr stmt, MYSQL_BIND_32[] result);

        public sbyte mysql_stmt_bind_result32(MYSQL_BIND_32[] result)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_bind_result32_win32(stmt, result);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_bind_result32_linux(stmt, result);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_bind_param")]
        public static extern sbyte mysql_stmt_bind_param32_linux(IntPtr stmt, MYSQL_BIND_32[] parms);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_bind_param")]
        public static extern sbyte mysql_stmt_bind_param32_win32(IntPtr stmt, MYSQL_BIND_32[] parms);

        public sbyte mysql_stmt_bind_param32(MYSQL_BIND_32[] parms)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_bind_param32_win32(stmt, parms);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_bind_param32_linux(stmt, parms);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_affected_rows")]
        public static extern uint mysql_stmt_affected_rows_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_affected_rows")]
        public static extern uint mysql_stmt_affected_rows_win32(IntPtr stmt);

        public uint mysql_stmt_affected_rows()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_affected_rows_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_affected_rows_linux(stmt);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_param_count")]
        public static extern uint mysql_stmt_param_count_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_param_count")]
        public static extern uint mysql_stmt_param_count_win32(IntPtr stmt);

        public uint mysql_stmt_param_count()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_param_count_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_param_count_linux(stmt);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_store_result")]
        public static extern int mysql_stmt_store_result_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_store_result")]
        public static extern int mysql_stmt_store_result_win32(IntPtr stmt);

        public int mysql_stmt_store_result(IntPtr stmt)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_store_result_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_store_result_linux(stmt);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_use_result")]
        public static extern IntPtr mysql_use_result_linux(IntPtr mysql);
        [DllImport("libmysql.dll", EntryPoint = "mysql_use_result")]
        public static extern IntPtr mysql_use_result_win32(IntPtr mysql);

        public IntPtr mysql_use_result(IntPtr mysql)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_use_result_win32(mysql);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_use_result_linux(mysql);
            else throw new NotSupportedException();
        }


        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_error")]
        static extern IntPtr mysql_stmt_error_native_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_error")]
        static extern IntPtr mysql_stmt_error_native_win32(IntPtr stmt);

        public string mysql_stmt_error()
        {
            IntPtr ret;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ret = mysql_stmt_error_native_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                ret = mysql_stmt_error_native_linux(stmt);
            else throw new NotSupportedException();
            return Marshal.PtrToStringAnsi(ret);
        }
        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_errno")]
        public static extern uint mysql_stmt_errno_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_errno")]
        public static extern uint mysql_stmt_errno_win32(IntPtr stmt);

        public uint mysql_stmt_errno()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_errno_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_errno_linux(stmt);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_num_rows")]
        public static extern uint mysql_stmt_num_rows_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_num_rows")]
        public static extern uint mysql_stmt_num_rows_win32(IntPtr stmt);

        public uint mysql_stmt_num_rows(IntPtr stmt)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_num_rows_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_num_rows_linux(stmt);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_free_result")]
        public static extern sbyte mysql_stmt_free_result_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_free_result")]
        public static extern sbyte mysql_stmt_free_result_win32(IntPtr stmt);

        public sbyte mysql_stmt_free_result()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_free_result_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_free_result_linux(stmt);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_fetch_column")]
        public static extern sbyte mysql_stmt_fetch_column32_linux(IntPtr stmt, MYSQL_BIND_32[] result, uint column, uint offset);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_fetch_column")]
        public static extern sbyte mysql_stmt_fetch_column32_win32(IntPtr stmt, MYSQL_BIND_32[] result, uint column, uint offset);

        public sbyte mysql_stmt_fetch_column32(MYSQL_BIND_32[] result, uint column, uint offset)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_fetch_column32_win32(stmt, result, column, offset);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_fetch_column32_linux(stmt, result, column, offset);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_fetch_column")]
        public static extern sbyte mysql_stmt_fetch_column64_linux(IntPtr stmt, MYSQL_BIND_64[] result, uint column, uint offset);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_fetch_column")]
        public static extern sbyte mysql_stmt_fetch_column64_win32(IntPtr stmt, MYSQL_BIND_64[] result, uint column, uint offset);

        public sbyte mysql_stmt_fetch_column64(MYSQL_BIND_64[] result, uint column, uint offset)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_fetch_column64_win32(stmt, result, column, offset);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_fetch_column64_linux(stmt, result, column, offset);
            else throw new NotSupportedException();
        }

        public void Dispose()
        {
            if(stmt==IntPtr.Zero) return;
            mysql_stmt_close(stmt);
            stmt=IntPtr.Zero;
        }
    }
}