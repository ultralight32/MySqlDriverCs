using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MySqlDriverCs;

namespace MySQLDriverCS.Interop
{
#pragma warning disable 1591
    /// <summary>
    /// Support for 64 bit
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    //Pack = 4  invalid for x64
    public struct MYSQL_BIND
    {
        /// <summary>
        ///  A pointer to an unsigned long  variable that indicates the actual number of bytes of data stored in *buffer. 
        ///  length is used for character or binary C data. For input parameter data binding, length points to an 
        ///  unsigned long variable that indicates the length of the parameter value stored in *buffer; this is used by 
        ///  mysql_stmt_execute(). For output value binding, the return value of mysql_stmt_fetch() determines the 
        ///  interpretation of the length. 
        ///  If mysql_stmt_fetch() returns 0, *length indicates the actual length of the parameter value. 
        ///  If mysql_stmt_fetch() returns MYSQL_DATA_TRUNCATED, *length indicates the non-truncated length of the parameter value. 
        ///  In this case, the minimum of *length and buffer_length indicates the actual length of the value.
        ///  Length is ignored for numeric and temporal data types because the length of the data value is determined by 
        ///  the buffer_type value. output length pointer * uint*
        /// </summary>
        /// <remarks> unsigned long	*length;</remarks>
        public IntPtr length;
        /// <summary>
        /// Pointer to null indicator * sbyte*
        /// </summary><remarks> my_bool       *is_null;</remarks>
        public IntPtr is_null;
        /// <summary>
        /// For input, this is a pointer to the buffer in which a statement parameter's data value is stored. 
        /// For output, it is a pointer to the buffer in which to return a result set column value. Buffer to get/put data 
        /// </summary>
        /// <remarks> void		*buffer;</remarks>
        public IntPtr buffer;
        /// <summary>
        /// set this if you want to track data truncations happened during fetc
        /// </summary>
        /// <remarks>my_bool       *error;</remarks>
        public IntPtr error;
        /// <summary>
        /// for the current data position
        /// </summary>
        /// <remarks>unsigned char *row_ptr</remarks>
        public IntPtr row_ptr;

        /// <summary>
        ///  void (*store_param_func)(NET *net, struct st_mysql_bind *param);
        /// </summary>
        public IntPtr AnonymousMember1;

        /// <summary>
        /// void (*fetch_result)(struct st_mysql_bind *, MYSQL_FIELD *, unsigned char** row);
        /// </summary>
        public IntPtr AnonymousMember2;

        /// <summary>
        ///  void (*skip_result)(struct st_mysql_bind *, MYSQL_FIELD *,unsigned char** row);
        /// </summary>
        public IntPtr AnonymousMember3;

        /// <summary>
        /// output buffer length, must be set when fetching str/binary
        /// </summary>
        public uint buffer_length; //   unsigned long buffer_length;

        /// <summary>
        /// offset position for char/binary fetch
        /// </summary>
        public uint offset; //   unsigned long offset; 

        /// <summary>
        /// Used if length is 0 
        /// </summary>
        uint length_value; //  unsigned long length_value; 

        /// <summary>
        /// For null count and error messages 
        /// </summary>
        uint param_number;

        /// <summary>
        ///  Internal length for packed data
        /// </summary>
        uint pack_length;

        /// <summary>
        /// buffer type
        /// </summary>
        enum_field_types buffer_type; //  enum enum_field_types buffer_type;

        /// <summary>
        /// used if error is 0
        /// </summary>
        byte error_value;//  my_bool       error_value;  

        /// <summary>
        /// set if integer type is unsigned
        /// </summary>
        /// <remarks> my_bool       is_unsigned; </remarks>
        byte is_unsigned;

        /// <summary>
        /// If used with mysql_send_long_data
        /// </summary>
        /// <remarks>my_bool	long_data_used;	</remarks>
        byte long_data_used;

        /// <summary>
        /// Used if is_null is 0
        /// </summary>
        sbyte is_null_value;    //  my_bool	is_null_value;

