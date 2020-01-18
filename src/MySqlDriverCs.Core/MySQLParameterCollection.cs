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

#endregion LICENSE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;

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

    public sealed class MySQLParameterCollection : IDataParameterCollection
    {
        private readonly List<MySQLParameter> m_List = new List<MySQLParameter>();
        private bool m_sorted;
        private ArrayList m_sortedList;

        public int Count
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

        /// <inheritdoc />
        public bool IsFixedSize => ((IList)m_List).IsFixedSize;

        /// <inheritdoc />
        public bool IsReadOnly => ((IList)m_List).IsReadOnly;

        /// <inheritdoc />
        public bool IsSynchronized => ((IList)m_List).IsSynchronized;

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

        public object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets the MySQLParameter at the specified index.
        /// </summary>
        public MySQLParameter this[string index]
        {
            get => (MySQLParameter)GetParameter(index);
            set => SetParameter(index, value);
        }

        /// <inheritdoc />
        object IDataParameterCollection.this[string index]
        {
            get => (MySQLParameter)GetParameter(index);
            set => SetParameter(index, (MySQLParameter)value);
        }

        public MySQLParameter this[int index]
        {
            get => (MySQLParameter)m_List[index];
            set => SetParameter(index, value);
        }

        /// <inheritdoc />
        object IList.this[int index]
        {
            get => (MySQLParameter)m_List[index];
            set => SetParameter(index, (MySQLParameter)value);
        }

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
                m_List.Add(value);
                return m_List.Count - 1;
            }

            throw new ArgumentException("parameter must be named");
        }

        /// <inheritdoc />
        public int Add(object value)
        {
            return Add((MySQLParameter)value);
        }

        /// <inheritdoc />he new MySQLParameter object.</returns>
        public int Add(string parameterName, DbType type)
        {
            return Add(new MySQLParameter(parameterName, type));
        }

        /// <inheritdoc />
        public void Clear()
        {
            m_List.Clear();
        }

        /// <inheritdoc />
        public bool Contains(object value)
        {
            return -1 != IndexOf(value);
        }

        /// <inheritdoc />
        public bool Contains(string value)
        {
            return -1 != IndexOf(value);
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            m_List.CopyTo((MySQLParameter[])array, index);
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        /// <inheritdoc />
        public int IndexOf(object value)
        {
            if (value != null)
            {
                for (int idx = 0; idx < m_List.Count; idx++)
                {
                    if (value == m_List[idx])
                    {
                        return idx;
                    }
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public int IndexOf(string parameterName)
        {
            return IndexOf(m_List, parameterName);
        }

        /// <inheritdoc />
        public void Insert(int index, object value)
        {
            m_List.Insert(index, (MySQLParameter)value);
        }

        /// <inheritdoc />
        public void Remove(object value)
        {
            int index = IndexOf(value);

            if (index < 0)
            {
                throw new InvalidOperationException($"Can't find parameter {value.ToString()}");
            }

            RemoveAt(index);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
        }

        /// <inheritdoc />
        public void RemoveAt(string parameterName)
        {
            RemoveAt(IndexOf(parameterName));
        }

        internal static int DstCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
        }

        internal static int SrcCompare(string strA, string strB)
        {
            if (strA != strB)
            {
                return 1;
            }
            return 0;
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

        private MySQLParameter GetParameter(string parameterName)
        {
            int num1 = IndexOf(parameterName);

            if (num1 < 0)
            {
                throw new InvalidOperationException($"Can't find parameter {parameterName}");
            }

            return m_List[num1];
        }

        private void SetParameter(string parameterName, MySQLParameter value)
        {
            int index = IndexOf(parameterName);

            if (index < 0)
            {
                throw new InvalidOperationException($"Can't find parameter {parameterName}");
            }

            SetParameter(index, value);
        }

        private void SetParameter(int index, MySQLParameter value)
        {
            m_List[index] = (MySQLParameter)value;
        }
        // AddSort Method
    }
}