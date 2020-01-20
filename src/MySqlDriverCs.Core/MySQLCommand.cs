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

#endregion LICENSE

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace MySQLDriverCS
{
    /// <summary>
    /// This class is IDbCommand compliant so take a look into MSDN help to unserstand how it works
    /// </summary>
    public class MySQLCommand : IDbCommand
    {
        private MySQLConnection _connection = null;
        private MySQLTransaction _transaction = null;


        /// <summary>Initializes a new instance of the MySQLCommand class.</summary>
        public MySQLCommand()
        {
        }

        /// <summary>Initializes a new instance of the MySQLCommand class with the text of the query.</summary>
        /// <param name="cmdText"></param>
        public MySQLCommand(string cmdText)
        {
            CommandText = cmdText;
        }

        /// <summary>Initializes a new instance of the MySQLCommand class with the text of the query and a MySQLConnection.</summary>
        /// <param name="cmdText"></param>
        /// <param name="connection"></param>
        public MySQLCommand(string cmdText, MySQLConnection connection)
        {
            CommandText = cmdText;
            this._connection = connection;
        }

        /// <summary>Initializes a new instance of the MySQLCommand class with the text of the query, a MySQLConnection, and the IDbTransaction.
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        public MySQLCommand(string cmdText, MySQLConnection connection, MySQLTransaction transaction) : this(cmdText, connection)
        {
            _transaction = transaction;
        }

        /// <summary>Initializes a new instance of the MySQLCommand class with the text of the query, a MySQLConnection, and the IDbTransaction.
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="usePreparedStatement"></param>
        public MySQLCommand(string cmdText, MySQLConnection connection, MySQLTransaction transaction, bool usePreparedStatement) : this(cmdText, connection, transaction)
        {
            this.UsePreparedStatement = usePreparedStatement;
        }

        /// <summary>
		/// Gets or sets the text command to run against the data source.
		/// </summary>
		public string CommandText { get; set; }

        /// <summary>
		/// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
		/// Set Is unsupported.
		/// </summary>
		public int CommandTimeout
        {
            get => 60;
            set { }
        }

        /// <summary>
        /// Indicates or specifies how the CommandText property is interpreted.
        /// </summary>
        public CommandType CommandType { get; set; } = CommandType.Text;

        /// <summary>
		/// Gets or sets the IDbConnection used by this instance of the IDbCommand.
		/// </summary>
		IDbConnection IDbCommand.Connection
        {
            get => _connection;
            set => SetConnection((MySQLConnection)value);
        }

        public MySQLConnection Connection
        {
            get => _connection;
            set => SetConnection(value);
        }

        private void SetConnection(MySQLConnection value)
        {
            _connection = value;

        }

        /// <summary>
		/// Return a MySQLParameterCollection.
		/// </summary>
		public MySQLParameterCollection Parameters { get; } = new MySQLParameterCollection();

        IDataParameterCollection IDbCommand.Parameters => Parameters;

        IDbTransaction IDbCommand.Transaction
        {
            get => _transaction;
            set => _transaction = (MySQLTransaction)value;
        }

        /// <summary>
        /// Sets and gets transaction
        /// </summary>
        public MySQLTransaction DbTransaction
        {
            get => _transaction;
            set => _transaction = value;
        }

        /// <summary>
        /// UpdateRowSource
        /// </summary>
        public UpdateRowSource UpdatedRowSource { get; set; } = UpdateRowSource.None;

        /// <summary>
		/// Multithreding operation: cancels current reading.
		/// </summary>
		public void Cancel()
        {

        }

        /// <summary>
        /// Create a new MySQLParameter.
        /// </summary>
        IDbDataParameter IDbCommand.CreateParameter()
        {
            return new MySQLParameter();
        }

        /// <summary>
        /// Create a new MySQLParameter.
        /// </summary>
        public MySQLParameter CreateParameter()
        {
            return new MySQLParameter();
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
        /// <remarks>
        /// Use parameter markers ('?') for PreparedStatements instead of named parameters
        ///
        /// This value is ignored if parameters are set
        /// </remarks>
        public bool UsePreparedStatement { get; set; } = false;

     
        private uint? _fetchSize = null;
        /// <summary>
        /// Number of rows to fetch from server at a time when using a cursor.
        /// </summary>
        public uint FetchSize
        {
            set => _fetchSize = value;
        }


        public CursorType CursorType { get; set; } = CursorType.NoCursor;
        /// <summary>
        /// Executes a SQL statement against the Connection object, and returns updated rows count.
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
            {
                throw new MySqlException("Connection must be valid and open.");
            }

            if (UsePreparedStatement || Parameters.Count > 0)
            {

                if (CommandType == CommandType.StoredProcedure)
                {
                    using (var exec = new PreparedExecute(Connection, "CALL " + CommandText, Parameters, _fetchSize, CursorType))
                    {
                        return exec.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var exec = new PreparedExecute(Connection, CommandText, Parameters, _fetchSize, CursorType))
                    {
                        return exec.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                if (CommandType == CommandType.StoredProcedure)
                {
                    var exec = new DirectStatement(Connection.NativeConnection, "CALL " + CommandText);
                    return exec.ExecuteNonQuery();
                }
                else
                {
                    var exec = new DirectStatement(Connection.NativeConnection, CommandText);
                    return exec.ExecuteNonQuery();
                }
            }


        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an MySQLDataReader.
        /// Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx).
        /// </summary>
        /// <returns>MySQLDataReader</returns>
        public IDataReader ExecuteReader()
        {
            return ExecuteReader(false);
        }

        /// <summary>
        /// Update to Overloaded protected by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx).
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <remarks>Use <c>ExecuteReaderEx</c> to avoid conversions.</remarks>
        /// <param name="closeConnection">Equal to true by ExecuteReader(CommandBehavior behavior)</param>
        /// <returns>IDataReader</returns>
        public IDataReader ExecuteReader(bool closeConnection)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
            {
                throw new MySqlException("Connection must be valid and open.");
            }

            // TODO: stored procedure returning rows?

            if (UsePreparedStatement || Parameters.Count > 0)
            {
                return new MySQLRealQueryDataReader(Connection, closeConnection, CommandText, Parameters, _fetchSize, CursorType);
            }
            else
            {
                var commandText = CommandText;
                if (CommandType == CommandType.StoredProcedure)
                {
                    commandText = "CALL " + CommandText;
                 
                }

                var nativeConnection = Connection.NativeConnection;
                if (nativeConnection.mysql_query(commandText) != 0) // real query instead
                {
                    // error
                    throw new MySqlException(nativeConnection);
                }
                else // query succeeded, process any data returned by it
                {
                    var result = nativeConnection.mysql_store_result();
                    if (result != IntPtr.Zero)  // there are rows
                    {
                      

                        // Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx)
                        // Don't close connection after close DataReader
                        var dr = new MySQLQueryDataReader(result, Connection, closeConnection);
                        return dr;
                    }
                    else  // mysql_store_result() returned nothing; should it have?
                    {
                        if (nativeConnection.mysql_errno() != 0)
                        {
                            throw new MySqlException(nativeConnection);
                        }
                        else if (nativeConnection.mysql_field_count() == 0)
                        {
                            // query does not return data
                            // (it was not a SELECT)
                            throw new MySqlException(nativeConnection,"No data returned");
                        }
                        else
                        {
                            // query without rows but fields
#warning fix this
                            return null;
                        }
                    }
                }


            }
        }


        /// <summary>
        /// Overloaded. Executes the CommandText against the Connection and builds an IDataReader.
        /// Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx).
        /// </summary>
        /// <param name="behavior">Only support CommandBehavior.CloseConnection</param>
        /// <returns>IDataReader</returns>
        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            // Close connection if CommandBehavior is set to CloseConnection
            // Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx).
            return ExecuteReader(((behavior & CommandBehavior.CloseConnection) > 0));
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query.
        /// Extra columns or rows are ignored.
        /// </summary>
        public object ExecuteScalar()
        {
            using (IDataReader dr = ExecuteReader())
            {
                if (dr.Read())
                    return dr.GetValue(0);
            }

            return null;
        }


        /// <summary>
        /// Prepares an SQL string for execution.
        /// </summary>
        public void Prepare()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new MySqlException("Connection must be valid and open.");
            }
            if (CommandText == null)
            {
                throw new MySqlException("Invalid query.");
            }

            //GetStmt().Prepare();
        }

        private bool _disposed = false;

        /// <summary>
        /// Dispose command
        /// Modified by Claudia Murialdo (05/18/06) in order to allow disposing
        /// commands without doing rollback on the transaction.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

         

            _transaction = null;
            _connection = null;
            _disposed = true;
        }
    }
}