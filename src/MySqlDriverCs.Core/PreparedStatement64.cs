using System;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    internal class PreparedStatement64 : PreparedStatementBase
    {
        MYSQL_BIND_64[] m_bindparms;

        internal PreparedStatement64(MySQLConnection connection, string query):base(connection)
        {
            this.connection = connection;
            this.query = query;

            prepared = false;
            m_parm_count = -1;
            m_fetch_size = 1;
            m_field_count = 0;
        }

        public override void Dispose()
        {
            if (m_bindparms != null)
            {
                for (int i = 0; i < m_bindparms.Length; i++)
                {
                    m_bindparms[i].Dispose();
                }
            }
            base.Dispose();
        }

        public override void BindParameters()
        {
            if (m_parm_count == -1)
            {
                m_parm_count = (int)stmt.mysql_stmt_param_count();
            }
            if (m_parm_count != m_parameters.Count)
            {
                throw new MySqlException("Invalid parameters, stmt parameters:" + m_parm_count + " parameters count:" + m_parameters.Count);
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
            int code = stmt.mysql_stmt_bind_param64(m_bindparms);
            if (code != 0)
                throw new MySqlException(stmt);
        }

    }
}