namespace MySQLDriverCS.Interop
{
    /// <summary>
    /// Field Types
    /// </summary>
    public enum FieldTypes : uint
    {
        /// <summary>
        /// DECIMAL
        /// </summary>
        MYSQL_TYPE_DECIMAL,
        /// <summary>
        /// TINYINT
        /// </summary>
        MYSQL_TYPE_TINY,
        /// <summary>
        /// SMALLINT
        /// </summary>
        MYSQL_TYPE_SHORT,
        /// <summary>
        /// LONG
        /// </summary>
        MYSQL_TYPE_LONG,
        /// <summary>
        /// FLOAT
        /// </summary>
        MYSQL_TYPE_FLOAT,
        /// <summary>
        /// DOUBLE
        /// </summary>
        MYSQL_TYPE_DOUBLE,
        /// <summary>
        /// NULL
        /// </summary>
        MYSQL_TYPE_NULL,
        /// <summary>
        /// TIMESTAMP
        /// </summary>
        MYSQL_TYPE_TIMESTAMP,
        /// <summary>
        /// BIGINT
        /// </summary>
        MYSQL_TYPE_LONGLONG,
        /// <summary>
        /// INT24
        /// </summary>
        MYSQL_TYPE_INT24,
        /// <summary>
        /// DATE
        /// </summary>
        MYSQL_TYPE_DATE,
        /// <summary>
        /// TIME
        /// </summary>
        MYSQL_TYPE_TIME,
        /// <summary>
        /// DATETIME
        /// </summary>
        MYSQL_TYPE_DATETIME,
        /// <summary>
        /// YEAR
        /// </summary>
        MYSQL_TYPE_YEAR,
        /// <summary>
        /// NEWDATE
        /// </summary>
        MYSQL_TYPE_NEWDATE,
        /// <summary>
        /// ENUM
        /// </summary>
        MYSQL_TYPE_ENUM = 247,
        /// <summary>
        /// 
        /// </summary>
        MYSQL_TYPE_SET = 248,
        /// <summary>
        /// TINYBLOB/TINYTEXT
        /// </summary>
        MYSQL_TYPE_TINY_BLOB = 249,
        /// <summary>
        /// 
        /// </summary>
        MYSQL_TYPE_MEDIUM_BLOB = 250,
        /// <summary>
        /// 
        /// </summary>
        MYSQL_TYPE_LONG_BLOB = 251,
        /// <summary>
        /// 
        /// </summary>
        MYSQL_TYPE_BLOB = 252,
        /// <summary>
        /// VARCHAR/VARBINARY
        /// </summary>
        MYSQL_TYPE_VAR_STRING = 253,
        /// <summary>
        /// CHAR/BINARY
        /// </summary>
        MYSQL_TYPE_STRING = 254,
        /// <summary>
        /// 
        /// </summary>
        MYSQL_TYPE_GEOMETRY = 255
    };
}