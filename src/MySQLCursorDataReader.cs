#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.

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
#endregion
using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
	/// <summary>
	/// 
	/// </summary>
	public class MySQLCursorDataReader64 : MySQLCursorDataReader
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

	    internal MySQLCursorDataReader64(uint fieldCount, IntPtr stmt, MySQLParameterCollection parameters, MySQLConnection _connection, Statement _stmt, bool CloseConnection)
		{
			m_CloseConnection = CloseConnection;
			m_stmt = stmt;
			m_closed = false;
			connection = _connection;
			m_fieldCount = fieldCount;
			//uint num_rows = NativeMethods.mysql_stmt_num_rows(result);
			IntPtr fields;
			if (NativeMethods.Client6_1())
			{
				MYSQL_STMT_6_1 mysql_stmt = (MYSQL_STMT_6_1)Marshal.PtrToStructure(stmt, typeof(MYSQL_STMT_6_1));
				//IntPtr fields = mysql_stmt.fields;
				fields = mysql_stmt.result.data;
			}
			else
			{
				MYSQL_STMT mysql_stmt = (MYSQL_STMT)Marshal.PtrToStructure(stmt, typeof(MYSQL_STMT));
				//IntPtr fields = mysql_stmt.fields;
				fields = mysql_stmt.result.data;
			}

			m_fields = (IMySqlField[])Array.CreateInstance(MYSQL_FIELD_FACTORY.GetInstance().GetType(), m_fieldCount);

			long pointer = fields.ToInt64();
			int index;
			m_row = new MYSQL_BIND_64[m_fieldCount];
			for (index = 0; index < m_fieldCount; index++)
			{
				IMySqlField fieldMetadata = MYSQL_FIELD_FACTORY.GetInstance();
				IntPtr ptr = new IntPtr(pointer);
				Marshal.PtrToStructure(ptr, fieldMetadata);
				pointer += Marshal.SizeOf(fieldMetadata);
				m_fields[index] = fieldMetadata;
				m_row[index] = new MYSQL_BIND_64();

				if (fieldMetadata.Type == (uint)FieldTypes5.FIELD_TYPE_BLOB)
				{
					fieldMetadata.Max_Length = 1024;
				}
				else if (fieldMetadata.Type == (uint)FieldTypes5.FIELD_TYPE_NULL && parameters != null && parameters.Count > index)//Caso select distinct donde mysql_stmt_bind_param3 mapea erroneamente a NULL 
				{
					fieldMetadata.Type = PreparedStatement.DbtoMysqlType(parameters[index].DbType);
				}
				m_row[index].InitForBind(fieldMetadata);
			}

			sbyte code = NativeMethods.mysql_stmt_bind_result64(stmt, m_row);
			if (code != 0)
				throw new MySQLException(NativeMethods.mysql_stmt_errno(stmt), "MySQLDriverCS Error: " + NativeMethods.mysql_stmt_error(stmt));
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
				IMySqlField ft = MYSQL_FIELD_FACTORY.GetInstance();
				ft.Type = (uint)FieldTypes5.FIELD_TYPE_BLOB;
				ft.Max_Length = (uint)length;
				newbind[0].InitForBind(ft);

				sbyte errorCode = NativeMethods.mysql_stmt_fetch_column64(m_stmt, newbind, (uint)i, (uint)fieldOffset);
				if (errorCode != 0)
					Console.WriteLine(NativeMethods.mysql_stmt_errno(m_stmt) + " MySQLDriverCS Error: " + NativeMethods.mysql_stmt_error(m_stmt));

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
	/// <summary>
	/// 
	/// </summary>
	public class MySQLCursorDataReader32 : MySQLCursorDataReader
	{
		/// <summary>
		/// 
		/// </summary>
		protected MYSQL_BIND_32[] m_row;
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

	    internal MySQLCursorDataReader32(uint fieldCount, IntPtr stmt, MySQLParameterCollection parameters, MySQLConnection _connection, Statement _stmt, bool CloseConnection)
		{
			m_CloseConnection = CloseConnection;
			m_stmt = stmt;
			m_closed = false;
			connection = _connection;
			m_fieldCount = fieldCount;
			//uint num_rows = NativeMethods.mysql_stmt_num_rows(result);

			MYSQL_STMT mysql_stmt = (MYSQL_STMT)Marshal.PtrToStructure(stmt, typeof(MYSQL_STMT));
			IntPtr fields = mysql_stmt.fields;

			m_fields = (IMySqlField[])Array.CreateInstance(MYSQL_FIELD_FACTORY.GetInstance().GetType(), m_fieldCount);

			int pointer = fields.ToInt32();
			int index;
			m_row = new MYSQL_BIND_32[m_fieldCount];
			for (index = 0; index < m_fieldCount; index++)
			{
				IMySqlField fieldMetadata = MYSQL_FIELD_FACTORY.GetInstance();
				IntPtr ptr = new IntPtr(pointer);
				Marshal.PtrToStructure(ptr, fieldMetadata);
				pointer += Marshal.SizeOf(fieldMetadata);
				m_fields[index] = fieldMetadata;
				m_row[index] = new MYSQL_BIND_32();

				if (fieldMetadata.Type == (uint)FieldTypes5.FIELD_TYPE_BLOB)
				{
					fieldMetadata.Max_Length = 1024;
				}
				else if (fieldMetadata.Type == (uint)FieldTypes5.FIELD_TYPE_NULL && parameters != null && parameters.Count > index)//Caso select distinct donde mysql_stmt_bind_param3 mapea erroneamente a NULL 
				{
					fieldMetadata.Type = PreparedStatement.DbtoMysqlType(parameters[index].DbType);
				}
				m_row[index].InitForBind(fieldMetadata);
			}

			sbyte code = NativeMethods.mysql_stmt_bind_result32(stmt, m_row);
			if (code != 0)
				throw new MySQLException(NativeMethods.mysql_stmt_errno(stmt), "MySQLDriverCS Error: " + NativeMethods.mysql_stmt_error(stmt));
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
				MYSQL_BIND_32[] newbind = new MYSQL_BIND_32[1];
				newbind[0] = new MYSQL_BIND_32();
				IMySqlField ft = MYSQL_FIELD_FACTORY.GetInstance();
				ft.Type = (uint)FieldTypes5.FIELD_TYPE_BLOB;
				ft.Max_Length = (uint)length;
				newbind[0].InitForBind(ft);

				sbyte errorCode = NativeMethods.mysql_stmt_fetch_column32(m_stmt, newbind, (uint)i, (uint)fieldOffset);
				if (errorCode != 0)
					Console.WriteLine(NativeMethods.mysql_stmt_errno(m_stmt) + " MySQLDriverCS Error: " + NativeMethods.mysql_stmt_error(m_stmt));

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

	/// <summary>
	/// This class is IDataReader compliant so take a look into MSDN help to understand how it works
	/// </summary>
	public abstract class MySQLCursorDataReader : MySQLDataReader
	{
		/// <summary>
		/// 
		/// </summary>
		protected IntPtr m_stmt;
		/// <summary>
		/// 
		/// </summary>
		protected bool m_closed;
		/// <summary>
		/// 
		/// </summary>
		protected uint m_fieldCount;
		/// <summary>
		/// 
		/// </summary>
		protected IMySqlField[] m_fields;
		/// <summary>
		/// 
		/// </summary>
		protected int MYSQL_NO_DATA = 100;

		#region Constructor & destructor
		/// <summary>
		/// 
		/// </summary>
		public MySQLCursorDataReader()
		{ }
		#endregion

		internal static MySQLCursorDataReader GetInstance(uint fieldCount, IntPtr stmt, MySQLParameterCollection parameters, MySQLConnection _connection, Statement _stmt, bool CloseConnection)
		{
			if (MySQLUtils.RunningOn64x)
				return new MySQLCursorDataReader64(fieldCount, stmt, parameters, _connection, _stmt, CloseConnection);
			else
				return new MySQLCursorDataReader32(fieldCount, stmt, parameters, _connection, _stmt, CloseConnection);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		public abstract void RowDispose(int i);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public abstract object RowValue(int i);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public abstract bool RowIsNull(int i);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public abstract uint RowType(int i);

		#region IDataReader
		/// <summary>
		/// Gets a value indicating whether the data reader is closed.
		/// </summary>
		public override bool IsClosed { get { return m_closed; } }
		/// <summary>
		/// Closes the MySQLDataReader 0bject.
		/// </summary>
		public override void Close()
		{
			if (!m_closed)
			{
				//Releases memory associated with the result set produced by execution of the prepared statement. 
				//If there is a cursor open for the statement, mysql_stmt_free_result() closes it.
				int errorCode = NativeMethods.mysql_stmt_free_result(m_stmt);
				if (errorCode != 0)//Error occurred
				{
					throw new MySQLException(NativeMethods.mysql_stmt_errno(m_stmt), "MySQLDriverCS MySQLCursorDataReader Close Error: " + NativeMethods.mysql_stmt_error(m_stmt));
				}
				if (connection != null && m_CloseConnection) connection.Close();
				m_closed = true;
				for (int i = 0; i < m_fieldCount; i++)
				{
					RowDispose(i);
				}
			}
		}
		/// <summary>
		/// Returns a DataTable that describes the column metadata of the MySQLDataReader and it's values.
		/// </summary>
		/// <returns></returns>
		public override DataTable GetSchemaTable()
		{
			throw new MySQLException("Operation not supported");
		}
		/// <summary>
		/// Advances the MySQLDataReader to the next record.
		/// </summary>
		/// <returns></returns>
		public override bool Read()
		{
			if (IsClosed) return false;
			int errorCode;
			errorCode = NativeMethods.mysql_stmt_fetch(m_stmt);
			if (errorCode == MYSQL_NO_DATA)//No more rows/data exists
			{
				return false;
			}
			else if (errorCode == 1)//Error occurred
			{
				throw new MySQLException(NativeMethods.mysql_stmt_errno(m_stmt), "MySQLDriverCS Error: " + NativeMethods.mysql_stmt_error(m_stmt));
			}
			else
			{
				//0	Successful, the data has been fetched to application data buffers.
				//MYSQL_DATA_TRUNCATED 	Data truncation occurred
				_RecordsAffected++;
				return true;
			}
		}
		#endregion
		#region IDataRecord
		/// <summary>
		/// Number of fields returned
		/// </summary>
		public override int FieldCount { get { if (IsClosed) return 0; else return (int)m_fieldCount; } }
		/// <summary>
		/// Get value by name
		/// </summary>
		public override object this[string name]
		{
			get
			{
				if (IsClosed) throw new MySQLException("Reader must be open");
				for (int i = 0; i < m_fields.Length; i++)
				{
					if (m_fields[i].Name == name)
						return this[i];
				}
				throw new MySQLException("Invalid column name");
			}
		}
		/// <summary>
		/// Get value by index
		/// </summary>
		public override object this[int i]
		{
			get
			{
				if (IsClosed) throw new MySQLException("Reader must be open");
				if (RowType(i) == (uint)FieldTypes.FIELD_TYPE_BLOB)
				{
					return getAllBytes(i);
				}
				else
				{
					return RowValue(i);
				}
			}
		}

		private object getAllBytes(int id)
		{
			if (RowIsNull(id))
			{
				return DBNull.Value;
			}
			else
			{

				MemoryStream fs;
				BinaryWriter bw;
				int bufferSize = 1024;
				byte[] outbyte = new byte[bufferSize];
				long retval;
				long startIndex = 0;
				try
				{
					// Reset the starting byte for the new BLOB.
					startIndex = 0;

					retval = GetBytes(id, startIndex, outbyte, 0, bufferSize);

					if (retval == 0)
						return new byte[0];


					fs = new MemoryStream();

					bw = new BinaryWriter(fs);

					// Continue reading and writing while there are bytes beyond the size of the buffer.
					while (retval == bufferSize)
					{
						bw.Write(outbyte);
						bw.Flush();
						// Reposition the start index to the end of the last buffer and fill the buffer.
						startIndex += bufferSize;
						retval = GetBytes(id, startIndex, outbyte, 0, bufferSize);
					}
					if (retval >= 0)
					{
						// Write the remaining buffer.
						bw.Write(outbyte, 0, (int)retval);
					}
					bw.Flush();

					// Close the output file.
					bw.Close();
					fs.Close();

				}
				catch (IOException e)
				{
					throw e;
				}
				return fs.ToArray();
			}

		}

		/// <summary>
		/// Get as DateTime
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override DateTime GetDateTime(int i)
		{
			return (DateTime)RowValue(i);
		}
		/// <summary>
		/// Get as double
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override double GetDouble(int i) { return (double)this[i]; }
		/// <summary>
		/// Get field type (class Type)
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override Type GetFieldType(int i) { return MySQLUtils.MySQLToNetType(m_fields[i].Type); }
		/// <summary>
		/// Get as float
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override float GetFloat(int i) { return (float)this[i]; }
		/// <summary>
		/// Get name of field by index
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override string GetName(int i) { return m_fields[i].Name; }
		/// <summary>
		/// Get index of field by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public override int GetOrdinal(string name)
		{
			for (int i = 0; i < m_fields.Length; i++)
			{
				if (m_fields[i].Name == name)
					return i;
			}
			throw new MySQLException("Field not found");
		}
		/// <summary>
		/// Returns ok if the field is null in database
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override bool IsDBNull(int i)
		{
			return RowIsNull(i);
		}

		#endregion
	}
}
