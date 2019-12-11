using System;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
	internal class Statement
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
		internal virtual void Dispose()
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
	/// <summary>
	/// Summary description for Statement.
	/// </summary>
	internal class DirectStatement:Statement
	{
		protected string repQuery;

		public DirectStatement(MySQLConnection connection, string query)
		{
			this.connection = connection;
			this.query = query;
		}
		#region Statement Members


		/// Modified by Claudia Murialdo (07/24/04) in order to support prepared
		/// commands's caching, time component of the datetime values, 
		/// culture-independent format of numeric values in a stmt and
		/// to work with stmts with similar parameters like: SELECT * FROM 
		/// Table1 WHERE A=@ParmName AND B=@ParmNameOther
		private string BindParameters()
		{
			string resQuery=query;

			//Parameters is a shallow copy of m_parameters
			ArrayList parameters = m_parameters.SortedList;

			foreach (MySQLParameter param in parameters)
			{
				string paramName = param.ParameterName;
				object Value = param.Value;

				if (param.Direction == ParameterDirection.Output && resQuery.StartsWith("CALL"))
				{
					// Hack - en calls no modifico el parametro de output, sino MySql da error
					continue;
				}

				if(Value==null || Value==DBNull.Value)
					resQuery = resQuery.Replace(paramName, "NULL");
				else
					if( (Value.GetType()==typeof(int))||
					(Value.GetType()==typeof(long))||
					(Value.GetType()==typeof(short))||
					(Value.GetType()==typeof(decimal))||
					(Value.GetType()==typeof(float))||
					(Value.GetType()==typeof(double)) )
					resQuery = resQuery.Replace(paramName, Convert.ToString(Value,CultureInfo.InvariantCulture.NumberFormat));
				else
					if(Value.GetType()==typeof(DateTime))
				{
					DateTime dt = (DateTime)Value;
					string dateStr;
                    if (param.DbType == DbType.Date)
                    {
                        dateStr = " \"" + dt.Year.ToString("D4") + "-" + dt.Month.ToString("D2") + "-" + dt.Day.ToString("D2") + "\" "; ;
                    }
                    else if (param.DbType == DbType.DateTime2)
                    {
                        dateStr = " \"" + dt.Year.ToString("D4") + "-" + dt.Month.ToString("D2") + "-" + dt.Day.ToString("D2") +
                            " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second + ((dt.Millisecond > 0) ? "." + dt.Millisecond.ToString("D3") : "") +  "\" ";                        
                    }
                    else
                    {
                        dateStr = " \"" + dt.Year.ToString("D4") + "-" + dt.Month.ToString("D2") + "-" + dt.Day.ToString("D2") +
                            " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second + "\" ";
                    }
					resQuery = resQuery.Replace(paramName, dateStr);
				}
				else if (Value.GetType()==typeof(byte[]))
				{
					resQuery = resQuery.Replace(paramName,  
						MySQLUtils.Escape(System.Text.Encoding.Default.GetString((byte[])Value), connection));
				}
				else
				{
					resQuery = resQuery.Replace(paramName, MySQLUtils.Escape(Value.ToString(), connection));
				}				
			}
			return resQuery;
		}

		internal override unsafe int ExecuteNonQuery()
		{
			void * result;
			repQuery = BindParameters();
			if(NativeMethods.mysql_query(connection.handle,repQuery)!=0)
			{
				// error
				throw new MySQLException(NativeMethods.mysql_errno(connection.handle), "MySQLDriverCS Error: wrong query."+NativeMethods.mysql_error(connection.handle) + " " + repQuery);
			}
			else // query succeeded, process any data returned by it
			{
				result = NativeMethods.mysql_store_result(connection.handle);
				if(result!=null)  // there are rows
				{
					//num_fields = mysql_num_fields(result);
					// retrieve rows, then call mysql_free_result(result)
					NativeMethods.mysql_free_result(result);
					return 0;
				}
				else  // mysql_store_result() returned nothing; should it have?
				{
					if(NativeMethods.mysql_errno(connection.handle)!=0)
					{
						throw new MySQLException(NativeMethods.mysql_errno(connection.handle), "MySQLDriverCS Error: "+NativeMethods.mysql_error(connection.handle));
					}
					else if (NativeMethods.mysql_field_count(connection.handle) == 0)
					{
						// query does not return data
						// (it was not a SELECT)
						return (int)NativeMethods.mysql_affected_rows(connection.handle);
					}
					else
						return 0;
				}
			}

		}

		internal override unsafe int ExecuteCall()
		{
			repQuery = BindParameters();
			if (NativeMethods.mysql_query(connection.handle, repQuery) != 0)
			{
				// error
				throw new MySQLException(NativeMethods.mysql_errno(connection.handle), "MySQLDriverCS Error: wrong query." + NativeMethods.mysql_error(connection.handle) + " " + repQuery);
			}
			else // query succeeded, process any data returned by it
			{
				// Retrieve output parameters
				return query_output_parameters();
			}

		}
		unsafe int query_output_parameters()
		{
			// Get output parameters - build select
			ArrayList parameters = m_parameters.SortedList;
			StringBuilder stmt = new StringBuilder();
			int pCount = 0;
			MySQLDataReader dr = null;
			foreach (MySQLParameter param in parameters)
			{
				if (param.Direction == ParameterDirection.Output)
				{
					if (pCount > 0) stmt.Append(", ");
					stmt.Append(param.ParameterName);
					pCount++;
				}
			}
			if (pCount > 0)
			{
				stmt.Insert(0, "select ");
				// Query output parameter values
				if (NativeMethods.mysql_query(connection.handle, stmt.ToString()) != 0)
				{
					// error
					throw new MySQLException(NativeMethods.mysql_errno(connection.handle), "MySQLDriverCS Error: wrong query." + NativeMethods.mysql_error(connection.handle) + " " + repQuery);
				}
				void* result = NativeMethods.mysql_store_result(connection.handle);
				if (result != null)  // there are rows
				{
					dr = new MySQLDataReader(result, this.connection, this, false);
					if (dr.Read())
					{
						// Load values
						foreach (MySQLParameter param in parameters)
							if (param.Direction == ParameterDirection.Output)
								param.Value = dr[param.ParameterName];
					}
					NativeMethods.mysql_free_result(result);
					dr.Close();
				}
			}
			return 0;
		}
		
		internal override unsafe DbDataReader ExecuteReader(bool CloseConnection)
		{
			bTryToCancel=false;
			repQuery = BindParameters();
			void* result;
			if(NativeMethods.mysql_query(connection.handle,repQuery)!=0)
			{
				// error
				throw new MySQLException(NativeMethods.mysql_errno(connection.handle), "MySQLDriverCS Error: wrong query."+NativeMethods.mysql_error(connection.handle));
			}
			else // query succeeded, process any data returned by it
			{
				result = NativeMethods.mysql_store_result(connection.handle);
				if(result!=null)  // there are rows
				{
					//num_fields = mysql_num_fields(result);
					// retrieve rows, then call mysql_free_result(result)

					// Update by Omar del Valle Rodríguez (omarvr72@yahoo.com.mx)
					// Don't close connection after close DataReader
					MySQLDataReader dr = new MySQLDataReader(result,this.connection,this, CloseConnection);
					NativeMethods.mysql_free_result(result);
					return dr;
				}
				else  // mysql_store_result() returned nothing; should it have?
				{
					if(NativeMethods.mysql_errno(connection.handle)!=0)
					{
						throw new MySQLException(NativeMethods.mysql_errno(connection.handle),"MySQLDriverCS Error: "+NativeMethods.mysql_error(connection.handle));
					}
					else if (NativeMethods.mysql_field_count(connection.handle) == 0)
					{
						// query does not return data
						// (it was not a SELECT)
						return null;
					}
					else
						return null;
				}
			}

		}

		#endregion
	}
    internal abstract unsafe class PreparedStatement : Statement
    {
        protected bool prepared;
        protected IntPtr stmt;
        protected void* result_metadata;
        protected int m_parm_count;
        protected int m_fetch_size;
        protected uint m_cursor_type;
        protected uint m_field_count;

        public PreparedStatement()
        {
        }
        public static PreparedStatement GetInstance(MySQLConnection connection, string query){
            if (MySQLUtils.RunningOn64x)
                return new PreparedStatement64(connection, query);
            else
                return new PreparedStatement32(connection, query);
        }

        public static uint DbtoMysqlType(DbType dbtype)
        {
            switch (dbtype)
            {
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.String:
                    return (uint)FieldTypes5.FIELD_TYPE_STRING;
                case DbType.AnsiString:
                    return (uint)FieldTypes5.FIELD_TYPE_VARCHAR;
                case DbType.Binary:
                    return (uint)FieldTypes5.FIELD_TYPE_BLOB;
                case DbType.Boolean:
                    return (uint)FieldTypes5.FIELD_TYPE_BIT;
                case DbType.Byte:
                    return (uint)FieldTypes5.FIELD_TYPE_TINY;
                /*case DbType.Currency: 
                    return (uint) FieldTypes5.FIELD_TYPE_MONEY;*/
                case DbType.Date:
                    return (uint)FieldTypes5.FIELD_TYPE_DATE;
                case DbType.DateTime:
                    return (uint)FieldTypes5.FIELD_TYPE_DATETIME;
                case DbType.DateTime2:
                    return (uint)FieldTypes5.FIELD_TYPE_DATETIME;
                case DbType.Decimal:
                    return (uint)FieldTypes5.FIELD_TYPE_DECIMAL;
                case DbType.Double:
                    return (uint)FieldTypes5.FIELD_TYPE_DOUBLE;
                /*case DbType.Guid: 
                    return (uint) FieldTypes5.FIELD_TYPE_DOUBLE;*/
                case DbType.Int16:
                    return (uint)FieldTypes5.FIELD_TYPE_SHORT;
                case DbType.Int32:
                    return (uint)FieldTypes5.FIELD_TYPE_LONG;
                case DbType.Int64:
                    return (uint)FieldTypes5.FIELD_TYPE_LONGLONG;
                /*case DbType.Object: 
                    return (uint) FieldTypes5.FIELD_TYPE_VARIANT;
                case DbType.SByte: 
                    return (uint) FieldTypes5.FIELD_TYPE_LONGLONG;*/
                case DbType.Single:
                    return (uint)FieldTypes5.FIELD_TYPE_FLOAT;
                case DbType.Time:
                    return (uint)FieldTypes5.FIELD_TYPE_TIME;
                default:
                    return (uint)FieldTypes5.FIELD_TYPE_SHORT;

            }
        }
        #region Statement Members

        internal override void Prepare()
        {
            if (!prepared)
            {
                int code = NativeMethods.mysql_stmt_prepare(stmt, query, query.Length);
                if (code != 0)
                {
                    uint errno = NativeMethods.mysql_stmt_errno(stmt);
                    throw new MySQLException(errno, "MySQLDriverCS Error: wrong cursor type." + NativeMethods.mysql_stmt_error(stmt));
                }
                prepared = true;
            }
        }
        /// <summary>
        /// Number of rows to fetch from server at a time when using a cursor.
        /// This options is supported since MySQL 5.0.6
        /// </summary>
        internal override uint FetchSize
        {
            set
            {
                if (m_fetch_size != value)
                {
                    m_fetch_size = (int)value;
                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
                    Marshal.StructureToPtr(value, ptr, false);
                    sbyte code = NativeMethods.mysql_stmt_attr_set(stmt, StmtAttrTypes.STMT_ATTR_PREFETCH_ROWS, ptr);
                    Marshal.FreeHGlobal(ptr);
                    if (code != 0)
                    {
                        string version = NativeMethods.GetClientVersion();
                        if (version.CompareTo("5.0.6") < 0)
                        {
                            throw new MySQLException(NativeMethods.mysql_stmt_errno(stmt), "MySQLDriverCS Error: wrong fetch type." + NativeMethods.mysql_stmt_error(stmt) + " Client version " + version + " does not support current fetch size; consider upgrading MySql client.");
                        }
                        else
                        {
                            throw new MySQLException(NativeMethods.mysql_stmt_errno(stmt), "MySQLDriverCS Error: wrong fetch type." + NativeMethods.mysql_stmt_error(stmt));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Type of cursor to open for statement 
        /// This options is supported since MySQL 5.0.2
        /// </summary>
        internal override uint CursorType
        {
            set
            {
                try
                {
                    if (m_cursor_type != value)
                    {
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
                        Marshal.StructureToPtr(value, ptr, false);
                        sbyte code = NativeMethods.mysql_stmt_attr_set(stmt, StmtAttrTypes.STMT_ATTR_CURSOR_TYPE, ptr);
                        Marshal.FreeHGlobal(ptr);
                        if (code != 0)
                        {
                            string version = NativeMethods.GetClientVersion();
                            if (version.CompareTo("5.0.2") < 0)
                            {
                                throw new MySQLException(NativeMethods.mysql_stmt_errno(stmt), "MySQLDriverCS Error: wrong cursor type." + NativeMethods.mysql_stmt_error(stmt) + " Client version " + version + " does not support current cursor type; consider upgrading MySql client.");
                            }
                            else
                            {
                                throw new MySQLException(NativeMethods.mysql_stmt_errno(stmt), "MySQLDriverCS Error: wrong cursor type." + NativeMethods.mysql_stmt_error(stmt));
                            }
                        }
                        m_cursor_type = value;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                    if (e is MySQLException)
                    {
                        throw e;
                    }
                }
            }
        }
        public abstract void BindParameters();
        internal override int ExecuteNonQuery()
        {
            BindParameters();
            if (NativeMethods.mysql_stmt_execute(stmt) != 0)
            {
                uint code = NativeMethods.mysql_stmt_errno(stmt);
                if (code != 1062)//Duplicated record ER_DUP_ENTRY
                    throw new MySQLException(code, "MySQLDriverCS Error: wrong query." + NativeMethods.mysql_stmt_error(stmt));
                else
                    throw new MySQLException(code, "MySQLDriverCS Error: wrong query.");
            }
            else
            {
                uint affectedRows = NativeMethods.mysql_stmt_affected_rows(stmt);
                return (int)affectedRows;
            }
        }
        internal override DbDataReader ExecuteReader(bool CloseConnection)
        {
            bTryToCancel = false;
            if (!prepared)
                Prepare();
            BindParameters();
            if (m_field_count == 0)
            {
                result_metadata = NativeMethods.mysql_stmt_result_metadata(stmt);
                m_field_count = NativeMethods.mysql_num_fields(result_metadata);
                NativeMethods.mysql_free_result(result_metadata);
            }
            if (NativeMethods.mysql_stmt_execute(stmt) != 0)
            {
                throw new MySQLException(NativeMethods.mysql_stmt_errno(stmt), "MySQLDriverCS Error: wrong query." + NativeMethods.mysql_stmt_error(stmt));
            }
            else
            {
                MySQLDataReader dr = MySQLCursorDataReader.GetInstance(m_field_count, stmt, m_parameters, this.connection, this, CloseConnection);
                return dr;
            }
        }
        internal override int ExecuteCall()
        {
            return 0;
        }

        #endregion
    }

	internal unsafe class PreparedStatement32:PreparedStatement
	{
        MYSQL_BIND_32[] m_bindparms;

		internal PreparedStatement32(MySQLConnection connection, string query)
		{
			this.connection=connection;
			this.query = query;
			stmt = NativeMethods.mysql_stmt_init(connection.handle);
			if (stmt == IntPtr.Zero)
				throw new MySQLException(NativeMethods.mysql_errno(connection.handle), "MySQLDriverCS Error: wrong cursor type."+NativeMethods.mysql_error(connection.handle));
			prepared=false;
			m_parm_count=-1;
			m_fetch_size=1;
			m_field_count=0;
		}

        internal override void Dispose()
        {
            if (m_bindparms != null)
            {
				for (int i = 0; i < m_bindparms.Length; i++)
                {
					m_bindparms[i].Dispose();
                }
            }
            NativeMethods.mysql_stmt_close(stmt);
            stmt = IntPtr.Zero;
        }

         public override void BindParameters()
        {
            if (m_parm_count == -1)
            {
                m_parm_count = (int)NativeMethods.mysql_stmt_param_count(stmt);
            }
            if (m_parm_count != m_parameters.Count)
            {
                throw new MySQLException("Invalid parameters, stmt parameters:" + m_parm_count + " parameters count:" + m_parameters.Count);
            }

            if (m_bindparms != null && m_bindparms.Length != m_parameters.Count)
            {
				for (int j = 0; j < m_bindparms.Length; j++)
                {
					m_bindparms[j].Dispose();
                }
                m_bindparms = null;
            }

            if (m_bindparms == null)
            {
                m_bindparms = new MYSQL_BIND_32[m_parameters.Count];
                for (int i = 0; i < m_parameters.Count; i++)
                {
                    m_bindparms[i] = new MYSQL_BIND_32();
                }
            }
			for (int i = 0; i < m_parameters.Count; i++)
            {
				MySQLParameter param = (MySQLParameter)m_parameters[i];
                m_bindparms[i].Type = DbtoMysqlType(param.DbType);
                m_bindparms[i].Value = param.Value;
                if (param.Value != null && param.Value is string)
                {
                    m_bindparms[i].Length = (uint)connection.CharacterEncoding.GetBytes((string)param.Value).Length; //si es string
                }
                else
                {
                    m_bindparms[i].Length = (uint)param.Size;
                }
				m_bindparms[i].IsNull = param.Value == null || param.Value == DBNull.Value;
            }
            int code = NativeMethods.mysql_stmt_bind_param32(stmt, m_bindparms);
            if (code != 0)
                throw new MySQLException(NativeMethods.mysql_stmt_errno(stmt), "MySQLDriverCS Error: wrong parameters." + NativeMethods.mysql_stmt_error(stmt));
        }

    }
    internal unsafe class PreparedStatement64 : PreparedStatement
    {
        MYSQL_BIND_64[] m_bindparms;

        internal PreparedStatement64(MySQLConnection connection, string query)
        {
            this.connection = connection;
            this.query = query;
            stmt = NativeMethods.mysql_stmt_init(connection.handle);
            if (stmt == IntPtr.Zero)
                throw new MySQLException(NativeMethods.mysql_errno(connection.handle), "MySQLDriverCS Error: wrong cursor type." + NativeMethods.mysql_error(connection.handle));
            prepared = false;
            m_parm_count = -1;
            m_fetch_size = 1;
            m_field_count = 0;
        }

        internal override void Dispose()
        {
            if (m_bindparms != null)
            {
				for (int i = 0; i < m_bindparms.Length; i++)
                {
					m_bindparms[i].Dispose();
                }
            }
            NativeMethods.mysql_stmt_close(stmt);
            stmt = IntPtr.Zero;
        }

        public override void BindParameters()
        {
            if (m_parm_count == -1)
            {
                m_parm_count = (int)NativeMethods.mysql_stmt_param_count(stmt);
            }
            if (m_parm_count != m_parameters.Count)
            {
                throw new MySQLException("Invalid parameters, stmt parameters:" + m_parm_count + " parameters count:" + m_parameters.Count);
            }

            if (m_bindparms != null && m_bindparms.Length != m_parameters.Count)
            {
				for (int j = 0; j < m_bindparms.Length; j++)
                {
					m_bindparms[j].Dispose();
                }
                m_bindparms = null;
            }

            if (m_bindparms == null)
            {
                m_bindparms = new MYSQL_BIND_64[m_parameters.Count];
                for (int i = 0; i < m_parameters.Count; i++)
                {
                    m_bindparms[i] = new MYSQL_BIND_64();
                }
            }

			for (int i = 0; i < m_parameters.Count; i++)
            {
				MySQLParameter param = (MySQLParameter)m_parameters[i];
                m_bindparms[i].Type = DbtoMysqlType(param.DbType);
                m_bindparms[i].Value = param.Value;
                m_bindparms[i].IsNull = param.Value == null || param.Value == DBNull.Value;
                if (param.Value != null && param.Value is string)
                {
                    m_bindparms[i].Length = (uint)connection.CharacterEncoding.GetBytes((string)param.Value).Length; //si es string
                }
                else
                {
                    m_bindparms[i].Length = (uint)param.Size;
                }
            }
            int code = NativeMethods.mysql_stmt_bind_param64(stmt, m_bindparms);
            if (code != 0)
                throw new MySQLException(NativeMethods.mysql_stmt_errno(stmt), "MySQLDriverCS Error: wrong parameters." + NativeMethods.mysql_stmt_error(stmt));
        }

    }
}
