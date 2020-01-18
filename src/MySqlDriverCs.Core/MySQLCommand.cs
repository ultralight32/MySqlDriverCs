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
	
	***** Update in MySQLCommand in order suport MySQLParameter and MySQLParameterColletion ****
	
	Items add by William Reinoso and Omar del Valle Rodríguez 
	- m_updatedRowSource : UpdateRowSource.None. See UpdateRowSource Enumeration in MSDN help.
	- m_parameters : MySQLParameterCollection
	- IDataParameterCollection IDbCommand.Parameters. See IDataParameterCollection Interface in MSDN help.
	- private _replaceparameters() : Replace parameters values in sql string.
	
	Items update by William Reinoso and Omar del Valle Rodríguez
	- Parameter property. It is read only and now return MySQLParameterCollection
	- Method CreateParameter(). Is now soport and return new MySQLParameter
	- Method ExecuteNonQuery(). Call to _replaceparameters() before execute query.
	- Method ExecuteReader(). Call to _replaceparameters() before execute query.			   
	
	***** Update in MySQLCommand in order suport ExecuteReader(CommandBehavior behavior) and ExecuteScalar ****
	
	Items update by Omar del Valle Rodríguez (01/18/2004)
	- Method ExecuteScalar is now support.
	- ExecuteReader now is declare protected and support CloseConnection parameter
	- Create method public ExecuteReader() with CloseConnection in false
	- Update method ExecuteReader(CommandBehavior behavior) in order support CommandBehavior.CloseConnection

	********************************************************************************************
