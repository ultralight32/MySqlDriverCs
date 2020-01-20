using System;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    internal class LiteralNonQueryStatement
    {
        private readonly NativeConnection _nativeConnection;
        private readonly string _commandText;

        public LiteralNonQueryStatement(NativeConnection nativeConnection, string commandText)
        {
            _nativeConnection = nativeConnection;
            _commandText = commandText;
        }
        internal int ExecuteNonQuery()
        {
            var nativeConnection = _nativeConnection;
            if (nativeConnection.mysql_query(_commandText) != 0)
            {
                // error
                throw new MySqlException(nativeConnection);
            }
            else // query succeeded, process any data returned by it
            {
                var mysqlStoreResult = nativeConnection.mysql_store_result();
                if (mysqlStoreResult == IntPtr.Zero)
                {
                    if (nativeConnection.mysql_errno() != 0)
                    {
                        throw new MySqlException(nativeConnection);
                    }
                    else if (nativeConnection.mysql_field_count() == 0)
                    {
                        // query does not return data (it was not a SELECT)
                        return (int)nativeConnection.mysql_affected_rows();
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
    }
}