using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    /// <summary>
    /// Simple exception. Can show multiple messages in one message sepparated by CR/LF
    /// </summary>
    public class MySqlException : System.Data.Common.DbException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public MySqlException(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Exception from native connection
        /// </summary>
        /// <param name="nativeConnection"></param>
        /// <param name="message"></param>
        public MySqlException(NativeConnection nativeConnection, string message = null)
        {
            Number = nativeConnection.mysql_errno();
            Message = nativeConnection.mysql_error() + " (" + Number + ")" + (message != null ? "\nContext:\n" + message : "");

        }
        /// <summary>
        /// Exception from native statement
        /// </summary>
        /// <param name="nativeStatement"></param>
        /// <param name="message"></param>
        public MySqlException(NativeStatement nativeStatement, string message = null)
        {
            Number = nativeStatement.mysql_stmt_errno();
            Message = nativeStatement.mysql_stmt_error() + " (" + Number + ")" + (message != null ? "\nContext:\n" + message : "");
        }

        /// <summary>
        /// Overload to show real message string
        /// </summary>
        public override string Message { get; }

        /// <summary>
        /// Overload to show real message s
        /// </summary>
        public uint Number { get; }
    }

}