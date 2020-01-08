#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Vi�as Livschitz.

	This file is part of MySQLDriverCS.

    MySQLDriverCS is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    MySQLDriverCS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MySQLDriverCS; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;



namespace MySQLDriverCS.Interop
{
    /// <summary>
    /// C prototypes warpper for mysqllib.
    /// </summary>
    public class NativeConnection : IDisposable
    {

        static readonly HashSet<string> Win32PathsAlreadyAdded = new HashSet<string>();
        public NativeConnection(string dllPath)
        {
            if (!string.IsNullOrWhiteSpace(dllPath) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { 
                lock (Win32PathsAlreadyAdded)
                {
                    if (!Win32PathsAlreadyAdded.Contains(dllPath))
                    {
                        var p = Environment.GetEnvironmentVariable("PATH");
                        Environment.SetEnvironmentVariable("PATH", p + ";" + dllPath, EnvironmentVariableTarget.Process);
                        Win32PathsAlreadyAdded.Add(dllPath);
                    }
                }
            }

            handle = mysql_init();
            if (handle == null)
                throw new MySqlException("MySQLDriverCS Error: can't create client.");
        }

        internal IntPtr handle;
        [DllImport("libmysqlclient.so", EntryPoint = "mysql_init")]
        public static extern IntPtr mysql_init_linux(IntPtr must_be_null);
        [DllImport("libmysql.dll", EntryPoint = "mysql_init")]
        public static extern IntPtr mysql_init_win32(IntPtr must_be_null);

        IntPtr mysql_init()
        {
            if(IntPtr.Size==sizeof(int))
                throw new Exception("32Bit process is not supported");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_init_win32(IntPtr.Zero);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_init_linux(IntPtr.Zero);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_close")]
        public static extern void mysql_close_linux(IntPtr handle);
        [DllImport("libmysql.dll", EntryPoint = "mysql_close")]
        public static extern void mysql_close_win32(IntPtr handle);

        void mysql_close(IntPtr handle)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                mysql_close_win32(handle);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mysql_close_linux(handle);
            else throw new NotSupportedException();
        }


        [DllImport("libmysqlclient.so", EntryPoint = "mysql_options")]
        public static extern void mysql_options_linux(IntPtr mysql, uint option, ref uint value);
        [DllImport("libmysql.dll", EntryPoint = "mysql_options")]
        public static extern void mysql_options_win32(IntPtr mysql, uint option, ref uint value);

        public void MySqlOptions(uint option, ref uint value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                mysql_options_win32(handle, option, ref value);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mysql_options_linux(handle, option, ref value);
            else throw new NotSupportedException();
        }


        [DllImport("libmysqlclient.so", EntryPoint = "mysql_real_connect")]
        public static extern IntPtr mysql_real_connect_linux(IntPtr mysql, string host, string user, string passwd, string db, uint port, string unix_socket, int client_flag);
        [DllImport("libmysql.dll", EntryPoint = "mysql_real_connect")]
        public static extern IntPtr mysql_real_connect_win32(IntPtr mysql, string host, string user, string passwd, string db, uint port, string unix_socket, int client_flag);


        public IntPtr MySqlRealConnect(string host, string user, string passwd, string db, uint port, string unix_socket, int client_flag)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_real_connect_win32(handle, host, user, passwd, db, port, unix_socket, client_flag);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_real_connect_linux(handle, host, user, passwd, db, port, unix_socket, client_flag);
            else throw new NotSupportedException();
        }


        [DllImport("libmysqlclient.so", EntryPoint = "mysql_query", CharSet = CharSet.Ansi)]
        public static extern int mysql_query_linux(IntPtr mysql, string query);
        [DllImport("libmysql.dll", EntryPoint = "mysql_query", CharSet = CharSet.Ansi)]
        public static extern int mysql_query_win32(IntPtr mysql, string query);


        public int mysql_query(string query)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_query_win32(handle, query);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_query_linux(handle, query);
            else throw new NotSupportedException();
        }



        [DllImport("libmysqlclient.so", EntryPoint = "mysql_store_result")]
        public static extern IntPtr mysql_store_result_linux(IntPtr mysql);
        [DllImport("libmysql.dll", EntryPoint = "mysql_store_result")]
        public static extern IntPtr mysql_store_result_win32(IntPtr mysql);

