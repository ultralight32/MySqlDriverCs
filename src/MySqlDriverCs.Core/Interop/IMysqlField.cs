namespace MySQLDriverCS.Interop
{
    /// <summary>
    ///  Interface extended by different MySQL_FIELD versions.
    /// </summary>
    public interface IMySqlField
    {
        /// <summary>
        /// Name of column
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Type of field
        /// </summary>
        uint Type { get; set; }
        /// <summary>
        /// Max width for selected set
        /// </summary>
        long MaxLength { get; set; }
        /// <summary>
        /// Width of column (create length)
        /// </summary>
        uint Length { get; }
 
        MySqlFieldFlags Flags { get; }
    }
}