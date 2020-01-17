using System;
using System.Data;
using System.Runtime.InteropServices;
using Microsoft.DotNet.PlatformAbstractions;
using MySQLDriverCS;
using Xunit;
using Xunit.Abstractions;

namespace MySqlDriverCs.Core.Tests
{
    public class MySqlConnectionShould
    {
        public MySqlConnectionShould(ITestOutputHelper  testOutputHelper)
        {
            MySQLConnection.SetDefaultNativeTracer(new LambdaNativeTracer(x => testOutputHelper.WriteLine(x)));
        }
        private static string ConnectionString
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "Data Source=mysql;Location=localhost;User ID=test;Password=password;Client Path=C:\\Program Files\\MySQL\\MySQL Connector C 6.1\\lib";
                else
                    return Environment.GetEnvironmentVariable("CONNECTION_STRING");
            }
        }

        [Fact]
        public void OpenAndDispose()
        {

            using (var c = new MySQLConnection(ConnectionString))
            {
                Assert.True(c.State==ConnectionState.Closed);
                c.Open();
                Assert.True(c.State == ConnectionState.Open);
            }
        }

        [Fact]
        public void OpenAndClose()
        {
            using (var c = new MySQLConnection(ConnectionString))
            {
                Assert.True(c.State == ConnectionState.Closed);
                c.Open();
                Assert.True(c.State == ConnectionState.Open);
                c.Close();
                Assert.True(c.State == ConnectionState.Closed);
            }
        }

        [Fact]
        public void SelectFunction()
        {
            using (var c = new MySQLConnection(ConnectionString))
            {

    
                c.Open();

                using (var cmd = new MySQLCommand("select now();", c))
                {
                    var time = cmd.ExecuteScalar();
                }
            }
        }
    }
}