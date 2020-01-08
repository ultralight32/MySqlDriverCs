using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    /// <summary>
    /// Summary description for Statement.
    /// </summary>
    internal class DirectStatement : Statement
    {
        protected string repQuery;

        public DirectStatement(MySQLConnection connection, string query)
        {
            this.connection = connection;
            this.query = query;
        }
        #region Statement Members


        /// Modified by Claudia Murialdo (07/24/04) in order to support prepared
        /// commands's caching, time component of the datetime values, 
        /// culture-independent format of numeric values in a stmt and
        /// to work with stmts with similar parameters like: SELECT * FROM 
        /// Table1 WHERE A=@ParmName AND B=@ParmNameOther
        private string BindParameters()
        {
            string resQuery = query;

            //Parameters is a shallow copy of m_parameters
            ArrayList parameters = m_parameters.SortedList;

            foreach (MySQLParameter param in parameters)
            {
                string paramName = param.ParameterName;
                object Value = param.Value;

                if (param.Direction == ParameterDirection.Output && resQuery.StartsWith("CALL"))
                {
                    // Hack - en calls no modifico el parametro de output, sino MySql da error
                    continue;
                }

                if (Value == null || Value == DBNull.Value)
                    resQuery = resQuery.Replace(paramName, "NULL");
                else
                if ((Value.GetType() == typeof(int)) ||
                    (Value.GetType() == typeof(long)) ||
                    (Value.GetType() == typeof(short)) ||
                    (Value.GetType() == typeof(decimal)) ||
                    (Value.GetType() == typeof(float)) ||
                    (Value.GetType() == typeof(double)))
                    resQuery = resQuery.Replace(paramName, Convert.ToString(Value, CultureInfo.InvariantCulture.NumberFormat));
                else
                if (Value.GetType() == typeof(DateTime))
                {
                    DateTime dt = (DateTime)Value;
                    string dateStr;
                    if (param.DbType == DbType.Date)
                    {
                        dateStr = " \"" + dt.Year.ToString("D4") + "-" + dt.Month.ToString("D2") + "-" + dt.Day.ToString("D2") + "\" "; ;
                    }
                    else if (param.DbType == DbType.DateTime2)
                    {
                        dateStr = " \"" + dt.Year.ToString("D4") + "-" + dt.Month.ToString("D2") + "-" + dt.Day.ToString("D2") +
                                  " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second + ((dt.Millisecond > 0) ? "." + dt.Millisecond.ToString("D3") : "") + "\" ";
                    }
                    else
                    {
                        dateStr = " \"" + dt.Year.ToString("D4") + "-" + dt.Month.ToString("D2") + "-" + dt.Day.ToString("D2") +
                                  " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second + "\" ";
                    }
                    resQuery = resQuery.Replace(paramName, dateStr);
                }
                else if (Value.GetType() == typeof(byte[]))
                {
                    resQuery = resQuery.Replace(paramName,
                        MySQLUtils.Escape(System.Text.Encoding.Default.GetString((byte[])Value), connection));
                }
                else
                {
                    resQuery = resQuery.Replace(paramName, MySQLUtils.Escape(Value.ToString(), connection));
                }
            }
            return resQuery;
        }

        internal override int ExecuteNonQuery()
        {

            repQuery = BindParameters();
            if (connection.NativeConnection.mysql_query(repQuery) != 0)
            {
                // error
                throw new MySqlException(connection.NativeConnection);
            }
            else // query succeeded, process any data returned by it
            {
                var mysqlStoreResult = connection.NativeConnection.mysql_store_result();
                if (mysqlStoreResult == IntPtr.Zero)
                {
                    if (connection.NativeConnection.mysql_errno() != 0)
                    {
                        throw new MySqlException(connection.NativeConnection);
                    }
                    else if (connection.NativeConnection.mysql_field_count() == 0)
                    {
                        // query does not return data (it was not a SELECT)
                        return (int)connection.NativeConnection.mysql_affected_rows();
                    }
                    else
                        return 0;
                }
                else using (new NativeResult(mysqlStoreResult))
                    {
                        // rows: TODO: return row count
                        return 0;
                    }
            }

        }

        internal override int ExecuteCall()
        {
            repQuery = BindParameters();
            if (connection.NativeConnection.mysql_query(repQuery) != 0)
            {
                // error
                throw new MySqlException(connection.NativeConnection);
            }
            else // query succeeded, process any data returned by it
            {
                // Retrieve output parameters
                return query_output_parameters();
            }

        }

        int query_output_parameters()
        {
            // Get output parameters - build select
            ArrayList parameters = m_parameters.SortedList;
            StringBuilder stmt = new StringBuilder();
            int pCount = 0;

            foreach (MySQLParameter param in parameters)
            {
                if (param.Direction == ParameterDirection.Output)
                {
                    if (pCount > 0) stmt.Append(", ");
                    stmt.Append(param.ParameterName);
                    pCount++;
                }
            }
            if (pCount > 0)
            {
                stmt.Insert(0, "select ");
                // Query output parameter values
                if (connection.NativeConnection.mysql_query( stmt.ToString()) != 0)
                {
                    // error
                    throw new MySqlException(connection.NativeConnection);
                }
                var resultPtr = connection.NativeConnection.mysql_store_result();
                if (resultPtr != IntPtr.Zero)  // there are rows
                {
                    using (var dr = new MySQLDataReader(resultPtr, this.connection, this, false))
                    {
                        if (dr.Read())
                        {
                            // Load values
                            foreach (MySQLParameter param in parameters)
                                if (param.Direction == ParameterDirection.Output)
                                    param.Value = dr[param.ParameterName];
                        }
             
                        dr.Close();
                    }
                }
            }
            return 0;
        }

        internal override DbDataReader ExecuteReader(bool CloseConnection)
        {
            bTryToCancel = false;
            repQuery = BindParameters();

            if (connection.NativeConnection.mysql_query(repQuery) != 0)
            {
                // error
                throw new MySqlException(connection.NativeConnection);
            }
            else // query succeeded, process any data returned by it
            {
                var result = connection.NativeConnection.mysql_store_result();
                if (result != IntPtr.Zero)  // there are rows
                {
                    //num_fields = mysql_num_fields(result);
                    // retrieve rows, then call mysql_free_result(result)

                    // Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx)
                    // Don't close connection after close DataReader
                    MySQLDataReader dr = new MySQLDataReader(result, this.connection, this, CloseConnection);
                    return dr;
                }
                else  // mysql_store_result() returned nothing; should it have?
                {
                    if (base.connection.NativeConnection.mysql_errno() != 0)
                    {
                        throw new MySqlException(connection.NativeConnection);
                    }
                    else if (connection.NativeConnection.mysql_field_count() == 0)
                    {
                        // query does not return data
                        // (it was not a SELECT)
                        return null;
                    }
                    else
                        return null;
                }
            }

        }

        #endregion
    }
}