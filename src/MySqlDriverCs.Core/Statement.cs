using System;
using System.Data.Common;

namespace MySQLDriverCS
{
	internal class Statement:IDisposable
	{
		protected MySQLParameterCollection m_parameters;
		public MySQLConnection connection;
		protected string query;
		protected bool bTryToCancel;

		internal MySQLParameterCollection Parameters
		{
			set{ m_parameters = value;}
		}
		internal bool TryToCancel
		{
			set{ bTryToCancel=value;}
			get{ return bTryToCancel;}
		}
		public virtual void Dispose()
		{
		}

		internal virtual uint CursorType
		{
			set
			{
			}
		}
		internal virtual uint FetchSize
		{
			set
			{
			}
		}
		internal virtual void Prepare()
		{
		}
		internal virtual int ExecuteNonQuery()
		{
			return 0;
		}
		internal virtual int ExecuteCall()
		{
			return 0;
		}
		internal virtual DbDataReader ExecuteReader(bool CloseConnection)
		{
			return null;
		}
	}
}
