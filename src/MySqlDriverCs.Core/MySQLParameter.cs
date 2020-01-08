#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.

	Please, Read first MySQLDriverCS LICENSE

	MySQLParameter.cs
	This file is add to MySQLDriverCS.
	Copyright (c) 2003 William Reinoso and Omar del Valle Rodríguez.

	MySQLParameter is part MySQLDriverCS: you can redistribute it and/or modify
	it only part with all set of MySQLDriverCS and under the terms of MySQLDriverCS License.

	You can use this part freely but WITHOUT ANY WARRANTY; without even the implied warranty 
	of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public 
	License for more details.

	Thank to Manuel Lucas Viñas Livschitz by modify MySQLCommand to good result
	in MySQLParameter.
	
	Please, Send me email to notify any bug - omarvr72@hotmail.com
*/
#endregion
using System;
using System.Data;
using System.Collections;
using System.Data.Common;

namespace MySQLDriverCS
{
	/// <summary>
	/// Represents a parameter to a MySQLCommand. This class cannot be inherited.
	/// </summary>
	/// <example>Example code to use MySQLParameter and MySQLParameterCollection
	/// <code>
	/// using MySQLDriverCS;
	/// ...
	/// MySQLConnection DBConn;
	/// DBConn = new MySQLConnection( new MySQLConnectionString("myhost","mydatabase","mylogin","mypassword").AsString );
	/// DBConn.Open();
	/// ...
	/// // Query to execute.. 
	/// // Note @myvaluefield1 and @myvaluefield2 are name parameters in query
	/// string query = "update mytable set myfield1 = @myvaluefield1 where myfield2 = @myvaluefield2";
	///
	/// MySQLParameter par = new MySQLParameter("@myvaluefield1",System.Data.DbType.String, "This value is ignored");
	/// par.Value = "ju`a'n";
	/// 
	/// MySQLParameter par1 = new MySQLParameter("@myvaluefield2",System.Data.DbType.Int16, "This value is ignored");
	/// par1.Value = 1;
	/// 
	/// MySQLCommand command = new MySQLCommand(query, conn);
	///		
	/// command.CommandType = CommandType.Text;
	/// 
	/// //Use Parameter property to add parameters in ParameterCollection
	/// command.Parameters.Add(par);
	/// command.Parameters.Add(par1);
	/// 
	/// 
	/// command.ExecuteNonQuery();
	/// ...
	/// DBConn.Close();
	/// </code></example>
	public sealed class MySQLParameter : DbParameter, ICloneable
	{
		DbType m_dbType = DbType.Object;
		ParameterDirection m_direction = ParameterDirection.Input;
		bool m_fNullable = false;
		string m_sParamName;
		string m_sSourceColumn;
		DataRowVersion m_sourceVersion = DataRowVersion.Current;
		object m_value = null;
		byte m_precision;
		byte m_scale;
		int m_size;
		bool _sourceColumnNullMapping;

		/// <summary>
		/// Initializes a new instance of the MySQLParameter class.
		/// </summary>
		public MySQLParameter()
		{
		}

		private MySQLParameter(DbType dbType, ParameterDirection direction, bool fNullable, string sParamName, string sSourceColumn, DataRowVersion sourceVersion, object value, byte precision, byte scale, int size)
		{
			m_dbType = dbType;
			m_direction = direction;
			m_fNullable = fNullable;
			m_sParamName = sParamName;
			m_sSourceColumn = sSourceColumn;
			m_sourceVersion = sourceVersion;
			m_value = value;
			m_precision = precision;
			m_scale = scale;
			m_size = size;

		}
		/// <summary>
		/// Initializes a new instance of the MySQLParameter class with the parameter name and the data type.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map.</param>
		/// <param name="type">One of the DbType values.</param>
		public MySQLParameter(string parameterName, DbType type)
		{
			m_sParamName = parameterName;
			m_dbType = type;
		}

		/// <summary>
		/// Initializes a new instance of the MySQLParameter class with the parameter name and a value of the new MySQLParameter.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map.</param>
		/// <param name="value">An Object that is the value of the MySQLParameter.</param>
		/// <remarks>Setting the value also infers the type.</remarks>
		public MySQLParameter(string parameterName, object value)
		{
			m_sParamName = parameterName;
			this.Value = value;
		}

		/// <summary>
		/// Use MySQLParameter(string parameterName, DbType type)
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map.</param>
		/// <param name="dbType">One of the DbType values</param>
		/// <param name="sourceColumn">This value is ignored. Suported by future Operation</param>
		public MySQLParameter(string parameterName, DbType dbType, string sourceColumn)
		{
			m_sParamName = parameterName;
			m_dbType = dbType;
			m_sSourceColumn = sourceColumn;
		}

		/// <summary>
		/// Gets or sets the DbType of the parameter. 
		/// </summary>
		/// <value>One of the DbType values.</value>
		public override DbType DbType
		{
			get { return m_dbType; }
			set { m_dbType = value; }
		}

