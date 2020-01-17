using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
    /// <summary>
    /// This structure is used to define bind information, and internally by the client library.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct MYSQL_BIND_32 : IMySqlBind
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
        IntPtr length;
        /// <summary>
        /// Pointer to null indicator * sbyte*
        /// </summary>
        IntPtr is_null;
        /// <summary>
        /// For input, this is a pointer to the buffer in which a statement parameter's data value is stored. 
        /// For output, it is a pointer to the buffer in which to return a result set column value. Buffer to get/put data 
        /// </summary>
        IntPtr buffer;
        sbyte error;                //*
        /// <summary>
        /// The type of the buffer. The allowable buffer_type values are listed later in this section. 
        /// For input, buffer_type indicates what type of value you are binding to a statement parameter. 
        /// For output, it indicates what type of value you expect to receive in a result buffer. (FieldTypes)
        /// </summary>
        uint buffer_type;
        /// <summary>
        /// The actual size of *buffer in bytes. This indicates the maximum amount of data that can be stored in the buffer. 
        /// For character and binary C data, the buffer_length value specifies the length of *buffer when used with mysql_stmt_bind_param(), 
        /// or the maximum number of data bytes that can be fetched into the buffer when used with mysql_stmt_bind_result().
        /// Must be set when fetching str/binary 
        /// </summary>
        uint buffer_length;
        sbyte row_ptr;         // for the current data position *
        uint offset;           // offset position for char/binary fetch 
        uint length_value;     // Used if length is 0 
        uint param_number;     // For null count and error messages
        uint pack_length;      // Internal length for packed data 
        sbyte error_value;      // used if error is 0 
        sbyte is_unsigned;      // set if integer type is unsigned 
        sbyte long_data_used;   // If used with mysql_send_long_data 
        sbyte is_null_value;    // Used if is_null is 0 
        void* nexrow;
        void* nexrow2;
        void* nexrow3;


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
        public uint Type
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
            buffer_type = (uint)fieldMetadata.Type;
            Type type = MySQLUtils.MySQLToNetType(buffer_type);
            if (type == typeof(string))
            {
                buffer_length = (uint)fieldMetadata.Length;
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
        }
        /// <summary>
        /// Data
        /// </summary>
        public object Value
        {
            set
            {
                if (value != null && value != DBNull.Value)
                {
                    Type type = value.GetType();
                    if (type == typeof(string))
                    {
                        if (buffer != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(buffer);
                        }
                        buffer = Marshal.StringToHGlobalAnsi((string)value);
                    }
                    else if (type == typeof(decimal))
                    {
                        if (buffer != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(buffer);
                        }
                        buffer = Marshal.StringToHGlobalAnsi(((decimal)value).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
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
            get
            {
                if (IsNull)
                {
                    return DBNull.Value;
                }
                Type type = MySQLUtils.MySQLToNetType(buffer_type);
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
                        return new DateTime((int)time.year, (int)time.month, (int)time.day, (int)time.hour, (int)time.minute, (int)time.second, (int) (time.second_part/1000.0));
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
                return (s == 1);
            }
            set
            {
                sbyte svalue = value ? (sbyte)1 : (sbyte)0;
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
                    length = IntPtr.Zero;
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