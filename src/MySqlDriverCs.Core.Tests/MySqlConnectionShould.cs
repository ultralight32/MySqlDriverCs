using System.Data;
using MySQLDriverCS;
using Xunit;

namespace MySqlDriverCs.Core.Tests
{
    public class MySqlConnectionShould
    {
        [Fact]
        public void OpenAndDispose()
        {
            using (var c = new MySQLConnection("Data Source=mysql;Location=localhost;User ID=root;Password=Root-99;Client Path=C:\\Program Files\\MySQL\\MySQL Connector C 6.1\\lib"))
            {
                Assert.True(c.State==ConnectionState.Closed);
                c.Open();
                Assert.True(c.State == ConnectionState.Open);
            }
        }

        [Fact]
        public void OpenAndClose()
        {
            using (var c = new MySQLConnection("Data Source=mysql;Location=localhost;User ID=root;Password=Root-99;Client Path=C:\\Program Files\\MySQL\\MySQL Connector C 6.1\\lib"))
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
            using (var c = new MySQLConnection("Data Source=mysql;Location=localhost;User ID=root;Password=Root-99;Client Path=C:\\Program Files\\MySQL\\MySQL Connector C 6.1\\lib"))
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