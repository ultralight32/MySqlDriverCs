using System;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS.Interop
{
    public interface INativeProxy
    {
        void mysql_close(IntPtr handle);
        int mysql_options(IntPtr mysql, mysql_option option, ref uint value);
        int mysql_server_init(int argc, IntPtr argv, IntPtr groups);
        IntPtr mysql_init(IntPtr must_be_null);
        int mysql_ping(IntPtr mysql);
        int mysql_query(IntPtr mysql, string query);
        IntPtr mysql_real_connect(IntPtr mysql, string host, string user, string passwd, string db, uint port, string unix_socket, int client_flag);
        int mysql_select_db(IntPtr mysql, string dbname);
        int mysql_set_character_set(IntPtr mysql, string csname);
        uint mysql_real_escape_string(IntPtr mysql, System.Text.StringBuilder to, string from, uint length);
        IntPtr mysql_store_result(IntPtr mysql);
        uint mysql_errno(IntPtr mysql);
        IntPtr mysql_error_native(IntPtr mysql);
        uint mysql_field_count(IntPtr mysql);
        uint mysql_affected_rows(IntPtr mysql);
        IntPtr mysql_get_client_info();
       // string get_charset_name(uint charset_number);
       MY_CHARSET_INFO mysql_get_character_set_info(IntPtr mysql);
    }
}