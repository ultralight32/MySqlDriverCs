using System;

namespace MySQLDriverCS.Interop
{
    public class MySqlField
    {
        public MySqlField(string name, enum_field_types type, long maxLength, uint length, MySqlFieldFlags flags, Type fieldType)
        {
            Name = name;
            Type = type;
            MaxLength = maxLength;
            Length = length;
            Flags = flags;
            FieldType = fieldType;
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
        public long MaxLength { get;  }
        /// <summary>
        /// Width of column (create length)
        /// </summary>
        public uint Length { get; }
        /// <summary>
        /// Flags
        /// </summary>
        public MySqlFieldFlags Flags { get; }

        public Type FieldType { get; }
    }
}