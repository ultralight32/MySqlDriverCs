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
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
    /// <inheritdoc cref="IMySqlField" />
    /// <summary>
    /// Field descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]//"3.23.32", 4.0.1-alpha
    // ReSharper disable once InconsistentNaming
    internal struct MYSQL_FIELD_VERSION_3 : IMySqlField
	{
		/// <summary>
		/// Name of column
		/// </summary>
		public string name;
		/// <summary>
		/// Table of column if column was a field
		/// </summary>
		public string table;
		//public string org_table;			/* Org table name if table was an alias */
		//public string db;			/* Database for table */
		/// <summary>
		/// def
		/// </summary>
		public string def;
		/// <summary>
		/// length
		/// </summary>
		public long length;
		/// <summary>
		/// max_length
		/// </summary>
		public long max_length;
		/// <summary>
		/// Div flags
		/// </summary>
		public uint flags;
		/// <summary>
		/// Number of decimals in field 
		/// </summary>
		public uint decimals;
		/// <summary>
		/// Type of field. Se mysql_com.h for types 
		/// </summary>
		public uint type;

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
			get { return type; }
			set { type = value; }
		}
		/// <summary>
		/// Max_Length
		/// </summary>
		public long Max_Length
		{
			get { return max_length; }
			set { max_length = value; }
		}
		public uint Length
		{
			get { return (uint)length; }
		}
	}
}
