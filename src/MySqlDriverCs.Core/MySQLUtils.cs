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

using System.Text;

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
    }
}
