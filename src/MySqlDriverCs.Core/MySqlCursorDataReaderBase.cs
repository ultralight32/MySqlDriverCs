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
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    /// <summary>
	/// This class is IDataReader compliant so take a look into MSDN help to understand how it works
	/// </summary>
	public abstract class MySqlCursorDataReaderBase : MySQLDataReader
	{
        protected readonly MySQLConnection _connection;
        protected readonly NativeStatement _stmt;

        protected MySqlCursorDataReaderBase(MySQLConnection connection, NativeStatement stmt)
        {
            _connection = connection;
            _stmt = stmt;
        }

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
		public MySqlCursorDataReaderBase()
		{ }
		#endregion

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
				int errorCode = _stmt.mysql_stmt_free_result();
				if (errorCode != 0)//Error occurred
				{
					throw new MySqlException(_stmt);
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
			throw new MySqlException("Operation not supported");
		}
		/// <summary>
		/// Advances the MySQLDataReader to the next record.
		/// </summary>
		/// <returns></returns>
		public override bool Read()
		{
			if (IsClosed) return false;
			int errorCode;
			errorCode = _stmt.mysql_stmt_fetch();
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
				if (IsClosed) throw new MySqlException("Reader must be open");
				for (int i = 0; i < m_fields.Length; i++)
				{
					if (m_fields[i].Name == name)
						return this[i];
				}
				throw new MySqlException("Invalid column name");
			}
		}
		/// <summary>
		/// Get value by index
		/// </summary>
		public override object this[int i]
		{
			get
			{
				if (IsClosed) throw new MySqlException("Reader must be open");
				if (RowType(i) == (uint)FieldTypes.MYSQL_TYPE_BLOB)
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
			throw new MySqlException("Field not found");
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
