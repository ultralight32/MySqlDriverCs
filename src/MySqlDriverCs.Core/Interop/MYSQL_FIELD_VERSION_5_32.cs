using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
    /// <inheritdoc />
    /// <summary>
    /// Field descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    public class MYSQL_FIELD_VERSION_5_32 : IMySqlField
    {
        /// <summary>
        /// The name of the field. If the field was given an alias with an AS  clause, the value of name is the alias.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;
        /// <summary>
        /// The name of the field. Aliases are ignored.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string org_name;
        /// <summary>
        /// The name of the table containing this field, if it isn't a calculated field. 
        /// For calculated fields, the table value is an empty string. 
        /// If the table was given an alias with an AS  clause, the value of table is the alias.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string table;
        /// <summary>
        /// The name of the table. Aliases are ignored.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string org_table;
        /// <summary>
        /// The name of the database that the field comes from. 
        /// If the field is a calculated field, db is an empty string.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string db;
        /// <summary>
        /// The catalog name. This value is always "def".
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string catalog;
        /// <summary>
        /// The default value of this field. This is set only if you use mysql_list_fields().
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string def;
        /// <summary>
        /// The width of the field, as specified in the table definition.
        /// </summary>
        public uint length;
        /// <summary>
        /// The maximum width of the field for the result set (the length of the longest field value for the rows actually in the result set). 
        /// If you use mysql_store_result() or mysql_list_fields(), this contains the maximum length for the field. 
        /// If you use mysql_use_result(), the value of this variable is zero.
        /// </summary>
        public uint max_length;
        /// <summary>
        /// The length of name.
        /// </summary>
        public uint name_length;
        /// <summary>
        /// The length of org_name.
        /// </summary>
        public uint org_name_length;
        /// <summary>
        /// The length of table.
        /// </summary>
        public uint table_length;
        /// <summary>
        /// The length of org_table.
        /// </summary>
        public uint org_table_length;
        /// <summary>
        /// The length of db.
        /// </summary>
        public uint db_length;
        /// <summary>
        /// The length of catalog.
        /// </summary>
        public uint catalog_length;
        /// <summary>
        /// The length of def.
        /// </summary>
        public uint def_length;
        /// <summary>
        /// Div flags
        /// </summary>
        public uint flags;
        /// <summary>
        /// Number of decimals in field
        /// </summary>
        public uint decimals;
        /// <summary>
        /// The character set number for the field.
        /// </summary>
        public uint charsetnr;
        /// <summary>
        /// The type of the field. 
        /// </summary>
        public /*FieldTypes*/ uint type;
        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get { return name; }
        }
        /// <summary>
        /// Type
        /// </summary>
        public uint Type
        {
            get { return (uint)type; }
            set { type = value; }
        }
        /// <summary>
        /// Max_Length
        /// </summary>
        public long MaxLength
        {
            get { return max_length; }
            set { max_length = (uint)value; }

        }
        /// <summary>
        /// Length
        /// </summary>
        public uint Length
        {
            get { return length; }
        }

     
        public MySqlFieldFlags Flags => (MySqlFieldFlags) flags;
    }
}