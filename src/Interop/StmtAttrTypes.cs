namespace MySQLDriverCS.Interop
{
#warning Enum values muts be provided
    /// <summary>
    /// Flags
    /// </summary>
    public enum StmtAttrTypes
    {
        /// <summary>
        /// If set to 1: Update metadata MYSQL_FIELD.max_length in mysql_stmt_store_result().
        /// </summary>
        STMT_ATTR_UPDATE_MAX_LENGTH,
        /// <summary>
        /// Type of cursor to open for statement when mysql_stmt_execute() is invoked. 
        /// It can be CURSOR_TYPE_NO_CURSOR (the default) or CURSOR_TYPE_READ_ONLY.
        /// </summary>
        STMT_ATTR_CURSOR_TYPE,
        /// <summary>
        /// Number of rows to fetch from server at a time when using a cursor. 
        /// It can be in the range from 1 to the maximum value of unsigned long. The default is 1.
        /// </summary>
        STMT_ATTR_PREFETCH_ROWS
    };
}