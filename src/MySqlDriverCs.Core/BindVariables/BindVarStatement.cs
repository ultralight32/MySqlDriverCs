using MySQLDriverCS.Interop;
using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS
{
    internal class BindVarStatement : IDisposable
    {
        protected NativeStatement Stmt;

        private readonly MYSQL_BIND[] _bindParams;

        public BindVarStatement(MySQLConnection connection, string query, MySQLParameterCollection parameterCollection, uint? fetchSize, CursorType cursorType)
        {
            Stmt = connection.NativeConnection.CreateStatement();

            if (fetchSize != null)
            {
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(fetchSize.Value));
                Marshal.StructureToPtr(fetchSize.Value, ptr, false);
                var code1 = Stmt.mysql_stmt_attr_set(StmtAttrTypes.STMT_ATTR_PREFETCH_ROWS, ptr);
                Marshal.FreeHGlobal(ptr);
                if (code1 != 0)
                {
                    throw new MySqlException(Stmt);
                }
            }

            if (cursorType != CursorType.NoCursor)
            {
                var value = (enum_cursor_type)cursorType;
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
                Marshal.StructureToPtr(value, ptr, false);
                var code1 = Stmt.mysql_stmt_attr_set(StmtAttrTypes.STMT_ATTR_CURSOR_TYPE, ptr);
                Marshal.FreeHGlobal(ptr);
                if (code1 != 0)
                {
                    throw new MySqlException(Stmt);
                }
            }

            // execute prepare to call mysql_stmt_param_count
            if (Stmt.mysql_stmt_prepare(query, query.Length) != 0)
            {
                throw new MySqlException(Stmt);
            }

            // This function will not deliver a valid result until mysql_stmt_prepare() was called.
            var paramCount = (int)Stmt.mysql_stmt_param_count();
            if (paramCount != parameterCollection.Count)
            {
                throw new MySqlException($"Invalid parameters, stmt parameters:{paramCount} parameters count:{parameterCollection.Count}");
            }


            // alloc & set binds
            _bindParams = new MYSQL_BIND[parameterCollection.Count];
            for (var i = 0; i < parameterCollection.Count; i++)
            {
                _bindParams[i] = new MYSQL_BIND();
          
                var param = parameterCollection[i];

                var bindInput = param.GetBindInput();
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

            // link binds to stmt
            if (Stmt.mysql_stmt_bind_param(_bindParams) != 0)
                throw new MySqlException(Stmt);
        }

        public virtual void Dispose()
        {
            if (_bindParams != null)
            {
                for (var i = 0; i < _bindParams.Length; i++)
                {
                    _bindParams[i].Dispose();
                }
            }

            if (Stmt == null) return;
            Stmt.Dispose();
            Stmt = null;
        }
    }
}