namespace MySQLDriverCS
{
    internal class BindVarNonQueryStatement: BindVarStatement
    {

        internal int ExecuteNonQuery()
        {

            if (Stmt.mysql_stmt_execute() != 0)
            {
                uint code = Stmt.mysql_stmt_errno();
                if (code != 1062)//Duplicated record ER_DUP_ENTRY
                    throw new MySqlException(Stmt);
                throw new MySqlException(Stmt, "Duplicated record");
            }

            uint affectedRows = Stmt.mysql_stmt_affected_rows();
            return (int)affectedRows;
        }

        public BindVarNonQueryStatement(MySQLConnection connection, string query, MySQLParameterCollection parameterCollection, uint? fetchSize, CursorType cursorType) : base(connection, query, parameterCollection, fetchSize, cursorType)
        {
        }
    }
}