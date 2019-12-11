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
using System.Data;
using System.Data.Common;

namespace MySQLDriverCS
{
	/// <summary>
	/// Transaction control in MySQL
	/// </summary>
	public class MySQLTransaction : DbTransaction
	{
        private MySQLConnection Conn = null;
		private IsolationLevel IL = IsolationLevel.Unspecified;
        private bool bDisposed = false;

		internal MySQLTransaction(MySQLConnection conn, IsolationLevel il)
		{
			Conn=conn;
			MySQLCommand cmd = null;
			switch(il)
			{
				case IsolationLevel.ReadCommitted:
                    cmd = new MySQLCommand("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED",conn);
					break;
				case IsolationLevel.ReadUncommitted:
                    cmd = new MySQLCommand("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED", conn);
					break;
				case IsolationLevel.RepeatableRead:
                    cmd = new MySQLCommand("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ", conn);
					break;
				case IsolationLevel.Serializable:
                    cmd = new MySQLCommand("SET SESSION TRANSACTION ISOLATION LEVEL SERIALIZABLE", conn);
					break;
				case IsolationLevel.Chaos:
					throw new MySQLException("MySQLDriverCS Error: Chaos isolation level is not implemented in MySQL.");
					
			}

			if(cmd != null)
			{
				IL=il;
				cmd.ExecuteNonQuery();
			}
			cmd = new MySQLCommand("BEGIN",conn);
			cmd.ExecuteNonQuery();
		}
		/// <summary>
		/// Performs a commit
		/// </summary>
		public override void Commit()
		{
			if(Conn!=null)
			{
				MySQLCommand cmd = new MySQLCommand("COMMIT",Conn);
				cmd.ExecuteNonQuery();
			}
		}
		/// <summary>
		/// Performs a rollback
		/// </summary>
		public override void Rollback()
		{
			if(Conn!=null)
			{
				MySQLCommand cmd = new MySQLCommand("ROLLBACK",Conn);
				cmd.ExecuteNonQuery();
			}
		}
		/// <summary>
		/// Connection property
		/// </summary>
		protected override DbConnection DbConnection
		{
			get
			{ 
				return Conn;
			}
		}
		/// <summary>
		/// Isolation level property
		/// </summary>
		public override IsolationLevel IsolationLevel 
		{
			get
			{
				return IL;
			}
		}
		
		/// <summary>
		/// Dispose destructor
		/// </summary>
		protected override void Dispose(bool disposing)
        {
            if (bDisposed) return;

            try
            {
				if (Conn.State != ConnectionState.Closed)
				{
					Rollback();
				}
            }
            finally
            {
                Conn = null;
                IL = IsolationLevel.Unspecified;
                bDisposed = true;
            }
		}
	}
}
