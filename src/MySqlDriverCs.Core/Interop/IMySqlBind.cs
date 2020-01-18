namespace MySQLDriverCS.Interop
{
#pragma warning disable 1591
    public interface IMySqlBind
    {
        uint BufferLength { get; set; }
        enum_field_types Type { get; set; }
        void GetBytes(byte[] buff, uint len);
        void InitForBind(IMySqlField fieldMetadata);
        object Value { get; set; }
        uint Length { get; set; }
        bool IsNull { get; set; }
        void Dispose();
    }
}