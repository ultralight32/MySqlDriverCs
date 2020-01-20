/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.
-
	This file is part of MySQLDriverCS.

    MySQLDriverCS is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    MySQLDriverCS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MySQLDriverCS; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

	***** Update MySQLDataReader in order suport close connection after close DataReader ****

	Items update by Omar del Valle Rodríguez (01/18/2004)
	- Constrctor MySQLDataReader now support bool parameter to close connection after close DataReader.
	- Update method Close in order support CommandBehavior.CloseConnection

*/

using MySQLDriverCS.Interop;
using System;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;

namespace MySQLDriverCS
{
    public class MySQLRealQueryDataReader : IDataReader
    {
        private readonly MySQLConnection _connection;
        private readonly uint _fieldCount;
        private readonly NativeStatement _stmt;

        private bool m_CloseConnection = false;
        private MYSQL_FIELD[] m_fields;

        private MYSQL_BIND[] m_row;

        private int MYSQL_NO_DATA = 100;

        public MySQLRealQueryDataReader(uint fieldCount, NativeStatement stmt, MySQLConnection connection, bool closeConnection)
        {
            _connection = connection;
            _stmt = stmt;
            m_CloseConnection = closeConnection;

            IsClosed = false;

            _fieldCount = fieldCount;

            IntPtr fields;
            if (_stmt._nativeConnection.ClientVersion.CompareTo("6.0.0") > 0)
            {
                MYSQL_STMT_6_1 mysql_stmt = (MYSQL_STMT_6_1)Marshal.PtrToStructure(stmt.stmt, typeof(MYSQL_STMT_6_1));

                fields = mysql_stmt.result.data;
            }
            else
            {
                MYSQL_STMT mysql_stmt = (MYSQL_STMT)Marshal.PtrToStructure(stmt.stmt, typeof(MYSQL_STMT));

                fields = mysql_stmt.result.data;
            }

            m_fields = (MYSQL_FIELD[])Array.CreateInstance(new MYSQL_FIELD().GetType(), _fieldCount);

            long pointer = fields.ToInt64();
            int index;
            m_row = new MYSQL_BIND[_fieldCount];
            for (index = 0; index < _fieldCount; index++)
            {
                var fieldMetadata = new MYSQL_FIELD();
                IntPtr ptr = new IntPtr(pointer);
                Marshal.PtrToStructure(ptr, fieldMetadata); // mysql fill out metadata information
                pointer += Marshal.SizeOf(fieldMetadata);
                m_fields[index] = fieldMetadata;
                m_row[index] = new MYSQL_BIND();

                if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_BLOB)
                {
                    fieldMetadata.MaxLength = 1024;    
                }
                //else if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_NULL && parameters != null && parameters.Count > index)//Caso select distinct donde mysql_stmt_bind_param3 mapea erroneamente a NULL
                //{
                //    // TODO: case needs deep review
                //    fieldMetadata.Type = PreparedStatement.DbtoMysqlType(parameters[index].DbType);
                //}
                m_row[index].InitForBind(fieldMetadata,_stmt._nativeConnection);
            }

            sbyte code = stmt.mysql_stmt_bind_result64(m_row);
            if (code != 0)
                throw new MySqlException(stmt);
        }

        /// <inheritdoc />
        public int Depth => 1;

        /// <inheritdoc />
        public int FieldCount => (int)_fieldCount;

        /// <inheritdoc />
        public bool IsClosed { get; private set; }

        /// <inheritdoc />
        public int RecordsAffected => (int)_stmt.mysql_stmt_affected_rows();

        /// <inheritdoc />
        public object this[string name]
        {
            get
            {
                if (IsClosed) throw new MySqlException("Reader must be open");
                for (int i = 0; i < m_fields.Length; i++)
                {
                    if (m_fields[i].Name == name)
                        return this[i];
                }
                throw new MySqlException("Invalid column name");
            }
        }

        /// <inheritdoc />
        public object this[int i]
        {
            get
            {
                if (IsClosed) throw new MySqlException("Reader must be open");
                return m_row[i].GetValue(i,_stmt,m_fields[i]);
            }
        }

        /// <inheritdoc />
        public void Close()
        {
            if (!IsClosed)
            {
                //Releases memory associated with the result set produced by execution of the prepared statement.
                //If there is a cursor open for the statement, mysql_stmt_free_result() closes it.
                int errorCode = _stmt.mysql_stmt_free_result();
                if (errorCode != 0)//Error occurred
                {
                    throw new MySqlException(_stmt);
                }
                IsClosed = true;
                for (int i = 0; i < _fieldCount; i++)
                {
                    RowDispose(i);
                }
                if (_connection != null && m_CloseConnection)
                    _connection.Close();
            }
        }

