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
using System;
namespace MySQLDriverCS
{
	/// <summary>
	/// This utility class is used in order to make string connection writting easier.
	/// </summary>
	/// <example>
	/// using MySQLDriverCS;
	/// ...
	/// MySQLConnection DBConn;
	/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
	/// DBConn.Open();
	/// ...
	/// DBConn.Close();
	/// </example>
	public class MySQLConnectionString
	{
		/// <summary>
		/// The connection string as a C# string.
		/// </summary>
		public string AsString;
		/// <summary>
		/// Constructor that takes the most important four arguments.
		/// </summary>
		/// <param name="server">The server name or IP.</param>
		/// <param name="database">The database name.</param>
		/// <param name="login">The login name.</param>
		/// <param name="pass">The password.</param>
		/// <param name="port">Port</param>
		/// <example>
		/// using MySQLDriverCS;
		/// ...
		/// MySQLConnection DBConn;
		/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword",3306).AsString );
		/// DBConn.Open();
		/// ...
		/// DBConn.Close();
		/// </example>
		public MySQLConnectionString(string server, string database, string login, string pass, int port)
		{
			Init( server,  database,  login,  pass, port);
		}
		/// <summary>
		/// Constructor that takes the most important four arguments.
		/// </summary>
		/// <param name="server">The server name or IP.</param>
		/// <param name="database">The database name.</param>
		/// <param name="login">The login name.</param>
		/// <param name="pass">The password.</param>
		/// <example>
		/// using MySQLDriverCS;
		/// ...
		/// MySQLConnection DBConn;
		/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
		/// DBConn.Open();
		/// ...
		/// DBConn.Close();
		/// </example>
		public MySQLConnectionString(string server, string database, string login, string pass)
		{
			Init( server,  database,  login,  pass,3306);
		}
		/// <summary>
		/// Constructor that takes database name, login, password and assumes localhost.
		/// </summary>
		/// <param name="database">The database name.</param>
		/// <param name="login">The login name.</param>
		/// <param name="pass">The password.</param>
		/// <example>
		/// using MySQLDriverCS;
		/// ...
		/// MySQLConnection DBConn;
		/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
		/// DBConn.Open();
		/// ...
		/// DBConn.Close();
		/// </example> 
		public MySQLConnectionString(string database, string login, string pass)
		{
			Init( "localhost",  database,  login,  pass,3306);
		}
		/// <summary>
		/// The real connection maker.
		/// </summary>
		/// <param name="server"></param>
		/// <param name="database"></param>
		/// <param name="login"></param>
		/// <param name="pass"></param>
		/// <param name="port"></param>
		internal void Init(string server, string database, string login, string pass, int port)
		{
			AsString = "Data Source="+database+";"+
				"Password="+pass+";"+
				"User ID="+login+";"+
				"Location="+server+";"+
				"Port="+port+";"+
				"Extended Properties=\"\"";
		}
	}
}
