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

#endregion LICENSE

using System.Data;

namespace MySQLDriverCS
{
    /// <summary>
    /// Transaction control in MySQL
    /// </summary>
    public class MySQLTransaction : IDbTransaction
    {
        private bool _disposed = false;

        internal MySQLTransaction(MySQLConnection connection, IsolationLevel isolationLevel)
        {
            Connection = connection;
            var sql = "";

            switch (isolationLevel)
            {
                case IsolationLevel.ReadCommitted:
                    sql = "SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED";

                    break;

                case IsolationLevel.ReadUncommitted:
                    sql = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";

                    break;

                case IsolationLevel.RepeatableRead:
                    sql = "SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ";

                    break;

                case IsolationLevel.Serializable:
                    sql = "SET SESSION TRANSACTION ISOLATION LEVEL SERIALIZABLE";

                    break;

                case IsolationLevel.Chaos:
                    throw new MySqlException("MySQLDriverCS Error: Chaos isolation level is not implemented in MySQL.");
            }

            IsolationLevel = isolationLevel;
            using (var cmd = new MySQLCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new MySQLCommand("BEGIN", connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <inheritdoc />
        IDbConnection IDbTransaction.Connection => Connection;

        public MySQLConnection Connection { get; private set; }

        /// <inheritdoc />
        public IsolationLevel IsolationLevel { get; private set; } = IsolationLevel.Unspecified;

        /// <inheritdoc />
        public void Commit()
        {
            if (Connection == null)
                throw new MySqlException("Connection was closed");
            using (var cmd = new MySQLCommand("COMMIT", Connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    Rollback();
                }
            }
            finally
            {
                Connection = null;
                IsolationLevel = IsolationLevel.Unspecified;
                _disposed = true;
            }
        }

        /// <inheritdoc />
        public void Rollback()
        {
            if (Connection == null)
                throw new MySqlException("Connection was closed");
            using (var cmd = new MySQLCommand("ROLLBACK", Connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}