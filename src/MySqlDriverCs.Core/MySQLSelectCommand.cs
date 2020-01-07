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
using System.Data;
using System.Globalization;
namespace MySQLDriverCS
{
	/// <summary>
	/// A shortcut to make a simple select command.
	/// </summary>
	/// <example>Example looking for SettingValue by SettingID<code>
	/// using MySQLDriverCS;
	/// ...
	/// MySQLConnection DBConn;
	/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
	/// DBConn.Open();
	/// ...
	/// DataTable dt = new MySQLSelectCommand(DBConn,
	///		new string[] {"SettingID","SettingValue"},
	///		new string[] {"Settings"},
	///		new object[,] {{"SettingID","=",SettingID}},
	///		null,
	///		null
	///	).Table;
	///	string Value = null;
	///	if(dt.Rows.Count!=0)
	///	{
	///		Value = dt.Rows[0]["SettingValue] as string;
	///	}
	///	else
	///	{
	///		// Value not found
	///	}
	/// ...
	/// DBConn.Close();
	/// </code></example>
	public class MySQLSelectCommand
	{
		/// <summary>
		/// The full result rowset as a DataTable
		/// </summary>
		public DataTable Table;
		/// <summary>
		/// A shortcut to make a simple select command with where and sorting part. (where and sorting would be null)
		/// </summary>
		/// <param name="conn">Database connection</param>
		/// <param name="Tables">An array of table names as strings. Examples:
		/// <code>
		/// new string[]{"trash","users"}
		/// 
		/// new string[]{"trash",users}
		/// 
		/// new string[]{"trash"}
		/// 
		/// null
		/// </code>
		/// </param>
		/// <param name="Fields">An array of field names (or expressions) as strings. Examples:
		/// <code>
		/// new string[]{"id","name"}
		/// 
		/// new string[]{"id",name}
		/// 
		/// new string[]{"id"}
		/// 
		/// null
		/// </code>
		/// </param>
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
		/// <param name="Sorting">Pairs of {field,sorttype} as an string array. For example:
		/// <code>
		/// new string[] {{"SettingID","ASC"},{"SettingValue","DESC"}}
		/// 
		/// new string[] {{"SettingID","ASC"}}
		/// 
		/// null
		/// </code></param>
		/// <example>Example looking for SettingValue by SettingID<code>
		/// using MySQLDriverCS;
		/// ...
		/// MySQLConnection DBConn;
		/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
		/// DBConn.Open();
		/// ...
		/// DataTable dt = new MySQLSelectCommand(DBConn,
		///		new string[] {"SettingID","SettingValue"},
		///		new string[] {"Settings"},
		///		new object[,] {{"SettingID","=",SettingID}},
		///		null,
		///		null
		///	).Table;
		///	string Value = null;
		///	if(dt.Rows.Count!=0)
		///	{
		///		Value = dt.Rows[0]["SettingValue] as string;
		///	}
		///	else
		///	{
		///		// Value not found
		///	}
		/// ...
		/// DBConn.Close();
		/// </code></example>
		public MySQLSelectCommand(
			MySQLConnection conn, // Database connection
			string[] Fields, // Fields to show (also admit expressions)
			string[] Tables, // Tables
			object[,] WhereParamFields, // Tripplets of parameters, operand and its values to match (null - nothing)
			string[,] WhereNoparamFields, // Tripplets of parameters, operand and expressions (or other fileds for joining) to match (null - nothing)
			string[,] Sorting // Pairs of sortings and its kind of sorting EJ: Priority, ASC (sorts ascending by priority)
			)
		{
			DoSelect(
				conn, // Database connection
				Fields, // Fields to show (also admit expressions)
				Tables, // Tables
				WhereParamFields, // Tripplets of parameters, operand and its values to match (null - nothing)
				WhereNoparamFields, // Tripplets of parameters, operand and expressions (or other fileds for joining) to match (null - nothing)
				Sorting, // Pairs of sortings and its kind of sorting EJ: Priority, ASC (sorts ascending by priority)
				false,0,0,false
				);
		}
		/// <summary>
		/// A shortcut to make a simple select command with where, sorting part, and limit usage. (where and sorting would be null, and limits false)
		/// </summary>
		/// <param name="conn">Database connection</param>
		/// <param name="Tables">An array of table names as strings. Examples:
		/// <code>
		/// new string[]{"trash","users"}
		/// 
		/// new string[]{"trash",users}
		/// 
		/// new string[]{"trash"}
		/// 
		/// null
		/// </code>
		/// </param>
		/// <param name="Fields">An array of field names (or expressions) as strings. Examples:
		/// <code>
		/// new string[]{"id","name"}
		/// 
		/// new string[]{"id",name}
		/// 
		/// new string[]{"id"}
		/// 
		/// null
		/// </code>
		/// </param>
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
		/// <param name="Sorting">Pairs of {field,sorttype} as an string array. For example:
		/// <code>
		/// new string[] {{"SettingID","ASC"},{"SettingValue","DESC"}}
		/// 
		/// new string[] {{"SettingID","ASC"}}
		/// 
		/// null
		/// </code></param>
		/// <param name="useLimits">True if using limits</param>
		/// <param name="limitStartRow">Start row number, 0 by default</param>
		/// <param name="limitRows">How many rows are show (maximun). Use <code>MySQLSelectCommand(..,true,0,1)</code> for expecting one value.</param>
		/// <param name="Distinct">Distinct is active</param>
		/// <example>Example looking for SettingValue by SettingID<code>
		/// using MySQLDriverCS;
		/// ...
		/// MySQLConnection DBConn;
		/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
		/// DBConn.Open();
		/// ...
		/// DataTable dt = new MySQLSelectCommand(DBConn,
		///		new string[] {"SettingID","SettingValue"},
		///		new string[] {"Settings"},
		///		new object[,] {{"SettingID","=",SettingID}},
		///		null,
		///		null
		///	).Table;
		///	string Value = null;
		///	if(dt.Rows.Count!=0)
		///	{
		///		Value = dt.Rows[0]["SettingValue] as string;
		///	}
		///	else
		///	{
		///		// Value not found
		///	}
		/// ...
		/// DBConn.Close();
		/// </code></example>
		public MySQLSelectCommand(
			MySQLConnection conn, // Database connection
			string[] Fields, // Fields to show (also admit expressions)
			string[] Tables, // Tables
			object[,] WhereParamFields, // Tripplets of parameters, operand and its values to match (null - nothing)
			string[,] WhereNoparamFields, // Tripplets of parameters, operand and expressions (or other fileds for joining) to match (null - nothing)
			string[,] Sorting, // Pairs of sortings and its kind of sorting EJ: Priority, ASC (sorts ascending by priority)
			bool useLimits, int limitStartRow, int limitRows, bool Distinct
			)
		{
			DoSelect(
				conn, // Database connection
				Fields, // Fields to show (also admit expressions)
				Tables, // Tables
				WhereParamFields, // Tripplets of parameters, operand and its values to match (null - nothing)
				WhereNoparamFields, // Tripplets of parameters, operand and expressions (or other fileds for joining) to match (null - nothing)
				Sorting, // Pairs of sortings and its kind of sorting EJ: Priority, ASC (sorts ascending by priority)
				useLimits,limitStartRow,limitRows,Distinct
				);
		}
		internal void DoSelect(
			MySQLConnection conn, // Database connection
			string[] Fields, // Fields to show (also admit expressions)
			string[] Tables, // Tables
			object[,] WhereParamFields, // Tripplets of parameters, operand and its values to match (null - nothing)
			string[,] WhereNoparamFields, // Tripplets of parameters, operand and expressions (or other fileds for joining) to match (null - nothing)
			string[,] Sorting, // Pairs of sortings and its kind of sorting EJ: Priority, ASC (sorts ascending by priority)
			bool useLimits, int limitStartRow, int limitRows, bool Distinct
			)
		{
			string query = "SELECT ";
			int n;

			// Text Building ->
			if(Distinct)
				query+="DISTINCT ";
			for(n=0;n<Fields.Length;n++)
			{
				query += " "+Fields[n].ToString();
				if(n!=(Fields.Length-1)) query+=",";
			}

			if(Tables!=null)
				query += " FROM ";
			else
				Tables = new string[0];

			for(n=0;n<Tables.Length;n++)
			{
				query += " "+conn.Database+"."+Tables[n].ToString();
				if(n!=(Tables.Length-1)) query+=",";
			}

			query += WhereQueryPart(WhereParamFields,WhereNoparamFields,"WHERE", conn);

			if((Sorting!=null)&&(Sorting.Length!=0))
			{
				query += " ORDER BY ";
				n = Sorting.Length/2;
				for(int m=0; m<(Sorting.Length/2); m++)
				{
					n--;
					query += " "+Sorting[m,0].ToString()+" "+Sorting[m,1].ToString();
					if(n!=0) query+=",";
				}
			}
			if(useLimits)
			{
				query+=" LIMIT "+limitStartRow.ToString()+","+limitRows.ToString();
			}
			// <- Text Building
			Exec(query,conn, Fields);
		}
		void Exec(string query, MySQLConnection conn, string[] Fields)
		{
			int n;
			MySQLCommand command = new MySQLCommand(query, conn);
			//GDBUtils.WhereCommandPart(ref command,WhereParamFields);
			command.Prepare();

			DataTable table;
			MySQLDataReader reader = null;
			// Execute query ->
			try
			{
				reader = (MySQLDataReader)command.ExecuteReader(/*CommandBehavior.SequentialAccess*/);


				// Get results ->
				table = new DataTable(null);

				// Bugfixed by Yann Sénécheau 2002-10-28 ->
//				for(n=0;n<Fields.Length;n++)
//				{
//					table.Columns.Add(Fields[n].ToString());
//				}
				foreach( System.Data.DataColumn dc in reader.dt.Columns ) 
				{
					String s = dc.ToString();
					System.Data.DataColumn dcNew = new System.Data.DataColumn( s );
					table.Columns.Add( dcNew );
				}
				// <- Bugfixed by Yann Sénécheau 2002-10-28

				while(reader.Read())
				{
					if(reader.IsClosed)
						throw new MySQLException("Reader is closed.");
					DataRow row = table.NewRow();
					// Bugfixed by Yann Sénécheau 2002-10-28 ->
//					for(n=0;n<Fields.Length;n++)
					for(n=0;n<reader.dt.Columns.Count;n++)
					// <- Bugfixed by Yann Sénécheau 2002-10-28
					{
						if(n<reader.FieldCount)
						{
							if(reader.GetValue(n)==null)
								row[n] = null;
							else
								if(reader.GetFieldType(n)==typeof(string))
								row[n]=reader.GetString(n).Clone();
							else
								row[n]=reader.GetValue(n);
						}
						else
						{
							break;
						}
					}
					table.Rows.Add(row);
				}
				// <- Get results
			}
			catch(Exception e)
			{
				throw new MySQLException(e.Message+" in query '"+query+"'");
			}
			finally
			{
				if(reader!=null)
					reader.Close();
				command.Dispose();
			}
			// <- Execute query
			Table=table;
		}
		/// Modified by Claudia Murialdo (07/24/04) in order to support
		/// culture-independent format of numeric values in a stmt.
		internal static string WhereQueryPart(object[,] WhereParamFields, string[,] WhereNoparamFields, string prefix, MySQLConnection conn)
		{
			string query=""; int n;
			bool Anything=false;
			if((WhereParamFields!=null)&&(WhereParamFields.Length!=0))
			{
				Anything=true;
				n = WhereParamFields.GetLength(0);
				for(int m=0; m<WhereParamFields.GetLength(0); m++)
				{
					n--;
					string Field = WhereParamFields[m,0] as string;
					string Operand = WhereParamFields[m,1] as string;
					object Value = WhereParamFields[m,2];
					if(Value==null)
						query += " "+Field+" "+Operand+" NULL ";
					else
						if( (Value.GetType()==typeof(int))||
						(Value.GetType()==typeof(long))||
						(Value.GetType()==typeof(short))||
						(Value.GetType()==typeof(decimal))||
						(Value.GetType()==typeof(float))||
						(Value.GetType()==typeof(double)) )
					{
						query += " "+Field+" "+Operand+" "+Convert.ToString(Value,CultureInfo.InvariantCulture.NumberFormat)+" ";
					}
					else
					{
						query += " "+Field+" "+Operand+" "+MySQLUtils.Escape(Value.ToString(), conn)+" ";
					}
					if(n!=0) query+=" AND ";
				}
			}
			if((WhereNoparamFields!=null)&&(WhereNoparamFields.Length!=0))
			{
				if(Anything) query+=" AND ";
				Anything=true;
				n = WhereNoparamFields.GetLength(0);
				for(int m=0; m<WhereNoparamFields.GetLength(0); m++)
				{
					n--;
					string Field = WhereNoparamFields[m,0] as string;
					string Operand = WhereNoparamFields[m,1] as string;
					object Value = WhereNoparamFields[m,2];
					if(Value==null)
						query += " "+Field+" "+Operand+" NULL ";
					else
						query += " "+Field+" "+Operand+" "+Value.ToString()+" ";
					if(n!=0) query+=" AND ";
				}
			}
			if(Anything)
				query = " "+prefix+" "+query;
			return query;
		}
		/// <summary>
		/// Extended SELECT with all settings (Except INTO OUTFILE/DUMPFILE for security reasons)
		/// </summary>
		/// <param name="conn">Database connection</param>
		/// <param name="PrevFlags">Flags used in SELECT: [STRAIGHT_JOIN] [SQL_SMALL_RESULT] [SQL_BIG_RESULT] [SQL_BUFFER_RESULT] [SQL_CACHE | SQL_NO_CACHE] [SQL_CALC_FOUND_ROWS] [HIGH_PRIORITY] [DISTINCT | DISTINCTROW | ALL]</param>
		/// <param name="FieldsAndAlias">fields like {{"f1"},{"f2"}} or {{"f1","aliasf1"},...}</param>
		/// <param name="FieldsTitles">If it's null they're retrieved from FieldsAndAlias. Useful for re-aliasing and required if you use AS or explicit index using.</param>
		/// <param name="Tables">Table list</param>
		/// <param name="WhereValues">Like {{field,op,value}...}</param>
		/// <param name="WhereExpressions">Like {{expr1,op,expr2}}</param>
		/// <param name="GroupBy">Like {{field}...} or {{field,ASC|DESC}...}</param>
		/// <param name="GroupByHavingValues">like {{field,op,value}...}</param>
		/// <param name="GroupByHavingExpressions">Like {expr1,expr2}</param>
		/// <param name="OrderBy">Like {{field}...} or {{field,ASC|DESC}...}</param>
		/// <param name="Limit">Like null, new int[]{}, new int{a number} (rows), or pair of numbers new int[]{offset,rows}</param>
		/// <param name="LastFlags">this includes PROCEDURE, FOR UPDATE and LOCK IN SHARE MODE</param>
		public MySQLSelectCommand(
			MySQLConnection conn, // Database connection
			string PrevFlags, // select flags like SQL_CACHE or SQL_BUFFER_RESULT or DISTINCT
			string[,] FieldsAndAlias, // fields like {{"f1"},{"f2"}} or {{"f1","aliasf1"},...}
			string[] FieldsTitles, 
			string[] Tables, // Table list 
			object[,] WhereValues, // like {{field,op,value}...}
			string[,] WhereExpressions, // Like {{expr1,op,expr2}}
			string[] GroupBy, // Like {"field "+"ASC"|"DESC"}...
			object[,] GroupByHavingValues, // like {{field,op,value}...}
			string[,] GroupByHavingExpressions, // Like {{expr1,op,expr2}}
			string[] OrderBy, // Like {"field "+"ASC"|"DESC"}...
			int[] Limit, // Like null, empty, a number(rows), or pair of numbers(offset,rows)
			string LastFlags // this includes PROCEDURE, FOR UPDATE and LOCK IN SHARE MODE
		)
		{
			string query=PrevFlags; int n;
			string[] Fields;
			if(FieldsTitles!=null) Fields=FieldsTitles; 
			else 
			{
				Fields = new string[FieldsAndAlias.GetLength(0)];
				for(n=0;n<Fields.Length;n++)
					Fields[n]=FieldsAndAlias[n,0];
			}
			// FieldsAndAlias
			if(true)
			{
				for(int i=0; i<FieldsAndAlias.GetLength(0);i++)
				{
					query+=" "+FieldsAndAlias[i,0];
					if(FieldsAndAlias[i,1]!=null)
						query+=" AS "+FieldsAndAlias[i,1];
				}
			}
			// Tables
			if(true)
			{
				query+=" FROM";
				for(n=0;n<Tables.Length;n++)
				{
					query += " "+conn.Database+"."+Tables[n].ToString();
					if(n!=(Tables.Length-1)) query+=",";
				}
			}
			// Where
			query += WhereQueryPart(WhereValues,WhereExpressions,"WHERE", conn);
			// Group by
			if(true)
			{
				if(GroupBy!=null)
				{
					for(n=0;n<GroupBy.Length;n++)
					{
						query += " "+GroupBy[n].ToString();
						if(n!=(GroupBy.Length-1)) query+=",";
					}
					// Having
					query += WhereQueryPart(GroupByHavingValues,GroupByHavingExpressions,"HAVING", conn);
				}
			}
			// Order by
			if(OrderBy!=null)
				for(n=0;n<OrderBy.Length;n++)
				{
					query += " "+OrderBy[n].ToString();
					if(n!=(OrderBy.Length-1)) query+=",";
				}
			// Limit
			if(Limit!=null)
			{
				if(Limit.GetLength(0)==1)
					query+=" LIMIT "+Limit[0];
				else
					query+=" LIMIT "+Limit[0]+","+Limit[1];
			}
			query+=" "+LastFlags;
			Exec(query,conn,Fields);
		}

	}
}
