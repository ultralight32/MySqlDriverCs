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
using System.Globalization;

namespace MySQLDriverCS
{
	/// <summary>A shortcut to make a simple insert command.</summary>
	/// <example>Example:
	/// <code>
	/// using MySQLDriverCS;
	/// ...
	/// MySQLConnection DBConn;
	/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
	/// DBConn.Open();
	/// ...
	/// new MySQLInsertCommand(
	///		DBConn,
	///		new object[,] {
	///			{"SettingID",SettingID},
	///			{"SettingValue",Value}
	///		},
	///		"Settings"
	/// );
	/// ...
	/// DBConn.Close();
	/// </code></example>
	/// 
	public class MySQLInsertCommand
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
		/// Construct and Run a simple insert command.
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
		/// <example>Example:
		/// <code>
		/// using MySQLDriverCS;
		/// ...
		/// MySQLConnection DBConn;
		/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
		/// DBConn.Open();
		/// ...
		/// new MySQLInsertCommand(
		///		DBConn,
		///		new object[,] {
		///			{"SettingID",SettingID},
		///			{"SettingValue",Value}
		///		},
		///		"Settings"
		/// );
		/// ...
		/// DBConn.Close();
		/// </code></example>

		/// Modified by Claudia Murialdo (07/24/04) in order to support time 
		/// component of the datetime values and culture-independent 
		/// format of numeric values in a stmt.
		public MySQLInsertCommand(
			MySQLConnection conn, 
			object[,] FieldsAndValues, 
			string Table
			)
		{
			if(FieldsAndValues==null)
				throw new MySQLException("FieldsAndValues is null.");
			if(FieldsAndValues.GetLength(0)==0)
				throw new MySQLException("FieldsAndValues is empty.");
			string query = "insert into "+Table+" ( ";
			int m;
			for(m=0; m<FieldsAndValues.GetLength(0); m++)
			{
				string Field = FieldsAndValues[m,0] as string;
				query += " "+Field.ToString()+" ";
				if(m!=(FieldsAndValues.GetLength(0)-1)) query+=" , ";
			}
			query+=" ) VALUES ( ";

			for(m=0; m<FieldsAndValues.GetLength(0); m++)
			{
				string Field = FieldsAndValues[m,0] as string;
				object Value = FieldsAndValues[m,1];
				if(Value==null)
					query += " NULL ";
				else
					if( (Value.GetType()==typeof(int))||
					(Value.GetType()==typeof(long))||
					(Value.GetType()==typeof(short))||
					(Value.GetType()==typeof(decimal))||
					(Value.GetType()==typeof(float))||
					(Value.GetType()==typeof(double)) )
					query += " "+Convert.ToString(Value,CultureInfo.InvariantCulture.NumberFormat)+" ";
				else
					if(Value.GetType()==typeof(bool))
				{
					bool bValue = (bool)Value;
					string str=(bValue)? "1" : "0" ;
					query += " " + str + " ";
				}
				else
					if(Value.GetType()==typeof(DateTime))
				    {
					    DateTime dt = (DateTime)Value;
					    query += " \""+dt.Year.ToString("D4")+"-"+dt.Month.ToString("D2")+"-"+dt.Day.ToString("D2")+
						    " "+dt.Hour+":"+dt.Minute+":"+dt.Second + ((dt.Millisecond > 0)? "." + dt.Millisecond.ToString("D3") : "" ) +  "\" ";
				   }
				   else
				   {
					    query += " "+MySQLUtils.Escape(Value.ToString(), conn)+" ";
				   }
				if(m!=(FieldsAndValues.GetLength(0)-1)) query+=" , ";
			}
			query += ") ;";

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
