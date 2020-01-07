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
using System.Runtime.InteropServices;
using NativeLibraryLoader;


namespace MySQLDriverCS.Interop
{
    public unsafe class Native
    {
        // private delegate IntPtr mysql_init(IntPtr must_be_null);

        // private mysql_init _mysql_init=null;
		// public IntPtr MySqlInit()
        // {
        //     if (_mysql_init == null)
        //     {
        //         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //         {
        //             NativeLibrary k32 = new NativeLibrary("C:\\Program Files\\MySQL\\MySQL Connector C 6.1\\lib\\libmysql.dll");
        //             _mysql_init = k32.LoadFunction<mysql_init>("mysql_init");
		// 		}
        //         else
        //         {
		// 			// net core
        //             NativeLibrary k32 = new NativeLibrary("/usr/lib/x86_64-linux-gnu/libmysqlclient.so", new UnixLibraryLoader());
        //             _mysql_init = k32.LoadFunction<mysql_init>("mysql_init");
		// 		}
        //     }

        //     return _mysql_init(IntPtr.Zero);

        // }
		[DllImport("libmysqlclient", EntryPoint = "mysql_init")]
		public static extern unsafe void* mysql_init(void* must_be_null);

 internal static class Libdl
    {
        private const string LibName = "/usr/lib/x86_64-linux-gnu/libdl.so";

        public const int RTLD_NOW = 0x002;

        [DllImport(LibName)]
        public static extern IntPtr dlopen(string fileName, int flags);

        [DllImport(LibName)]
        public static extern IntPtr dlsym(IntPtr handle, string name);

        [DllImport(LibName)]
        public static extern int dlclose(IntPtr handle);

        [DllImport(LibName)]
        public static extern string dlerror();
    }
		
        private class UnixLibraryLoader : LibraryLoader
        {
            protected override void CoreFreeNativeLibrary(IntPtr handle)
            {
                Libdl.dlclose(handle);
            }

            protected override IntPtr CoreLoadFunctionPointer(IntPtr handle, string functionName)
            {
                return Libdl.dlsym(handle, functionName);
            }

            protected override IntPtr CoreLoadNativeLibrary(string name)
            {
                return Libdl.dlopen(name, Libdl.RTLD_NOW);
            }
        }
	}
	/// <summary>
	/// C prototypes warpper for mysqllib.
	/// </summary>
	internal class NativeMethods
	{
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_init")]
		public static extern unsafe void* mysql_init(void* must_be_null);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_close")]
		public static extern unsafe void mysql_close(void* handle);

		// BEGIN ADDITION 2004-07-01 BY Alex Seewald
		// Enables us to call mysql_option to activate compression and timeout
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_options")]
		public static extern unsafe void mysql_options(void* mysql, uint option, uint* value);
		// END ADDITION 2004-07-01 By Alex Seewald

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_real_connect")]
		public static extern unsafe void* mysql_real_connect(void* mysql, string host, string user, string passwd, string db, uint port, string unix_socket, int client_flag);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_query")]
		public static extern unsafe int mysql_query(void* mysql, string query);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_store_result")]
		public static extern unsafe void* mysql_store_result(void* mysql);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_free_result")]
		public static extern unsafe void mysql_free_result(void* result);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_errno")]
		public static extern unsafe uint mysql_errno(void* mysql);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_error")]
		static extern unsafe IntPtr mysql_error_native(void* mysql);

		public static unsafe string mysql_error(void* mysql)
		{
			IntPtr error = mysql_error_native(mysql);
			return Marshal.PtrToStringAnsi(error);
		}
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_field_count")]
		public static extern unsafe uint mysql_field_count(void* mysql);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_affected_rows")]
		public static extern unsafe uint mysql_affected_rows(void* mysql);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_num_fields")]
		public static extern unsafe uint mysql_num_fields(void* result);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_num_rows")]
		public static extern unsafe uint mysql_num_rows(void* result);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_fetch_field_direct")]
		public static extern unsafe IntPtr mysql_fetch_field_direct(void* result, uint fieldnr);

