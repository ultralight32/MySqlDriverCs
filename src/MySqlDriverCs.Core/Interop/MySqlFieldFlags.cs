#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.

	This file is part of MySQLDriverCS.

    MySQLDriverCS is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    MySQLDriverCS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MySQLDriverCS; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
    [Flags]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum MySqlFieldFlags : uint
    {
        /// <summary> Field can't be NULL </summary>
        NOT_NULL_FLAG = 1,
        /// <summary> Field is part of a primary key </summary>
        PRI_KEY_FLAG = 2,
        /// <summary> Field is part of a unique key </summary>
        UNIQUE_KEY_FLAG = 4,
        /// <summary> Field is part of a key </summary>
        MULTIPLE_KEY_FLAG = 8,
        /// <summary> Field is a blob </summary>
        BLOB_FLAG = 16,
        /// <summary> Field is unsigned </summary>
        UNSIGNED_FLAG = 32,
        /// <summary> Field is zerofill </summary>
        ZEROFILL_FLAG = 64,
        /// <summary> Field is binary </summary>
        BINARY_FLAG = 128,
        /// <summary> field is an enum </summary>
        ENUM_FLAG = 256,
        /// <summary> field is a autoincrement field </summary>
        AUTO_INCREMENT_FLAG = 512,
        /// <summary> Field is a timestamp </summary>
        TIMESTAMP_FLAG = 1024,
        /// <summary> field is a set </summary>
        SET_FLAG = 2048,
        /// <summary> Field doesn't have default value </summary>
        NO_DEFAULT_VALUE_FLAG = 4096,
        // <summary> Field is set to NOW on UPDATE </summary>
        ON_UPDATE_NOW_FLAG = 8192,
        /// <summary> Field is num (for clients) </summary>
        NUM_FLAG = 32768,
        /// <summary> Intern; Part of some key </summary>
        PART_KEY_FLAG = 16384,
        /// <summary> Intern: Group field </summary>
        GROUP_FLAG = 32768,
        /// <summary> Intern: Used by sql_yacc </summary>
        UNIQUE_FLAG = 65536,
        /// <summary> Intern: Used by sql_yacc </summary>
        BINCMP_FLAG = 131072,
        /// <summary> Used to get fields in item tree </summary>
        GET_FIXED_FIELDS_FLAG = (1 << 18),
        /// <summary> Field part of partition func </summary>
        FIELD_IN_PART_FUNC_FLAG = (1 << 19)
    }

}
