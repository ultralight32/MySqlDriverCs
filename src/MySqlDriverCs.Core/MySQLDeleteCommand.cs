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
	/// A shortcut to make a simple delete command.
	/// </summary>
	/// <example>Example:<code>
	/// using MySQLDriverCS;
	/// ...
	/// MySQLConnection DBConn;
	/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
	/// DBConn.Open();
	/// ...
	/// new MySQLDeleteCommand(DBConn,"Trash",null,null);
	/// ...
	/// DBConn.Close();
	/// </code></example>
	public class MySQLDeleteCommand
	{
		/// <summary>
		/// This flags is true if operation was successful.
		/// </summary>
		public bool bSuccess=false;
		/// <summary>
		/// The SQL string used for this command (useful for debugging).
		/// </summary>
		public string Query;
		/// <summary>
		/// Construct and Run a simple delete command.
		/// </summary>
		/// <param name="conn">Database connection</param>
		/// <param name="Table">Table name</param>
		/// <param name="WhereParamFields">Tripplets of parameters, operand and its values to match (null - nothing). For example: 
		/// <code>
		/// new object[,]{{"myfield","=",myvalue},{"myfield2","&lt;&gt;",myvalue2}}
		///  
		/// new object[,]{{"myfield","=",100}}
		///  
		/// new object[,]{{"myfield","=",myvalue}}
		/// 
		/// null
		/// </code>
		/// </param>
		/// <param name="WhereNoparamFields">Tripplets of parameters, operand and expressions (or other fields for joining) to match (null - nothing). For example: 
		/// <code>
		/// new object[,]{{"myfield","=","myfield2"}}
		/// 
		/// null
		/// </code>
		/// </param>
		/// <example>Example:<code>
		/// using MySQLDriverCS;
		/// ...
		/// MySQLConnection DBConn;
		/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
		/// DBConn.Open();
		/// ...
		/// new MySQLDeleteCommand(
		///		DBConn,
		///		"Trash",
		///		new object[,]{{"myfield","=",myvalue}},
		///		new object[,]{{"myfield","=","myfield2"}}
		///		);
		/// ...
		/// new MySQLDeleteCommand(
		///		DBConn,
		///		"Trash",
		///		new object[,]{{"myfield2","=",myvalue2},{"myfield3","&lt;&gt;",myvalue3}},
		///		null
		///		);
		/// ...
		/// DBConn.Close();
		/// </code></example> 
		public MySQLDeleteCommand(
			MySQLConnection conn,
			string Table,
			object[,] WhereParamFields, 
			string[,] WhereNoparamFields 
			)
		{
			string query = "delete from "+conn.Database+"."+Table+" ";
			query += MySQLSelectCommand.WhereQueryPart(WhereParamFields,WhereNoparamFields,"WHERE", conn) + " ";

			MySQLCommand command = new MySQLCommand(query, conn);
			command.Prepare();
			// Execute query ->
			try
			{
				command.ExecuteNonQuery();
			}
			catch(Exception e)
			{
				throw new MySQLException(e.Message+" in query '"+query+"'");
			}
			// <- Execute query
			command.Dispose();
			Query=query;
			bSuccess=true;
		}
	}
}
