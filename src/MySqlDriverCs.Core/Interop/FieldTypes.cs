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
        FIELD_TYPE_DECIMAL,
        /// <summary>
        /// TINYINT
        /// </summary>
        FIELD_TYPE_TINY,
        /// <summary>
        /// SMALLINT
        /// </summary>
        FIELD_TYPE_SHORT,
        /// <summary>
        /// LONG
        /// </summary>
        FIELD_TYPE_LONG,
        /// <summary>
        /// FLOAT
        /// </summary>
        FIELD_TYPE_FLOAT,
        /// <summary>
        /// DOUBLE
        /// </summary>
        FIELD_TYPE_DOUBLE,
        /// <summary>
        /// NULL
        /// </summary>
        FIELD_TYPE_NULL,
        /// <summary>
        /// TIMESTAMP
        /// </summary>
        FIELD_TYPE_TIMESTAMP,
        /// <summary>
        /// BIGINT
        /// </summary>
        FIELD_TYPE_LONGLONG,
        /// <summary>
        /// INT24
        /// </summary>
        FIELD_TYPE_INT24,
        /// <summary>
        /// DATE
        /// </summary>
        FIELD_TYPE_DATE,
        /// <summary>
        /// TIME
        /// </summary>
        FIELD_TYPE_TIME,
        /// <summary>
        /// DATETIME
        /// </summary>
        FIELD_TYPE_DATETIME,
        /// <summary>
        /// YEAR
        /// </summary>
        FIELD_TYPE_YEAR,
        /// <summary>
        /// NEWDATE
        /// </summary>
        FIELD_TYPE_NEWDATE,
        /// <summary>
        /// ENUM
        /// </summary>
        FIELD_TYPE_ENUM = 247,
        /// <summary>
        /// 
        /// </summary>
        FIELD_TYPE_SET = 248,
        /// <summary>
        /// TINYBLOB/TINYTEXT
        /// </summary>
        FIELD_TYPE_TINY_BLOB = 249,
        /// <summary>
        /// 
        /// </summary>
        FIELD_TYPE_MEDIUM_BLOB = 250,
        /// <summary>
        /// 
        /// </summary>
        FIELD_TYPE_LONG_BLOB = 251,
        /// <summary>
        /// 
        /// </summary>
        FIELD_TYPE_BLOB = 252,
        /// <summary>
        /// VARCHAR/VARBINARY
        /// </summary>
        FIELD_TYPE_VAR_STRING = 253,
        /// <summary>
        /// CHAR/BINARY
        /// </summary>
        FIELD_TYPE_STRING = 254,
        /// <summary>
        /// 
        /// </summary>
        FIELD_TYPE_GEOMETRY = 255
    };
}