        public IntPtr mysql_store_result()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_store_result_win32(handle);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_store_result_linux(handle);
            else throw new NotSupportedException();
        }

       


        [DllImport("libmysqlclient.so", EntryPoint = "mysql_errno")]
        public static extern uint mysql_errno_linux(IntPtr mysql);
        [DllImport("libmysql.dll", EntryPoint = "mysql_errno")]
        public static extern uint mysql_errno_win32(IntPtr mysql);

        public uint mysql_errno()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_errno_win32(handle);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_errno_linux(handle);
            else throw new NotSupportedException();
        }


        [DllImport("libmysqlclient.so", EntryPoint = "mysql_error")]
        static extern IntPtr mysql_error_native_linux(IntPtr mysql);

        [DllImport("libmysql.dll", EntryPoint = "mysql_error")]
        static extern IntPtr mysql_error_native_win32(IntPtr mysql);


        IntPtr mysql_error_native()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_error_native_win32(handle);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_error_native_linux(handle);
            else throw new NotSupportedException();
        }

        public string mysql_error()
        {
            IntPtr error = mysql_error_native();
            return Marshal.PtrToStringAnsi(error);
        }


        [DllImport("libmysqlclient.so", EntryPoint = "mysql_field_count")]
        public static extern uint mysql_field_count_linux(IntPtr mysql);
        [DllImport("libmysql.dll", EntryPoint = "mysql_field_count")]
        public static extern uint mysql_field_count_win32(IntPtr mysql);


        public uint mysql_field_count()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_field_count_win32(handle);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_field_count_linux(handle);
            else throw new NotSupportedException();
        }



        [DllImport("libmysqlclient.so", EntryPoint = "mysql_affected_rows")]
        public static extern uint mysql_affected_rows_linux(IntPtr mysql);
        [DllImport("libmysql.dll", EntryPoint = "mysql_affected_rows")]
        public static extern uint mysql_affected_rows_win32(IntPtr mysql);

        public uint mysql_affected_rows()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_affected_rows_win32(handle);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_affected_rows_linux(handle);
            else throw new NotSupportedException();
        }

       

     

     

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_get_client_info")]
        private static extern IntPtr mysql_get_client_info_linux();
        [DllImport("libmysql.dll", EntryPoint = "mysql_get_client_info")]
        private static extern IntPtr mysql_get_client_info_win32();

        public IntPtr mysql_get_client_info()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_get_client_info_win32();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_get_client_info_linux();
            else throw new NotSupportedException();
        }



        static string version;
        static int CLIENT_6_1_OR_GREATER = -1;
        public static int INT64_ADDITIONAL_MEMORY_BUFFER = 4;

        public bool Client6_1()
        {
            if (CLIENT_6_1_OR_GREATER == -1)
            {
                CLIENT_6_1_OR_GREATER = GetClientVersion().CompareTo("6.0.0") > 0 ? 1 : 0;
            }
            return (CLIENT_6_1_OR_GREATER == 1);
        }
        public string GetClientVersion()
        {
            if (string.IsNullOrEmpty(version))
            {
                IntPtr vers = mysql_get_client_info();
                version = Marshal.PtrToStringAnsi(vers);
            }
            return version;
        }

     

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_select_db")]
        public static extern int mysql_select_db_linux(IntPtr mysql, string dbname);
        [DllImport("libmysql.dll", EntryPoint = "mysql_select_db")]
        public static extern int mysql_select_db_win32(IntPtr mysql, string dbname);

        public int mysql_select_db(string dbname)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_select_db_win32(handle, dbname);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_select_db_linux(handle, dbname);
            else throw new NotSupportedException();
        }


     



       

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_set_character_set")]
        public static extern int mysql_set_character_set_linux(IntPtr mysql, string csname);
        [DllImport("libmysql.dll", EntryPoint = "mysql_set_character_set")]
        public static extern int mysql_set_character_set_win32(IntPtr mysql, string csname);

        public int mysql_set_character_set(string csname)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_set_character_set_win32(handle, csname);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_set_character_set_linux(handle, csname);
            else throw new NotSupportedException();
        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_real_escape_string")]
        public static extern uint mysql_real_escape_string_linux(IntPtr mysql, System.Text.StringBuilder to, string from, uint length);
        [DllImport("libmysql.dll", EntryPoint = "mysql_real_escape_string")]
        public static extern uint mysql_real_escape_string_win32(IntPtr mysql, System.Text.StringBuilder to, string from, uint length);

        public uint mysql_real_escape_string(System.Text.StringBuilder to, string from, uint length)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_real_escape_string_win32(handle, to, from, length);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_real_escape_string_linux(handle, to, from, length);
            else throw new NotSupportedException();
        }


        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                mysql_close(handle);
                handle = IntPtr.Zero;
            }
        }
    }
}
