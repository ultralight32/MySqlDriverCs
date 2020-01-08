using System;
using System.Data;
using System.Runtime.InteropServices;
using Microsoft.DotNet.PlatformAbstractions;
using MySQLDriverCS;
using Xunit;

namespace MySqlDriverCs.Core.Tests
{
    public class MySqlConnectionShould
    {
        private static string ConnectionString
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "Data Source=mysql;Location=localhost;User ID=root;Password=Root-99;Client Path=C:\\Program Files\\MySQL\\MySQL Connector C 6.1\\lib";
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

                using (var cmd = new MySQLCommand("select now()", c))
                {
                    var time = cmd.ExecuteScalar();
                }
            }
        }
    }
}