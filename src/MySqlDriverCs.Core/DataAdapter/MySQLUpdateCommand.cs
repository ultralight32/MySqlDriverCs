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
using System.Collections;
namespace MySQLDriverCS
{
	/// <summary>
	/// A shortcut to make a simple update command.
	/// </summary>
	/// <example>Example updating SettingValue by SettingID in Settings table<code>
	/// using MySQLDriverCS;
	/// ...
	/// MySQLConnection DBConn;
	/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
	/// DBConn.Open();
	/// ...
	/// new MySQLUpdateCommand(DBConn,
	///		new object[,] {{"SettingValue",Value}},
	///		"Settings",
	///		new object[,] {{"SettingID","=",SettingID}},
	///		null
	/// );
	/// ...
	/// DBConn.Close();
	/// </code></example> 
	public class MySQLUpdateCommand
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
		///  A shortcut to make a simple update command with where part. (where would be null)
		/// </summary>
		/// <param name="conn">Database connection</param>
		/// <param name="FieldsAndValues">Pairs of Fields and values as an object array. Examples:
		/// <code>
		/// new object[,]{{"SettingID",SettingID},{"SettingValue",Value}}
		/// 
		/// new object[,]{{"SettingID","times"},{"SettingValue",100}}
		/// 
		/// null
		/// </code>
		/// </param>
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
		/// <example>Example updating SettingValue by SettingID in Settings table<code>
		/// using MySQLDriverCS;
		/// ...
		/// MySQLConnection DBConn;
		/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
		/// DBConn.Open();
		/// ...
		/// new MySQLUpdateCommand(DBConn,
		///		new object[,] {{"SettingValue",Value}},
		///		"Settings",
		///		new object[,] {{"SettingID","=",SettingID}},
		///		null
		/// );
		/// ...
		/// DBConn.Close();
		/// </code></example> 
		/// Modified by Claudia Murialdo (07/24/04) in order to support time 
		/// component of the datetime values.
		public MySQLUpdateCommand(
			MySQLConnection conn, // Database connection
			object[,] FieldsAndValues, // Pairs of Fields and values
			string Table, // Table
			object[,] WhereParamFields, // Tripplets of parameters, operand and its values to match (null - nothing)
			string[,] WhereNoparamFields // Tripplets of parameters, operand and expressions (or other fields for joining) to match (null - nothing)
			)
		{
			if(FieldsAndValues==null)
				throw new MySqlException("FieldsAndValues is null.");
			if(FieldsAndValues.GetLength(0)==0)
				throw new MySqlException("FieldsAndValues is empty.");
			string query = "update "+conn.Database+"."+Table+" set ";
			ArrayList NewValues = new ArrayList();
			for(int m=0; m<FieldsAndValues.GetLength(0); m++)
			{
				string Field = FieldsAndValues[m,0] as string;
				object Value = FieldsAndValues[m,1];
				if(Value==null)
					query += " "+Field+"=NULL ";
				else
					if(Value.GetType()==typeof(DateTime))
				{
					DateTime dt = (DateTime)Value;
					query += " "+Field+"=\""+dt.Year.ToString("D4")+"-"+dt.Month.ToString("D2")+"-"+dt.Day.ToString("D2")+
						" "+dt.Hour+":"+dt.Minute+":"+dt.Second + ((dt.Millisecond > 0) ? "." + dt.Millisecond.ToString("D3") : "") + "\" ";
				}
				else
					if(Value.GetType()==typeof(bool))
				{
					bool bValue = (bool) Value;
					string str=(bValue)? "1" : "0" ;
					query += " " + Field+ "=" + str + " ";
				}
				else
					if(Value.GetType()==typeof(string))
				{
					string str = Value as string;
					query += " "+Field+"="+MySQLUtils.Escape(str, conn)+" ";
				}
			
				else
					if(Value.GetType()==typeof(int))
				{
					query += " "+Field+"="+((int)Value).ToString()+" ";
				}

				if(m!=(FieldsAndValues.GetLength(0)-1)) query+=", ";
			}
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
				throw new MySqlException(e.Message+" in query '"+query+"'");
			}
			// <- Execute query
			command.Dispose();
			Query=query;
			bSuccess=true;
		}
	}
}
