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
using System.Collections;
using System.Data;
using System.Linq;

namespace MySQLDriverCS
{
    public enum MySqlDbType
    {
        BigInt = 0,
        Binary = 1,
        Bit = 2,
        String = 3,
        Date = 4,
        DateTime = 5,
        Decimal = 6,
        Double = 7,
        Int = 8,
        Json = 9,
        Short = 10,
        Byte = 11,
        Boolean = 12,
        Time = 13,
        Year = 14,
        Point = 15,
        Line = 16,
        Polygon = 17,
        Geometry = 18,
        Float = 19,
        Int24 = 20
    }

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

        private object m_value = null;
        private bool _sourceColumnNullMapping;

        internal enum_field_types GetFieldType()
        {
            switch (DbType)
            {
                case MySqlDbType.BigInt:
                    return enum_field_types.MYSQL_TYPE_LONGLONG;

                case MySqlDbType.Binary:
                    return enum_field_types.MYSQL_TYPE_BLOB;

                case MySqlDbType.Bit:
                    return enum_field_types.MYSQL_TYPE_BIT;

                case MySqlDbType.String:
                    return enum_field_types.MYSQL_TYPE_STRING;

                case MySqlDbType.Date:
                    return enum_field_types.MYSQL_TYPE_DATETIME;

                case MySqlDbType.DateTime:
                    return enum_field_types.MYSQL_TYPE_DATETIME;

                case MySqlDbType.Decimal:
                    return enum_field_types.MYSQL_TYPE_DECIMAL;

                case MySqlDbType.Double:
                    return enum_field_types.MYSQL_TYPE_DOUBLE;

                case MySqlDbType.Int:
                    return enum_field_types.MYSQL_TYPE_LONG;

                case MySqlDbType.Json:
                    return enum_field_types.MYSQL_TYPE_STRING;

                case MySqlDbType.Short:
                    return enum_field_types.MYSQL_TYPE_SHORT;

                case MySqlDbType.Byte:
                    return enum_field_types.MYSQL_TYPE_TINY;

                case MySqlDbType.Boolean:
                    return enum_field_types.MYSQL_TYPE_TINY;

                case MySqlDbType.Time:
                    return enum_field_types.MYSQL_TYPE_DATETIME;

                case MySqlDbType.Year:
                    return enum_field_types.MYSQL_TYPE_DATETIME;

                case MySqlDbType.Point:
                    return enum_field_types.MYSQL_TYPE_GEOMETRY;

                case MySqlDbType.Line:
                    return enum_field_types.MYSQL_TYPE_GEOMETRY;

                case MySqlDbType.Polygon:
                    return enum_field_types.MYSQL_TYPE_GEOMETRY;

                case MySqlDbType.Geometry:
                    return enum_field_types.MYSQL_TYPE_GEOMETRY;

                case MySqlDbType.Float:
                    return enum_field_types.MYSQL_TYPE_FLOAT;
                case MySqlDbType.Int24:
                    return enum_field_types.MYSQL_TYPE_INT24;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

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
            m_value = value;
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
            m_value = DBNull.Value;
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
            _dbType = GuessMySqlDbTypes(m_value).First();
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
                    return new[] { MySqlDbType.Int, MySqlDbType.Int24, MySqlDbType.BigInt, MySqlDbType.Bit };
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
                case DateTime _:
                    return new[] { MySqlDbType.DateTime, MySqlDbType.Time, MySqlDbType.Date, MySqlDbType.Year };
                default:
                    throw new ArgumentOutOfRangeException(nameof(value),value?.ToString());
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
        /// Use MySQLParameter(string parameterName, DbType type)
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="dbType">One of the DbType values</param>
        /// <param name="sourceColumn">This value is ignored. Suported by future Operation</param>
        public MySQLParameter(string parameterName, MySqlDbType dbType, string sourceColumn)
        {
            ParameterName = parameterName;
            _dbType = dbType;
            SourceColumn = sourceColumn;
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
                return m_value;
            }
            set
            {
                if (_dbType == null)
                {
                    if (value != null && value != DBNull.Value)
                    {
                        var type = GuessMySqlDbTypes(value).First();
                        m_value = value;
                        DbType = type;
                    }
                    else
                    {
                        m_value = DBNull.Value;
                    }
                }
                else
                {
                    if (value != null && value != DBNull.Value)
                    {
                        var types = GuessMySqlDbTypes(value);
                        if (types.All(x => x != _dbType))
                            throw new ArgumentException("Incompatible types");
                        m_value = value;
                    }
                    else
                    {
                        m_value = DBNull.Value;
                    }
                }
            }
        }

        #region IDbDataParameter Members

        /// <summary>
        /// Gets or sets the maximum number of digits used to represent the Value property.
        /// </summary>

        public byte Precision { get; set; }

        /// <summary>
        /// Gets or sets the number of decimal places to which Value is resolved.
        /// </summary>
        public byte Scale { get; set; }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        public int Size { get; set; }

        #endregion IDbDataParameter Members

        #region ICloneable Members

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new MySQLParameter(_dbType, Direction, IsNullable, ParameterName,
                SourceColumn, SourceVersion, m_value, Precision, Scale, Size);
        }

        #endregion ICloneable Members

        /// <summary>
        ///
        /// </summary>
        public bool SourceColumnNullMapping
        {
            get { return this._sourceColumnNullMapping; }
            set { this._sourceColumnNullMapping = value; }
        }
    }

    /// <summary>
    /// Provides a mechanism to customize the sort ordering of a MySqlParameter's
    /// collection
    /// </summary>
    public class MySQLParameterComparer : IComparer
    {
        #region IComparer Members

        /// <summary>
        /// Returns the sort order of two MySqlParameter instances.
        /// </summary>
        /// <param name="x">The first MySqlParameter to compare.</param>
        /// <param name="y">The second MySqlParameter to compare. </param>
        /// <returns>A Int32 containing a value that reflects the sort order
        /// of a as compared to b. The following lines defines the
        /// conditions under which the returned value is a negative number,
        /// zero, or a positive number.
        /// Any negative number if a.ParameterName.Length &lt; b.ParameterName.Length.
        /// Zero if a.ParameterName.Length == b.ParameterName.Length.
        /// Any positive number a.ParameterName.Length &gt; b.ParameterName.Length.
        /// </returns>
        public int Compare(object x, object y)
        {
            MySQLParameter p1 = (MySQLParameter)x;
            MySQLParameter p2 = (MySQLParameter)y;
            if (p1.ParameterName.Length < p2.ParameterName.Length)
                return 1;
            else if (p1.ParameterName.Length == p2.ParameterName.Length)
                return 0;
            else
                return -1;
        }

        #endregion IComparer Members
    }
}