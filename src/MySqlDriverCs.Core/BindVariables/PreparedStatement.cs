using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    internal class PreparedExecute: PreparedStatement
    {

        internal int ExecuteNonQuery()
        {

            if (_stmt.mysql_stmt_execute() != 0)
            {
                uint code = _stmt.mysql_stmt_errno();
                if (code != 1062)//Duplicated record ER_DUP_ENTRY
                    throw new MySqlException(_stmt);
                throw new MySqlException(_stmt, "Duplicated record");
            }

            uint affectedRows = _stmt.mysql_stmt_affected_rows();
            return (int)affectedRows;
        }

        public PreparedExecute(MySQLConnection connection, string query, MySQLParameterCollection parameterCollection, uint? fetchSize, CursorType cursorType) : base(connection, query, parameterCollection, fetchSize, cursorType)
        {
        }
    }


    internal class PreparedStatement:IDisposable
    {

        protected NativeStatement _stmt;

 

    
        private MYSQL_BIND[] _bindParams;


        public PreparedStatement(MySQLConnection connection, string query, MySQLParameterCollection parameterCollection, uint? fetchSize, CursorType cursorType)
        {
            _stmt = connection.NativeConnection.CreateStatement();
 
           

     
  
      

            if (fetchSize != null)
                SetFetchSize(fetchSize.Value);
            if (cursorType != CursorType.NoCursor)
                SetCursorType((enum_cursor_type)cursorType);
            BindParameters(parameterCollection,query);
        }

        public virtual void Dispose()
        {
            if (_bindParams != null)
            {
                for (int i = 0; i < _bindParams.Length; i++)
                {
                    _bindParams[i].Dispose();
                }
            }

            if (_stmt == null) return;
            _stmt.Dispose();
            _stmt = null;
        }


        /// <summary>
        /// Number of rows to fetch from server at a time when using a cursor.
        /// This options is supported since MySQL 5.0.6
        /// </summary>
        private void SetFetchSize(uint value)
        {

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
            Marshal.StructureToPtr(value, ptr, false);
            sbyte code = _stmt.mysql_stmt_attr_set(StmtAttrTypes.STMT_ATTR_PREFETCH_ROWS, ptr);
            Marshal.FreeHGlobal(ptr);
            if (code != 0)
            {
                throw new MySqlException(_stmt);
            }
        }

        /// <summary>
        /// Type of cursor to open for statement
        /// This options is supported since MySQL 5.0.2
        /// </summary>
        private void SetCursorType(enum_cursor_type value)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
            Marshal.StructureToPtr(value, ptr, false);
            sbyte code = _stmt.mysql_stmt_attr_set(StmtAttrTypes.STMT_ATTR_CURSOR_TYPE, ptr);
            Marshal.FreeHGlobal(ptr);
            if (code != 0)
            {
                throw new MySqlException(_stmt);
            }
        }

        public void BindParameters(MySQLParameterCollection m_parameters, string query)
        {
            int codex = _stmt.mysql_stmt_prepare(query, query.Length);
            if (codex != 0)
            {
                throw new MySqlException(_stmt);
            }

            // This function will not deliver a valid result until mysql_stmt_prepare()() was called.
            var detectedParamCount = (int) _stmt.mysql_stmt_param_count();
            if (detectedParamCount != m_parameters.Count)
            {
                throw new MySqlException("Invalid parameters, stmt parameters:" + detectedParamCount + " parameters count:" + m_parameters.Count);
            }

            if (_bindParams != null && _bindParams.Length != m_parameters.Count)
            {
                for (int j = 0; j < _bindParams.Length; j++)
                {
                    _bindParams[j].Dispose();
                }
                _bindParams = null;
            }

            if (_bindParams == null)
            {
                _bindParams = new MYSQL_BIND[m_parameters.Count];
                for (int i = 0; i < m_parameters.Count; i++)
                {
                    _bindParams[i] = new MYSQL_BIND();
                }
            }

            for (int i = 0; i < m_parameters.Count; i++)
            {
                MySQLParameter param = m_parameters[i];

                BindInput bindInput = param.GetBindInput();
                if (bindInput != null)
                {
                    _bindParams[i].buffer_type = bindInput.BufferType;
                    if (bindInput.Buffer == null)
                        _bindParams[i].ResetBuffer();
                    else
                        _bindParams[i].SetBuffer(bindInput.Buffer);

                    if (bindInput.IsNull.HasValue)
                        _bindParams[i].SetIsNull(bindInput.IsNull.Value);
                    else
                        _bindParams[i].ResetIsNull();
                    if (bindInput.Length.HasValue)
                        _bindParams[i].SetLength((uint)bindInput.Length.Value);
                    else
                        _bindParams[i].ResetLength();
                    _bindParams[i].is_unsigned = (byte)(bindInput.IsUnsigned ? 1 : 0);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            int code = _stmt.mysql_stmt_bind_param(_bindParams);
            if (code != 0)
                throw new MySqlException(_stmt);
        }


       
    }
}