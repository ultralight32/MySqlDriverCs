using MySQLDriverCS.Interop;
using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace MySQLDriverCS
{
    internal class PreparedStatement : Statement
    {
        protected bool prepared;
        protected NativeStatement stmt;

        protected int _detectedParamCount;
        protected int m_fetch_size;
        protected uint m_cursor_type;
        protected uint m_field_count;
        private MYSQL_BIND[] m_bindparms;

        public PreparedStatement(MySQLConnection connection, string query)
        {
            stmt = new NativeStatement(connection.NativeConnection);
            this.connection = connection;
            this.query = query;

            prepared = false;
            _detectedParamCount = -1;
            m_fetch_size = 1;
            m_field_count = 0;
        }

        public override void Dispose()
        {
            if (m_bindparms != null)
            {
                for (int i = 0; i < m_bindparms.Length; i++)
                {
                    m_bindparms[i].Dispose();
                }
            }

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
                    return (uint)enum_field_types.MYSQL_TYPE_STRING;

                case DbType.AnsiString:
                    return (uint)enum_field_types.MYSQL_TYPE_VARCHAR;

                case DbType.Binary:
                    return (uint)enum_field_types.MYSQL_TYPE_BLOB;

                case DbType.Boolean:
                    return (uint)enum_field_types.MYSQL_TYPE_BIT;

                case DbType.Byte:
                    return (uint)enum_field_types.MYSQL_TYPE_TINY;
                /*case DbType.Currency:
                    return (uint) FieldTypes5.MYSQL_TYPE_MONEY;*/
                case DbType.Date:
                    return (uint)enum_field_types.MYSQL_TYPE_DATE;

                case DbType.DateTime:
                    return (uint)enum_field_types.MYSQL_TYPE_DATETIME;

                case DbType.DateTime2:
                    return (uint)enum_field_types.MYSQL_TYPE_DATETIME;

                case DbType.Decimal:
                    return (uint)enum_field_types.MYSQL_TYPE_DECIMAL;

                case DbType.Double:
                    return (uint)enum_field_types.MYSQL_TYPE_DOUBLE;
                /*case DbType.Guid:
                    return (uint) FieldTypes5.MYSQL_TYPE_DOUBLE;*/
                case DbType.Int16:
                    return (uint)enum_field_types.MYSQL_TYPE_SHORT;

                case DbType.Int32:
                    return (uint)enum_field_types.MYSQL_TYPE_LONG;

                case DbType.Int64:
                    return (uint)enum_field_types.MYSQL_TYPE_LONGLONG;
                /*case DbType.Object:
                    return (uint) FieldTypes5.MYSQL_TYPE_VARIANT;

                case DbType.SByte:
                    return (uint) FieldTypes5.MYSQL_TYPE_LONGLONG;*/
                case DbType.Single:
                    return (uint)enum_field_types.MYSQL_TYPE_FLOAT;

                case DbType.Time:
                    return (uint)enum_field_types.MYSQL_TYPE_TIME;

                default:
                    return (uint)enum_field_types.MYSQL_TYPE_SHORT;
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

        public void BindParameters()
        {
            if (_detectedParamCount == -1)
            {
                if (!prepared)
                    Prepare();
                // This function will not deliver a valid result until mysql_stmt_prepare()() was called.
                _detectedParamCount = (int)stmt.mysql_stmt_param_count();
            }
            if (_detectedParamCount != m_parameters.Count)
            {
                throw new MySqlException("Invalid parameters, stmt parameters:" + _detectedParamCount + " parameters count:" + m_parameters.Count);
            }

            if (m_bindparms != null && m_bindparms.Length != m_parameters.Count)
            {
                for (int j = 0; j < m_bindparms.Length; j++)
                {
                    m_bindparms[j].Dispose();
                }
                m_bindparms = null;
            }

            if (m_bindparms == null)
            {
                m_bindparms = new MYSQL_BIND[m_parameters.Count];
                for (int i = 0; i < m_parameters.Count; i++)
                {
                    m_bindparms[i] = new MYSQL_BIND();
                }
            }

            for (int i = 0; i < m_parameters.Count; i++)
            {
                MySQLParameter param = (MySQLParameter)m_parameters[i];
                m_bindparms[i].Type = DbtoMysqlType(param.DbType);
                m_bindparms[i].Value = param.Value;
                m_bindparms[i].IsNull = param.Value == null || param.Value == DBNull.Value;
                if (param.Value != null && param.Value is string)
                {
                    m_bindparms[i].Length = (uint)connection.CharacterEncoding.GetBytes((string)param.Value).Length; //si es string
                }
                else
                {
                    m_bindparms[i].Length = (uint)param.Size;
                }
            }
            int code = stmt.mysql_stmt_bind_param64(m_bindparms);
            if (code != 0)
                throw new MySqlException(stmt);
        }

        internal override int ExecuteNonQuery()
        {
            BindParameters();
            if (stmt.mysql_stmt_execute() != 0)
            {
                uint code = stmt.mysql_stmt_errno();
                if (code != 1062)//Duplicated record ER_DUP_ENTRY
                    throw new MySqlException(stmt);
                else
                    throw new MySqlException(stmt, "Duplicated record");
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
                using (var resultMetadata = new NativeResultMetadata(stmt))
                {
                    m_field_count = resultMetadata.mysql_num_fields();
                }
            }
            if (stmt.mysql_stmt_execute() != 0)
            {
                throw new MySqlException(stmt);
            }
            else
            {
                MySqlCursorDataReaderBase ret;

                ret = new MySqlCursorDataReader64(m_field_count, stmt, m_parameters, this.connection, closeConnection);

                MySQLDataReader dr = ret;
                return dr;
            }
        }

        internal override int ExecuteCall()
        {
            return 0;
        }

        #endregion Statement Members
    }
}