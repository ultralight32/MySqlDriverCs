#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.

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

// BEGIN ADDITION : Compression and Timeout
//
// by Alex Seewald alex@righar.com 2004-07-01
// - Added the timeout option at connection - if you work like I do with many
// - applications connected onto a database over the internet and that you need 
// - to detect that the connection timed out instead of having the application
// - mysteriously freeze under some conditions, you'll find that the mysql_options 
// - function found in the mysql library comes in handy. (I did try to use threads 
// - to kill a frozen connection but mysql didn't seem to like that .. much).

// - Added the following options to the connection string :  

//			timeout=<timeout in seconds>
//			compression=<anything>

// i.e. start by creating your connection string
//	conn.ConnectionString = 
//			New MySQLDriverCS.MySQLConnectionString(serv, databaseName, user, pass, 3306).AsString

// Then if you want to add a 5 seconds timeout  (defaults to 10)
//		conn.ConnectionString += "timeout=5;";

// Then if you want to activate server/client compression
//		conn.ConnectionString += "compression=yes;";

// This is the connection code I used in my programs (VB version)
/*
Dim conn As New MySQLDriverCS.MySQLConnection
Dim req As New MySQLDriverCS.MySQLCommand

 Try
	conn.ConnectionString = 
		New MySQLDriverCS.MySQLConnectionString(serv, databaseName, user, pass, 3306).AsString
    conn.Open()
    req = conn.CreateCommand()
 Catch ex As Exception
	// Error Connecting, or timeout
	Debug.WriteLine("Error connection : " + ex.ToString())
 End Try
*/

// I recommend you activate BOTH if you work over the internet (as opposed to a local network)
// It'll speed things up and make your program more reliable

// END ADDITION

using System;
using System.Data;
using System.Data.Common;
using System.Text;
using MySQLDriverCS.Interop;

//using MySQLDriverCPPProxy;
namespace MySQLDriverCS
{
    /// <summary>
    /// This class is IDbConnection compliant so take a look into MSDN help to understand how it works. 
    /// </summary>
    /// <remarks>All members are supported now (2002-10-28)</remarks>
    /// 

    public class MySQLConnection : DbConnection
    {
        // BEGIN ADDITION 2004-07-01 adding timeout and compression		
        // these come from the libmysql include enumerations (assuming the compilers starts 
        // enumerating at 0, which *should* be the case)
        // END ADDITION

        const uint MYSQL_OPTION_CONNECTION_TIMEOUT = 0;
        const uint MYSQL_OPTION_COMPRESS = 1;

        private string connectionString;
        internal string dbname = null;
        internal Encoding encoding = Encoding.Default;

        #region Constructors & Destructor
        /// <summary>
        /// Creates a connection
        /// </summary>
        public MySQLConnection()
        {
        }
        /// <summary>
        /// Creates a connection with a connection string
        /// </summary>
        /// <param name="cs"></param>
        public MySQLConnection(string cs)
        {
            this.connectionString = cs;
        }
        private bool bDisposed = false;
        /// <summary>
        /// Dispose destructor
        /// </summary>
        public new void Dispose()
        {
            if (bDisposed) return;
            this.Close();
            bDisposed = true;
        }
        #endregion
        #region ConnectionString property
        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        public override string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }
        /// <summary>
        /// CharacterSet
        /// </summary>
        public Encoding CharacterEncoding
        {
            get { return encoding; }
        }

