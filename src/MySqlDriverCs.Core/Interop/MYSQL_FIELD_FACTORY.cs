using System;

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
        public  IMySqlField GetInstance()
        {
            if (version == null)
            {
                version = _nativeConnection.GetClientVersion();
            }
            if (version.CompareTo("4.1.2-alpha") >= 0)
            {
                if (MySQLUtils.RunningOn64x)
                    return new MYSQL_FIELD_VERSION_5_64();
                else
                    return new MYSQL_FIELD_VERSION_5_32();
            }
            else
                return new MYSQL_FIELD_VERSION_3();
        }
        /// <summary>
        /// Converts a MySQL Type to a System.Type 
        /// </summary>
        /// <param name="type">MySQL type</param>
        /// <returns>System Type</returns>
        public  Type MysqltoNetType(uint type)
        {
            if (version == null)
            {
                version = _nativeConnection.GetClientVersion();
            }

            if (version.StartsWith("5"))
            {
                if (type == (uint)FieldTypes5.FIELD_TYPE_BLOB) //252)
                {
                    return System.Type.GetType("System.Byte[]");
                }
                else if (type == (uint)FieldTypes5.FIELD_TYPE_SHORT ||
                         type == (uint)FieldTypes5.FIELD_TYPE_TINY)
                {
                    return System.Type.GetType("System.Int16");
                }
                else if (type == (uint)FieldTypes5.FIELD_TYPE_DECIMAL ||
                         type == (uint)FieldTypes5.FIELD_TYPE_NEWDECIMAL)
                {
                    return System.Type.GetType("System.Decimal");
                }
                else if (type == (uint)FieldTypes5.FIELD_TYPE_DOUBLE)
                {
                    return System.Type.GetType("System.Double");
                }
                else if (type == (uint)FieldTypes5.FIELD_TYPE_INT24 ||
                         type == (uint)FieldTypes5.FIELD_TYPE_LONG)
                {
                    return System.Type.GetType("System.Int32");
                }
                else if (type == (uint)FieldTypes5.FIELD_TYPE_TIMESTAMP ||//7 )
                         type == (uint)FieldTypes5.FIELD_TYPE_DATETIME ||
                         type == (uint)FieldTypes5.FIELD_TYPE_DATE ||
                         type == (uint)FieldTypes5.FIELD_TYPE_TIME ||
                         type == (uint)FieldTypes5.FIELD_TYPE_NEWDATE)
                {
                    return System.Type.GetType("System.DateTime");
                }
                else if (type == (uint)FieldTypes5.FIELD_TYPE_STRING)
                {
                    return System.Type.GetType("System.String");
                }
            }
            else
            {
                /* Text, BLOB and DateTime fields management
				 * "Christophe Ravier" <c.ravier@laposte.net> 2003-11-27*/
                if (type == (uint)FieldTypes.FIELD_TYPE_BLOB) //252)
                {
                    return System.Type.GetType("System.Byte[]");
                }
                else if (type == (uint)FieldTypes.FIELD_TYPE_TIMESTAMP ||
                         type == (uint)FieldTypes.FIELD_TYPE_DATETIME ||
                         type == (uint)FieldTypes.FIELD_TYPE_DATE ||
                         type == (uint)FieldTypes.FIELD_TYPE_TIME ||
                         type == (uint)FieldTypes.FIELD_TYPE_NEWDATE)
                {
                    return System.Type.GetType("System.DateTime");
                }
            }
            return null;
        }
    }
}