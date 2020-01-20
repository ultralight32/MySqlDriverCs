using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    internal class BindInput
    {
        public BindInput(enum_field_types bufferType, byte[] buffer, int? length, bool? isNull, bool is_unsigned)
        {
            BufferType = bufferType;
            this.Buffer = buffer;
            this.Length = length;
            IsNull = isNull;
            IsUnsigned = is_unsigned;
        }

        /// <summary>
        /// Sets buffer_size
        /// </summary>
        public enum_field_types BufferType { get; }
        /// <summary>
        /// Sets buffer size and buffer
        /// </summary>
        public byte[] Buffer { get; }
        /// <summary>
        /// sets length
        /// </summary>
        public int? Length { get; }
        /// <summary>
        /// sets is_null
        /// </summary>
        public bool? IsNull { get; }
        /// <summary>
        /// Is unsigned
        /// </summary>

        public bool IsUnsigned { get; }
    }
}