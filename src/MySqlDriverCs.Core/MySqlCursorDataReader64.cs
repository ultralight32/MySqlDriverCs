using System;
using System.Runtime.InteropServices;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    /// <summary>
    /// 
    /// </summary>
    public class MySqlCursorDataReader64 : MySqlCursorDataReaderBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected MYSQL_BIND_64[] m_row;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        public override void RowDispose(int i)
        {
            m_row[i].Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override uint RowType(int i)
        {
            return m_row[i].Type;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override bool RowIsNull(int i)
        {
            return m_row[i].IsNull;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override object RowValue(int i)
        {
            return m_row[i].Value;
        }

        internal MySqlCursorDataReader64(uint fieldCount, NativeStatement stmt, MySQLParameterCollection parameters, MySQLConnection _connection, bool CloseConnection):base(_connection,stmt)
        {
            m_CloseConnection = CloseConnection;
        
            m_closed = false;
            connection = _connection;
            m_fieldCount = fieldCount;

            IntPtr fields;
            if (_connection.NativeConnection.ClientVersion.CompareTo("6.0.0") > 0)
            {
                MYSQL_STMT_6_1 mysql_stmt = (MYSQL_STMT_6_1)Marshal.PtrToStructure(stmt.stmt, typeof(MYSQL_STMT_6_1));

                fields = mysql_stmt.result.data;
            }
            else
            {
                MYSQL_STMT mysql_stmt = (MYSQL_STMT)Marshal.PtrToStructure(stmt.stmt, typeof(MYSQL_STMT));
            
                fields = mysql_stmt.result.data;
            }

            m_fields = (IMySqlField[])Array.CreateInstance(new MYSQL_FIELD_VERSION_5_64().GetType(), m_fieldCount);

            long pointer = fields.ToInt64();
            int index;
            m_row = new MYSQL_BIND_64[m_fieldCount];
            for (index = 0; index < m_fieldCount; index++)
            {
                IMySqlField fieldMetadata = new MYSQL_FIELD_VERSION_5_64();
                IntPtr ptr = new IntPtr(pointer);
                Marshal.PtrToStructure(ptr, fieldMetadata);
                pointer += Marshal.SizeOf(fieldMetadata);
                m_fields[index] = fieldMetadata;
                m_row[index] = new MYSQL_BIND_64();

                if (fieldMetadata.Type == (uint)enum_field_types.MYSQL_TYPE_BLOB)
                {
                    fieldMetadata.MaxLength = 1024;
                }
                else if (fieldMetadata.Type == (uint)enum_field_types.MYSQL_TYPE_NULL && parameters != null && parameters.Count > index)//Caso select distinct donde mysql_stmt_bind_param3 mapea erroneamente a NULL 
                {
                    fieldMetadata.Type = PreparedStatementBase.DbtoMysqlType(parameters[index].DbType);
                }
                m_row[index].InitForBind(fieldMetadata);
            }

            sbyte code = stmt.mysql_stmt_bind_result64(m_row);
            if (code != 0)
                throw new MySqlException(stmt);
        }
        /// <summary>
        /// Unsupported
        /// </summary>
        /// <param name="i"></param> he zero-based column ordinal
        /// <param name="fieldOffset"></param> The index within the field from which to begin the read operation
        /// <param name="buffer"></param> The buffer into which to read the stream of bytes
        /// <param name="bufferoffset"></param>The index for buffer to begin the read operation
        /// <param name="length"></param>The maximum length to copy into the buffer
        /// <returns></returns>The actual number of bytes rea
        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            if (m_row[i].Length > m_row[i].BufferLength)//data truncation
            {
                MYSQL_BIND_64[] newbind = new MYSQL_BIND_64[1];
                newbind[0] = new MYSQL_BIND_64();
                IMySqlField ft = new MYSQL_FIELD_VERSION_5_64();
                ft.Type = (uint)enum_field_types.MYSQL_TYPE_BLOB;
                ft.MaxLength = (uint)length;
                newbind[0].InitForBind(ft);

                sbyte errorCode = _stmt.mysql_stmt_fetch_column64(newbind, (uint)i, (uint)fieldOffset);
                if (errorCode != 0)
                    throw new MySqlException(_stmt);

                long result = Math.Min(length, newbind[0].Length - fieldOffset);
                newbind[0].GetBytes(buffer, (uint)result);
                newbind[0].Dispose();
                return result;
            }
            else
            {
                m_row[i].GetBytes(buffer, (uint)length);
                return m_row[i].Length;
            }
        }
    }
}