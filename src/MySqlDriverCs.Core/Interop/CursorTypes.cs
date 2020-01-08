namespace MySQLDriverCS.Interop
{
    /// <summary>
    /// Cursor Type.
    /// Server-side cursors are implemented beginning with the C API in MySQL 5.0.2 via the mysql_stmt_attr_set() function. 
    /// A server-side cursor allows a result set to be generated on the server side, 
    /// but not transferred to the client except for those rows that the client requests. 
    /// For example, if a client executes a query but is only interested in the first row, 
    /// the remaining rows are not transferred.
    /// In MySQL, a server-side cursor is materialized into a temporary table. Initially, this is a MEMORY table, 
    /// but is converted to a MyISAM table if its size reaches the value of the max_heap_table_size system variable. 
    /// (Beginning with MySQL 5.0.14, the same temporary-table implementation also is used for cursors in stored routines.) 
    /// One limitation of the implementation is that for a large result set, retrieving its rows through a cursor might be slow.
    /// Cursors are read-only; you cannot use a cursor to update rows.
    /// UPDATE WHERE CURRENT OF and DELETE WHERE CURRENT OF are not implemented, because updatable cursors are not supported.
    /// Cursors are non-holdable (not held open after a commit).
    /// Cursors are asensitive.
    /// Cursors are non-scrollable.
    /// Cursors are not named. The statement handler acts as the cursor ID.
    /// You can have open only a single cursor per prepared statement. If you need several cursors, you must prepare several statements.
    /// You cannot use a cursor for a statement that generates a result set if the statement is not supported in prepared mode. 
    /// This includes statements such as CHECK TABLES, HANDLER READ, and SHOW BINLOG EVENTS. 
    /// </summary>
    public enum CursorTypes
    {
        /// <summary>
        /// No cursor
        /// </summary>
        CURSOR_TYPE_NO_CURSOR = 0,
        /// <summary>
        /// ReadOnly cursor
        /// </summary>
        CURSOR_TYPE_READ_ONLY = 1,
        /// <summary>
        /// Forupdate cursor
        /// </summary>
        CURSOR_TYPE_FOR_UPDATE = 2,
        /// <summary>
        /// Scrollable cursor
        /// </summary>
        CURSOR_TYPE_SCROLLABLE = 4
    };
}