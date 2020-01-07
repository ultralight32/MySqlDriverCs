#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Vi�as Livschitz.

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
		/// 
		/// </summary>
		public static bool RunningOn64x = (IntPtr.Size == 8);
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
		/// <summary>
		/// Escapes characters in html way but without altering text that may be in tags, that is less-than, more-than, ampersand, space, doublequote, filter simbols.
		/// </summary>
		/// <param name="strIn">The string to translate</param>
		/// <returns>The translated string</returns>
		public static string HTMLEscapeSpecialCharacters(string strIn)
		{
			string retval = "";
			foreach (char c in strIn)
			{
				if (false) { }
				//				else if(c=='&') retval+="&amp;";
				//				else if(c=='\"') retval+="&quot;";
				//				else if(c=='<') retval+="&lt;";
				//				else if(c=='>') retval+="&gt;";
				//				else if(c==' ') retval+="&nbsp;"; 
				else if (c == '�') retval += "&iexcl;";
				else if (c == '�') retval += "&cent; ";
				else if (c == '�') retval += "&pound;";
				else if (c == '�') retval += "&curren;";
				else if (c == '�') retval += "&yen;";
				//				else if(c=='�') retval+="&brvbar;";
				else if (c == '�') retval += "&sect;"; // Section sign  
				else if (c == '�') retval += "&uml;"; // or &die;"; // Di�resis / Umlaut  
				else if (c == '�') retval += "&copy;"; // Copyright  
				else if (c == '�') retval += "&ordf;"; // Feminine ordinal  
				else if (c == '�') retval += "&laquo;"; // Left angle quote, guillemet left  
				else if (c == '�') retval += "&not;"; // Not sign  
				else if (c == '�') retval += "&shy;"; // Soft hyphen  
				else if (c == '�') retval += "&reg;"; // Registered trademark  
				else if (c == '�') retval += "&macr;"; // or &hibar;"; // Macron accent  
				else if (c == '�') retval += "&deg;"; // Degree sign  
				else if (c == '�') retval += "&plusmn;"; // Plus or minus  
				else if (c == '�') retval += "&sup2;"; // Superscript two  
				else if (c == '�') retval += "&sup3;"; // Superscript three  
				else if (c == '�') retval += "&acute;"; // Acute accent  
				else if (c == '�') retval += "&micro;"; // Micro sign  
				else if (c == '�') retval += "&para;"; // Paragraph sign  
				else if (c == '�') retval += "&middot;"; // Middle dot  
				else if (c == '�') retval += "&cedil;"; // Cedilla  
				else if (c == '�') retval += "&sup1;"; // Superscript one  
				else if (c == '�') retval += "&ordm;"; // Masculine ordinal  
				else if (c == '�') retval += "&raquo;"; // Right angle quote, guillemet right  
				else if (c == '�') retval += "&frac14;"; // Fraction one-fourth  
				else if (c == '�') retval += "&frac12;"; // Fraction one-half  
				else if (c == '�') retval += "&frac34;"; // Fraction three-fourths  
				else if (c == '�') retval += "&iquest;"; // Inverted question mark  
				else if (c == '�') retval += "&Agrave;"; // Capital A, grave accent  
				else if (c == '�') retval += "&Aacute;"; // Capital A, acute accent  
				else if (c == '�') retval += "&Acirc;"; // Capital A, circumflex  
				else if (c == '�') retval += "&Atilde;"; // Capital A, tilde  
				else if (c == '�') retval += "&Auml;"; // Capital A, di�resis / umlaut  
				else if (c == '�') retval += "&Aring;"; // Capital A, ring  
				else if (c == '�') retval += "&AElig;"; // Capital AE ligature  
				else if (c == '�') retval += "&Ccedil;"; // Capital C, cedilla  
				else if (c == '�') retval += "&Egrave;"; // Capital E, grave accent  
				else if (c == '�') retval += "&Eacute;"; // Capital E, acute accent  
				else if (c == '�') retval += "&Ecirc;"; // Capital E, circumflex  
				else if (c == '�') retval += "&Euml;"; // Capital E, di�resis / umlaut  
				else if (c == '�') retval += "&Igrave;"; // Capital I, grave accent  
				else if (c == '�') retval += "&Iacute;"; // Capital I, acute accent  
				else if (c == '�') retval += "&Icirc;"; // Capital I, circumflex  
				else if (c == '�') retval += "&Iuml;"; // Capital I, di�resis / umlaut  
				else if (c == '�') retval += "&ETH;"; // Capital Eth, Icelandic  
				else if (c == '�') retval += "&Ntilde;"; // Capital N, tilde  
				else if (c == '�') retval += "&Ograve;"; // Capital O, grave accent  
				else if (c == '�') retval += "&Oacute;"; // Capital O, acute accent  
				else if (c == '�') retval += "&Ocirc;"; // Capital O, circumflex  
				else if (c == '�') retval += "&Otilde;"; // Capital O, tilde  
				else if (c == '�') retval += "&Ouml;"; // Capital O, di�resis / umlaut  
				else if (c == '�') retval += "&times;"; // Multiply sign  
				else if (c == '�') retval += "&Oslash;"; // Capital O, slash  
				else if (c == '�') retval += "&Ugrave;"; // Capital U, grave accent  
				else if (c == '�') retval += "&Uacute;"; // Capital U, acute accent  
				else if (c == '�') retval += "&Ucirc;"; // Capital U, circumflex  
				else if (c == '�') retval += "&Uuml;"; // Capital U, di�resis / umlaut  
				else if (c == '�') retval += "&Yacute;"; // Capital Y, acute accent  
				else if (c == '�') retval += "&THORN;"; // Capital Thorn, Icelandic  
				else if (c == '�') retval += "&szlig;"; // Small sharp s, German sz  
				else if (c == '�') retval += "&agrave;"; // Small a, grave accent  
				else if (c == '�') retval += "&aacute;"; // Small a, acute accent  
				else if (c == '�') retval += "&acirc;"; // Small a, circumflex  
				else if (c == '�') retval += "&atilde;"; // Small a, tilde  
				else if (c == '�') retval += "&auml;"; // Small a, di�resis / umlaut  
				else if (c == '�') retval += "&aring;"; // Small a, ring  
				else if (c == '�') retval += "&aelig;"; // Small ae ligature  
				else if (c == '�') retval += "&ccedil;"; // Small c, cedilla  
				else if (c == '�') retval += "&egrave;"; // Small e, grave accent  
				else if (c == '�') retval += "&eacute;"; // Small e, acute accent  
				else if (c == '�') retval += "&ecirc;"; // Small e, circumflex  
				else if (c == '�') retval += "&euml;"; // Small e, di�resis / umlaut  
				else if (c == '�') retval += "&igrave;"; // Small i, grave accent  
				else if (c == '�') retval += "&iacute;"; // Small i, acute accent  
				else if (c == '�') retval += "&icirc;"; // Small i, circumflex  
				else if (c == '�') retval += "&iuml;"; // Small i, di�resis / umlaut  
				else if (c == '�') retval += "&eth;"; // Small eth, Icelandic  
				else if (c == '�') retval += "&ntilde;"; // Small n, tilde  
				else if (c == '�') retval += "&ograve;"; // Small o, grave accent  
				else if (c == '�') retval += "&oacute;"; // Small o, acute accent  
				else if (c == '�') retval += "&ocirc;"; // Small o, circumflex  
				else if (c == '�') retval += "&otilde;"; // Small o, tilde  
				else if (c == '�') retval += "&ouml;"; // Small o, di�resis / umlaut  
				else if (c == '�') retval += "&divide;"; // Division sign  
				else if (c == '�') retval += "&oslash;"; // Small o, slash  
				else if (c == '�') retval += "&ugrave;"; // Small u, grave accent  
				else if (c == '�') retval += "&uacute;"; // Small u, acute accent  
				else if (c == '�') retval += "&ucirc;"; // Small u, circumflex  
				else if (c == '�') retval += "&uuml;"; // Small u, di�resis / umlaut  
				else if (c == '�') retval += "&yacute;"; // Small y, acute accent  
				else if (c == '�') retval += "&thorn;"; // Small thorn, Icelandic  
				else if (c == '�') retval += "&yuml;"; // Small y, di�resis / umlaut  
				else retval += c.ToString();
			}
			return retval;
		}
		internal static DateTime MYSQL_NULL_DATE = new DateTime(1000, 1, 1);

		/// <summary>
		/// Converts a MySQL Type to a System.Type. See http://dev.mysql.com/doc/refman/5.0/en/c-api-prepared-statement-datatypes.html
		/// See also http://msdn.microsoft.com/msdnmag/issues/03/07/NET/default.aspx?print=true
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type MySQLToNetType(uint type)
		{
			FieldTypes5 mysql_type = (FieldTypes5)type;
			switch (mysql_type)
			{
				case FieldTypes5.FIELD_TYPE_BIT: return typeof(ulong);
				case FieldTypes5.FIELD_TYPE_BLOB: return typeof(sbyte[]);
				case FieldTypes5.FIELD_TYPE_DATE: return typeof(MYSQL_TIME);
				case FieldTypes5.FIELD_TYPE_DATETIME: return typeof(MYSQL_TIME);
				//case FieldTypes5.FIELD_TYPE_DECIMAL: return typeof(sbyte[]); 
				case FieldTypes5.FIELD_TYPE_DOUBLE: return typeof(Double);
				//case FieldTypes5.FIELD_TYPE_ENUM: return typeof(uint);
				case FieldTypes5.FIELD_TYPE_FLOAT: return typeof(Single);
				//case FieldTypes5.FIELD_TYPE_GEOMETRY: return typeof(long);
				case FieldTypes5.FIELD_TYPE_INT24: return typeof(int);
				case FieldTypes5.FIELD_TYPE_LONG: return typeof(int);
				case FieldTypes5.FIELD_TYPE_LONG_BLOB: return typeof(sbyte[]);
				case FieldTypes5.FIELD_TYPE_LONGLONG: return typeof(long);
				case FieldTypes5.FIELD_TYPE_MEDIUM_BLOB: return typeof(sbyte[]);
				//case FieldTypes5.FIELD_TYPE_NEWDATE: return typeof(long);
				case FieldTypes5.FIELD_TYPE_NEWDECIMAL: return typeof(string);//sbyte[]);
				case FieldTypes5.FIELD_TYPE_DECIMAL: return typeof(string);
				//case FieldTypes5.FIELD_TYPE_NULL: return typeof(long);
				//case FieldTypes5.FIELD_TYPE_SET: return typeof(long);
				case FieldTypes5.FIELD_TYPE_SHORT: return typeof(short);
				case FieldTypes5.FIELD_TYPE_STRING: return typeof(string);//sbyte[]);
				case FieldTypes5.FIELD_TYPE_TIME: return typeof(MYSQL_TIME);
				case FieldTypes5.FIELD_TYPE_TIMESTAMP: return typeof(MYSQL_TIME);
				case FieldTypes5.FIELD_TYPE_TINY: return typeof(byte);
				case FieldTypes5.FIELD_TYPE_TINY_BLOB: return typeof(sbyte[]);
				case FieldTypes5.FIELD_TYPE_VAR_STRING: return typeof(string);//sbyte[]);
				case FieldTypes5.FIELD_TYPE_VARCHAR: return typeof(sbyte[]);
				default:
					Console.WriteLine("Warning MySQLToNetType could not map type: " + type);
					return typeof(string);
					//case FieldTypes5.FIELD_TYPE_YEAR: return typeof(long);
			}
		}
	}
}
