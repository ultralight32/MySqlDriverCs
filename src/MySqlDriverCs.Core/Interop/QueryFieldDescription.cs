using System;

namespace MySQLDriverCS.Interop
{
    public class QueryFieldDescription
    {
        public QueryFieldDescription(string name, enum_field_types type, long maxLength, uint length, MySqlFieldFlags flags)
        {
            Name = name;
            Type = type;
            MaxLength = maxLength;
            Length = length;
            Flags = flags;

            // computed fields
            FieldType = MysqltoNetType();
        }

        /// <summary>
        /// Name of column
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Type of field
        /// </summary>
        public enum_field_types Type { get; }
        /// <summary>
        /// Max width for selected set
        /// </summary>
        /// <remarks>
        /// contains the length of the largest value for this column in the specific query.
        /// </remarks>
        public long MaxLength { get; }
        /// <summary>
        /// Width of column (create length)
        /// </summary>
        public uint Length { get; }
        /// <summary>
        /// Flags
        /// </summary>
        public MySqlFieldFlags Flags { get; }

        public Type FieldType { get; }


        private Type MysqltoNetType()
        {
            var type = Type;
            var unsigned = Flags.HasFlag(MySqlFieldFlags.UNSIGNED_FLAG);
            var len = MaxLength;

            switch (type)
            {

                case enum_field_types.MYSQL_TYPE_SHORT:
                    return unsigned ? typeof(ushort) : typeof(short);
                case enum_field_types.MYSQL_TYPE_TINY:
                    if (!unsigned && len == 1)
                        return typeof(bool);
                    else
                        return unsigned ? typeof(byte) : typeof(sbyte);
                case enum_field_types.MYSQL_TYPE_DECIMAL:
                case enum_field_types.MYSQL_TYPE_NEWDECIMAL:
                    return typeof(decimal);
                case enum_field_types.MYSQL_TYPE_DOUBLE:
                    return typeof(double);
                case enum_field_types.MYSQL_TYPE_INT24:
                case enum_field_types.MYSQL_TYPE_LONG:
                    return unsigned ? typeof(uint) : typeof(int);
                case enum_field_types.MYSQL_TYPE_LONGLONG:
                    return unsigned ? typeof(ulong) : typeof(long);
                case enum_field_types.MYSQL_TYPE_TIMESTAMP:
                case enum_field_types.MYSQL_TYPE_TIMESTAMP2:
                case enum_field_types.MYSQL_TYPE_DATETIME:
                case enum_field_types.MYSQL_TYPE_DATETIME2:
                case enum_field_types.MYSQL_TYPE_DATE:
                case enum_field_types.MYSQL_TYPE_TIME2:
                case enum_field_types.MYSQL_TYPE_TIME:
                case enum_field_types.MYSQL_TYPE_NEWDATE:
                case enum_field_types.MYSQL_TYPE_YEAR:
                    return typeof(DateTime);
                case enum_field_types.MYSQL_TYPE_STRING:
                case enum_field_types.MYSQL_TYPE_VARCHAR:
                case enum_field_types.MYSQL_TYPE_VAR_STRING:
                    return typeof(string);
                case enum_field_types.MYSQL_TYPE_FLOAT:
                    return typeof(float);
                case enum_field_types.MYSQL_TYPE_NULL:
                    return null;
                case enum_field_types.MYSQL_TYPE_BIT:
                    return typeof(ulong);
                case enum_field_types.MYSQL_TYPE_JSON:
                    return typeof(string);
                case enum_field_types.MYSQL_TYPE_ENUM:
                    return typeof(string);
                case enum_field_types.MYSQL_TYPE_SET:
                    return typeof(string);
                case enum_field_types.MYSQL_TYPE_MEDIUM_BLOB:
                case enum_field_types.MYSQL_TYPE_LONG_BLOB:
                case enum_field_types.MYSQL_TYPE_TINY_BLOB:
                case enum_field_types.MYSQL_TYPE_BLOB:
                    if (Flags.HasFlag(MySqlFieldFlags.BINARY_FLAG))
                        return typeof(byte[]);
                    else 
                        return typeof(string);
                case enum_field_types.MYSQL_TYPE_GEOMETRY:
                    return typeof(IMySQLGeometryObject);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}