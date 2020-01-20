using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MySQLDriverCS.Interop
{
    public class NativeProxyFactory
    {
        public static INativeProxy GetProxy(INativeTracer nativeTracer)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new NativeImportsWin32(nativeTracer);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new NativeImportsLinux(nativeTracer);
            else throw new NotSupportedException();
        }
    }
}
