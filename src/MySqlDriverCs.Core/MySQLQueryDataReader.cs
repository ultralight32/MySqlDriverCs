using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Runtime.InteropServices;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    /// <summary>
    /// This class is IDataReader compliant so take a look into MSDN help to understand how it works
    /// </summary>
    public class MySQLQueryDataReader : DbDataReader
    {
        private NativeResult nativeResult;

        internal DataTable dt;
        // Add by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx)
        // In order support CommandBehavior.CloseConnection 
        /// <summary>
        /// 
        /// </summary>
        protected bool m_CloseConnection = false;
        internal MySQLConnection connection;
        internal System.Globalization.NumberFormatInfo MySQLNumberFormatInfo = new CultureInfo("en-US").NumberFormat;


        /// <summary>
        /// Get unmanaged string or throw exception
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        private string GetValidString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new ArgumentNullException(nameof(ptr));
            var s = Marshal.PtrToStringAnsi(ptr);
            if (s == null)
                throw new ArgumentNullException(nameof(ptr));
            return s;
        }


        #region Constructor & destructor

       
        // Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx)
        // In order support CommandBehavior.CloseConnection
        internal  MySQLQueryDataReader(IntPtr resultPtr, MySQLConnection _connection, Statement _stmt, bool CloseConnection)
        {

            nativeResult = new NativeResult(resultPtr);
            // Add by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx)
            // Save if close connection after close MySQLDataReader
            m_CloseConnection = CloseConnection;

            connection = _connection;
            dt = new DataTable();
            uint i; ulong j;
            uint num_fields = nativeResult.mysql_num_fields();
            ulong num_rows = nativeResult.mysql_num_rows();
            List<QueryFieldDescription> fields = new List<QueryFieldDescription>();
            for (i = 0; i < num_fields; i++)
            {

                var field = new MYSQL_FIELD();
                var ptr = nativeResult.mysql_fetch_field_direct(i);
                Marshal.PtrToStructure(ptr, field);


                dt.Columns.Add(field.Name);

                //Type ftype = MySqlField.MysqltoNetType(field);
                var mySqlField = new QueryFieldDescription(field.Name, (enum_field_types)field.Type, field.MaxLength, field.Length, field.Flags);
                if (mySqlField.FieldType != null)
                {
                    dt.Columns[field.Name].DataType = mySqlField.FieldType;
                    if (dt.Columns[field.Name].DataType == System.Type.GetType("System.Byte[]"))
                    {
                        dt.Columns[field.Name].ExtendedProperties.Add("max_length", (int)field.MaxLength);
                    }
                }
                fields.Add(mySqlField);
            }

            for (j = 0; j < num_rows; j++)
            {
                lock (_stmt)
                {
                    if (_stmt.TryToCancel) break;
                }

                IntPtr myrow = nativeResult.mysql_fetch_row();
                if (myrow==IntPtr.Zero)
                {
                    throw new MySqlException(_connection.NativeConnection);
                }
                DataRow dr = dt.NewRow();

                /* for BLOB support
				 * "Christophe Ravier" <c.ravier@laposte.net> 2003-11-27*/
                var lengths = nativeResult.mysql_fetch_lengths((int)num_fields);

                for (i = 0; i < num_fields; i++)
                {
                    QueryFieldDescription queryFieldDescription = fields[(int)i];

                    IntPtr ptr = Marshal.ReadIntPtr(myrow, (int)i * Marshal.SizeOf<IntPtr>());

                    if (ptr == IntPtr.Zero)
                    {
                        dr[(int)i] = Convert.DBNull;
                    }
                    else if (queryFieldDescription.FieldType == typeof(bool))
                    {
                        dr[(int)i] = GetValidString(ptr) != "0";
                    }
                    else if (queryFieldDescription.FieldType == typeof(byte[]))
                    {
                      
                        int length = (int) lengths[i];
                        var val = new byte[length];
                        Marshal.Copy(ptr, val, 0, length);

                        dr[(int) i] = val;
                       
                    }
                    else if (queryFieldDescription.FieldType == typeof(short))
                    {
                        dr[(int) i] = Convert.ToInt16(GetValidString(ptr));
                    }
                    else if (queryFieldDescription.FieldType == typeof(ushort))
                    {
                        dr[(int) i] = Convert.ToUInt16(GetValidString(ptr));
                    }
                    else if (queryFieldDescription.FieldType == typeof(byte))
                    {
                        dr[(int) i] = Convert.ToByte(GetValidString(ptr));
                    }
                    else if (queryFieldDescription.FieldType == typeof(sbyte))
                    {
                        dr[(int) i] = Convert.ToSByte(GetValidString(ptr));
                    }
                    else if (queryFieldDescription.FieldType == typeof(decimal))
                    {
                        dr[(int) i] = Convert.ToDecimal(GetValidString(ptr), CultureInfo.InvariantCulture.NumberFormat);
                    }
                    else if (queryFieldDescription.FieldType == typeof(double))
                    {
                        dr[(int) i] = Convert.ToDouble(GetValidString(ptr), CultureInfo.InvariantCulture.NumberFormat);
                    }
                    else if (queryFieldDescription.FieldType == typeof(float))
                    {
                        dr[(int) i] = Convert.ToSingle(GetValidString(ptr), CultureInfo.InvariantCulture.NumberFormat);
                    }
                    else if (queryFieldDescription.FieldType == typeof(int))
                    {
                        dr[(int) i] = Convert.ToInt32(GetValidString(ptr));
                    }
                    else if (queryFieldDescription.FieldType == typeof(uint))
                    {
                        dr[(int) i] = Convert.ToUInt32(GetValidString(ptr));
                    }
                    else if (queryFieldDescription.FieldType == typeof(long))
                    {
                        dr[(int)i] = Convert.ToInt64(GetValidString(ptr));
                        
                    }
                    else if (queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_GEOMETRY)
                    {
                        // TODO: returns empty string
                        string val = GetValidString(ptr);
                        dr[(int)i] = val;
                    }
                    else if (queryFieldDescription.FieldType == typeof(ulong))
                    {
                        if (queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_BIT)
                        {

                            {
                                int byteCount;
                                if (queryFieldDescription.Length <= 8)
                                    byteCount = 1;
                                else if (queryFieldDescription.Length <= 16)
                                    byteCount = 2;
                                else if (queryFieldDescription.Length <= 24)
                                    byteCount = 3;
                                else if (queryFieldDescription.Length <= 32)
                                    byteCount = 4;
                                else if (queryFieldDescription.Length <= 32 + 8)
                                    byteCount = 5;
                                else if (queryFieldDescription.Length <= 32 + 16)
                                    byteCount = 6;
                                else if (queryFieldDescription.Length <= 32 + 24)
                                    byteCount = 7;
                                else
                                    byteCount = 8;
                                var buffer = new byte[sizeof(long)];
                                for (int l = 0; l < byteCount; l++)
                                {
                                    buffer[l] = Marshal.ReadByte(ptr, l);
                                }


                                dr[(int)i] = BitConverter.ToInt64(buffer, 0);
                            }
                        }
                        else
                        {
                            string val = GetValidString(ptr);
                            if (val == null)
                                dr[(int)i] = Convert.DBNull;
                            else
                            {
                                dr[(int)i] = Convert.ToUInt64(val);
                            }
                        }
                    }
                    else if (queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_DATE || queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_DATE)
                    {
                        string val = GetValidString(ptr);
                        dr[(int)i] = DateTime.ParseExact(val, "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat);
                    }
                    else if (queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_DATETIME || queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_DATETIME2
                                                                                     || queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_TIMESTAMP
                                                                                     || queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_TIMESTAMP2)
                    {
                        string val = GetValidString(ptr);
                        dr[(int)i] = DateTime.ParseExact(val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture.DateTimeFormat);
                    }
                    else if (queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_TIME || queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_TIME2)
                    {
                        string val = GetValidString(ptr);
                        dr[(int)i] = DateTime.ParseExact("0001-01-01 " + val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture.DateTimeFormat);
                    }
                    else if (queryFieldDescription.Type == enum_field_types.MYSQL_TYPE_YEAR)
                    {
                        string val = GetValidString(ptr);
                        dr[(int)i] = DateTime.ParseExact(val, "yyyy", CultureInfo.InvariantCulture.DateTimeFormat);
                    }
                    else if (queryFieldDescription.FieldType == typeof(DateTime))
                    {
                        string val = GetValidString(ptr);
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
                    else if (queryFieldDescription.FieldType == typeof(string))
                    {
                        string val = GetValidString(ptr);
                        if (val == null)
                            dr[(int)i] = Convert.DBNull;
                        else
                            dr[(int)i] = val;

                    }
                    else
                        throw new Exception();

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

            // Add by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx)
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