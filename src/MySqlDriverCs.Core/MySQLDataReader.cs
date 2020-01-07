#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Vi�as Livschitz.

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
	
	Items update by Omar del Valle Rodr�guez (01/18/2004)
	- Constrctor MySQLDataReader now support bool parameter to close connection after close DataReader.
	- Update method Close in order support CommandBehavior.CloseConnection

*/
#endregion
using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Data.Common;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
	/// <summary>
	/// This class is IDataReader compliant so take a look into MSDN help to understand how it works
	/// </summary>
	public class MySQLDataReader : DbDataReader
    {
        private NativeResult nativeResult;

		internal DataTable dt;
		// Add by Omar del Valle Rodr�guez (omarvr72@yahoo.com.mx)
		// In order support CommandBehavior.CloseConnection 
		/// <summary>
		/// 
		/// </summary>
		protected bool m_CloseConnection = false;
		internal MySQLConnection connection;
		internal System.Globalization.NumberFormatInfo MySQLNumberFormatInfo = new CultureInfo("en-US").NumberFormat;
		#region Constructor & destructor

		internal MySQLDataReader() { }
		// Update by Omar del Valle Rodr�guez (omarvr72@yahoo.com.mx)
		// In order support CommandBehavior.CloseConnection
		internal unsafe MySQLDataReader(IntPtr resultPtr, MySQLConnection _connection, Statement _stmt, bool CloseConnection)
		{
            nativeResult= new NativeResult(resultPtr);
			// Add by Omar del Valle Rodr�guez (omarvr72@yahoo.com.mx)
			// Save if close connection after close MySQLDataReader
			m_CloseConnection = CloseConnection;

			connection = _connection;
			dt = new DataTable();
			uint i; ulong j;
			uint num_fields = nativeResult.mysql_num_fields();
			ulong num_rows = nativeResult.mysql_num_rows();
			for (i = 0; i < num_fields; i++)
			{
				IMySqlField field = new MYSQL_FIELD_FACTORY(_connection.NativeConnection).GetInstance();
				IntPtr ptr;
				ptr = nativeResult.mysql_fetch_field_direct( i);
				Marshal.PtrToStructure(ptr, field);

				dt.Columns.Add(field.Name);

				Type ftype = new MYSQL_FIELD_FACTORY(_connection.NativeConnection).MysqltoNetType(field.Type);
				if (ftype != null)
				{
					dt.Columns[field.Name].DataType = ftype;
					if (dt.Columns[field.Name].DataType == System.Type.GetType("System.Byte[]"))
					{
						dt.Columns[field.Name].ExtendedProperties.Add("max_length", (int)field.Max_Length);
					}
				}
			}
		
			for (j = 0; j < num_rows; j++)
			{
				lock (_stmt)
				{
					if (_stmt.TryToCancel) break;
				}
				//string[] row = null;
				IntPtr myrow = nativeResult.mysql_fetch_row();
				if (myrow.ToPointer() == null)
				{
					throw new MySqlException(_connection.NativeConnection);
				}
				DataRow dr = dt.NewRow();

				/* for BLOB support
				 * "Christophe Ravier" <c.ravier@laposte.net> 2003-11-27*/
				var lengths = nativeResult.mysql_fetch_lengths((int)num_fields);

				for (i = 0; i < num_fields; i++)
				{
					IntPtr ptr = Marshal.ReadIntPtr(myrow, (int)i * sizeof(IntPtr));

					// Old Implementation (before 2003-11-27) ->
					//	string val = Marshal.PtrToStringAnsi(ptr);
					//	if(val==null)
					//		dr[(int)i]=null;
					//	else
					//		dr[(int)i]=val;
					// <- Old Implementation

					/* Text, BLOB and DateTime fields management
					 * "Christophe Ravier" <c.ravier@laposte.net> 2003-11-27*/
					if (dt.Columns[(int)i].DataType == System.Type.GetType("System.Byte[]"))
					{
						byte[] val = null;
						if (Marshal.PtrToStringAnsi(ptr) != null)
						{
							int length = (int)lengths[i];

							val = new byte[length];
							Marshal.Copy(ptr, val, 0, length);
						}

						if (val == null)
							dr[(int)i] = Convert.DBNull;
						else
							dr[(int)i] = val;
					}
					else if (dt.Columns[(int)i].DataType == System.Type.GetType("System.Int16"))
					{
						string val = Marshal.PtrToStringAnsi(ptr);
						if (val == null)
							dr[(int)i] = Convert.DBNull;
						else
						{
							dr[(int)i] = Convert.ToInt16(val);

						}
					}
					else if (dt.Columns[(int)i].DataType == System.Type.GetType("System.Decimal"))
					{
						string val = Marshal.PtrToStringAnsi(ptr);
						if (val == null)
							dr[(int)i] = Convert.DBNull;
						else
						{
							dr[(int)i] = Convert.ToDecimal(val, CultureInfo.InvariantCulture.NumberFormat);
						}
					}
					else if (dt.Columns[(int)i].DataType == System.Type.GetType("System.Double"))
					{
						string val = Marshal.PtrToStringAnsi(ptr);
						if (val == null)
							dr[(int)i] = Convert.DBNull;
						else
						{
							dr[(int)i] = Convert.ToDouble(val, CultureInfo.InvariantCulture.NumberFormat);
						}
					}
					else if (dt.Columns[(int)i].DataType == System.Type.GetType("System.Int32"))
					{
						string val = Marshal.PtrToStringAnsi(ptr);
						if (val == null)
							dr[(int)i] = Convert.DBNull;
						else
						{
							dr[(int)i] = Convert.ToInt32(val);
						}
					}
					else if (dt.Columns[(int)i].DataType == System.Type.GetType("System.DateTime"))
					{
						string val = Marshal.PtrToStringAnsi(ptr);
						if (val == null)
							dr[(int)i] = Convert.DBNull;
						else
						{
							DateTimeFormatInfo format = new DateTimeFormatInfo();
                            if (val.Length > 20)
                            {
                                dr[(int)i] = DateTime.ParseExact(val, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture.DateTimeFormat);
                            }
                            else
                            {
                                if (val.Length > 10)
                                {
                                    dr[(int)i] = DateTime.ParseExact(val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture.DateTimeFormat);
                                }
                                else
                                {
                                    dr[(int)i] = DateTime.ParseExact(val, "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat);
                                }
                            }
						}
					}
					else
					{
						string val = Marshal.PtrToStringAnsi(ptr);
						if (val == null)
							dr[(int)i] = Convert.DBNull;
						else
							dr[(int)i] = val;

					}

				}
				dt.Rows.Add(dr);
			}
		}
		/// <summary>
		/// Closes this reader
		/// </summary>
		public new void Dispose()
		{
			Close();
		}
		#endregion
		#region IDataReader
		/// <summary>
		/// returns 1
		/// </summary>
		public override int Depth { get { return 1; } }
		/// <summary>
		/// Gets a value indicating whether the data reader is closed.
		/// </summary>
		public override bool IsClosed { get { return dt == null; } }
		/// <summary>
		/// 
		/// </summary>
		protected int _RecordsAffected = 0;
		/// <summary>
		/// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
		/// </summary>
		/// <remarks>The RecordsAffected property is not set until all rows are read and you close the MySQLDataReader.</remarks>
		public override int RecordsAffected { get { return _RecordsAffected; } }
		/// <summary>
		/// Closes the MySQLDataReader 0bject.
		/// </summary>
		public override void Close()
		{
            if (nativeResult != null)
            {
                nativeResult.Dispose();
                nativeResult = null;
            }

			// Add by Omar del Valle Rodr�guez (omarvr72@yahoo.com.mx)
			// Close connection if connection is not null and CommandBehavior is CloseConnection
			if (connection != null && m_CloseConnection) 
                connection.Close();



			if (dt == null) return;
			_RecordsAffected = dt.Rows.Count;
			dt.Dispose();
			dt = null;
		}
		/// <summary>
		/// Returns a DataTable that describes the column metadata of the MySQLDataReader and it's values.
		/// </summary>
		/// <returns></returns>
		public override DataTable GetSchemaTable()
		{
			return dt;
		}
		int rowpos = -1;
		/// <summary>
		/// No more results expected
		/// </summary>
		/// <returns></returns>
		public override bool NextResult()
		{
			// MLVL 24/05/2003 ->
			// throw new MySQLException("Operation not supported");
			// <-
			return false;
		}
		/// <summary>
		/// Advances the MySQLDataReader to the next record.
		/// </summary>
		/// <returns></returns>
		public override bool Read()
		{
			if (IsClosed) return false;
			rowpos++;
			if (rowpos < dt.Rows.Count)
				return true;
			else
				return false;
		}
		#endregion
		#region IDataRecord
		/// <summary>
		/// Number of fields returned
		/// </summary>
		public override int FieldCount { get { if (IsClosed) return 0; else return dt.Columns.Count; } }
		/// <summary>
		/// Get value by name
		/// </summary>
		public override object this[string name]
		{
			get
			{
				if (IsClosed) throw new MySqlException("Reader must be open");
				return dt.Rows[rowpos][name];
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
				return dt.Rows[rowpos][i];
			}
		}
		/// <summary>
		/// Get as boolean
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override bool GetBoolean(int i) { return (Convert.ToByte(this[i]) > 0) ? true : false; }
		/// <summary>
		/// Get as byte
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override byte GetByte(int i) { return Convert.ToByte(this[i]); }
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
			object val = this[i];
			if (val == Convert.DBNull)
				return 0;
			else
			{
				byte[] bytes = (byte[])val;
				int maxLen = bytes.Length;
				int bufferIndex = bufferoffset;
				int read = 0;
				for (long j = fieldOffset; read < length && j < maxLen; j++)
				{
					buffer[bufferIndex] = bytes[j];
					bufferIndex++;
					read++;
				}
				return read;
			}
		}
		/// <summary>
		/// Get as char
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override char GetChar(int i) { return Convert.ToChar(this[i]); }
		/// <summary>
		/// Unsupported
		/// </summary>
		/// <param name="i"></param>
		/// <param name="fieldoffset"></param>
		/// <param name="buffer"></param>
		/// <param name="bufferoffset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) { throw new MySqlException("Operation not supported"); }
		/// <summary>
		/// Unsupported
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		protected override DbDataReader GetDbDataReader(int i) { throw new MySqlException("Operation not supported"); }
		/// <summary>
		/// Unsupported
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override string GetDataTypeName(int i) { throw new MySqlException("Operation not supported"); }
		/// <summary>
		/// Get as DateTime
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override DateTime GetDateTime(int i) { return Convert.ToDateTime(this[i], CultureInfo.InvariantCulture.NumberFormat); }
		/// <summary>
		/// Get as decimal
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		/// Modified by Claudia Murialdo (07/24/04) in order to work with 
		/// culture-independent format of numeric values in a stmt.
		public override decimal GetDecimal(int i) { return Convert.ToDecimal(this[i], CultureInfo.InvariantCulture.NumberFormat); }
		/// <summary>
		/// Get as double
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override double GetDouble(int i) { return Convert.ToDouble(this[i].ToString(), MySQLNumberFormatInfo); }
		/// <summary>
		/// Get field type (class Type)
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override Type GetFieldType(int i) { return dt.Columns[i].DataType; } //i{ return typeof(string); }
		/// <summary>
		/// Get as float
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override float GetFloat(int i) { return (float)Convert.ToDouble(this[i].ToString(), MySQLNumberFormatInfo); }
		/// <summary>
		/// Unsupported
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override Guid GetGuid(int i) { throw new MySqlException("Operation not supported"); }
		/// <summary>
		/// Get as Int16
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override short GetInt16(int i) { return Convert.ToInt16(this[i]); }
		/// <summary>
		/// Get as Int32
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override int GetInt32(int i) { return Convert.ToInt32(this[i]); }
		/// <summary>
		/// Get as Int64
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override long GetInt64(int i) { return Convert.ToInt64(this[i]); }
		/// <summary>
		/// Get name of field by index
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override string GetName(int i) { return dt.Columns[i].ColumnName; }
		/// <summary>
		/// Get index of field by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public override int GetOrdinal(string name) { return dt.Columns[name].Ordinal; }
		/// <summary>
		/// Get as string
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		/// <remarks>Remake by "Christophe Ravier" (c.ravier@laposte.net) 2003-11-27</remarks>
		public override string GetString(int i)
		{
			//Convert Text field to string
			object val = this[i];
			if (val is byte[])
				return System.Text.Encoding.Default.GetString((byte[])val);
			else
				return Convert.ToString(val);
		}
		/// <summary>
		/// Get as object
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override object GetValue(int i)
		{
			return this[i];
		}
		/// <summary>
		/// Get full row as array of object
		/// </summary>
		/// <param name="values"></param>
		/// <returns>Fields retrived, if it fails returns -1.</returns>
		/// <remarks>
		/// <list type="bullet">
		/// <listheader><item><description>Bugfixed:</description></item></listheader>
		/// <item><term>2002-10-28</term><description>Values was ignored</description></item>
		/// </list>
		/// </remarks>
		public override int GetValues(object[] values)
		{
			if (values.Length < FieldCount)
				return -1;
			for (int i = 0; i < FieldCount; i++)
				values[i] = GetValue(i);
			return FieldCount;
		}
		/// <summary>
		/// Returns ok if the field is null in database
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		//public bool IsDBNull( int i ){ throw new MySQLException("Operation not supported"); }
		public override bool IsDBNull(int i)
		{
			if (this[i] == Convert.DBNull)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool HasRows
		{
			get { return this.dt.Rows.Count > 0; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override System.Collections.IEnumerator GetEnumerator()
		{
			return dt.Rows.GetEnumerator();
		}
		#endregion
	}
}
