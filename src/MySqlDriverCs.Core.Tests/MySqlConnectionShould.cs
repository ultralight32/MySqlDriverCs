using MySQLDriverCS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace MySqlDriverCs.Core.Tests
{
    public class MySqlCommandShould : BaseTest
    {
        public MySqlCommandShould(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ExecuteNonQuery()
        {
            using (var c = new MySQLConnection(ConnectionString))
            {
                c.Open();

                DropTableNumberTestType(c);

                CreateTableNumberTestType(c);
            }
        }

        public class CalculatorTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "TINYINT", "1", true };
                yield return new object[] { "TINYINT UNSIGNED", byte.MaxValue.ToString(CultureInfo.InvariantCulture),byte.MaxValue };
                yield return new object[] { "SMALLINT", "-32768",short.MinValue};
                yield return new object[] { "SMALLINT UNSIGNED", "65535", ushort.MaxValue };
                yield return new object[] { "MEDIUMINT", "-8388608", (int)-8388608 };
                yield return new object[] { "MEDIUMINT UNSIGNED", "16777215", (uint)16777215 };
                yield return new object[] { "INT", "-2147483648", int.MinValue };
                yield return new object[] { "INT UNSIGNED", "4294967295", uint.MaxValue};
                yield return new object[] { "BIGINT", "-9223372036854775808", long.MinValue };
                yield return new object[] { "BIGINT UNSIGNED", "18446744073709551615", ulong.MaxValue };
                yield return new object[] { "DECIMAL(18,6)", "-345435.567894", -345435.567894m };
                yield return new object[] { "DECIMAL(18,6) UNSIGNED", "345435.567894", 345435.567894m };
                yield return new object[] { "FLOAT", "1.25e-5", 1.25e-5f };
                yield return new object[] { "DOUBLE", "345435.567894", 345435.567894d };
                yield return new object[] { "BIT(2)", "B'01'", ((ulong)1),  };

                yield return new object[] { "TINYINT", "NULL", Convert.DBNull };
                yield return new object[] { "TINYINT UNSIGNED", "NULL", Convert.DBNull };
                yield return new object[] { "SMALLINT", "NULL", Convert.DBNull };
                yield return new object[] { "SMALLINT UNSIGNED", "NULL", Convert.DBNull };
                yield return new object[] { "MEDIUMINT", "NULL", Convert.DBNull };
                yield return new object[] { "MEDIUMINT UNSIGNED", "NULL", Convert.DBNull };
                yield return new object[] { "INT", "NULL", Convert.DBNull };
                yield return new object[] { "INT UNSIGNED", "NULL", Convert.DBNull };
                yield return new object[] { "BIGINT", "NULL", Convert.DBNull };
                yield return new object[] { "BIGINT UNSIGNED", "NULL", Convert.DBNull };
                yield return new object[] { "DECIMAL(18,6)", "NULL", Convert.DBNull };
                yield return new object[] { "DECIMAL(18,6) UNSIGNED", "NULL", Convert.DBNull };
                yield return new object[] { "FLOAT", "NULL", Convert.DBNull };
                yield return new object[] { "DOUBLE", "NULL", Convert.DBNull };
                yield return new object[] { "BIT(2)", "NULL", Convert.DBNull };
            }


            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [Theory]
        [ClassData(typeof(CalculatorTestData))]
        public void ExecuteScalarForNumericTypes(string mySqlTypeDeclaration, string insertedLiteral, object expectedValue)
        {
            using (var c = new MySQLConnection(ConnectionString))
            {
                c.Open();

                using (var cmd1 = new MySQLCommand("DROP TABLE IF EXISTS number_type_test", c))
                    cmd1.ExecuteNonQuery();

                using (var cmd2 = new MySQLCommand($@"CREATE TABLE number_type_test ( id INT NOT NULL,COL_VALUE {mySqlTypeDeclaration},PRIMARY KEY (id));", c))
                    cmd2.ExecuteNonQuery();

                using (var cmd3 = new MySQLCommand($@"INSERT INTO number_type_test ( id ,COL_VALUE)values(0,{insertedLiteral});", c)) cmd3.ExecuteNonQuery();

                using (var cmd = new MySQLCommand("select COL_VALUE from number_type_test where id=0", c))
                {
                    object executeScalar = cmd.ExecuteScalar();
                    Assert.NotNull(executeScalar);
                    if (expectedValue == null)
                        Assert.Null(executeScalar);
                    else
                    {
                        Assert.NotNull(executeScalar);
                        Assert.IsType(expectedValue.GetType(), executeScalar);
                        Assert.Equal(expectedValue, executeScalar);
                    }
                }
            }
        }

        private static void InsertTableNumberTestType(MySQLConnection c)
        {
            using (var cmd = new MySQLCommand(@"
INSERT INTO number_type_test
(
id ,

COL_TINYINT  ,
COL_U_TINYINT,

COL_SMALLINT  ,
COL_MEDIUMINT   ,
COL_INT  ,
COL_BIGINT  ,
COL_DECIMAL  ,
COL_FLOAT  ,
COL_DOUBLE  ,

COL_U_SMALLINT  ,
COL_U_MEDIUMINT   ,
COL_U_INT  ,
COL_U_BIGINT  ,
COL_U_DECIMAL  ,

COL_BIT
)values(
0,

1,
255,

-32768,
-8388608,
-2147483648,
-9223372036854775808,
-345435.567894,
'12.2',
'12.2',

65535,
16777215,
4294967295,
18446744073709551615,
345435.567894,

B'01'
);
", c))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void DropTableNumberTestType(MySQLConnection c)
        {
            using (MySQLCommand cmd = new MySQLCommand("DROP TABLE IF EXISTS number_type_test", c))
                cmd.ExecuteNonQuery();
        }

        private static void CreateTableNumberTestType(MySQLConnection c)
        {
            using (var cmd = new MySQLCommand(@"
CREATE TABLE number_type_test
(
id INT NOT NULL,
COL_TINYINT TINYINT ,
COL_U_TINYINT  TINYINT unsigned,

COL_SMALLINT SMALLINT ,
COL_MEDIUMINT  MEDIUMINT ,
COL_INT INT ,
COL_BIGINT BIGINT ,
COL_DECIMAL DECIMAL(18,6) ,
COL_FLOAT FLOAT ,
COL_DOUBLE DOUBLE ,

COL_U_SMALLINT SMALLINT UNSIGNED,
COL_U_MEDIUMINT  MEDIUMINT UNSIGNED,
COL_U_INT INT UNSIGNED,
COL_U_BIGINT  BIGINT UNSIGNED,
COL_U_DECIMAL  DECIMAL UNSIGNED,

COL_BIT BIT(2) ,
PRIMARY KEY (id));
", c))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class BaseTest
    {
        public BaseTest(ITestOutputHelper testOutputHelper)
        {
            MySQLConnection.SetDefaultNativeTracer(new LambdaNativeTracer(testOutputHelper.WriteLine));
        }

        protected static string ConnectionString
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "Data Source=mysql;Location=localhost;User ID=test;Password=password;Client Path=C:\\Program Files\\MySQL\\MySQL Connector C 6.1\\lib";
                else
                    return Environment.GetEnvironmentVariable("CONNECTION_STRING");
            }
        }
    }

    public class MySqlConnectionShould : BaseTest
    {
        public MySqlConnectionShould(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void OpenAndDispose()
        {
            using (var c = new MySQLConnection(ConnectionString))
            {
                Assert.True(c.State == ConnectionState.Closed);
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