		/// <summary>
		/// 
		/// </summary>
		///<returns>Returns a string that represents the client library version</returns>
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_get_client_info")]
		private static extern IntPtr GetClientInfo();

		static string version;
		static int CLIENT_6_1_OR_GREATER = -1;
		public static int INT64_ADDITIONAL_MEMORY_BUFFER = 4;

		public static bool Client6_1()
		{
			if (CLIENT_6_1_OR_GREATER == -1)
			{
				CLIENT_6_1_OR_GREATER = GetClientVersion().CompareTo("6.0.0") > 0 ? 1 : 0;
			}
			return (CLIENT_6_1_OR_GREATER == 1);
		}
		public static string GetClientVersion()
		{
			if (string.IsNullOrEmpty(version))
			{
				IntPtr vers = NativeMethods.GetClientInfo();
				version = Marshal.PtrToStringAnsi(vers);
			}
			return version;
		}

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_fetch_row")]
		public static extern unsafe IntPtr mysql_fetch_row(void* result);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_select_db")]
		public static extern unsafe int mysql_select_db(void* mysql, string dbname);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_fetch_lengths")]
		public static extern unsafe UInt32* mysql_fetch_lengths(void* result);

		//Functions for Prepared Statements

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_close")]
		public static extern sbyte mysql_stmt_close(IntPtr handle);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_prepare")]
		public static extern int mysql_stmt_prepare(IntPtr stmt, string query, int length);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_init")]
		public static extern unsafe IntPtr mysql_stmt_init(void* mysql);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_attr_set")]
		public static extern sbyte mysql_stmt_attr_set(IntPtr stmt, StmtAttrTypes option, IntPtr value);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_execute")]
		public static extern int mysql_stmt_execute(IntPtr stmt);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_fetch")]
		public static extern int mysql_stmt_fetch(IntPtr stmt);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_bind_result")]
		public static extern sbyte mysql_stmt_bind_result64(IntPtr stmt, MYSQL_BIND_64[] result);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_bind_param")]
		public static extern sbyte mysql_stmt_bind_param64(IntPtr stmt, MYSQL_BIND_64[] parms);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_bind_result")]
		public static extern sbyte mysql_stmt_bind_result32(IntPtr stmt, MYSQL_BIND_32[] result);
		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_bind_param")]
		public static extern sbyte mysql_stmt_bind_param32(IntPtr stmt, MYSQL_BIND_32[] parms);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_affected_rows")]
		public static extern uint mysql_stmt_affected_rows(IntPtr stmt);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_param_count")]
		public static extern uint mysql_stmt_param_count(IntPtr stmt);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_store_result")]
		public static extern int mysql_stmt_store_result(IntPtr stmt);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_use_result")]
		public static extern IntPtr mysql_use_result(IntPtr mysql);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_result_metadata")]
		public static extern unsafe void* mysql_stmt_result_metadata(IntPtr stmt);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_error")]
		static extern IntPtr mysql_stmt_error_native(IntPtr stmt);

		public static string mysql_stmt_error(IntPtr stmt)
		{
			IntPtr error = mysql_stmt_error_native(stmt);
			return Marshal.PtrToStringAnsi(error);
		}

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_errno")]
		public static extern uint mysql_stmt_errno(IntPtr stmt);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_num_rows")]
		public static extern unsafe uint mysql_stmt_num_rows(void* stmt);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_free_result")]
		public static extern sbyte mysql_stmt_free_result(IntPtr stmt);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_fetch_column")]
		public static extern sbyte mysql_stmt_fetch_column32(IntPtr stmt, MYSQL_BIND_32[] result, uint column, uint offset);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_fetch_column")]
		public static extern sbyte mysql_stmt_fetch_column64(IntPtr stmt, MYSQL_BIND_64[] result, uint column, uint offset);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_set_character_set")]
		public static extern unsafe int mysql_set_character_set(void* mysql, string csname);

		[DllImport("libmysqlclient.so", EntryPoint = "mysql_real_escape_string")]
		public static extern unsafe uint mysql_real_escape_string(void* mysql, System.Text.StringBuilder to, string from, uint length);
	}
}
