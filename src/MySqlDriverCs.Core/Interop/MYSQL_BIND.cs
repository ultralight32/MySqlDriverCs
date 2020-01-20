using MySqlDriverCs;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MySQLDriverCS.Interop
{
#pragma warning disable 1591

    /// <summary>
    /// Support for 64 bit
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
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
        public uint length_value; //  unsigned long length_value;

        /// <summary>
        /// For null count and error messages
        /// </summary>
        public uint param_number;

        /// <summary>
        ///  Internal length for packed data
        /// </summary>
        public uint pack_length;

        /// <summary>
        /// buffer type
        /// </summary>
        public enum_field_types buffer_type; //  enum enum_field_types buffer_type;

        /// <summary>
        /// used if error is 0
        /// </summary>
        public byte error_value;//  my_bool       error_value;

        /// <summary>
        /// set if integer type is unsigned
        /// </summary>
        /// <remarks> my_bool       is_unsigned; </remarks>
        public byte is_unsigned;

        /// <summary>
        /// If used with mysql_send_long_data
        /// </summary>
        /// <remarks>my_bool	long_data_used;	</remarks>
        public byte long_data_used;

        /// <summary>
        /// Used if is_null is 0
        /// </summary>
        public sbyte is_null_value;    //  my_bool	is_null_value;

        /// <summary>
        ///  void *extension;
        /// </summary>
        public IntPtr extension;

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
        /// Bytes in the buffer (when it is a byte[]).
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="len"></param>
        public void GetBytes(byte[] buff, uint len)
        {
            if (!GetIsNull())
            {
                if (len < GetLength())
                    Marshal.Copy(buffer, buff, 0, (int)len);
                else
                    Marshal.Copy(buffer, buff, 0, (int)GetLength());
            }
        }

        /// <summary>
        /// Inits structure for binding.
        /// </summary>
        /// <param name="fieldMetadata"></param>
        public void InitForBind(IMySqlField fieldMetadata, NativeConnection nativeConnection)
        {
            buffer_type = fieldMetadata.Type;
            Type type = MySQLUtils.MySQLToNetType(buffer_type);
            if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_DATETIME || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_DATE || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_DATETIME2 || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_TIME || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_TIME2 || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_TIMESTAMP || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_TIMESTAMP2 || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_NEWDATE)
            {
                SetBuffer(new byte[Marshal.SizeOf<MYSQL_TIME>()]);
            }
            else if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_TINY)
            {
                SetBuffer(new byte[sizeof(byte)]);
            }
            else if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_SHORT)
            {
                SetBuffer(new byte[sizeof(ushort)]);
            }
            else if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_INT24 || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_LONG)
            {
                SetBuffer(new byte[sizeof(uint)]);
            }
            else if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_BIT || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_LONGLONG)
            {
                SetBuffer(new byte[sizeof(ulong)]);
            }
            else if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_JSON || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_TINY_BLOB || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_BLOB || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_LONG_BLOB || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_MEDIUM_BLOB)
            {
                var maxLen = 1024;
                buffer = Marshal.AllocHGlobal((int)maxLen);
                buffer_length = (uint)maxLen;
            }
            else if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_VARCHAR || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_STRING || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_VAR_STRING)
            {
                buffer_length = fieldMetadata.Length * nativeConnection.mysql_get_character_set_info().mbmaxlen; // reserve the max number of chars for this characterset
                buffer = Marshal.StringToHGlobalAnsi(new string(' ', (int)buffer_length));
            }
            else if (fieldMetadata.Type == enum_field_types.MYSQL_TYPE_DECIMAL || fieldMetadata.Type == enum_field_types.MYSQL_TYPE_NEWDECIMAL)
            {
                var maxDecimalLengthString = 65 + 2; // 65 digits, 1 dot, 1 sign
                buffer_length = (uint)(maxDecimalLengthString * nativeConnection.mysql_get_character_set_info().mbmaxlen); // reserve the max number of chars for this characterset
                buffer = Marshal.StringToHGlobalAnsi(new string(' ', (int)buffer_length));
            }
            else if (type == typeof(sbyte[]) || type == typeof(byte[]))
            {
                buffer = Marshal.AllocHGlobal((int)fieldMetadata.MaxLength);
                buffer_length = (uint)fieldMetadata.MaxLength;
            }
            else
            {
                buffer = Marshal.AllocHGlobal(Marshal.SizeOf(type) + 0);
            }
            length = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
            is_null = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(sbyte)));
            is_unsigned = (byte)(fieldMetadata.Flags.HasFlag(MySqlFieldFlags.UNSIGNED_FLAG) ? 1 : 0);
            // TODO: not all fields are filled so in many cases it may respond failing
        }

        public void ResetIsNull()
        {
            if (is_null != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
                is_null = IntPtr.Zero;
            }
        }

        public object GetValue(int columnIndex, NativeStatement nativeStatement, MYSQL_FIELD fieldMetadata)
        {
            if (GetIsNull())
            {
                return DBNull.Value;
            }

            switch (buffer_type)
            {
                case enum_field_types.MYSQL_TYPE_NULL:
                    return DBNull.Value;

                case enum_field_types.MYSQL_TYPE_TINY when (fieldMetadata.Flags & MySqlFieldFlags.UNSIGNED_FLAG) == 0 && fieldMetadata.length == 1:
                    return Marshal.ReadByte(buffer) != 0;

                case enum_field_types.MYSQL_TYPE_TINY when (fieldMetadata.Flags & MySqlFieldFlags.UNSIGNED_FLAG) != 0:
                    return Marshal.ReadByte(buffer);

                case enum_field_types.MYSQL_TYPE_TINY:
                    return unchecked((sbyte)Marshal.ReadByte(buffer));

                case enum_field_types.MYSQL_TYPE_LONGLONG when is_unsigned != 0:
                    return BitConverter.ToUInt64(BitConverter.GetBytes(Marshal.ReadInt64(buffer)), 0);

                case enum_field_types.MYSQL_TYPE_LONGLONG:
                    return Marshal.ReadInt64(buffer);

                case enum_field_types.MYSQL_TYPE_LONG when is_unsigned != 0:
                    return BitConverter.ToUInt32(BitConverter.GetBytes(Marshal.ReadInt32(buffer)), 0);

                case enum_field_types.MYSQL_TYPE_LONG:
                    return Marshal.ReadInt32(buffer);

                case enum_field_types.MYSQL_TYPE_SHORT when is_unsigned != 0:
                    return BitConverter.ToUInt16(BitConverter.GetBytes(Marshal.ReadInt16(buffer)), 0);

                case enum_field_types.MYSQL_TYPE_SHORT:
                    return Marshal.ReadInt16(buffer);

                case enum_field_types.MYSQL_TYPE_INT24:
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
                case enum_field_types.MYSQL_TYPE_LONG_BLOB:
                case enum_field_types.MYSQL_TYPE_MEDIUM_BLOB:
                case enum_field_types.MYSQL_TYPE_TINY_BLOB:
                case enum_field_types.MYSQL_TYPE_BLOB:
                case enum_field_types.MYSQL_TYPE_JSON:
                    {
                        var x = GetAllBytes(columnIndex, nativeStatement);
                        if (x == null)
                            return DBNull.Value;

                        if ((fieldMetadata.Flags & MySqlFieldFlags.BINARY_FLAG) != 0)
                            return x;

                        var encoding = CollationEntries.TryGetEncoding((int)fieldMetadata.charsetnr) ?? Encoding.UTF8;

                        return encoding.GetString(x);
                    }
                case enum_field_types.MYSQL_TYPE_DATE:
                case enum_field_types.MYSQL_TYPE_NEWDATE:
                    {
                        MYSQL_TIME time = (MYSQL_TIME)Marshal.PtrToStructure(buffer, typeof(MYSQL_TIME));
                        return new DateTime((int)time.year, (int)time.month, (int)time.day);
                    }
                case enum_field_types.MYSQL_TYPE_TIME:
                case enum_field_types.MYSQL_TYPE_TIME2:
                    {
                        MYSQL_TIME time = (MYSQL_TIME)Marshal.PtrToStructure(buffer, typeof(MYSQL_TIME));
                        time.year = 1;
                        time.month = 1;
                        time.day = 1;
                        /*
                        There is one exception to this rule though if this structure holds time
                        value (time_type == MYSQL_TIMESTAMP_TIME) days and hour member can hold
                        bigger values
                        */
                        return new DateTime((int)time.year, (int)time.month, (int)time.day, (int)time.hour % 24, (int)time.minute, (int)time.second, (int)(time.second_part / 1000.0));
                    }
                case enum_field_types.MYSQL_TYPE_DATETIME:
                case enum_field_types.MYSQL_TYPE_DATETIME2:
                case enum_field_types.MYSQL_TYPE_TIMESTAMP:
                case enum_field_types.MYSQL_TYPE_TIMESTAMP2:
                    {
                        MYSQL_TIME time = (MYSQL_TIME)Marshal.PtrToStructure(buffer, typeof(MYSQL_TIME));

                        return new DateTime((int)time.year, (int)time.month, (int)time.day, (int)time.hour, (int)time.minute, (int)time.second, (int)(time.second_part / 1000.0));
                    }
                case enum_field_types.MYSQL_TYPE_YEAR:
                    return new DateTime(Marshal.ReadInt16(buffer), 1, 1, 0, 0, 0, 0);

                case enum_field_types.MYSQL_TYPE_FLOAT:
                    return BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(buffer)), 0);

                case enum_field_types.MYSQL_TYPE_DOUBLE:
                    return BitConverter.ToDouble(BitConverter.GetBytes(Marshal.ReadInt64(buffer)), 0);

                case enum_field_types.MYSQL_TYPE_BIT:
                    {
                        //  string string_data = Marshal.PtrToStringAnsi(buffer/*, (int)GetLength()*/);
                        var blen = this.buffer_length;
                        var len = this.GetLength();
                        //return BitConverter.ToUInt64(BitConverter.GetBytes(Marshal.ReadInt64(buffer)), 0);
                        var byteCnt = (7 + fieldMetadata.length) / 8;
                        var b = new byte[sizeof(ulong)];
                        Marshal.Copy(buffer, b, 0, (int)byteCnt);
                        return BitConverter.ToUInt64(b, 0);
                    }
                case enum_field_types.MYSQL_TYPE_SET:
                case enum_field_types.MYSQL_TYPE_STRING:
                case enum_field_types.MYSQL_TYPE_VARCHAR:
                case enum_field_types.MYSQL_TYPE_VAR_STRING:
                case enum_field_types.MYSQL_TYPE_ENUM:
                    {
                        string string_data = Marshal.PtrToStringAnsi(buffer, (int)GetLength());
                        int index = string_data.IndexOf('\0');
                        if (index >= 0)
                            string_data = string_data.Substring(0, index);
                        if (buffer_type == enum_field_types.MYSQL_TYPE_STRING && string_data.Length < buffer_length && (fieldMetadata.Flags & MySqlFieldFlags.ZEROFILL_FLAG) != 0)
                        {
                            string_data = string_data.PadRight((int)buffer_length);
                        }

                        return string_data;
                    }
                case enum_field_types.MYSQL_TYPE_DECIMAL:
                case enum_field_types.MYSQL_TYPE_NEWDECIMAL:
                    {
                        string string_data = Marshal.PtrToStringAnsi(buffer, (int)GetLength());
                        int index = string_data.IndexOf('\0');
                        if (index >= 0)
                            string_data = string_data.Substring(0, index);
                        return decimal.Parse(string_data, CultureInfo.InvariantCulture);
                    }

                case enum_field_types.MYSQL_TYPE_GEOMETRY:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotSupportedException();
            //Type ret;
            //switch (buffer_type)
            //{
            //    case enum_field_types.MYSQL_TYPE_BIT:
            //        ret = typeof(ulong);
            //        break;

            //    default:
            //        Console.WriteLine("Warning MySQLToNetType could not map type: " + buffer_type);
            //        ret = typeof(string);
            //        break;
            //        //case FieldTypes5.MYSQL_TYPE_YEAR: return typeof(long);
            //}

            //Type type = ret;
            //if (type == typeof(string))
            //{
            //    string string_data = Marshal.PtrToStringAnsi(buffer, (int)GetLength());
            //    int index = string_data.IndexOf('\0');
            //    if (index >= 0)
            //        string_data = string_data.Substring(0, index);
            //    if (((enum_field_types)buffer_type) == enum_field_types.MYSQL_TYPE_STRING && string_data.Length < buffer_length)
            //    {
            //        string_data = string_data.PadRight((int)buffer_length);
            //    }

            //    return string_data;
            //}
            //else if (type == typeof(sbyte[]))
            //{
            //    uint len = GetLength();
            //    if (buffer_length < len)
            //        len = buffer_length;
            //    byte[] result = new byte[len];
            //    if (len > 0)
            //    {
            //        Marshal.Copy(buffer, result, 0, (int)len);
            //    }

            //    return result;
            //}
            //else if (type == typeof(MYSQL_TIME))
            //{
            //    MYSQL_TIME time = (MYSQL_TIME)Marshal.PtrToStructure(buffer, typeof(MYSQL_TIME));
            //    try
            //    {
            //        return new DateTime((int)time.year, (int)time.month, (int)time.day, (int)time.hour, (int)time.minute, (int)time.second, (int)(time.second_part / 1000.0));
            //    }
            //    catch
            //    {
            //        return MySQLUtils.MYSQL_NULL_DATE;
            //    }
            //}
            //else
            //{
            //    return Marshal.PtrToStructure(buffer, type);
            //}
        }

        private byte[] GetAllBytes(int id, NativeStatement _stmt)
        {
            if (GetIsNull())
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
            if (GetLength() > BufferLength)//data truncation
            {
                MYSQL_BIND[] newbind = new MYSQL_BIND[1];
                newbind[0] = new MYSQL_BIND();
                IMySqlField ft = new MYSQL_FIELD();
                ft.Type = enum_field_types.MYSQL_TYPE_BLOB;
                ft.MaxLength = (uint)len;
                newbind[0].InitForBind(ft, _stmt._nativeConnection);

                sbyte errorCode = _stmt.mysql_stmt_fetch_column(newbind, (uint)i, (uint)fieldOffset);
                if (errorCode != 0)
                    throw new MySqlException(_stmt);

                long result = Math.Min(len, newbind[0].GetLength() - fieldOffset);
                newbind[0].GetBytes(buf, (uint)result);
                newbind[0].Dispose();
                return result;
            }
            else
            {
                GetBytes(buf, (uint)len);
                return GetLength();
            }
        }

        /// <summary>
        /// Length
        /// </summary>
        public void SetLength(uint value)
        {
            if (length == IntPtr.Zero)
            {
                length = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
            }

            Marshal.WriteInt32(length, (int)value);
        }

        /// <summary>
        /// Length
        /// </summary>
        public uint GetLength()
        {
            if (this.length == IntPtr.Zero) return 0;
            return (uint)Marshal.ReadInt32(this.length);
        }

        /// <summary>
        /// IsNull
        /// </summary>
        public void SetIsNull(bool value)
        {
            if (is_null == IntPtr.Zero)
            {
                is_null = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(sbyte)));
            }

            Marshal.WriteByte(is_null, value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// IsNull
        /// </summary>
        public bool GetIsNull()
        {
            return (sbyte)Marshal.ReadByte(this.is_null) == 1;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            ResetIsNull();
            ResetLength();
            ResetBuffer();
        }

        /// <summary>
        /// Frees buffer and set it to NULL
        /// </summary>
        public void ResetBuffer()
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
            }

            buffer_length = 0;
        }

        public void SetBuffer(byte[] bindInputBuffer)
        {
            if (buffer != IntPtr.Zero)
                Marshal.FreeHGlobal(buffer);
            buffer = Marshal.AllocHGlobal(bindInputBuffer.Length);
            Marshal.Copy(bindInputBuffer, 0, buffer, bindInputBuffer.Length);
            buffer_length = (uint)bindInputBuffer.Length;
        }

        /// <summary>
        /// Frees length and set it to NULL
        /// </summary>
        public void ResetLength()
        {
            if (length == IntPtr.Zero) return;
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
    }
}