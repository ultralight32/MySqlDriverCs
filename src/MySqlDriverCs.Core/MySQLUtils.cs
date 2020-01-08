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
				else if (c == '¡') retval += "&iexcl;";
				else if (c == '¢') retval += "&cent; ";
				else if (c == '£') retval += "&pound;";
				else if (c == '¤') retval += "&curren;";
				else if (c == '¥') retval += "&yen;";
				//				else if(c=='¦') retval+="&brvbar;";
				else if (c == '§') retval += "&sect;"; // Section sign  
				else if (c == '¨') retval += "&uml;"; // or &die;"; // Diæresis / Umlaut  
				else if (c == '©') retval += "&copy;"; // Copyright  
				else if (c == 'ª') retval += "&ordf;"; // Feminine ordinal  
				else if (c == '«') retval += "&laquo;"; // Left angle quote, guillemet left  
				else if (c == '¬') retval += "&not;"; // Not sign  
				else if (c == '­') retval += "&shy;"; // Soft hyphen  
				else if (c == '®') retval += "&reg;"; // Registered trademark  
				else if (c == '¯') retval += "&macr;"; // or &hibar;"; // Macron accent  
				else if (c == '°') retval += "&deg;"; // Degree sign  
				else if (c == '±') retval += "&plusmn;"; // Plus or minus  
				else if (c == '²') retval += "&sup2;"; // Superscript two  
				else if (c == '³') retval += "&sup3;"; // Superscript three  
				else if (c == '´') retval += "&acute;"; // Acute accent  
				else if (c == 'µ') retval += "&micro;"; // Micro sign  
				else if (c == '¶') retval += "&para;"; // Paragraph sign  
				else if (c == '·') retval += "&middot;"; // Middle dot  
				else if (c == '¸') retval += "&cedil;"; // Cedilla  
				else if (c == '¹') retval += "&sup1;"; // Superscript one  
				else if (c == 'º') retval += "&ordm;"; // Masculine ordinal  
				else if (c == '»') retval += "&raquo;"; // Right angle quote, guillemet right  
				else if (c == '¼') retval += "&frac14;"; // Fraction one-fourth  
				else if (c == '½') retval += "&frac12;"; // Fraction one-half  
				else if (c == '¾') retval += "&frac34;"; // Fraction three-fourths  
				else if (c == '¿') retval += "&iquest;"; // Inverted question mark  
				else if (c == 'À') retval += "&Agrave;"; // Capital A, grave accent  
				else if (c == 'Á') retval += "&Aacute;"; // Capital A, acute accent  
				else if (c == 'Â') retval += "&Acirc;"; // Capital A, circumflex  
				else if (c == 'Ã') retval += "&Atilde;"; // Capital A, tilde  
				else if (c == 'Ä') retval += "&Auml;"; // Capital A, diæresis / umlaut  
				else if (c == 'Å') retval += "&Aring;"; // Capital A, ring  
				else if (c == 'Æ') retval += "&AElig;"; // Capital AE ligature  
				else if (c == 'Ç') retval += "&Ccedil;"; // Capital C, cedilla  
				else if (c == 'È') retval += "&Egrave;"; // Capital E, grave accent  
				else if (c == 'É') retval += "&Eacute;"; // Capital E, acute accent  
				else if (c == 'Ê') retval += "&Ecirc;"; // Capital E, circumflex  
				else if (c == 'Ë') retval += "&Euml;"; // Capital E, diæresis / umlaut  
				else if (c == 'Ì') retval += "&Igrave;"; // Capital I, grave accent  
				else if (c == 'Í') retval += "&Iacute;"; // Capital I, acute accent  
				else if (c == 'Î') retval += "&Icirc;"; // Capital I, circumflex  
				else if (c == 'Ï') retval += "&Iuml;"; // Capital I, diæresis / umlaut  
				else if (c == 'Ð') retval += "&ETH;"; // Capital Eth, Icelandic  
				else if (c == 'Ñ') retval += "&Ntilde;"; // Capital N, tilde  
				else if (c == 'Ò') retval += "&Ograve;"; // Capital O, grave accent  
				else if (c == 'Ó') retval += "&Oacute;"; // Capital O, acute accent  
				else if (c == 'Ô') retval += "&Ocirc;"; // Capital O, circumflex  
				else if (c == 'Õ') retval += "&Otilde;"; // Capital O, tilde  
				else if (c == 'Ö') retval += "&Ouml;"; // Capital O, diæresis / umlaut  
				else if (c == '×') retval += "&times;"; // Multiply sign  
				else if (c == 'Ø') retval += "&Oslash;"; // Capital O, slash  
				else if (c == 'Ù') retval += "&Ugrave;"; // Capital U, grave accent  
				else if (c == 'Ú') retval += "&Uacute;"; // Capital U, acute accent  
				else if (c == 'Û') retval += "&Ucirc;"; // Capital U, circumflex  
				else if (c == 'Ü') retval += "&Uuml;"; // Capital U, diæresis / umlaut  
				else if (c == 'Ý') retval += "&Yacute;"; // Capital Y, acute accent  
				else if (c == 'Þ') retval += "&THORN;"; // Capital Thorn, Icelandic  
				else if (c == 'ß') retval += "&szlig;"; // Small sharp s, German sz  
				else if (c == 'à') retval += "&agrave;"; // Small a, grave accent  
				else if (c == 'á') retval += "&aacute;"; // Small a, acute accent  
				else if (c == 'â') retval += "&acirc;"; // Small a, circumflex  
				else if (c == 'ã') retval += "&atilde;"; // Small a, tilde  
				else if (c == 'ä') retval += "&auml;"; // Small a, diæresis / umlaut  
				else if (c == 'å') retval += "&aring;"; // Small a, ring  
				else if (c == 'æ') retval += "&aelig;"; // Small ae ligature  
				else if (c == 'ç') retval += "&ccedil;"; // Small c, cedilla  
				else if (c == 'è') retval += "&egrave;"; // Small e, grave accent  
				else if (c == 'é') retval += "&eacute;"; // Small e, acute accent  
				else if (c == 'ê') retval += "&ecirc;"; // Small e, circumflex  
				else if (c == 'ë') retval += "&euml;"; // Small e, diæresis / umlaut  
				else if (c == 'ì') retval += "&igrave;"; // Small i, grave accent  
				else if (c == 'í') retval += "&iacute;"; // Small i, acute accent  
				else if (c == 'î') retval += "&icirc;"; // Small i, circumflex  
				else if (c == 'ï') retval += "&iuml;"; // Small i, diæresis / umlaut  
				else if (c == 'ð') retval += "&eth;"; // Small eth, Icelandic  
				else if (c == 'ñ') retval += "&ntilde;"; // Small n, tilde  
				else if (c == 'ò') retval += "&ograve;"; // Small o, grave accent  
				else if (c == 'ó') retval += "&oacute;"; // Small o, acute accent  
				else if (c == 'ô') retval += "&ocirc;"; // Small o, circumflex  
				else if (c == 'õ') retval += "&otilde;"; // Small o, tilde  
				else if (c == 'ö') retval += "&ouml;"; // Small o, diæresis / umlaut  
				else if (c == '÷') retval += "&divide;"; // Division sign  
				else if (c == 'ø') retval += "&oslash;"; // Small o, slash  
				else if (c == 'ù') retval += "&ugrave;"; // Small u, grave accent  
				else if (c == 'ú') retval += "&uacute;"; // Small u, acute accent  
				else if (c == 'û') retval += "&ucirc;"; // Small u, circumflex  
				else if (c == 'ü') retval += "&uuml;"; // Small u, diæresis / umlaut  
				else if (c == 'ý') retval += "&yacute;"; // Small y, acute accent  
				else if (c == 'þ') retval += "&thorn;"; // Small thorn, Icelandic  
				else if (c == 'ÿ') retval += "&yuml;"; // Small y, diæresis / umlaut  
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
