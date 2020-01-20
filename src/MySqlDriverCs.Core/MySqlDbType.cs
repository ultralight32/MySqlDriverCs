namespace MySQLDriverCS
{
    /// <summary>
    /// Variable types
    /// </summary>
    public enum MySQLDbType
    {
        /// <summary>
        /// Int 64
        /// </summary>
        BigInt = 0,
        /// <summary>
        /// Byte arrays
        /// </summary>
        Binary = 1,
        /// <summary>
        /// Bitset up to 64 bit
        /// </summary>
        Bit = 2,
        /// <summary>
        /// String
        /// </summary>
        String = 3,
        /// <summary>
        /// Date
        /// </summary>
        Date = 4,
        /// <summary>
        /// The DATETIME type is used for values that contain both date and time parts. MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format. The supported range is '1000-01-01 00:00:00' to '9999-12-31 23:59:59'
        /// </summary>
        DateTime = 5,
        /// <summary>
        /// Decimal
        /// </summary>
        Decimal = 6,
        /// <summary>
        /// IEEE double precision
        /// </summary>
        Double = 7,
        /// <summary>
        /// Int 32
        /// </summary>
        Int = 8,
        /// <summary>
        /// A string containing a JSON
        /// </summary>
        Json = 9,
        /// <summary>
        /// Int 16
        /// </summary>
        Short = 10,
        /// <summary>
        /// Integer of 8 bit
        /// </summary>
        Byte = 11,
        /// <summary>
        /// Boolean
        /// </summary>
        Boolean = 12,
        /// <summary>
        /// Time
        /// </summary>
        Time = 13,
        /// <summary>
        /// Year (1 number)
        /// </summary>
        Year = 14,
        /// <summary>
        /// A 2-dimensional point
        /// </summary>
        Point = 15,
        /// <summary>
        /// A sequence of points
        /// </summary>
        Line = 16,
        /// <summary>
        /// A sequence of lines
        /// </summary>
        Polygon = 17,
        /// <summary>
        /// Geometry collection
        /// </summary>
        Geometry = 18,
        /// <summary>
        /// IEEE single precision
        /// </summary>
        Float = 19,
        /// <summary>
        /// A 24 bit integer
        /// </summary>
        Int24 = 20,
        /// <summary>
        /// when using the TIMESTAMP data type, the values are converted by the database to UTC (universal time zone) and are stored in that timezone. This means that when you fetch (SELECT) this data, a conversion will be done from UTC to the current time zone, and only then the data will be returned. This behavior doesn’t occur for DATETIME stored values
        /// </summary>
        TimeStamp = 21
    }
}