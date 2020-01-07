#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.

	Please, Read first MySQLDriverCS LICENSE

	MySQLDataAdapter.cs
	This file is add to MySQLDriverCS.
	Copyright (c) 2003 Omar del Valle Rodríguez.

	MySQLDataAdapter is part MySQLDriverCS: you can redistribute it and/or modify
	it only part with all set of MySQLDriverCS and under the terms of MySQLDriverCS License.

	You can use this part freely but WITHOUT ANY WARRANTY; without even the implied warranty 
	of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public 
	License for more details.

	Thank to Manuel Lucas Viñas Livschitz by modify MySQLDataReader.NextResult to good result
	in MySQLDataAdapter.
*/
#endregion

using System;
using System.Data;
using System.Data.Common;

namespace MySQLDriverCS
{
	/// <summary>
	/// MySQLDataAdapter.
	/// </summary>
	public class MySQLDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		private MySQLCommand m_selectCommand;
		private MySQLCommand m_insertCommand;
		private MySQLCommand m_updateCommand;
		private MySQLCommand m_deleteCommand;

		private static readonly object EventRowUpdated = new object();
		private static readonly object EventRowUpdating = new object();

		/// <summary>
		/// Constructor
		/// </summary>
		public MySQLDataAdapter()
		{
		}
		/// <summary>
		/// Constructor from MySQLCommand
		/// </summary>
		/// <param name="cmd"></param>
		public MySQLDataAdapter(MySQLCommand cmd)
		{
			m_selectCommand = cmd;
		}
		/// <summary>
		/// Constructor from MySQLCommand and MySQLConnection
		/// </summary>
		/// <param name="strCommand"></param>
		/// <param name="conn"></param>
		public MySQLDataAdapter(String strCommand, MySQLConnection conn)
		{
			m_selectCommand = new MySQLCommand(strCommand, conn);
		}
		/// <summary>
		/// The SQL SELECT command as MySQLCommand
		/// </summary>
		public new MySQLCommand SelectCommand
		{
			get { return m_selectCommand; }
			set { m_selectCommand = value; }
		}
		/// <summary>
		/// The SQL SELECT command as ADO.NET IDbCommand
		/// </summary>
		IDbCommand IDbDataAdapter.SelectCommand
		{
			get { return m_selectCommand; }
			set { m_selectCommand = (MySQLCommand)value; }
		}
		/// <summary>
		/// The SQL INSERT command as MySQLCommand
		/// </summary>
		public new MySQLCommand InsertCommand
		{
			get { return m_insertCommand; }
			set { m_insertCommand = value; }
		}
		/// <summary>
		/// The SQL INSERT command as ADO.NET IDbCommand
		/// </summary>
		IDbCommand IDbDataAdapter.InsertCommand
		{
			get { return m_insertCommand; }
			set { m_insertCommand = (MySQLCommand)value; }
		}
		/// <summary>
		/// The SQL UPDATE command as MySQLCommand
		/// </summary>
		public new MySQLCommand UpdateCommand
		{
			get { return m_updateCommand; }
			set { m_updateCommand = value; }
		}
		/// <summary>
		/// The SQL UPDATE command as ADO.NET IDbCommand
		/// </summary>
		IDbCommand IDbDataAdapter.UpdateCommand
		{
			get { return m_updateCommand; }
			set { m_updateCommand = (MySQLCommand)value; }
		}
		/// <summary>
		/// The SQL DELETE command as MySQLCommand
		/// </summary>
		public new MySQLCommand DeleteCommand
		{
			get { return m_deleteCommand; }
			set { m_deleteCommand = value; }
		}
		/// <summary>
		/// The SQL DELETE command as ADO.NET IDbCommand
		/// </summary>
		IDbCommand IDbDataAdapter.DeleteCommand
		{
			get { return m_deleteCommand; }
			set { m_deleteCommand = (MySQLCommand)value; }
		}

		/*
		 * Implement abstract methods inherited from DbDataAdapter.
		 */
		/// <summary>
		/// Inherited from DbDataAdapter
		/// </summary>
		/// <param name="dataRow"></param>
		/// <param name="command"></param>
		/// <param name="statementType"></param>
		/// <param name="tableMapping"></param>
		/// <returns></returns>
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new MySQLRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
		}
		/// <summary>
		/// Inherited from DbDataAdapter
		/// </summary>
		/// <param name="dataRow"></param>
		/// <param name="command"></param>
		/// <param name="statementType"></param>
		/// <param name="tableMapping"></param>
		/// <returns></returns>
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new MySQLRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}
		/// <summary>
		/// Inherited from DbDataAdapter
		/// </summary>
		/// <param name="value"></param>
		protected override void OnRowUpdating(RowUpdatingEventArgs value)
		{
			MySQLRowUpdatingEventHandler handler = (MySQLRowUpdatingEventHandler)Events[EventRowUpdating];
			if ((null != handler) && (value is MySQLRowUpdatingEventArgs))
			{
				handler(this, (MySQLRowUpdatingEventArgs)value);
			}
		}
		/// <summary>
		/// Inherited from DbDataAdapter
		/// </summary>
		/// <param name="value"></param>
		protected override void OnRowUpdated(RowUpdatedEventArgs value)
		{
			MySQLRowUpdatedEventHandler handler = (MySQLRowUpdatedEventHandler)Events[EventRowUpdated];
			if ((null != handler) && (value is MySQLRowUpdatedEventArgs))
			{
				handler(this, (MySQLRowUpdatedEventArgs)value);
			}
		}
		/// <summary>
		/// Updating event
		/// </summary>
		public event MySQLRowUpdatingEventHandler RowUpdating
		{
			add { Events.AddHandler(EventRowUpdating, value); }
			remove { Events.RemoveHandler(EventRowUpdating, value); }
		}
		/// <summary>
		/// After update event
		/// </summary>
		public event MySQLRowUpdatedEventHandler RowUpdated
		{
			add { Events.AddHandler(EventRowUpdated, value); }
			remove { Events.RemoveHandler(EventRowUpdated, value); }
		}
	}
	/// <summary>
	/// Updating event handler
	/// </summary>
	public delegate void MySQLRowUpdatingEventHandler(object sender, MySQLRowUpdatingEventArgs e);
	/// <summary>
	/// After update event handler
	/// </summary>
	public delegate void MySQLRowUpdatedEventHandler(object sender, MySQLRowUpdatedEventArgs e);
	/// <summary>
	/// Updating event arguments
	/// </summary>
	public class MySQLRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="row"></param>
		/// <param name="command"></param>
		/// <param name="statementType"></param>
		/// <param name="tableMapping"></param>
		public MySQLRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			: base(row, command, statementType, tableMapping)
		{
		}
		/// <summary>
		/// Gets MySQLCommand
		/// </summary>
		/// <remarks>Hide the inherited implementation of the command property.</remarks>
		public new MySQLCommand Command
		{
			get { return (MySQLCommand)base.Command; }
			set { base.Command = value; }
		}
	}
	/// <summary>
	/// After update event handler
	/// </summary>
	public class MySQLRowUpdatedEventArgs : RowUpdatedEventArgs
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="row"></param>
		/// <param name="command"></param>
		/// <param name="statementType"></param>
		/// <param name="tableMapping"></param>
		public MySQLRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			: base(row, command, statementType, tableMapping)
		{
		}
		/// <summary>
		/// Gets MySQLCommand
		/// </summary>
		/// <remarks>Hide the inherited implementation of the command property.</remarks>
		public new MySQLCommand Command
		{
			get { return (MySQLCommand)base.Command; }
		}

	}
}
