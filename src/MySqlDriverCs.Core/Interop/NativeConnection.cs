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

#endregion LICENSE

using MySqlDriverCs.Interop;
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
     
        private static readonly HashSet<string> Win32PathsAlreadyAdded = new HashSet<string>();

        private readonly INativeProxy _nativeProxy;

        public NativeConnection(string dllPath, INativeTracer nativeTracer)
        {

            _nativeProxy = NativeProxyFactory.GetProxy(nativeTracer);
            if (!string.IsNullOrWhiteSpace(dllPath) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
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

            if (IntPtr.Size == sizeof(int))
                throw new Exception("32Bit process is not supported");

            if (!_mysqlServerInitCalled)
            {
                int rv = _nativeProxy.mysql_server_init(0, IntPtr.Zero, IntPtr.Zero);
                if (rv != 0)
                    throw new MySqlException("Could not initialize MySQL client library");
                _mysqlServerInitCalled = true;
            }

            MySql = _nativeProxy.mysql_init(IntPtr.Zero);
            if (MySql == null)
                throw new MySqlException("mysql_init failed");

            IntPtr vers = mysql_get_client_info();
            if (vers == null)
                throw new MySqlException("mysql_get_client_info failed");
            ClientVersion = Marshal.PtrToStringAnsi(vers);
        }

        internal IntPtr MySql;

        private static bool _mysqlServerInitCalled = false;

        public int mysql_options(mysql_option option, ref uint value)
        {
            return _nativeProxy.mysql_options(MySql, option, ref value);
        }

        public IntPtr mysql_real_connect(string host, string user, string passwd, string db, uint port, string unixSocket, int clientFlag)
        {
            return _nativeProxy.mysql_real_connect(MySql, host, user, passwd, db, port, unixSocket, clientFlag);
        }

        public int mysql_query(string query)
        {
            return _nativeProxy.mysql_query(MySql, query);
        }

        public void mysql_ping()
        {
            var rv = _nativeProxy.mysql_ping(MySql);
            if (rv == 0) return;
            throw new MySqlException(this);
        }

        public IntPtr mysql_store_result()
        {
            return _nativeProxy.mysql_store_result(MySql);
        }

        public uint mysql_errno()
        {
            return _nativeProxy.mysql_errno(MySql);
        }

        private IntPtr mysql_error_native()
        {
            return _nativeProxy.mysql_error_native(MySql);
        }

        public string mysql_error()
        {
            IntPtr error = mysql_error_native();
            return Marshal.PtrToStringAnsi(error);
        }

        public uint mysql_field_count()
        {
            return _nativeProxy.mysql_field_count(MySql);
        }

        public uint mysql_affected_rows()
        {
            return _nativeProxy.mysql_affected_rows(MySql);
        }

        public IntPtr mysql_get_client_info()
        {
            return _nativeProxy.mysql_get_client_info();
        }

        public static int INT64_ADDITIONAL_MEMORY_BUFFER = 4;

        public string ClientVersion { get; }

        public int mysql_select_db(string dbname)
        {
            return _nativeProxy.mysql_select_db(MySql, dbname);
        }

        public int mysql_set_character_set(string csname)
        {
            return _nativeProxy.mysql_set_character_set(MySql, csname);
        }

        public uint mysql_real_escape_string(System.Text.StringBuilder to, string from, uint length)
        {
            return _nativeProxy.mysql_real_escape_string(MySql, to, from, length);
        }

        public void Dispose()
        {
            if (MySql != IntPtr.Zero)
            {
                _nativeProxy.mysql_close(MySql);
                MySql = IntPtr.Zero;
            }
        }
    }
}