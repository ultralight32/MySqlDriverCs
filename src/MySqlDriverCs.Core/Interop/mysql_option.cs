namespace MySQLDriverCS.Interop
{
    public enum mysql_option:int
    {
        MYSQL_OPT_CONNECT_TIMEOUT = 0, MYSQL_OPT_COMPRESS = 1, MYSQL_OPT_NAMED_PIPE = 2,
        MYSQL_INIT_COMMAND = 3, MYSQL_READ_DEFAULT_FILE = 4, MYSQL_READ_DEFAULT_GROUP = 5,
        MYSQL_SET_CHARSET_DIR = 6, MYSQL_SET_CHARSET_NAME = 7, MYSQL_OPT_LOCAL_INFILE = 8,
        MYSQL_OPT_PROTOCOL = 9, MYSQL_SHARED_MEMORY_BASE_NAME = 10, MYSQL_OPT_READ_TIMEOUT = 11,
        MYSQL_OPT_WRITE_TIMEOUT = 12, MYSQL_OPT_USE_RESULT = 13,
        MYSQL_OPT_USE_REMOTE_CONNECTION = 14, MYSQL_OPT_USE_EMBEDDED_CONNECTION = 15,
        MYSQL_OPT_GUESS_CONNECTION = 16, MYSQL_SET_CLIENT_IP = 17, MYSQL_SECURE_AUTH = 18,
        MYSQL_REPORT_DATA_TRUNCATION = 19, MYSQL_OPT_RECONNECT = 20,
        MYSQL_OPT_SSL_VERIFY_SERVER_CERT = 21, MYSQL_PLUGIN_DIR = 22, MYSQL_DEFAULT_AUTH = 23,
        MYSQL_OPT_BIND = 24,
        MYSQL_OPT_SSL_KEY = 25, MYSQL_OPT_SSL_CERT = 26,
        MYSQL_OPT_SSL_CA = 27, MYSQL_OPT_SSL_CAPATH = 28, MYSQL_OPT_SSL_CIPHER = 29,
        MYSQL_OPT_SSL_CRL = 30, MYSQL_OPT_SSL_CRLPATH = 31,
        MYSQL_OPT_CONNECT_ATTR_RESET = 32, MYSQL_OPT_CONNECT_ATTR_ADD = 33,
        MYSQL_OPT_CONNECT_ATTR_DELETE = 34,
        MYSQL_SERVER_PUBLIC_KEY = 35,
        MYSQL_ENABLE_CLEARTEXT_PLUGIN = 36,
        MYSQL_OPT_CAN_HANDLE_EXPIRED_PASSWORDS = 37,
        MYSQL_OPT_SSL_ENFORCE = 38,
        MYSQL_OPT_MAX_ALLOWED_PACKET = 39, MYSQL_OPT_NET_BUFFER_LENGTH = 40,
        MYSQL_OPT_TLS_VERSION = 41,
        MYSQL_OPT_SSL_MODE = 42
    };
}