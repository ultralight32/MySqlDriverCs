using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
#pragma warning disable 1591
    /// <summary>
    /// Structure which is used to represent datetime values inside MySQL.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MYSQL_TIME
    {
        /// <summary>
        ///   We assume that values in this structure are normalized
        /// </summary>
        public uint year, month, day, hour, minute, second;
        /// <summary>
        /// Second Part
        /// </summary>
        public uint second_part;
        /// <summary>
        /// neg
        /// </summary>
        public sbyte neg;
        /// <summary>
        /// Time type
        /// </summary>
        public enum_mysql_timestamp_type time_type;
    }

    public enum enum_mysql_timestamp_type: int
    {
        MYSQL_TIMESTAMP_NONE = -2, MYSQL_TIMESTAMP_ERROR = -1,
        MYSQL_TIMESTAMP_DATE = 0, MYSQL_TIMESTAMP_DATETIME = 1, MYSQL_TIMESTAMP_TIME = 2
    };
}