        /// <summary>
        ///  void *extension;
        /// </summary>
        IntPtr extension;

        /// <summary>
        /// The length of the buffer. You don't have to set it for any fixed length buffer: float, double,	int, etc. 
        /// It must be set however for variable-length types, such as BLOBs or STRINGs.
        /// </summary>
        public uint BufferLength
        {
            set { buffer_length = value; }
            get { return buffer_length; }
        }
        /// <summary>
        /// One of the MYSQL_* types, used to describe the host language type of buffer.
        /// On output: if column type is different from buffer_type, column value is automatically converted
        /// to buffer_type before it is stored in the buffer.
        /// </summary>
        public enum_field_types Type
        {
            get { return buffer_type; }
            set { buffer_type = value; }
        }
        /// <summary>
        /// Bytes in the buffer (when it is a byte[]).
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="len"></param>
        public void GetBytes(byte[] buff, uint len)
        {
            if (!IsNull)
            {
                if (len < Length)
                    Marshal.Copy(buffer, buff, 0, (int)len);
                else
                    Marshal.Copy(buffer, buff, 0, (int)Length);
            }
        }
        /// <summary>
        /// Inits structure for binding. 
        /// </summary>
        /// <param name="fieldMetadata"></param>
        public void InitForBind(IMySqlField fieldMetadata)
        {
            buffer_type = fieldMetadata.Type;
            Type type = MySQLUtils.MySQLToNetType(buffer_type);
            if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_JSON)
            {
                // TODO: SKip this case
                throw new Exception();
            }
            else if (type == typeof(string))
            {
                buffer_length = fieldMetadata.Length;
                buffer = Marshal.StringToHGlobalAnsi(new string(' ', (int)buffer_length));
            }
            else if (type == typeof(sbyte[]) || type == typeof(byte[]))
            {
                buffer = Marshal.AllocHGlobal((int)fieldMetadata.MaxLength);
                buffer_length = (uint)fieldMetadata.MaxLength;
            }
            else
            {
                int add = 0;
                if (type == typeof(Int64))
                {
                    add = NativeConnection.INT64_ADDITIONAL_MEMORY_BUFFER;
                }
                buffer = Marshal.AllocHGlobal(Marshal.SizeOf(type) + add);
            }
            length = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
            is_null = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(sbyte)));
            is_unsigned = (byte)(fieldMetadata.Flags.HasFlag(MySqlFieldFlags.UNSIGNED_FLAG) ? 1 : 0);
            // TODO: not all fields are filled so in many cases it may respond failing
        }


        public void SetValue(object value)
        {
            if (value != null && value != DBNull.Value)
            {
                Type type = value.GetType();
                if (buffer_type == enum_field_types.MYSQL_TYPE_TINY)
                {
                    if (value is bool o)
                    {
                        value = o ? (byte)1 : (byte)0;
                    }

                    if (value is byte)
                    {
                        var b = (byte)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(sizeof(byte));
                        Marshal.StructureToPtr(b, buffer, false);
                        is_unsigned = 1;
                    }
                    else if (value is sbyte)
                    {
                        var b = (sbyte)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(sizeof(sbyte));
                        Marshal.StructureToPtr(b, buffer, false);
                    }
                    else throw new ArgumentOutOfRangeException(nameof(value));
                }
                else if (buffer_type == enum_field_types.MYSQL_TYPE_SHORT)
                {
                    if (value is short)
                    {
                        var b = (short)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(sizeof(short));
                        Marshal.StructureToPtr(b, buffer, false);
                    }
                    else if (value is ushort)
                    {
                        var b = (ushort)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(sizeof(ushort));
                        Marshal.StructureToPtr(b, buffer, false);
                        is_unsigned = 1;
                    }
                    else throw new ArgumentOutOfRangeException(nameof(value));
                }
                else if (buffer_type == enum_field_types.MYSQL_TYPE_LONGLONG || buffer_type == enum_field_types.MYSQL_TYPE_BIT)
                {
                    if (value is long)
                    {
                        var b = (long)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(sizeof(long));
                        Marshal.StructureToPtr(b, buffer, false);
                    }
                    else if (value is ulong)
                    {
                        var b = (ulong)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(sizeof(ulong));
                        Marshal.StructureToPtr(b, buffer, false);
                        is_unsigned = 1;
                    }
                    else throw new ArgumentOutOfRangeException(nameof(value));
                }
                else if (buffer_type == enum_field_types.MYSQL_TYPE_LONG)
                {
                    if (value is int)
                    {
                        var b = (int)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(sizeof(int));
                        Marshal.StructureToPtr(b, buffer, false);
                    }
                    else if (value is uint)
                    {
                        var b = (uint)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(sizeof(uint));
                        Marshal.StructureToPtr(b, buffer, false);
                        is_unsigned = 1;
                    }
                    else throw new ArgumentOutOfRangeException(nameof(value));
                }
                else if (buffer_type == enum_field_types.MYSQL_TYPE_INT24)
                {
                    if (value is int)
                    {
                        var b = (int)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(3);
                        var bx = BitConverter.GetBytes(b).Take(3).ToArray();
                        Marshal.Copy(bx, 0, buffer, 3);
                        is_unsigned = 0;
                    }
                    else if (value is uint)
                    {
                        var b = (uint)value;
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(3);
                        var bx = BitConverter.GetBytes(b).Take(3).ToArray();
                        Marshal.Copy(bx, 0, buffer, 3);
                        is_unsigned = 1;
                    }
                    else throw new ArgumentOutOfRangeException(nameof(value));
                }
                else if (value is byte[] blob && (buffer_type == enum_field_types.MYSQL_TYPE_BLOB || buffer_type == enum_field_types.MYSQL_TYPE_TINY_BLOB || buffer_type == enum_field_types.MYSQL_TYPE_MEDIUM_BLOB || buffer_type == enum_field_types.MYSQL_TYPE_LONG_BLOB))
                {
                    buffer = Marshal.AllocHGlobal(blob.Length);
                    Marshal.Copy(blob, 0, buffer, blob.Length);
                    Length = (uint) blob.Length;
                }

                else if (type == typeof(string))
                {
                    if (buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(buffer);
                    }

                    buffer = Marshal.StringToHGlobalAnsi((string)value);
                }
                else if (buffer_type == enum_field_types.MYSQL_TYPE_DECIMAL || buffer_type == enum_field_types.MYSQL_TYPE_NEWDECIMAL)
                {
                    if (buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(buffer);
                    }

                    var s = ((decimal)value).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    buffer = Marshal.StringToHGlobalAnsi(s);
                }
                else if (value is Array)
                {
                    if (buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(buffer);
                    }

                    if (value is byte[] || value is sbyte[])
                    {
                        byte[] byteArrValue = (byte[])value;
                        int size = byteArrValue.Length > 0 ? Marshal.SizeOf(byteArrValue[0]) * byteArrValue.Length : 0;
                        // Initialize unmanged memory to hold the array.
                        buffer = Marshal.AllocHGlobal(size);
                        // Copy the array to unmanaged memory.
                        Marshal.Copy(byteArrValue, 0, buffer, byteArrValue.Length);
                    }
                    else
                    {
                        buffer = Marshal.UnsafeAddrOfPinnedArrayElement((Array)value, 0);
                    }
                }
                else if (type == typeof(DateTime))
                {
                    if (buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(buffer);
                    }

                    DateTime dvalue = (DateTime)value;
                    MYSQL_TIME mysqltime = new MYSQL_TIME();
                    mysqltime.year = (uint)dvalue.Year;
                    mysqltime.month = (uint)dvalue.Month;
                    mysqltime.day = (uint)dvalue.Day;
                    mysqltime.hour = (uint)dvalue.Hour;
                    mysqltime.minute = (uint)dvalue.Minute;
                    mysqltime.second = (uint)dvalue.Second;
                    mysqltime.second_part = (uint)(dvalue.Millisecond * 1000);
                    buffer = Marshal.AllocHGlobal(Marshal.SizeOf(mysqltime));
                    Marshal.StructureToPtr(mysqltime, buffer, false);
                }
                else
                {
                    if (buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(buffer);
                    }

                    buffer = Marshal.AllocHGlobal(Marshal.SizeOf(value));
                    Marshal.StructureToPtr(value, buffer, false);
                }
            }
            else
            {
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buffer);
                    buffer = IntPtr.Zero;
                }
            }
        }

        public object GetValue(int columnIndex, NativeStatement nativeStatement, MYSQL_FIELD fieldMetadata)
        {
            if (IsNull || buffer_type == enum_field_types.MYSQL_TYPE_NULL)
            {
                return DBNull.Value;
            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_TINY)
            {
                if ((fieldMetadata.Flags & MySqlFieldFlags.UNSIGNED_FLAG) == 0 && fieldMetadata.length == 1)
                    return Marshal.ReadByte(buffer) != 0;
                if ((fieldMetadata.Flags & MySqlFieldFlags.UNSIGNED_FLAG) != 0)
                    return Marshal.ReadByte(buffer);
                else return unchecked((sbyte)Marshal.ReadByte(buffer));

            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_LONGLONG)
            {
                if (is_unsigned != 0)
                    return BitConverter.ToUInt64(BitConverter.GetBytes(Marshal.ReadInt64(buffer)), 0);
                else
                    return Marshal.ReadInt64(buffer);
            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_LONG)
            {
                if (is_unsigned != 0)
                    return BitConverter.ToUInt32(BitConverter.GetBytes(Marshal.ReadInt32(buffer)), 0);
                else
                    return Marshal.ReadInt32(buffer);
            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_SHORT)
            {
                if (is_unsigned != 0)
                    return BitConverter.ToUInt16(BitConverter.GetBytes(Marshal.ReadInt16(buffer)), 0);
                else
                    return Marshal.ReadInt16(buffer);
            }


            if (buffer_type == enum_field_types.MYSQL_TYPE_INT24)
            {
                var readInt16 = Marshal.ReadInt16(buffer);
                var readByte = Marshal.ReadByte(buffer, 2);
                var b = BitConverter.GetBytes(readInt16).Concat(BitConverter.GetBytes(readByte)).Concat(new byte[] { (byte)(readByte > 127 ? 0xFF : 0) }).ToArray();
                if (is_unsigned != 0)
                {
                    return BitConverter.ToUInt32(b, 0);
                }
                else
                {
                    return BitConverter.ToInt32(b, 0);
                }
            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_LONG_BLOB ||
                buffer_type == enum_field_types.MYSQL_TYPE_MEDIUM_BLOB ||
                buffer_type == enum_field_types.MYSQL_TYPE_TINY_BLOB ||
                buffer_type == enum_field_types.MYSQL_TYPE_BLOB)
            {
                var x = GetAllBytes(columnIndex, nativeStatement);
                if (x == null)
                    return DBNull.Value;

                if ((fieldMetadata.Flags & MySqlFieldFlags.BINARY_FLAG) != 0)
                    return x;

                var encoding = CollationEntries.TryGetEncoding((int)fieldMetadata.charsetnr) ?? Encoding.UTF8;

                return encoding.GetString(x);

            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_TIME || buffer_type == enum_field_types.MYSQL_TYPE_TIME2)
            {
                MYSQL_TIME time = (MYSQL_TIME)Marshal.PtrToStructure(buffer, typeof(MYSQL_TIME));
                time.year = 1;
                time.month = 1;
                time.day = 1;
                return new DateTime((int)time.year, (int)time.month, (int)time.day, (int)time.hour, (int)time.minute, (int)time.second, (int)(time.second_part / 1000.0));

            }
            if (buffer_type == enum_field_types.MYSQL_TYPE_DATETIME || buffer_type == enum_field_types.MYSQL_TYPE_DATETIME2 || buffer_type == enum_field_types.MYSQL_TYPE_TIMESTAMP || buffer_type == enum_field_types.MYSQL_TYPE_TIMESTAMP2)
            {
                MYSQL_TIME time = (MYSQL_TIME)Marshal.PtrToStructure(buffer, typeof(MYSQL_TIME));

                return new DateTime((int)time.year, (int)time.month, (int)time.day, (int)time.hour, (int)time.minute, (int)time.second, (int)(time.second_part / 1000.0));

            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_YEAR)
            {

                return new DateTime(Marshal.ReadInt16(buffer), 1, 1, 0, 0, 0, 0);

            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_FLOAT)
            {

                return BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(buffer)), 0);

            }
            if (buffer_type == enum_field_types.MYSQL_TYPE_DOUBLE)
            {

                return BitConverter.ToDouble(BitConverter.GetBytes(Marshal.ReadInt64(buffer)), 0);

            }
            if (buffer_type == enum_field_types.MYSQL_TYPE_BIT)
            {
                //return BitConverter.ToUInt64(BitConverter.GetBytes(Marshal.ReadInt64(buffer)), 0);
                var byteCnt = (7 + fieldMetadata.length) / 8;
                var b = new byte[sizeof(ulong)];
                Marshal.Copy(buffer, b, 0, (int)byteCnt);
                return BitConverter.ToUInt64(b, 0);

            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_SET
                || buffer_type == enum_field_types.MYSQL_TYPE_STRING
                || buffer_type == enum_field_types.MYSQL_TYPE_VARCHAR
                || buffer_type == enum_field_types.MYSQL_TYPE_VAR_STRING)
            {
                string string_data = Marshal.PtrToStringAnsi(buffer, (int)Length);
                int index = string_data.IndexOf('\0');
                if (index >= 0)
                    string_data = string_data.Substring(0, index);
                if (buffer_type == enum_field_types.MYSQL_TYPE_STRING && string_data.Length < buffer_length && (fieldMetadata.Flags & MySqlFieldFlags.ZEROFILL_FLAG) != 0)
                {
                    string_data = string_data.PadRight((int)buffer_length);
                }

                return string_data;
            }

            if (buffer_type == enum_field_types.MYSQL_TYPE_DECIMAL
                || buffer_type == enum_field_types.MYSQL_TYPE_NEWDECIMAL)
            {
                string string_data = Marshal.PtrToStringAnsi(buffer, (int)Length);
                int index = string_data.IndexOf('\0');
                if (index >= 0)
                    string_data = string_data.Substring(0, index);
                return decimal.Parse(string_data, CultureInfo.InvariantCulture);

            }

            Type ret;
            switch (buffer_type)
            {
                case enum_field_types.MYSQL_TYPE_BIT:
                    ret = typeof(ulong);
                    break;




                default:
                    Console.WriteLine("Warning MySQLToNetType could not map type: " + buffer_type);
                    ret = typeof(string);
                    break;
                    //case FieldTypes5.MYSQL_TYPE_YEAR: return typeof(long);
            }

            Type type = ret;
            if (type == typeof(string))
            {
                string string_data = Marshal.PtrToStringAnsi(buffer, (int)Length);
                int index = string_data.IndexOf('\0');
                if (index >= 0)
                    string_data = string_data.Substring(0, index);
                if (((enum_field_types)buffer_type) == enum_field_types.MYSQL_TYPE_STRING && string_data.Length < buffer_length)
                {
                    string_data = string_data.PadRight((int)buffer_length);
                }

                return string_data;
            }
            else if (type == typeof(sbyte[]))
            {
                uint len = Length;
                if (buffer_length < len)
                    len = buffer_length;
                byte[] result = new byte[len];
                if (len > 0)
                {
                    Marshal.Copy(buffer, result, 0, (int)len);
                }

                return result;
            }
            else if (type == typeof(MYSQL_TIME))
            {
                MYSQL_TIME time = (MYSQL_TIME)Marshal.PtrToStructure(buffer, typeof(MYSQL_TIME));
                try
                {
                    return new DateTime((int)time.year, (int)time.month, (int)time.day, (int)time.hour, (int)time.minute, (int)time.second, (int)(time.second_part / 1000.0));
                }
                catch
                {
                    return MySQLUtils.MYSQL_NULL_DATE;
                }
            }
            else
            {
                return Marshal.PtrToStructure(buffer, type);
            }
        }

        private byte[] GetAllBytes(int id, NativeStatement _stmt)
        {
            if (IsNull)
            {
                return null;
                //return DBNull.Value;
            }
            else
            {
                const int bufferSize = 1024;
                byte[] outbyte = new byte[bufferSize];

                // Reset the starting byte for the new BLOB.
                long startIndex = 0;

                var retval = GetBytes(id, startIndex, outbyte, bufferSize, _stmt);

                if (retval == 0)
                    return new byte[0];

                using (var fs = new MemoryStream())
                {
                    var bw = new BinaryWriter(fs);

                    // Continue reading and writing while there are bytes beyond the size of the buffer.
                    while (retval == bufferSize)
                    {
                        bw.Write(outbyte);
                        bw.Flush();
                        // Reposition the start index to the end of the last buffer and fill the buffer.
                        startIndex += bufferSize;
                        retval = GetBytes(id, startIndex, outbyte, bufferSize, _stmt);
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

                    return fs.ToArray();
                }
            }
        }

        private long GetBytes(int i, long fieldOffset, byte[] buf, int len, NativeStatement _stmt)
        {
            if (Length > BufferLength)//data truncation
            {
                MYSQL_BIND[] newbind = new MYSQL_BIND[1];
                newbind[0] = new MYSQL_BIND();
                IMySqlField ft = new MYSQL_FIELD();
                ft.Type = enum_field_types.MYSQL_TYPE_BLOB;
                ft.MaxLength = (uint)len;
                newbind[0].InitForBind(ft);

                sbyte errorCode = _stmt.mysql_stmt_fetch_column(newbind, (uint)i, (uint)fieldOffset);
                if (errorCode != 0)
                    throw new MySqlException(_stmt);

                long result = Math.Min(len, newbind[0].Length - fieldOffset);
                newbind[0].GetBytes(buf, (uint)result);
                newbind[0].Dispose();
                return result;
            }
            else
            {
                GetBytes(buf, (uint)len);
                return Length;
            }
        }

        /// <summary>
        /// Length
        /// </summary>
        public uint Length
        {
            get
            {
                if (this.length == IntPtr.Zero) return 0;
                else
                    return (uint)Marshal.ReadInt32(this.length);
            }
            set
            {
                if (length == IntPtr.Zero)
                {
                    length = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
                }
                Marshal.WriteInt32(length, (int)value);
            }
        }
        /// <summary>
        /// IsNull
        /// </summary>
        public bool IsNull
        {
            get
            {
                sbyte s = (sbyte)Marshal.ReadByte(this.is_null);
                return s == 1;
            }
            set
            {

                if (is_null == IntPtr.Zero)
                {
                    is_null = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(sbyte)));
                }
                Marshal.WriteByte(is_null, value ? (byte)1 : (byte)0);
            }
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (is_null != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(is_null);
                    is_null = IntPtr.Zero;
                }
                if (length != IntPtr.Zero)
                {
                    //Marshal.FreeHGlobal(length); Cannot free length, for some reason it throws "The handle is invalid" in some cases
                    // cmurialdo fix
                    try
                    {
                        Marshal.FreeHGlobal(length);
                    }
                    catch (COMException)
                    {
                        // Previously we didn't release at all, now we try to release and catch exception
                    }
                    finally
                    {
                        length = IntPtr.Zero;
                    }
                }
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buffer);
                    buffer = IntPtr.Zero;
                }
            }
            catch (COMException ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}