using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    internal class PreparedStatement : Statement
    {
        protected bool prepared;
        protected NativeStatement stmt;

        protected int _detectedParamCount;
        protected int m_fetch_size;
        protected uint m_cursor_type;

        private MYSQL_BIND[] m_bindparms;

        public PreparedStatement(MySQLConnection connection, string query)
        {
            stmt = connection.NativeConnection.CreateStatement();
            this.connection = connection;
            this.query = query;

            prepared = false;
            _detectedParamCount = -1;
            m_fetch_size = 1;

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
                MySQLParameter param = m_parameters[i];

                BindInput bindInput = param.GetBindInput();
                if (bindInput != null)
                {
                    m_bindparms[i].buffer_type = bindInput.BufferType;
                    if (bindInput.Buffer == null)
                        m_bindparms[i].ResetBuffer();
                    else
                        m_bindparms[i].SetBuffer(bindInput.Buffer);

                    if (bindInput.IsNull.HasValue)
                        m_bindparms[i].SetIsNull(bindInput.IsNull.Value);
                    else
                        m_bindparms[i].ResetIsNull();
                    if (bindInput.Length.HasValue)
                        m_bindparms[i].SetLength((uint)bindInput.Length.Value);
                    else
                        m_bindparms[i].ResetLength();
                    m_bindparms[i].is_unsigned = (byte)(bindInput.IsUnsigned ? 1 : 0);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            int code = stmt.mysql_stmt_bind_param(m_bindparms);
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
                throw new MySqlException(stmt, "Duplicated record");
            }

            uint affectedRows = stmt.mysql_stmt_affected_rows();
            return (int)affectedRows;
        }

        internal override IDataReader ExecuteReader(bool closeConnection)
        {
            bTryToCancel = false;
            if (!prepared)
                Prepare();
            BindParameters();

            var fields = new List<MYSQL_FIELD>();

            using (var resultMetadata = new NativeResultMetadata(stmt))
            {
                for (var i = 0; i < resultMetadata.mysql_num_fields(); i++)
                {
                    IntPtr fieldPtr = resultMetadata.mysql_fetch_field_direct((uint) i);
                    var field = Marshal.PtrToStructure<MYSQL_FIELD>(fieldPtr);
                    fields.Add(field);
                }
            }

            if (stmt.mysql_stmt_execute() != 0)
            {
                throw new MySqlException(stmt);
            }

            return new MySQLRealQueryDataReader(fields.ToArray(), stmt, connection, closeConnection);
        }

        internal override int ExecuteCall()
        {
            return 0;
        }

        #endregion Statement Members
    }
}