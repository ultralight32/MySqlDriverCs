namespace MySQLDriverCS.Interop
{
#warning some enum values are missing
#pragma warning disable 1591
    /// <summary>
    /// Field types for MySQL 5
    /// </summary>
    public enum FieldTypes5 : uint
    {
        FIELD_TYPE_DECIMAL, FIELD_TYPE_TINY,
        FIELD_TYPE_SHORT, FIELD_TYPE_LONG,
        FIELD_TYPE_FLOAT, FIELD_TYPE_DOUBLE,
        FIELD_TYPE_NULL, FIELD_TYPE_TIMESTAMP,
        FIELD_TYPE_LONGLONG, FIELD_TYPE_INT24,
        FIELD_TYPE_DATE, FIELD_TYPE_TIME,
        FIELD_TYPE_DATETIME, FIELD_TYPE_YEAR,
        FIELD_TYPE_NEWDATE, FIELD_TYPE_VARCHAR,
        FIELD_TYPE_BIT,
        FIELD_TYPE_NEWDECIMAL = 246,
        FIELD_TYPE_ENUM = 247,
        FIELD_TYPE_SET = 248,
        FIELD_TYPE_TINY_BLOB = 249,
        FIELD_TYPE_MEDIUM_BLOB = 250,
        FIELD_TYPE_LONG_BLOB = 251,
        FIELD_TYPE_BLOB = 252,
        FIELD_TYPE_VAR_STRING = 253,
        FIELD_TYPE_STRING = 254,
        FIELD_TYPE_GEOMETRY = 255
    }
}