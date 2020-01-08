using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    internal abstract class PreparedStatementBase : Statement
    {
        protected bool prepared;
        protected NativeStatement stmt;

        protected int m_parm_count;
        protected int m_fetch_size;
        protected uint m_cursor_type;
        protected uint m_field_count;

        protected PreparedStatementBase(MySQLConnection connection)
        {
            stmt = new NativeStatement(connection.NativeConnection);
        }

        public override void Dispose()
        {
            if (stmt == null) return;
            stmt.Dispose();
            stmt = null;
        }

        public static uint DbtoMysqlType(DbType dbtype)
        {
            switch (dbtype)
            {
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.String:
                    return (uint)FieldTypes5.FIELD_TYPE_STRING;
                case DbType.AnsiString:
                    return (uint)FieldTypes5.FIELD_TYPE_VARCHAR;
                case DbType.Binary:
                    return (uint)FieldTypes5.FIELD_TYPE_BLOB;
                case DbType.Boolean:
                    return (uint)FieldTypes5.FIELD_TYPE_BIT;
                case DbType.Byte:
                    return (uint)FieldTypes5.FIELD_TYPE_TINY;
                /*case DbType.Currency: 
                    return (uint) FieldTypes5.FIELD_TYPE_MONEY;*/
                case DbType.Date:
                    return (uint)FieldTypes5.FIELD_TYPE_DATE;
                case DbType.DateTime:
                    return (uint)FieldTypes5.FIELD_TYPE_DATETIME;
                case DbType.DateTime2:
                    return (uint)FieldTypes5.FIELD_TYPE_DATETIME;
                case DbType.Decimal:
                    return (uint)FieldTypes5.FIELD_TYPE_DECIMAL;
                case DbType.Double:
                    return (uint)FieldTypes5.FIELD_TYPE_DOUBLE;
                /*case DbType.Guid: 
                    return (uint) FieldTypes5.FIELD_TYPE_DOUBLE;*/
                case DbType.Int16:
                    return (uint)FieldTypes5.FIELD_TYPE_SHORT;
                case DbType.Int32:
                    return (uint)FieldTypes5.FIELD_TYPE_LONG;
                case DbType.Int64:
                    return (uint)FieldTypes5.FIELD_TYPE_LONGLONG;
                /*case DbType.Object: 
                    return (uint) FieldTypes5.FIELD_TYPE_VARIANT;
                case DbType.SByte: 
                    return (uint) FieldTypes5.FIELD_TYPE_LONGLONG;*/
                case DbType.Single:
                    return (uint)FieldTypes5.FIELD_TYPE_FLOAT;
                case DbType.Time:
                    return (uint)FieldTypes5.FIELD_TYPE_TIME;
                default:
                    return (uint)FieldTypes5.FIELD_TYPE_SHORT;

            }
        }
        #region Statement Members

        internal override void Prepare()
        {
            if (!prepared)
            {
                int code = stmt.mysql_stmt_prepare(query, query.Length);
                if (code != 0)
                {
                    throw new MySqlException(stmt);
                }
                prepared = true;
            }
        }
        /// <summary>
        /// Number of rows to fetch from server at a time when using a cursor.
        /// This options is supported since MySQL 5.0.6
        /// </summary>
        internal override uint FetchSize
        {
            set
            {
                if (m_fetch_size != value)
                {
                    m_fetch_size = (int)value;
                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
                    Marshal.StructureToPtr(value, ptr, false);
                    sbyte code = stmt.mysql_stmt_attr_set(StmtAttrTypes.STMT_ATTR_PREFETCH_ROWS, ptr);
                    Marshal.FreeHGlobal(ptr);
                    if (code != 0)
                    { 
                        throw new MySqlException(stmt);
                    }
                }
            }
        }
        /// <summary>
        /// Type of cursor to open for statement 
        /// This options is supported since MySQL 5.0.2
        /// </summary>
        internal override uint CursorType
        {
            set
            {
                try
                {
                    if (m_cursor_type != value)
                    {
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
                        Marshal.StructureToPtr(value, ptr, false);
                        sbyte code = stmt.mysql_stmt_attr_set(StmtAttrTypes.STMT_ATTR_CURSOR_TYPE, ptr);
                        Marshal.FreeHGlobal(ptr);
                        if (code != 0)
                        {
                            throw new MySqlException(stmt);
                        }
                        m_cursor_type = value;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                    if (e is MySqlException)
                    {
                        throw e;
                    }
                }
            }
        }
        public abstract void BindParameters();
        internal override int ExecuteNonQuery()
        {
            BindParameters();
            if (stmt.mysql_stmt_execute() != 0)
            {
                uint code = stmt.mysql_stmt_errno();
                if (code != 1062)//Duplicated record ER_DUP_ENTRY
                    throw new MySqlException(stmt);
                else
                    throw new MySqlException(stmt,"Duplicated record");
            }
            else
            {
                uint affectedRows = stmt.mysql_stmt_affected_rows();
                return (int)affectedRows;
            }
        }
        internal override DbDataReader ExecuteReader(bool closeConnection)
        {
            bTryToCancel = false;
            if (!prepared)
                Prepare();
            BindParameters();
            if (m_field_count == 0)
            {
                using (var result_metadata = new NativeResultMetadata(stmt))
                {
                    m_field_count = result_metadata.mysql_num_fields();
                }
            }
            if (stmt.mysql_stmt_execute() != 0)
            {
                throw new MySqlException(stmt);
            }
            else
            {
                MySqlCursorDataReaderBase ret;
                if (MySQLUtils.RunningOn64x)
                    ret = new MySqlCursorDataReader64(m_field_count,  stmt, m_parameters, this.connection, closeConnection);
                else
                    ret = new MySqlCursorDataReader32(m_field_count,  stmt, m_parameters, this.connection, this, closeConnection);
                MySQLDataReader dr = ret;
                return dr;
            }
        }
        internal override int ExecuteCall()
        {
            return 0;
        }

        #endregion
    }
}