using System;
using System.Collections;

namespace MySQLDriverCS.Interop
{
    /// <summary>
    ///   This class is used to get information about client library.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class MYSQL_FIELD_FACTORY
    {
        private readonly NativeConnection _nativeConnection;
        static string version;

        internal MYSQL_FIELD_FACTORY(NativeConnection nativeConnection)
        {
            _nativeConnection = nativeConnection;
        }
        /// <summary>
        /// Returns a IMYSQL_FIELD instance according to the client library version.
        /// </summary>
        /// <returns></returns>
        public IMySqlField GetInstance()
        {
            if (version == null)
            {
                version = _nativeConnection.ClientVersion;
            }

            if (MySQLUtils.RunningOn64x)
                return new MYSQL_FIELD_VERSION_5_64();
            else
                return new MYSQL_FIELD_VERSION_5_32();

        }

        /// <summary>
        /// Converts a MySQL Type to a System.Type 
        /// </summary>
        /// <param name="type">MySQL type</param>
        /// <param name="fieldMetadata"></param>
        /// <returns>System Type</returns>
        public Type MysqltoNetType(IMySqlField fieldMetadata)
        {
            if (version == null)
            {
                version = _nativeConnection.ClientVersion;
            }

            var type = fieldMetadata.Type;
            var unsigned = fieldMetadata.Flags.HasFlag(MySqlFieldFlags.UNSIGNED_FLAG);
            var len = fieldMetadata.MaxLength;

            switch ((enum_field_types)type)
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
                    return typeof(DateTime);
                case enum_field_types.MYSQL_TYPE_STRING:
                case enum_field_types.MYSQL_TYPE_VARCHAR:
                case enum_field_types.MYSQL_TYPE_VAR_STRING:
                    return typeof(string);
                case enum_field_types.MYSQL_TYPE_FLOAT:
                    return typeof(float);
                case enum_field_types.MYSQL_TYPE_NULL:
                    return null;
                case enum_field_types.MYSQL_TYPE_YEAR:
                    throw new NotSupportedException();
                case enum_field_types.MYSQL_TYPE_BIT:
                    return typeof(ulong);
                case enum_field_types.MYSQL_TYPE_JSON:
                    throw new NotSupportedException();
                case enum_field_types.MYSQL_TYPE_ENUM:
                    throw new NotSupportedException();
                case enum_field_types.MYSQL_TYPE_SET:
                    throw new NotSupportedException();
                case enum_field_types.MYSQL_TYPE_MEDIUM_BLOB:
                case enum_field_types.MYSQL_TYPE_LONG_BLOB:
                case enum_field_types.MYSQL_TYPE_TINY_BLOB:
                case enum_field_types.MYSQL_TYPE_BLOB:
                    return typeof(byte[]);
                case enum_field_types.MYSQL_TYPE_GEOMETRY:
                    throw new NotSupportedException();
                default:
                    throw new NotSupportedException();
            }
        }
    }
}