        #endregion
        #region Port property
        /// <summary>
        /// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// </summary>
        public int Port
        {
            get
            {
                string val = FindValueInCS("Port");
                if (val == "")
                    return 3306;
                else
                    return Convert.ToInt32(val);
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public override string ServerVersion
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #region ConnectionTimeout property
        /// <summary>
        /// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// </summary>
        public override int ConnectionTimeout
        {
            get
            {
                string val = FindValueInCS("timeout");
                if (val == "")
                    return 60;
                else
                    return Convert.ToInt32(val);
            }
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        public override string DataSource
        {
            get { return Database; }
        }

        #region Database property
        /// <summary>
        /// Gets the name of the current database or the database to be used once a connection is open.
        /// </summary>
        public override string Database
        {
            get
            {
                if (dbname != null)
                    return dbname;
                else
                    return FindValueInCS("Data Source=");
            }
        }
        #endregion
        #region FindValueInCS
        private string FindValueInCS(string keyword)
        {
            keyword = keyword.ToLower();
            string connectionStringLo = connectionString.ToLower();
            int pos = connectionStringLo.IndexOf(keyword);
            if (pos == -1)
            {
                return "";
            }
            else
            {
                pos = connectionString.IndexOf('=', pos) + 1;
                int pos2 = connectionString.IndexOf(';', pos);
                if (pos2 == -1)
                {
                    return connectionString.Substring(pos);
                }
                else
                {
                    return connectionString.Substring(pos, pos2 - pos);
                }
            }
        }
        #endregion
        #region State property
        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        public override ConnectionState State
        {
            get
            {
                if (NativeConnection == null)
                    return ConnectionState.Closed;
                else
                    return ConnectionState.Open;
            }
        }
        #endregion
        #region BeginTransaction(IsolationLevel il)
        /// <summary>
        /// Begins a transaction
        /// </summary>
        /// <param name="il"></param>
        /// <returns></returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel il)
        {
            return new MySQLTransaction(this, il);
        }

        #endregion

        #region ChangeDatabase
        /// <summary>
        /// Changes database
        /// </summary>
        /// <param name="databaseName"></param>
        public override void ChangeDatabase(string databaseName)
        {
            if (0 == NativeConnection.mysql_select_db(databaseName))
                this.dbname = databaseName;
            else
            {
                throw new MySqlException("MySQLDriverCS Error: change database failed, perhaps user is not authorized to access that database." + NativeConnection.mysql_error());
            }
        }
        #endregion
        #region CreateCommand 
        /// <summary>
        /// Creates an empty command linked to this connection
        /// </summary>
        /// <returns></returns>
        public new IDbCommand CreateCommand()
        {
            return new MySQLCommand("", this);
        }
        #endregion

        internal NativeConnection NativeConnection = null;
        #region Open
        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString property of the provider-specific Connection object.
        /// </summary>
        public override void Open()
        {
            string database = this.FindValueInCS("Data Source=");
            string location = this.FindValueInCS("Location=");
            string userid = this.FindValueInCS("User ID=");
            string password = this.FindValueInCS("Password=");
            string port = this.FindValueInCS("Port=");
            string characterset = this.FindValueInCS("Character Set=");
            string clientPath = this.FindValueInCS("Client Path=");

            // BEGIN ADDITION 2004-07-01 by Alex Seewald
            string strtimeout = this.FindValueInCS("Timeout=");
            uint timeout = strtimeout != "" ? uint.Parse(strtimeout) : 0;
            bool activate_compression = (this.FindValueInCS("Compression=") != ""); // will default to false if omitted

            if (timeout == 0)
                timeout = 100;  // 100 seconds timeout by default, changed in 2004-07-24 by M.L. Viñas Livschitz in favor of connection pooling
                                // END ADDITION 2004-07-01 by Alex Seewald

            if (port == "")
                port = "3306";


            // HACK/TODO: to remove
            if (NativeConnection == null)
                NativeConnection = new NativeConnection(clientPath);
            else
                throw new MySqlException("Connection already open");




            // BEGIN ADDITION 2004-07-01 adding timeout and compression
            // mysql_options must be called after mysql_init and before mysql_real_connect

            // Timeout option
            NativeConnection.MySqlOptions(0, ref timeout);

            // Compression option
            uint _null = 0;
            if (activate_compression)
                NativeConnection.MySqlOptions(0, ref _null);

            // END ADDITION 2004-07-01

            var retval = NativeConnection.MySqlRealConnect(location, userid, password, database, Convert.ToUInt32(port), null, 0);
            /* Explicit error conection:
			 * "Christophe Ravier" <c.ravier@laposte.net> 2003-11-27*/
            if (retval == null)
            {
                throw new MySqlException(NativeConnection);
            }
            try
            {
                if (!string.IsNullOrEmpty(characterset))
                {
                    if (NativeConnection.mysql_set_character_set(characterset) == 0)
                    {
                        encoding = Encoding.GetEncoding(characterset);
                    }
                }
            }
            catch (EntryPointNotFoundException)
            {
                string version = NativeConnection.GetClientVersion();
                throw new MySqlException(NativeConnection, "'Character Set' keyword not supported in client version " + version);
            }
        }
        #endregion
        #region Close
        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        public override void Close()
        {
            if (NativeConnection == null) return;
            NativeConnection.Dispose();
            NativeConnection = null;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DbCommand CreateDbCommand()
        {
            return new MySQLCommand(null, this);
        }
    }
}
