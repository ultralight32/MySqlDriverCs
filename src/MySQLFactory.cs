/* $History: MySQLFactory.cs $
//
//*****************  Version 2  *****************
//User: GMilano      Date: 5/04/06    Time: 5:51p
//Updated in $/Tauro/Deklarit/MySQLDriverCS/MySQLDriverCS.root/MySQLDriverCS/Deklarit/MySQLDriverCS
//CreateConnectionString implemented
 */
using System.Data.Common;

namespace MySQLDriverCS
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class MySQLFactory : DbProviderFactory
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly MySQLFactory Instance;

		static MySQLFactory()
		{
			if (Instance == null)
				Instance = new MySQLFactory();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override DbCommand CreateCommand()
		{
			return new MySQLCommand();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override DbConnection CreateConnection()
		{
			return new MySQLConnection();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override DbDataAdapter CreateDataAdapter()
		{
			return new MySQLDataAdapter();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override DbParameter CreateParameter()
		{
			return new MySQLParameter();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override DbConnectionStringBuilder CreateConnectionStringBuilder()
		{
			return new DbConnectionStringBuilder();
		}
	}
}
