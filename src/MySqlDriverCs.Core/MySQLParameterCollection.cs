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
        private readonly List<MySQLParameter> _list = new List<MySQLParameter>();

        public int Count => _list.Count;

        /// <inheritdoc />
        public bool IsFixedSize => ((IList)_list).IsFixedSize;

        /// <inheritdoc />
        public bool IsReadOnly => ((IList)_list).IsReadOnly;

        /// <inheritdoc />
        public bool IsSynchronized => ((IList)_list).IsSynchronized;

        public object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets the MySQLParameter at the specified index.
        /// </summary>
        public MySQLParameter this[string index]
        {
            get => GetParameter(index);
            set => SetParameter(index, value);
        }

        /// <inheritdoc />
        object IDataParameterCollection.this[string index]
        {
            get => GetParameter(index);
            set => SetParameter(index, (MySQLParameter)value);
        }

        public MySQLParameter this[int index]
        {
            get => _list[index];
            set => SetParameter(index, value);
        }

        /// <inheritdoc />
        object IList.this[int index]
        {
            get => _list[index];
            set => SetParameter(index, (MySQLParameter)value);
        }

        /// <summary>
        /// Adds the specified MySQLParameter object to the MySQLParameterCollection.
        /// </summary>
        /// <param name="value">The MySQLParameter to add to the collection.</param>
        /// <returns>The MySQLParameter object.</returns>
        public MySQLParameter Add(MySQLParameter value)
        {
            return _list[AddAndReturnIndex(value)];
        }

        /// <inheritdoc />
        public int Add(object value)
        {
            if (!(value is MySQLParameter v))
                throw new ArgumentException("value must be " + nameof(MySQLParameter));
            return AddAndReturnIndex(v);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _list.Clear();
        }

        /// <inheritdoc />
        public bool Contains(object value)
        {
            if (!(value is MySQLParameter v))
                throw new ArgumentException("value must be " + nameof(MySQLParameter));
            return -1 != IndexOf(v);
        }

        /// <inheritdoc />
        public bool Contains(string value)
        {
            return -1 != IndexOf(value);
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            _list.CopyTo((MySQLParameter[])array, index);
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <inheritdoc />
        public int IndexOf(object value)
        {
            if (!(value is MySQLParameter v))
                throw new ArgumentException("value must be " + nameof(MySQLParameter));
            for (int idx = 0; idx < _list.Count; idx++)
            {
                if (v == _list[idx])
                {
                    return idx;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public int IndexOf(string parameterName)
        {
            return _list.FindIndex(x => x.ParameterName == parameterName);
        }

        /// <inheritdoc />
        public void Insert(int index, object value)
        {
            if (!(value is MySQLParameter v))
                throw new ArgumentException("value must be " + nameof(MySQLParameter));
            _list.Insert(index, v);
        }

        /// <inheritdoc />
        public void Remove(object value)
        {
            if (!(value is MySQLParameter v))
                throw new ArgumentException("value must be " + nameof(MySQLParameter));
            int index = IndexOf(v);

            if (index < 0)
            {
                throw new InvalidOperationException($"Can't find parameter {v}");
            }

            RemoveAt(index);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        /// <inheritdoc />
        public void RemoveAt(string parameterName)
        {
            RemoveAt(IndexOf(parameterName));
        }

        /// <summary>
		/// Adds the specified MySQLParameter object to the MySQLParameterCollection.
		/// </summary>
		/// <param name="value">The MySQLParameter to add to the collection.</param>
		/// <returns>The index in the collection of the new MySQLParameter object.</returns>
        private int AddAndReturnIndex(MySQLParameter value)
        {
            if (value.ParameterName == null) throw new ArgumentException("parameter must be named");

            _list.Add(value);
            return _list.Count - 1;
        }
        private MySQLParameter GetParameter(string parameterName)
        {
            int num1 = IndexOf(parameterName);

            if (num1 < 0)
            {
                throw new InvalidOperationException($"Can't find parameter {parameterName}");
            }

            return _list[num1];
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
            _list[index] = value;
        }

        // AddSort Method
    }
}