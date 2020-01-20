using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS.Interop
{
    internal class NativeImportsTemplate : INativeProxy
    {
        private const string DllName = "$dll$";

        private readonly INativeTracer _nativeTracer;
        internal NativeImportsTemplate(INativeTracer nativeTracer)
        {
            _nativeTracer = nativeTracer;
        }

        /// <summary>
        /// mysql - a mysql handle, which was previously allocated by mysql_init() or mysql_real_connect().
        /// charset - a pointer to a MY_CHARSET_INFO structure, in which the information will be copied.
        /// </summary>
        [DllImport("libmySQL.dll", EntryPoint = "mysql_get_character_set_info")]
        private static extern void mysql_get_character_set_info(IntPtr mysql, IntPtr charsetinfo);

        MY_CHARSET_INFO INativeProxy.mysql_get_character_set_info(IntPtr mysql)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);

            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<MY_CHARSET_INFO>());
            mysql_get_character_set_info(mysql, ptr);
            var charsetinfo = new MY_CHARSET_INFO();
            Marshal.PtrToStructure(ptr, charsetinfo);
            Marshal.FreeHGlobal(ptr);
            return charsetinfo;
        }


        [DllImport(DllName, EntryPoint = "mysql_close")]
        private static extern void mysql_close(IntPtr handle);

        void INativeProxy.mysql_close(IntPtr handle)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            mysql_close(handle);
        }

        [DllImport(DllName, EntryPoint = "mysql_options")]
        private static extern int mysql_options(IntPtr mysql, mysql_option option, ref uint value);
        int INativeProxy.mysql_options(IntPtr mysql, mysql_option option, ref uint value)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_options(mysql, option, ref value);
        }

        [DllImport(DllName, EntryPoint = "mysql_server_init")]
        private static extern int mysql_server_init(int argc, IntPtr argv, IntPtr groups);

        int INativeProxy.mysql_server_init(int argc, IntPtr argv, IntPtr groups)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_server_init(argc, argv, groups);
        }

        [DllImport(DllName, EntryPoint = "mysql_init")]
        public static extern IntPtr mysql_init(IntPtr must_be_null);

        IntPtr INativeProxy.mysql_init(IntPtr mustBeNull)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_init(mustBeNull);
        }


        [DllImport(DllName, EntryPoint = "mysql_ping", CharSet = CharSet.Ansi)]
        public static extern int mysql_ping(IntPtr mysql);

        int INativeProxy.mysql_ping(IntPtr mysql)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_ping(mysql);
        }

        [DllImport(DllName, EntryPoint = "mysql_query", CharSet = CharSet.Ansi)]
        public static extern int mysql_query(IntPtr mysql, string query);
        int INativeProxy.mysql_query(IntPtr mysql, string query)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_query(mysql, query);
        }

        [DllImport(DllName, EntryPoint = "mysql_real_connect")]
        public static extern IntPtr mysql_real_connect(IntPtr mysql, string host, string user, string passwd, string db, uint port, string unix_socket, int client_flag);

        IntPtr INativeProxy.mysql_real_connect(IntPtr mysql, string host, string user, string passwd, string db, uint port, string unix_socket, int client_flag)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_real_connect(mysql, host, user, passwd, db, port, unix_socket, client_flag);
        }

        [DllImport(DllName, EntryPoint = "mysql_select_db")]
        private static extern int mysql_select_db(IntPtr mysql, string dbname);

        int INativeProxy.mysql_select_db(IntPtr mysql, string dbname)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_select_db(mysql, dbname);
        }
        [DllImport(DllName, EntryPoint = "mysql_set_character_set")]
        private static extern int mysql_set_character_set(IntPtr mysql, string csname);

        int INativeProxy.mysql_set_character_set(IntPtr mysql, string csname)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_set_character_set(mysql, csname);
        }

        [DllImport(DllName, EntryPoint = "mysql_real_escape_string")]
        private static extern uint mysql_real_escape_string(IntPtr mysql, System.Text.StringBuilder to, string from, uint length);

        uint INativeProxy.mysql_real_escape_string(IntPtr mysql, System.Text.StringBuilder to, string from, uint length)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_real_escape_string(mysql, to, from, length);
        }

        [DllImport(DllName, EntryPoint = "mysql_store_result")]
        private static extern IntPtr mysql_store_result(IntPtr mysql);

        IntPtr INativeProxy.mysql_store_result(IntPtr mysql)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_store_result(mysql);
        }
        [DllImport(DllName, EntryPoint = "mysql_errno")]
        private static extern uint mysql_errno(IntPtr mysql);

        uint INativeProxy.mysql_errno(IntPtr mysql)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_errno(mysql);
        }

        [DllImport(DllName, EntryPoint = "mysql_error")]
        private static extern IntPtr mysql_error_native(IntPtr mysql);

        IntPtr INativeProxy.mysql_error_native(IntPtr mysql)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_error_native(mysql);
        }

        [DllImport(DllName, EntryPoint = "mysql_field_count")]
        private static extern uint mysql_field_count(IntPtr mysql);

        uint INativeProxy.mysql_field_count(IntPtr mysql)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_field_count(mysql);
        }
        [DllImport(DllName, EntryPoint = "mysql_affected_rows")]
        private static extern uint mysql_affected_rows(IntPtr mysql);

        uint INativeProxy.mysql_affected_rows(IntPtr mysql)
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_affected_rows(mysql);
        }
        [DllImport(DllName, EntryPoint = "mysql_get_client_info")]
        private static extern IntPtr mysql_get_client_info();

        IntPtr INativeProxy.mysql_get_client_info()
        {
            _nativeTracer?.Trace(MethodBase.GetCurrentMethod().Name);
            return mysql_get_client_info();
        }

    }
}
