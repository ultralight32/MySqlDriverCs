/* $History: MySQLFactory.cs $
//
//*****************  Version 2  *****************
//User: GMilano      Date: 5/04/06    Time: 5:51p
//Updated in $/Tauro/Deklarit/MySQLDriverCS/MySQLDriverCS.root/MySQLDriverCS/Deklarit/MySQLDriverCS
//CreateConnectionString implemented
 */

using MySQLDriverCS;
using System.Data;
using System.Data.Common;

namespace MySQLDriverCS
{
   
    public sealed class MySQLFactory
    {
   
        public static readonly MySQLFactory Instance;

        static MySQLFactory()
        {
            if (Instance == null)
                Instance = new MySQLFactory();
        }

    
        /// <returns></returns>
        public IDbCommand CreateCommand()
        {
            return new MySQLCommand();
        }

 
        /// <returns></returns>
        public IDbConnection CreateConnection()
        {
            return new MySQLConnection();
        }


        /// <returns></returns>
        public IDbDataAdapter CreateDataAdapter()
        {
            return new MySQLDataAdapter();
        }


        /// <returns></returns>
        public IDataParameter CreateParameter()
        {
            return new MySQLParameter();
        }

  
        /// <returns></returns>
        public DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new DbConnectionStringBuilder();
        }
    }
}