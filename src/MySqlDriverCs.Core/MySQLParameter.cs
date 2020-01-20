#region LICENSE

/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.

	Please, Read first MySQLDriverCS LICENSE

	MySQLParameter.cs
	This file is add to MySQLDriverCS.
	Copyright (c) 2003 William Reinoso and Omar del Valle Rodríguez.

	MySQLParameter is part MySQLDriverCS: you can redistribute it and/or modify
	it only part with all set of MySQLDriverCS and under the terms of MySQLDriverCS License.

	You can use this part freely but WITHOUT ANY WARRANTY; without even the implied warranty
	of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
	License for more details.

	Thank to Manuel Lucas Viñas Livschitz by modify MySQLCommand to good result
	in MySQLParameter.

	Please, Send me email to notify any bug - omarvr72@hotmail.com
*/

#endregion LICENSE

using MySQLDriverCS.Interop;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MySQLDriverCS
{
    /// <summary>
    /// Represents a parameter to a MySQLCommand. This class cannot be inherited.
    /// </summary>
    /// <example>Example code to use MySQLParameter and MySQLParameterCollection
    /// <code>
    /// using MySQLDriverCS;
    /// ...
    /// MySQLConnection DBConn;
    /// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
    /// DBConn.Open();
    /// ...
    /// // Query to execute..
    /// // Note @myvaluefield1 and @myvaluefield2 are name parameters in query
    /// string query = "update mytable set myfield1 = @myvaluefield1 where myfield2 = @myvaluefield2";
    ///
    /// MySQLParameter par = new MySQLParameter("@myvaluefield1",System.Data.DbType.String, "This value is ignored");
    /// par.Value = "ju`a'n";
    ///
    /// MySQLParameter par1 = new MySQLParameter("@myvaluefield2",System.Data.DbType.Int16, "This value is ignored");
    /// par1.Value = 1;
    ///
    /// MySQLCommand command = new MySQLCommand(query, conn);
    ///
    /// command.CommandType = CommandType.Text;
    ///
    /// //Use Parameter property to add parameters in ParameterCollection
    /// command.Parameters.Add(par);
    /// command.Parameters.Add(par1);
    ///
    ///
    /// command.ExecuteNonQuery();
    /// ...
    /// DBConn.Close();
    /// </code></example>
    public sealed class MySQLParameter : IDbDataParameter, ICloneable
    {
        private MySqlDbType? _dbType = null;

        private object _value = null;
        private int? _size;

        /// <summary>
        /// Initializes a new instance of the MySQLParameter class.
        /// </summary>
        public MySQLParameter()
        {
        }

        private MySQLParameter(MySqlDbType? dbType, ParameterDirection direction, bool fNullable, string sParamName, string sSourceColumn, DataRowVersion sourceVersion, object value, byte precision, byte scale, int size)
        {
            _dbType = dbType;
            Direction = direction;
            IsNullable = fNullable;
            ParameterName = sParamName;
            SourceColumn = sSourceColumn;
            SourceVersion = sourceVersion;
            _value = value;
            Precision = precision;
            Scale = scale;
            Size = size;
        }

        /// <summary>
        /// Initializes a new instance of the MySQLParameter class with the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="type">One of the DbType values.</param>
        public MySQLParameter(string parameterName, MySqlDbType type)
        {
            ParameterName = parameterName;
            _dbType = type;

        }

        /// <summary>
        /// Initializes a new instance of the MySQLParameter class with the parameter name and a value of the new MySQLParameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="value">An Object that is the value of the MySQLParameter.</param>
        /// <remarks>Setting the value also infers the type.</remarks>
        public MySQLParameter(string parameterName, object value)
        {
            ParameterName = parameterName;
            _dbType = GuessMySqlDbTypes(_value).First();
            this.Value = value;
        }

        private static MySqlDbType[] GuessMySqlDbTypes(object value)
        {

            switch (value)
            {
                case byte[] _:
                    return new[] { MySqlDbType.Binary };
                case int _:
                case uint _:
                    return new[] { MySqlDbType.Int, MySqlDbType.Int24, MySqlDbType.BigInt, MySqlDbType.Year, MySqlDbType.Bit };
                case long _:
                case ulong _:
                    return new[] { MySqlDbType.BigInt, MySqlDbType.Bit };
                case string _:
                    return new[] { MySqlDbType.String, MySqlDbType.Json };
                case decimal _:
                    return new[] { MySqlDbType.Decimal };
                case short _:
                case ushort _:
                    return new[] { MySqlDbType.Short, MySqlDbType.Int, MySqlDbType.Int24, MySqlDbType.BigInt, MySqlDbType.Bit };
                case bool _:
                    return new[] { MySqlDbType.Boolean };
                case byte _:
                case sbyte _:
                    return new[] { MySqlDbType.Byte, MySqlDbType.Short, MySqlDbType.Int, MySqlDbType.Int24, MySqlDbType.BigInt, MySqlDbType.Bit };
                case double _:
                    return new[] { MySqlDbType.Double, MySqlDbType.Float };
                case float _:
                    return new[] { MySqlDbType.Float, MySqlDbType.Double };
                case DateTime datetime:
                    if (datetime.Kind == DateTimeKind.Utc)
                        return new[] { MySqlDbType.TimeStamp, MySqlDbType.DateTime, MySqlDbType.Time, MySqlDbType.Date, MySqlDbType.Year };
                    else
                        return new[] { MySqlDbType.DateTime, MySqlDbType.TimeStamp, MySqlDbType.Time, MySqlDbType.Date, MySqlDbType.Year };
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value?.ToString());
            }
        }

        /// <summary>
        /// Use MySQLParameter(string parameterName, DbType type)
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="dbType">One of the DbType values</param>
        /// <param name="parameterDirection"></param>
        /// <param name="value">value</param>
        public MySQLParameter(string parameterName, MySqlDbType dbType, ParameterDirection parameterDirection, object value)
        {
            ParameterName = parameterName;
            _dbType = dbType;
            Direction = parameterDirection;
            Value = value;
        }



        /// <summary>
        /// Gets or sets the DbType of the parameter.
        /// </summary>
        /// <value>One of the DbType values.</value>
        DbType IDataParameter.DbType
        {
            get
            {
                if (_dbType == null)
                    throw new ArgumentException("DbType not set");
                switch (_dbType.Value)
                {
                    case MySqlDbType.BigInt:
                        return System.Data.DbType.Int64;

                    case MySqlDbType.Binary:
                        return System.Data.DbType.Binary;

                    case MySqlDbType.Bit:
                        return System.Data.DbType.Int64;

                    case MySqlDbType.String:
                        return System.Data.DbType.String;

                    case MySqlDbType.Date:
                        return System.Data.DbType.Date;

                    case MySqlDbType.DateTime:
                        return System.Data.DbType.DateTime;

                    case MySqlDbType.Decimal:
                        return System.Data.DbType.Decimal;

                    case MySqlDbType.Double:
                        return System.Data.DbType.Double;

                    case MySqlDbType.Int:
                    case MySqlDbType.Int24:
                        return System.Data.DbType.Int32;

                    case MySqlDbType.Json:
                        return System.Data.DbType.String;

                    case MySqlDbType.Short:
                        return System.Data.DbType.Int16;

                    case MySqlDbType.Byte:
                        return System.Data.DbType.Byte;

                    case MySqlDbType.Boolean:
                        return System.Data.DbType.Boolean;

                    case MySqlDbType.Time:
                        return System.Data.DbType.DateTime;

                    case MySqlDbType.Year:
                        return System.Data.DbType.DateTime;

                    case MySqlDbType.Point:
                        return System.Data.DbType.Object;

                    case MySqlDbType.Line:
                        return System.Data.DbType.Object;

                    case MySqlDbType.Polygon:
                        return System.Data.DbType.Object;

                    case MySqlDbType.Geometry:
                        return System.Data.DbType.Object;

                    case MySqlDbType.Float:
                        return System.Data.DbType.Single;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set => throw new NotSupportedException();
        }

        public MySqlDbType DbType
        {
            get
            {
                if (_dbType == null)
                    throw new ArgumentException("DbType not set");
                return _dbType.Value;
            }
            set
            {
                if (Value != null && Value != DBNull.Value)
                {
                    var types = GuessMySqlDbTypes(Value);
                    if (types.All(x => x != value))
                        throw new ArgumentException("Incompatible types");

                }

                _dbType = value;
            }
        }

        /// <summary>
        /// Gets value ParameterDirection.Input
        /// </summary>
        /// <value>This value is ignored.</value>
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;

        /// <summary>
		/// Gets a value indicating whether the parameter accepts null values.
		/// </summary>
		public Boolean IsNullable { get; set; } = false;

        /// <summary>
		///
		/// </summary>
		public void ResetDbType()
        { }

        /// <summary>
        /// Gets or sets the name of the MySQLParameter.
        /// </summary>
        /// <value>The name of the MySQLParameter.</value>
        public String ParameterName { get; set; }

        /// <summary>
		/// Oported by future operation.
		/// </summary>
		/// <remarks>This value is ignored by MySQLParameter</remarks>
		public String SourceColumn { get; set; }

        /// <summary>
		///
		/// </summary>
		public DataRowVersion SourceVersion { get; set; } = DataRowVersion.Current;

        /// <summary>
		/// Gets or sets the value of the parameter.
		/// </summary>
		/// <value>An Object that is the value of the parameter. The default value is null.</value>
		public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_dbType == null)
                {
                    if (value != null && value != DBNull.Value)
                    {
                        var type = GuessMySqlDbTypes(value).First();
                        _value = value;
                        DbType = type;
                    }
                    else
                    {
                        _value = DBNull.Value;
                    }
                }
                else
                {
                    if (value != null && value != DBNull.Value)
                    {
                        var types = GuessMySqlDbTypes(value);
                        if (types.All(x => x != _dbType))
                            throw new ArgumentException("Incompatible types");
                        _value = value;
                    }
                    else
                    {
                        _value = DBNull.Value;
                    }
                }
            }
        }

        /// <inheritdoc />
        public byte Precision { get; set; }

        /// <inheritdoc />
        public byte Scale { get; set; }

        /// <inheritdoc />
        public int Size
        {
            get
            {
                if (_size == null) throw new ArgumentException("Size was not set");

                return _size.Value;
            }
            set => _size = value;
        }

        /// <inheritdoc />
        public object Clone()
        {
            return new MySQLParameter(_dbType, Direction, IsNullable, ParameterName,
                SourceColumn, SourceVersion, _value, Precision, Scale, Size);
        }


        internal BindInput GetBindInput()
        {
            var value = _value;
            if (value == DBNull.Value)
                value = null;
            switch (DbType)
            {

                case MySqlDbType.BigInt:
                    if (value == null)
                        return new BindInput(enum_field_types.MYSQL_TYPE_LONGLONG, null, null, true, false);
                    else if (value is ulong || value is uint || value is ushort || value is byte)
                        return new BindInput(enum_field_types.MYSQL_TYPE_LONGLONG, BitConverter.GetBytes(Convert.ToUInt64(value)), null, null, true);
                    else
                        return new BindInput(enum_field_types.MYSQL_TYPE_LONGLONG, BitConverter.GetBytes(Convert.ToInt64(value)), null, null, false);
                case MySqlDbType.Int24:
                case MySqlDbType.Int:
                    if (value == null)
                        return new BindInput(enum_field_types.MYSQL_TYPE_LONG, null, null, true, false);
                    else if (value is ulong || value is uint || value is ushort || value is byte)
                        return new BindInput(enum_field_types.MYSQL_TYPE_LONG, BitConverter.GetBytes(Convert.ToUInt32(value)), null, null, true);
                    else
                        return new BindInput(enum_field_types.MYSQL_TYPE_LONG, BitConverter.GetBytes(Convert.ToInt32(value)), null, null, false);
                case MySqlDbType.Short:
                    if (value == null)
                        return new BindInput(enum_field_types.MYSQL_TYPE_SHORT, null, null, true, false);
                    else if (value is ulong || value is uint || value is ushort || value is byte)
                        return new BindInput(enum_field_types.MYSQL_TYPE_SHORT, BitConverter.GetBytes(Convert.ToUInt16(value)), null, null, true);
                    else
                        return new BindInput(enum_field_types.MYSQL_TYPE_SHORT, BitConverter.GetBytes(Convert.ToInt16(value)), null, null, false);
                case MySqlDbType.Byte:
                    if (value == null)
                        return new BindInput(enum_field_types.MYSQL_TYPE_TINY, null, null, true, false);
                    else if (value is ulong || value is uint || value is ushort || value is byte)
                        return new BindInput(enum_field_types.MYSQL_TYPE_TINY, BitConverter.GetBytes(Convert.ToByte(value)), null, null, true);
                    else
                        return new BindInput(enum_field_types.MYSQL_TYPE_TINY, BitConverter.GetBytes(Convert.ToSByte(value)), null, null, false);
                case MySqlDbType.Binary:
                    {
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_BLOB, null, null, true, false);
                        else if (value is byte[] bytea)
                            return new BindInput(enum_field_types.MYSQL_TYPE_BLOB, bytea, bytea.Length, null, false);
                        else
                            throw new ArgumentException("Invalid value type");
                    }
                case MySqlDbType.Year:
                    if (value == null)
                        return new BindInput(enum_field_types.MYSQL_TYPE_YEAR, null, null, true, false);
                    else if (value is DateTime dateTime)
                        return new BindInput(enum_field_types.MYSQL_TYPE_LONG, BitConverter.GetBytes(dateTime.Year), null, null, false);
                    else
                        return new BindInput(enum_field_types.MYSQL_TYPE_LONG, BitConverter.GetBytes(Convert.ToInt32(value)), null, null, false);
                case MySqlDbType.Boolean:
                    if (value == null)
                        return new BindInput(enum_field_types.MYSQL_TYPE_TINY, null, null, true, false);
                    else if (value is bool boolean)
                        return new BindInput(enum_field_types.MYSQL_TYPE_TINY, BitConverter.GetBytes(Convert.ToByte(boolean ? 1 : 0)), null, null, false);
                    else
                        return new BindInput(enum_field_types.MYSQL_TYPE_TINY, BitConverter.GetBytes(Convert.ToByte(Convert.ToInt64(value) != 0 ? 1 : 0)), null, null, false);
                case MySqlDbType.Decimal:
                    {
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_DECIMAL, null, null, true, false);
                        var dec = Convert.ToDecimal(value);
                        var str = dec.ToString(CultureInfo.InvariantCulture);
                        var bytes = Encoding.ASCII.GetBytes(str);
                        return new BindInput(enum_field_types.MYSQL_TYPE_DECIMAL, bytes, bytes.Length, null, false);
                    }
                case MySqlDbType.String:
                case MySqlDbType.Json:
                    {
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_STRING, null, null, true, false);
                        var bytes = Encoding.ASCII.GetBytes(value.ToString());
                        return new BindInput(enum_field_types.MYSQL_TYPE_STRING, bytes, bytes.Length, null, false);
                    }
                case MySqlDbType.Date:
                    {
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_DATE, null, null, true, false);
                        if (value is DateTime datetime)
                            return new BindInput(enum_field_types.MYSQL_TYPE_DATE, ToMysqlTimeBytes(datetime, enum_mysql_timestamp_type.MYSQL_TIMESTAMP_DATE), null, false, false);
                        throw new ArgumentException("Invalid value type");
                    }

                case MySqlDbType.DateTime:
                    {
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_DATETIME, null, null, true, false);
                        if (value is DateTime datetime)
                            return new BindInput(enum_field_types.MYSQL_TYPE_DATETIME, ToMysqlTimeBytes(datetime, enum_mysql_timestamp_type.MYSQL_TIMESTAMP_DATETIME), null, false, false);
                        throw new ArgumentException("Invalid value type");
                    }

                case MySqlDbType.Time:
                    {
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_TIME, null, null, true, false);
                        if (value is DateTime datetime)
                            return new BindInput(enum_field_types.MYSQL_TYPE_TIME, ToMysqlTimeBytes(datetime, enum_mysql_timestamp_type.MYSQL_TIMESTAMP_TIME), null, false, false);
                        throw new ArgumentException("Invalid value type");
                    }
                case MySqlDbType.TimeStamp:
                    {
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_TIMESTAMP, null, null, true, false);
                        if (value is DateTime datetime)
                            return new BindInput(enum_field_types.MYSQL_TYPE_TIMESTAMP, ToMysqlTimeBytes(datetime, enum_mysql_timestamp_type.MYSQL_TIMESTAMP_DATETIME), null, false, false);
                        throw new ArgumentException("Invalid value type");
                    }
                case MySqlDbType.Double:
                    {
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_DOUBLE, null, null, true, false);
                        var bytes = BitConverter.GetBytes(Convert.ToDouble(value));
                        return new BindInput(enum_field_types.MYSQL_TYPE_DOUBLE, bytes, null, null, false);
                    }
                case MySqlDbType.Float:
                    {
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_FLOAT, null, null, true, false);
                        var bytes = BitConverter.GetBytes(Convert.ToSingle(value));
                        return new BindInput(enum_field_types.MYSQL_TYPE_FLOAT, bytes, null, null, false);
                    }
                case MySqlDbType.Bit:
                    {
                        // workaround for  Using unsupported buffer type: 16 
                        if (value == null)
                            return new BindInput(enum_field_types.MYSQL_TYPE_NULL, null, null, true, false);
                        var bytes=BitConverter.GetBytes(Convert.ToUInt64(value));
                        return new BindInput(enum_field_types.MYSQL_TYPE_LONG, bytes, null, false, false);
                    }
                case MySqlDbType.Point:
                    throw new ArgumentOutOfRangeException();
                case MySqlDbType.Line:
                    throw new ArgumentOutOfRangeException();
                case MySqlDbType.Polygon:
                    throw new ArgumentOutOfRangeException();
                case MySqlDbType.Geometry:
                    throw new ArgumentOutOfRangeException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private byte[] ToMysqlTimeBytes(DateTime datetime, enum_mysql_timestamp_type timeType)
        {
            return StructToBytes(new MYSQL_TIME
            {
                year = (uint) datetime.Year,
                month = (uint) datetime.Month,
                day = (uint) datetime.Day,
                hour = (uint) datetime.Hour,
                minute = (uint) datetime.Minute,
                second = (uint) datetime.Second,
                second_part = (uint) (datetime.Millisecond * 1000),
                time_type = timeType
            });
        }

        private static byte[] StructToBytes(object str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }
}