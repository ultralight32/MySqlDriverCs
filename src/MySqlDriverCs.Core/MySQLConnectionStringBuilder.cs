using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;

namespace MySQLDriverCS
{
	/// <summary>
	/// 
	/// </summary>
	public class MySQLConnectionStringBuilder : DbConnectionStringBuilder
	{
		private enum Keywords
		{
			DataSource,
			Host,
			Password,
			UserId
		}

		private string[] validKeywords = { "Data Source", "Host", "User Id", "Password" };
		private Dictionary<string, Keywords> mapKeyword = new Dictionary<string, Keywords>();

		/// <summary>
		/// 
		/// </summary>
		public MySQLConnectionStringBuilder()
		{
			mapKeyword["Data Source"] = Keywords.DataSource;
			mapKeyword["Host"] = Keywords.Host;
			mapKeyword["Password"] = Keywords.Password;
			mapKeyword["User Id"] = Keywords.UserId;
		}

		private string dataSource;

		/// <summary>
		/// 
		/// </summary>
		public string DataSource
		{
			get { return dataSource; }
			set { dataSource = value; base["Data Source"] = value; }
		}

		private string host;
		/// <summary>
		/// 
		/// </summary>
		public string Host
		{
			get { return host; }
			set { host = value; base["Host"] = value; }
		}

		private string userId;
		/// <summary>
		/// 
		/// </summary>
		public string UserId
		{
			get { return userId; }
			set { userId = value; base["User Id"] = value; }
		}

		private string password;
		/// <summary>
		/// 
		/// </summary>
		public string Password
		{
			get { return password; }
			set { password = value; base["Password"] = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="propertyDescriptors"></param>
		protected override void GetProperties(System.Collections.Hashtable propertyDescriptors)
		{
			foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this, true))
			{
				propertyDescriptors[descriptor.DisplayName] = descriptor;
			}
			base.GetProperties(propertyDescriptors);
		}

		private object GetAt(Keywords index)
		{
			switch (index)
			{
				case Keywords.Password: return this.Password;
				case Keywords.Host: return this.Host;
				case Keywords.DataSource: return this.DataSource;
				case Keywords.UserId: return this.UserId;
				default:
					throw new Exception("Keyword not supported");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyword"></param>
		/// <returns></returns>
		public override bool ContainsKey(string keyword)
		{
			return this.mapKeyword.ContainsKey(keyword);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool TryGetValue(string keyword, out object value)
		{
			Keywords key;
			if (this.mapKeyword.TryGetValue(keyword, out key))
			{
				value = GetAt(key);
				return true;
			}
			value = null;
			return false;
		}
	}
}
