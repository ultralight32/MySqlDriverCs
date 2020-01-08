using System;
using System.Runtime.InteropServices;

namespace MySQLDriverCS.Interop
{
    public class NativeResultMetadata : NativeResult
    {
       
        public NativeResultMetadata(NativeStatement nativeStatement):base(mysql_stmt_result_metadata(nativeStatement.stmt))
        {

        }

        [DllImport("libmysqlclient.so", EntryPoint = "mysql_stmt_result_metadata")]
        public static extern IntPtr mysql_stmt_result_metadata_linux(IntPtr stmt);
        [DllImport("libmysql.dll", EntryPoint = "mysql_stmt_result_metadata")]
        public static extern IntPtr mysql_stmt_result_metadata_win32(IntPtr stmt);

        static IntPtr mysql_stmt_result_metadata(IntPtr stmt)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return mysql_stmt_result_metadata_win32(stmt);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return mysql_stmt_result_metadata_linux(stmt);
            else throw new NotSupportedException();
        }

    }
}