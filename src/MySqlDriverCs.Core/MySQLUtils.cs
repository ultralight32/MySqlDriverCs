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
using System.Text;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
	/// <summary>
	/// Various static functions to help MySQLDriverCS engime.
	/// </summary>
	public class MySQLUtils
	{
        /// <summary>
		/// Escapes characters to make a MySQL readable query.
		/// </summary>
		/// <param name="str">The string to translate</param>
		/// <param name="conn">A valid, open connection</param>
		/// <returns>The quoted escaped string</returns>
		/// Modified by Chris Palowitch (chrispalo@bellsouth.net)
		/// utilizing StringBuilder for acceptable performance with large data
		internal static string Escape(string str, MySQLConnection conn)
		{
			byte[] bytes = conn.CharacterEncoding.GetBytes(str);
			StringBuilder to = new StringBuilder(bytes.Length * 2 + 1);
			StringBuilder result = new StringBuilder(bytes.Length * 2 + 1);
			result.Append('\'');
			conn.NativeConnection.mysql_real_escape_string(to, str, (uint)bytes.Length);
			result.Append(to);
			result.Append('\'');
			return result.ToString();
		}

        internal static DateTime MYSQL_NULL_DATE = new DateTime(1000, 1, 1);

		/// <summary>
		/// Converts a MySQL Type to a System.Type. See http://dev.mysql.com/doc/refman/5.0/en/c-api-prepared-statement-datatypes.html
		/// See also http://msdn.microsoft.com/msdnmag/issues/03/07/NET/default.aspx?print=true
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type MySQLToNetType(enum_field_types type)
		{
			enum_field_types mysql_type = (enum_field_types)type;
			switch (mysql_type)
			{
				case enum_field_types.MYSQL_TYPE_BIT: return typeof(ulong);
				case enum_field_types.MYSQL_TYPE_BLOB: return typeof(sbyte[]);
				case enum_field_types.MYSQL_TYPE_DATE: return typeof(MYSQL_TIME);
				case enum_field_types.MYSQL_TYPE_DATETIME: return typeof(MYSQL_TIME);
				//case FieldTypes5.MYSQL_TYPE_DECIMAL: return typeof(sbyte[]); 
				case enum_field_types.MYSQL_TYPE_DOUBLE: return typeof(Double);
				//case FieldTypes5.MYSQL_TYPE_ENUM: return typeof(uint);
				case enum_field_types.MYSQL_TYPE_FLOAT: return typeof(Single);
				//case FieldTypes5.MYSQL_TYPE_GEOMETRY: return typeof(long);
				case enum_field_types.MYSQL_TYPE_INT24: return typeof(int);
				case enum_field_types.MYSQL_TYPE_LONG: return typeof(int);
				case enum_field_types.MYSQL_TYPE_LONG_BLOB: return typeof(sbyte[]);
				case enum_field_types.MYSQL_TYPE_LONGLONG: return typeof(long);
				case enum_field_types.MYSQL_TYPE_MEDIUM_BLOB: return typeof(sbyte[]);
				//case FieldTypes5.MYSQL_TYPE_NEWDATE: return typeof(long);
				case enum_field_types.MYSQL_TYPE_NEWDECIMAL: return typeof(string);//sbyte[]);
				case enum_field_types.MYSQL_TYPE_DECIMAL: return typeof(string);
				//case FieldTypes5.MYSQL_TYPE_NULL: return typeof(long);
				//case FieldTypes5.MYSQL_TYPE_SET: return typeof(long);
				case enum_field_types.MYSQL_TYPE_SHORT: return typeof(short);
				case enum_field_types.MYSQL_TYPE_STRING: return typeof(string);//sbyte[]);
				case enum_field_types.MYSQL_TYPE_TIME: return typeof(MYSQL_TIME);
				case enum_field_types.MYSQL_TYPE_TIMESTAMP: return typeof(MYSQL_TIME);
				case enum_field_types.MYSQL_TYPE_TINY: return typeof(byte);
				case enum_field_types.MYSQL_TYPE_TINY_BLOB: return typeof(sbyte[]);
				case enum_field_types.MYSQL_TYPE_VAR_STRING: return typeof(string);//sbyte[]);
				case enum_field_types.MYSQL_TYPE_VARCHAR: return typeof(sbyte[]);
				default:
					Console.WriteLine("Warning MySQLToNetType could not map type: " + type);
					return typeof(string);
					//case FieldTypes5.MYSQL_TYPE_YEAR: return typeof(long);
			}
		}
	}
}