*/
#endregion
using System.Data;
using System.Data.Common;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
	/// <summary>
	/// This class is IDbCommand compliant so take a look into MSDN help to unserstand how it works
	/// </summary>
	public class MySQLCommand : DbCommand
	{
		private MySQLConnection connection = null;
		internal DbTransaction _Transaction = null;
		internal Statement pStmt;
        private string _query;

        //Add by William Reinoso and Omar del Valle in order soport Parameter Property 
        MySQLParameterCollection m_parameters = new MySQLParameterCollection();

		/// <summary>Initializes a new instance of the MySQLCommand class.</summary>
		public MySQLCommand()
		{
		}
		/// <summary>Initializes a new instance of the MySQLCommand class with the text of the query.</summary>
		/// <param name="cmdText"></param>
		public MySQLCommand(string cmdText)
		{
			_query = cmdText;
		}
		/// <summary>Initializes a new instance of the MySQLCommand class with the text of the query and a MySQLConnection.</summary>
		/// <param name="cmdText"></param>
		/// <param name="connection"></param>
		public MySQLCommand(string cmdText, MySQLConnection connection)
		{
			_query = cmdText;
			this.connection = connection;
		}
		/// <summary>Initializes a new instance of the MySQLCommand class with the text of the query, a MySQLConnection, and the IDbTransaction. 
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="connection"></param>
		/// <param name="transaction"></param>
		public MySQLCommand(string cmdText, MySQLConnection connection, DbTransaction transaction) : this(cmdText, connection)
		{
			_Transaction = transaction;
		}
		/// <summary>Initializes a new instance of the MySQLCommand class with the text of the query, a MySQLConnection, and the IDbTransaction. 
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="connection"></param>
		/// <param name="transaction"></param>
		/// <param name="usePreparedStatement"></param>
		public MySQLCommand(string cmdText, MySQLConnection connection, DbTransaction transaction, bool usePreparedStatement) : this(cmdText, connection, transaction)
		{
			this.UsePreparedStatement = usePreparedStatement;
		}
		/// <summary>
		/// 
		/// </summary>
		public override bool DesignTimeVisible
		{
			get => false;
            set { }
		}

		/// <summary>
		/// Gets or sets the text command to run against the data source.
		/// </summary>
		public override string CommandText { get => _query;
            set => _query = value;
        }
		/// <summary>
		/// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
		/// Set Is unsupported.
		/// </summary>
		public override int CommandTimeout { get => 60;
            set { } }
		/// <summary>
		/// Indicates or specifies how the CommandText property is interpreted.
		/// </summary>
		public override CommandType CommandType { get; set; } = CommandType.Text;

        /// <summary>
		/// Gets or sets the IDbConnection used by this instance of the IDbCommand.
		/// </summary>
		protected override DbConnection DbConnection
		{
			get => connection;
            set
			{
				connection = (MySQLConnection)value;
				if (pStmt != null) pStmt.connection = connection;
			}
		}
		/// <summary>
		/// Return a MySQLParameterCollection.
		/// </summary>
		public new MySQLParameterCollection Parameters => m_parameters;

        /// <summary>
		/// Inherited from IDbCommand 
		/// </summary>
		/// <remarks>Add by William Reinoso and Omar del Valle Rodríguez in order suport Parameter property</remarks>
		protected override DbParameterCollection DbParameterCollection => m_parameters;

        /// <summary>
		/// Sets and gets transaction
		/// </summary>
		protected override DbTransaction DbTransaction { get => _Transaction;
            set => _Transaction = value;
        }

		/// <summary>
		/// UpdateRowSource
		/// </summary>
		public override UpdateRowSource UpdatedRowSource { get; set; } = UpdateRowSource.None;

        /// <summary>
		/// Multithreding operation: cancels current reading.
		/// </summary>
		public override void Cancel()
		{
			if (pStmt != null)
				lock (pStmt)
				{
					pStmt.TryToCancel = true;
				}
			//throw new MySQLException("Operation not supported");
		}

		/// <summary>
		/// Create a new MySQLParameter.
		/// </summary>
		protected override DbParameter CreateDbParameter()
		{
			return new MySQLParameter();
		}

		/// <summary>
		/// Create a new MySQLParameter.
		/// </summary>
		public MySQLParameter CreateMySQLParameter()
		{
			return new MySQLParameter();
		}

		/// <summary>
		/// A server-side cursor allows a result set to be generated on the server side, but not transferred to the client 
		/// except for those rows that the client requests. For example, if a client executes a query but is only 
		/// interested in the first row, the remaining rows are not transferred.
		/// In MySQL, a server-side cursor is materialized into a temporary table. 
		/// Initially, this is a MEMORY table, but is converted to a MyISAM table if its size reaches the value of the 
		/// max_heap_table_size system variable. One limitation of the implementation is that for a large result set, 
		/// retrieving its rows through a cursor might be slow.
		/// Cursors are read-only; you cannot use a cursor to update rows. 
		/// When ServerCursor is true it sets UsePreparedStatement true automatically.
		/// </summary>
		public bool ServerCursor
		{
			set
			{
				if (value)
				{
					if (!UsePreparedStatement)
					{
						UsePreparedStatement = true;
					}
					Stmt.CursorType = (uint)CursorTypes.CURSOR_TYPE_READ_ONLY;
				}
			}
		}
		/// <summary>
		/// Number of rows to fetch from server at a time when using a cursor.
		/// Can be in the range from 1 to the maximum value of UInt32.
		/// </summary>
		public uint FetchSize
		{
			set
			{
				if (!UsePreparedStatement)
				{
					UsePreparedStatement = true;
				}
				Stmt.FetchSize = value;
			}
		}
		/// <summary>
		/// Provides for the use of prepared statements. Prepared execution is an efficient way to execute a statement 
		/// more than once. The statement is first parsed to prepare it for execution. 
		/// Then it is executed one or more times at a later time, using the statement handle returned by the 
		/// initialization function.
		/// Prepared execution is faster than direct execution for statements executed more than once, 
		/// primarily because the query is parsed only once. 
		/// In the case of direct execution, the query is parsed every time it is executed. 
		/// Prepared execution also can provide a reduction of network traffic because for each execution of the 
		/// prepared statement, it is necessary only to send the data for the parameters.
		/// Prepared statements might not provide a performance increase in some situations. 
		/// For best results, test your application both with prepared and non-prepared statements and choose whichever 
		/// yields best performance. Another advantage of prepared statements is that it uses a binary protocol that 
		/// makes data transfer between client and server more efficient. 
		/// The following statements can be used as prepared statements: CREATE TABLE, DELETE, DO, INSERT, REPLACE, SELECT, 
		/// SET, UPDATE, and most SHOW statements. Other statements are not supported in MySQL 5.0. 
		/// </summary>
		/// <remarks>Use parameter markers ('?') for PreparedStatements instead of named parameters</remarks>
		public bool UsePreparedStatement { get; set; } = false;

        private Statement Stmt
		{
			get
			{
				if (pStmt == null)
                {
                    if (connection == null || connection.State != ConnectionState.Open)
					{
						throw new MySqlException("Connection must be valid and open.");
					}

                    pStmt = UsePreparedStatement ? (Statement) new PreparedStatement(connection, _query) : new SqlInjectedParametersStatement(connection, _query);
                }
				return pStmt;
			}

		}

		/// <summary>
		/// Executes a SQL statement against the Connection object, and returns updated rows count.
		/// </summary>
		/// <returns></returns>
		public override int ExecuteNonQuery()
		{
			if (connection == null || connection.State != ConnectionState.Open)
			{
				throw new MySqlException("Connection must be valid and open.");
			}
			Stmt.Parameters = m_parameters;
			if (CommandType == CommandType.StoredProcedure)
				return Stmt.ExecuteCall();
			else
				return Stmt.ExecuteNonQuery();
		}

		/// <summary>
		/// Executes the CommandText against the Connection and builds an MySQLDataReader.
		/// Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx). 
		/// </summary>
		/// <returns>MySQLDataReader</returns>
		public MySQLDataReader ExecuteReaderEx()
		{
			return (MySQLDataReader)ExecuteReader(false);
		}

		/// <summary>
		/// Update to Overloaded protected by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx). 
		/// Executes the CommandText against the Connection and builds an IDataReader.
		/// </summary>
		/// <remarks>Use <c>ExecuteReaderEx</c> to avoid conversions.</remarks>
		/// <param name="CloseConnection">Equal to true by ExecuteReader(CommandBehavior behavior)</param>
		/// <returns>IDataReader</returns>
		protected DbDataReader ExecuteReader(bool CloseConnection)
		{
			if ((connection == null) || (connection.State == ConnectionState.Closed)) { throw new MySqlException("Connection must be open"); }
			Stmt.Parameters = m_parameters;
			return Stmt.ExecuteReader(CloseConnection);
		}

		

		/// <summary>
		/// Overloaded. Executes the CommandText against the Connection and builds an IDataReader.
		/// Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx).
		/// </summary>
		/// <param name="behavior">Only support CommandBehavior.CloseConnection</param>
		/// <returns>IDataReader</returns>
		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			// Close connection if CommandBehavior is set to CloseConnection  
			// Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx).
			return ExecuteReader(((behavior & CommandBehavior.CloseConnection) > 0));
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the resultset returned by the query. 
		/// Extra columns or rows are ignored.
		/// </summary>
		public override object ExecuteScalar()
		{
            using (IDataReader dr = ExecuteReader())
            {
                if (dr.Read()) 
                   return dr.GetValue(0);
            }

            return null;
		}
		/// <summary>
		/// Executes the query and loads output parameter values.
		/// </summary>
		public int ExecuteCall()
		{
			Stmt.Parameters = m_parameters;
			Stmt.ExecuteCall();
			return 0;
		}

		/// <summary>
		/// Prepares an SQL string for execution.
		/// </summary>
		public override void Prepare()
		{
			if (connection == null || connection.State != ConnectionState.Open)
			{
				throw new MySqlException("Connection must be valid and open.");
			}
			if (_query == null)
			{
				throw new MySqlException("Invalid query.");
			}

			Stmt.Prepare();
		}
		private bool _disposed = false;
		/// <summary>
		/// Dispose command
		/// Modified by Claudia Murialdo (05/18/06) in order to allow disposing
		/// commands without doing rollback on the transaction.
		/// </summary>
		public new void Dispose()
		{
			if (_disposed) return;
			Stmt.Dispose();
			_Transaction = null;
			connection = null;
			_disposed = true;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			this.Dispose();
		}
	}
}
