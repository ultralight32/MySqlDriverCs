using System.Collections;

namespace MySQLDriverCS
{
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

        #endregion IComparer Members
    }
}