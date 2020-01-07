using MySQLDriverCS;
using System;
using System.Data;
using MySQLDriverCS.Interop;
using Xunit;

namespace MySqlDriverCs.Core.Tests
{
    public unsafe class NativeDllMustBeFound
    {

        [Fact]
        public unsafe void Test()
        {
Console.WriteLine("LD_LIBRARY_PATH: "+Environment.GetEnvironmentVariable("LD_LIBRARY_PATH"));

            Native native = new Native();
            var ptr = Native.mysql_init(null);
        }
    }
    public class PreparedStatementVsSqlInjection
    {
        private const int count = 10;

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldWorkWithoutDifference(bool usePrepared)
        {
            long t1, t2, t3, t4, t5, t6;
            int affectedRows;

            MySQLConnection con = new MySQLConnection("Data Source=mysql;Location=localhost;User ID=root;Password=Root-99");
            bool usenew = usePrepared, useold = !usePrepared;

            this.CreateTable(con);
            this.Delete(con);

            //--------------------------------Direct statements-------------------
            if (useold)
            {
                t1 = DateTime.Now.Ticks;
                affectedRows = this.InsertTest(con, false);
                t2 = DateTime.Now.Ticks;
                PrintTimeDiff("Insert of " + affectedRows + " rows with DIRECT statement", t2 - t1);

                affectedRows = this.TestSelect(con, false);
                t5 = DateTime.Now.Ticks;
                PrintTimeDiff("Select of " + affectedRows + " rows with DIRECT statement", t5 - t2);
            }

            //--------------------------------Prepared statements-------------------
            if (usenew)
            {
                t3 = DateTime.Now.Ticks;
                affectedRows = this.InsertTest(con, true);
                t4 = DateTime.Now.Ticks;
                PrintTimeDiff("Insert of " + affectedRows + " rows with PREPARED statement", t4 - t3);

                affectedRows = this.TestSelect(con, true);
                t6 = DateTime.Now.Ticks;
                PrintTimeDiff("Select of " + affectedRows + " rows with PREPARED statement", t6 - t4);
            }
        }

        private static void PrintTimeDiff(string msg, long diff)
        {
            Console.WriteLine("Total Time " + msg + ":" + TimeSpan.FromTicks(diff).Minutes + ":" + TimeSpan.FromTicks(diff).Seconds + ":" + TimeSpan.FromTicks(diff).Milliseconds);
        }

        private void CreateTable(MySQLConnection con)
        {
            try
            {
                con.Open();
                MySQLCommand cmd = new MySQLCommand("DROP TABLE IF EXISTS test_table", con);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = new MySQLCommand("CREATE TABLE test_table(col1 INT,col2 VARCHAR(40),col3 SMALLINT,col4 TIMESTAMP)", con);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception e)
            {
                if (con != null) con.Close();
                throw e;
            }
        }

        private int InsertTest(MySQLConnection con, bool useNew)
        {
            try
            {
                con.Open();

                MySQLCommand cmd;
                if (useNew)
                {
                    cmd = new MySQLCommand("INSERT INTO `test_table` (col1, col2, col3, col4) values (?,?,?,?)", con);//, `SerialNumberLastUsed`
                    cmd.UsePreparedStatement = true;
                    cmd.ServerCursor = false;
                    cmd.Prepare();
                }
                else
                {
                    cmd = new MySQLCommand("INSERT INTO `test_table`(col1, col2, col3, col4) values (@col1, @col2, @col3, @col4)", con);//, `SerialNumberLastUsed`
                }

                MySQLParameter p1 = new MySQLParameter("@col1", DbType.Int32);
                cmd.Parameters.Add(p1);
                MySQLParameter p2 = new MySQLParameter("@col2", DbType.String);
                cmd.Parameters.Add(p2);
                MySQLParameter p3 = new MySQLParameter("@col1", DbType.Int16);
                cmd.Parameters.Add(p3);
                MySQLParameter p4 = new MySQLParameter("@col4", DbType.DateTime);
                cmd.Parameters.Add(p4);
                int affectedrows = 0;
                for (int i = 0; i < count; i++)
                {
                    p4.Value = DateTime.Now;
                    p1.Value = i;
                    p2.Value = "Registro n�mero " + i;
                    p2.Size = p2.Value.ToString().Length;
                    p3.Value = i * 10;
                    affectedrows += cmd.ExecuteNonQuery();
                }
                cmd.Dispose();
                con.Close();
                return affectedrows;
            }
            catch (Exception e)
            {
                if (con != null) con.Close();
                throw e;
            }
        }

        private void Delete(MySQLConnection con)
        {
            try
            {
                con.Open();

                MySQLCommand cmd = new MySQLCommand("DELETE FROM `test_table`", con);//, `SerialNumberLastUsed`
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception e)
            {
                if (con != null) con.Close();
                throw e;
            }
        }

        private int TestSelect(MySQLConnection con, bool useNew)
        {
            int affectedrows = 0;
            try
            {
                con.Open();

                MySQLCommand cmd = new MySQLCommand("SELECT col1, col2, col3, col4 FROM `test_table`", con);
                if (useNew)
                {
                    cmd.UsePreparedStatement = true;
                    cmd.ServerCursor = true;
                    cmd.Prepare();
                }
                IDataReader reader = cmd.ExecuteReader();
                MySQLCommand cmd1 = new MySQLCommand("SELECT col1, col2, col3, col4 FROM `test_table`", con);
                if (useNew)
                {
                    cmd1.UsePreparedStatement = true;
                    //cmd.ServerCursor = true;
                    cmd.FetchSize = 10;
                    cmd1.Prepare();
                }
                while (reader.Read())
                {
                    affectedrows++;
                    IDataReader reader1 = cmd1.ExecuteReader();
                    while (reader1.Read())
                    {
                        affectedrows++;
                        long s1 = reader1.GetInt64(0);
                        string st1 = reader1.GetString(1);
                        DateTime dt1 = reader1.GetDateTime(3);
                    }
                    reader1.Close();

                    long s = reader.GetInt64(0);
                    string st = reader.GetString(1);
                    DateTime dt = reader.GetDateTime(3);
                }
                reader.Close();
                cmd.Dispose();
                con.Close();
                return affectedrows;
            }
            catch (Exception e)
            {
                if (con != null) con.Close();
                throw e;
            }
        }

    }
}