using System;
using System.Collections;

namespace MySQLDriverCS.Interop
{
    /// <summary>
    ///   This class is used to get information about client library.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class MYSQL_FIELD_FACTORY
    {
        private readonly NativeConnection _nativeConnection;
        static string version;

        internal MYSQL_FIELD_FACTORY(NativeConnection nativeConnection)
        {
            _nativeConnection = nativeConnection;
        }
        /// <summary>
        /// Returns a IMYSQL_FIELD instance according to the client library version.
        /// </summary>
        /// <returns></returns>
        public IMySqlField GetInstance()
        {
            if (version == null)
            {
                version = _nativeConnection.ClientVersion;
            }

            if (MySQLUtils.RunningOn64x)
                return new MYSQL_FIELD_VERSION_5_64();
            else
                return new MYSQL_FIELD_VERSION_5_32();

        }
    }
}