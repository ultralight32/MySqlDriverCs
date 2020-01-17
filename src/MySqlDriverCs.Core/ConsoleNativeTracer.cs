using System;
using MySqlDriverCs.Interop;
using MySQLDriverCS.Interop;

namespace MySQLDriverCS
{
    public class ConsoleNativeTracer : INativeTracer
    {
        public void Trace(string line)
        {
         
            Console.WriteLine(line);
        }
    }

    public class LambdaNativeTracer : INativeTracer
    {
        private readonly Action<string> _lambda;

        public LambdaNativeTracer(Action<string> lambda)
        {
            _lambda = lambda ?? throw new ArgumentNullException(nameof(lambda));
        }
        public void Trace(string line)
        {

            _lambda(line);
        }
    }
}