		/// <summary>
		/// Gets value ParameterDirection.Input 
		/// </summary>
		/// <value>This value is ignored.</value>
		public override ParameterDirection Direction
		{
			get { return m_direction; }
			set { m_direction = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the parameter accepts null values.
		/// </summary>
		public override Boolean IsNullable
		{
			get { return m_fNullable; }
			set { m_fNullable = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override void ResetDbType()
		{ }

		/// <summary>
		/// Gets or sets the name of the MySQLParameter.
		/// </summary>
		/// <value>The name of the MySQLParameter.</value>
		public override String ParameterName
		{
			get { return m_sParamName; }
			set { m_sParamName = value; }
		}

		/// <summary>
		/// Oported by future operation.
		/// </summary>
		/// <remarks>This value is ignored by MySQLParameter</remarks>
		public override String SourceColumn
		{
			get { return m_sSourceColumn; }
			set { m_sSourceColumn = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override DataRowVersion SourceVersion
		{
			get { return m_sourceVersion; }
			set { m_sourceVersion = value; }
		}

		/// <summary>
		/// Gets or sets the value of the parameter.
		/// </summary>
		/// <value>An Object that is the value of the parameter. The default value is null.</value>
		public override object Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
				if (!(value is Array))
				{
					m_dbType = _inferType(value);
				}
			}
		}

		private DbType _inferType(Object value)
		{
			TypeCode code;

			if (value == DBNull.Value || value == null)
				code = TypeCode.DBNull;
			else
				code = Type.GetTypeCode(value.GetType());
			switch (code)
			{
				case TypeCode.Empty:
					throw new SystemException("Invalid data type");

				case TypeCode.Object:
					return DbType.Object;

				case TypeCode.DBNull:
					return DbType.Object;

				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					// Throw a SystemException for unsupported data types.
					throw new SystemException("Invalid data type");

				case TypeCode.Boolean:
					return DbType.Byte;

				case TypeCode.Byte:
					return DbType.Byte;

				case TypeCode.Int16:
					return DbType.Int16;

				case TypeCode.Int32:
					return DbType.Int32;

				case TypeCode.Int64:
					return DbType.Int64;

				case TypeCode.Single:
					return DbType.Single;

				case TypeCode.Double:
					return DbType.Double;

				case TypeCode.Decimal:
					return DbType.Decimal;

				case TypeCode.DateTime:
                    if (m_dbType == DbType.Date)
                        return DbType.Date;
                    else if (m_dbType == DbType.DateTime2)
                        return DbType.DateTime2;
                    else
                        return DbType.DateTime;

				case TypeCode.String:
					return DbType.String;

				default:
					throw new SystemException("Value is of unknown data type");
			}
		}
		#region IDbDataParameter Members
		/// <summary>
		/// Gets or sets the maximum number of digits used to represent the Value property.
		/// </summary>

		public override byte Precision
		{
			get { return m_precision; }
			set { m_precision = value; }
		}

		/// <summary>
		/// Gets or sets the number of decimal places to which Value is resolved.
		/// </summary>
		public override byte Scale
		{
			get { return m_scale; }
			set { m_scale = value; }
		}
		/// <summary>
		/// Gets or sets the maximum size, in bytes, of the data within the column.
		/// </summary>
		public override int Size
		{
			get { return m_size; }
			set { m_size = value; }
		}

		#endregion

		#region ICloneable Members

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new MySQLParameter(m_dbType, m_direction, m_fNullable, m_sParamName,
				m_sSourceColumn, m_sourceVersion, m_value, m_precision, m_scale, m_size);
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		public override bool SourceColumnNullMapping
		{
			get { return this._sourceColumnNullMapping; }
			set { this._sourceColumnNullMapping = value; }
		}
	}
	/// <summary>
	/// Provides a mechanism to customize the sort ordering of a MySqlParameter's
	/// collection
	/// </summary>
	public class MySQLParameterComparer : IComparer
	{
		#region IComparer Members

		/// <summary>
		/// Returns the sort order of two MySqlParameter instances.
		/// </summary>
		/// <param name="x">The first MySqlParameter to compare.</param>
		/// <param name="y">The second MySqlParameter to compare. </param>
		/// <returns>A Int32 containing a value that reflects the sort order 
		/// of a as compared to b. The following lines defines the 
		/// conditions under which the returned value is a negative number, 
		/// zero, or a positive number.
		/// Any negative number if a.ParameterName.Length &lt; b.ParameterName.Length. 
		/// Zero if a.ParameterName.Length == b.ParameterName.Length. 
		/// Any positive number a.ParameterName.Length &gt; b.ParameterName.Length.
		/// </returns>
		public int Compare(object x, object y)
		{
			MySQLParameter p1 = (MySQLParameter)x;
			MySQLParameter p2 = (MySQLParameter)y;
			if (p1.ParameterName.Length < p2.ParameterName.Length)
				return 1;
			else if (p1.ParameterName.Length == p2.ParameterName.Length)
				return 0;
			else
				return -1;
		}

		#endregion
	}
}
