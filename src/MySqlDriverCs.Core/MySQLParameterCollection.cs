#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.

	Please, Read first MySQLDriverCS LICENSE

	MySQLParameterCollection.cs
	This file is add to MySQLDriverCS.
	Copyright (c) 2003 William Reinoso and Omar del Valle Rodríguez.

	MySQLParameterCollection is part MySQLDriverCS: you can redistribute it and/or modify
	it only part with all set of MySQLDriverCS and under the terms of MySQLDriverCS License.

	You can use this part freely but WITHOUT ANY WARRANTY; without even the implied warranty 
	of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public 
	License for more details.

	Thank to Manuel Lucas Viñas Livschitz by modify MySQLCommand to good result
	in MySQLParameterCollection.
	
	Please, Send me email to notify any bug - omarvr72@hotmail.com
*/
#endregion

using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Data.Common;
using System.Collections.Generic;

namespace MySQLDriverCS
{
	/// <summary>
	/// Collects all parameters relevant to a MySQLCommand. This class cannot be inherited
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
	/// 

	public sealed class MySQLParameterCollection : DbParameterCollection
	{
		private bool m_sorted;
		private ArrayList m_sortedList;

		private List<MySQLParameter> m_List;

		private List<MySQLParameter> InnerList
		{
			get
			{

				if (m_List == null)
				{
					m_List = new List<MySQLParameter>();
				}
				return m_List;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		public override void AddRange(Array values)
		{
			foreach (MySQLParameter parameter in values)
			{
				Add(parameter);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Clear()
		{
			//this.OnChange();

			foreach (MySQLParameter parameter in this.InnerList)
			{
				//                parameter.ResetParent();
			}

			this.InnerList.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool Contains(object value)
		{
			return (-1 != this.IndexOf(value));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool Contains(string value)
		{
			return (-1 != this.IndexOf(value));
		}

		private static int IndexOf(IEnumerable items, string parameterName)
		{
			if (items != null)
			{
				int num1 = 0;
				foreach (MySQLParameter parameter2 in items)
				{
					if (SrcCompare(parameterName, parameter2.ParameterName) == 0)
					{
						return num1;
					}
					num1++;
				}
				num1 = 0;
				foreach (MySQLParameter parameter1 in items)
				{
					if (DstCompare(parameterName, parameter1.ParameterName) == 0)
					{
						return num1;
					}
					num1++;
				}
			}
			return -1;
		}

		internal static int SrcCompare(string strA, string strB)
		{
			if (strA != strB)
			{
				return 1;
			}
			return 0;
		}

		internal static int DstCompare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreWidth | (CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public override void CopyTo(Array array, int index)
		{
			this.InnerList.CopyTo((MySQLParameter[])array, index);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override IEnumerator GetEnumerator()
		{
			return this.InnerList.GetEnumerator();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameterName"></param>
		/// <returns></returns>
		protected override DbParameter GetParameter(string parameterName)
		{
			int num1 = this.IndexOf(parameterName);

			if (num1 < 0)
			{
				throw new InvalidOperationException(string.Format("Can't find parameter {0}", parameterName));
			}

			return this.InnerList[num1];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected override DbParameter GetParameter(int index)
		{
			return this.InnerList[index];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override int IndexOf(object value)
		{
			if (value != null)
			{
				List<MySQLParameter> list = this.InnerList;
				for (int idx = 0; idx < list.Count; idx++)
				{
					if (value == list[idx])
					{
						return idx;
					}
				}
			}

			return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameterName"></param>
		/// <returns></returns>
		public override int IndexOf(string parameterName)
		{
			return IndexOf(this.InnerList, parameterName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public override void Insert(int index, object value)
		{
			this.InnerList.Insert(index, (MySQLParameter)value);
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsFixedSize
		{
			get { return ((IList)this.InnerList).IsFixedSize; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsReadOnly
		{
			get { return ((IList)this.InnerList).IsReadOnly; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsSynchronized
		{
			get { return ((IList)this.InnerList).IsSynchronized; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public override void Remove(object value)
		{
			int index = this.IndexOf(value);

			if (index < 0)
			{
				throw new InvalidOperationException(string.Format("Can't find parameter {0}", value.ToString()));
			}

			this.RemoveAt(index);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		public override void RemoveAt(int index)
		{
			this.InnerList.RemoveAt(index);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameterName"></param>
		public override void RemoveAt(string parameterName)
		{
			RemoveAt(IndexOf(parameterName));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameterName"></param>
		/// <param name="value"></param>
		protected override void SetParameter(string parameterName, DbParameter value)
		{
			int index = this.IndexOf(parameterName);

			if (index < 0)
			{
				throw new InvalidOperationException(string.Format("Can't find parameter {0}", parameterName));
			}

			SetParameter(index, value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		protected override void SetParameter(int index, DbParameter value)
		{
			this.InnerList[index] = (MySQLParameter)value;
		}

		/// <summary>
		/// 
		/// </summary>
		public override object SyncRoot
		{
			get { return ((IList)this.InnerList).SyncRoot; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override int Count
		{
			get
			{
				if (m_List == null)
				{
					return 0;
				}

				return m_List.Count;
			}
		}

		#region Additional Methods
		/// <summary>
		/// Adds the specified MySQLParameter object to the MySQLParameterCollection.
		/// </summary>
		/// <param name="value">The MySQLParameter to add to the collection.</param>
		/// <returns>The index in the collection of the new MySQLParameter object.</returns>
		public int Add(MySQLParameter value)
		{
			if (value.ParameterName != null)
			{
				//sorted = false;
				this.InnerList.Add(value);
				return this.InnerList.Count - 1;
			}

			throw new ArgumentException("parameter must be named");
		}

		/// <summary>
		/// Adds the specified MySQLParameter object to the MySQLParameterCollection.
		/// </summary>
		/// <param name="value">The MySQLParameter to add to the collection.</param>
		/// <returns>The index in the collection of the new MySQLParameter object.</returns>
		public override int Add(object value)
		{
			return Add((MySQLParameter)value);
		}

		/// <summary>
		/// Adds a MySQLParameter to the MySQLParameterCollection with the parameter name and the data type.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="type">One of the DbType values.</param>
		/// <returns>The index in the collection of the new MySQLParameter object.</returns>
		public int Add(string parameterName, DbType type)
		{
			return Add(new MySQLParameter(parameterName, type));
		}


		/// <summary>
		/// Use Add(string parameterName, DbType type)
		/// </summary>
		/// <param name="parameterName"></param>
		/// <param name="dbType">One of the DbType values.</param>
		/// <param name="sourceColumn">This value is ignored. Soported by future operation</param>
		/// <returns>The index in the collection of the new MySQLParameter object.</returns>
		public int Add(string parameterName, DbType dbType, string sourceColumn)
		{
			return Add(new MySQLParameter(parameterName, dbType, sourceColumn));
		}

		/// <summary>
		/// Gets the MySQLParameter at the specified index.
		/// </summary>
		public new MySQLParameter this[string index]
		{
			get
			{
				return (MySQLParameter)GetParameter(index);
			}
			set
			{
				this.SetParameter(index, value);
			}
		}

		#endregion

		// AddSort Method 

		/// <summary>
		/// Return a sorted list that is a shadow copy of the original elements, using the MySQLParameter's comparer.
		/// </summary>
		public ArrayList SortedList
		{
			get
			{
				if (!m_sorted)
				{
					m_sortedList = new ArrayList();
					foreach (MySQLParameter p in this)
					{
						m_sortedList.Add(p);
					}
					m_sortedList.Sort(new MySQLParameterComparer());
					m_sorted = true;
				}
				return m_sortedList;
			}
		}

	}
}