        /// <summary>
        /// Closes this reader
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <inheritdoc />
        public bool GetBoolean(int i) { return (Convert.ToByte(this[i]) > 0) ? true : false; }

        /// <inheritdoc />
        public byte GetByte(int i) { return Convert.ToByte(this[i]); }

        /// <inheritdoc />
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            if (m_row[i].GetLength() > m_row[i].BufferLength)//data truncation
            {
                MYSQL_BIND[] newbind = new MYSQL_BIND[1];
                newbind[0] = new MYSQL_BIND();
                IMySqlField ft = new MYSQL_FIELD();
                ft.Type = enum_field_types.MYSQL_TYPE_BLOB;
                ft.MaxLength = (uint)length;
                newbind[0].InitForBind(ft,_stmt._nativeConnection);

                sbyte errorCode = _stmt.mysql_stmt_fetch_column(newbind, (uint)i, (uint)fieldOffset);
                if (errorCode != 0)
                    throw new MySqlException(_stmt);

                long result = Math.Min(length, newbind[0].GetLength() - fieldOffset);
                newbind[0].GetBytes(buffer, (uint)result);
                newbind[0].Dispose();
                return result;
            }
            else
            {
                m_row[i].GetBytes(buffer, (uint)length);
                return m_row[i].GetLength();
            }
        }

        /// <inheritdoc />
        public char GetChar(int i) { return Convert.ToChar(this[i]); }

        /// <inheritdoc />
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) { throw new MySqlException("Operation not supported"); }

        public IDataReader GetData(int i)
        {
            throw new MySqlException("Operation not supported");
        }

        /// <inheritdoc />
        public string GetDataTypeName(int i) { throw new MySqlException("Operation not supported"); }

        /// <inheritdoc />
        public DateTime GetDateTime(int i)
        {
            return (DateTime)this[i];
        }

        /// <inheritdoc />
        public decimal GetDecimal(int i) { return Convert.ToDecimal(this[i], CultureInfo.InvariantCulture.NumberFormat); }

        /// <inheritdoc />
        public double GetDouble(int i) { return (double)this[i]; }

        /// <inheritdoc />
        public Type GetFieldType(int i) { return MySQLUtils.MySQLToNetType(m_fields[i].Type); }

        /// <inheritdoc />
        public float GetFloat(int i) { return (float)this[i]; }

        /// <inheritdoc />
        public Guid GetGuid(int i) { throw new MySqlException("Operation not supported"); }

        /// <inheritdoc />
        public short GetInt16(int i) { return Convert.ToInt16(this[i]); }

        /// <inheritdoc />
        public int GetInt32(int i) { return Convert.ToInt32(this[i]); }

        /// <inheritdoc />
        public long GetInt64(int i) { return Convert.ToInt64(this[i]); }

        /// <inheritdoc />
        public string GetName(int i) { return m_fields[i].Name; }

        /// <inheritdoc />
        public int GetOrdinal(string name)
        {
            for (int i = 0; i < m_fields.Length; i++)
            {
                if (m_fields[i].Name == name)
                    return i;
            }
            throw new MySqlException("Field not found");
        }

        /// <inheritdoc />
        public DataTable GetSchemaTable()
        {
            throw new MySqlException("Operation not supported");
        }

        /// <inheritdoc />
        public string GetString(int i)
        {
            //Convert Text field to string
            object val = this[i];
            if (val is byte[])
                return System.Text.Encoding.Default.GetString((byte[])val);
            else
                return Convert.ToString(val);
        }

        /// <inheritdoc />
        public object GetValue(int i)
        {
            return this[i];
        }

        /// <inheritdoc />
        public int GetValues(object[] values)
        {
            if (values.Length < FieldCount)
                return -1;
            for (int i = 0; i < FieldCount; i++)
                values[i] = GetValue(i);
            return FieldCount;
        }

        /// <inheritdoc />
        public bool IsDBNull(int i)
        {
            return RowIsNull(i);
        }

        /// <inheritdoc />
        public bool NextResult()
        {
            // MLVL 24/05/2003 ->
            // throw new MySQLException("Operation not supported");
            // <-
            return false;
        }

        /// <inheritdoc />
        public bool Read()
        {
            if (IsClosed) return false;
            var errorCode = _stmt.mysql_stmt_fetch();
            if (errorCode == MYSQL_NO_DATA)//No more rows/data exists
            {
                return false;
            }
            else if (errorCode == 1)//Error occurred
            {
                throw new MySqlException(_stmt);
            }
            else
            {
                //0	Successful, the data has been fetched to application data buffers.
                //MYSQL_DATA_TRUNCATED 	Data truncation occurred
                return true;
            }
        }

        public void RowDispose(int i)
        {
            m_row[i].Dispose();
        }

        public bool RowIsNull(int i)
        {
            return m_row[i].GetIsNull();
        }
    }
}