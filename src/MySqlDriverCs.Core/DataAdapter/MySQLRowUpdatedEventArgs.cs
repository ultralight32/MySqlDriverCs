using System.Data;
using System.Data.Common;

namespace MySQLDriverCS
{
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