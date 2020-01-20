using MySQLDriverCS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MySqlDriverCs.Core.Tests
{
    public class MySqlCommandShould : BaseTest, IDisposable
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

        public class FieldTypeTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                foreach (var tuple in DataTypes())
                {

                    // with value
                    yield return tuple;

                    // with null
                    tuple[1] = "NULL";
                    tuple[2] = Convert.DBNull;
                    yield return tuple;
                }

            }

            private IEnumerable<object[]> DataTypes()
            {
                // numerics
                yield return new object[] { "TINYINT", "1", true };
                yield return new object[] { "TINYINT UNSIGNED", byte.MaxValue.ToString(CultureInfo.InvariantCulture), byte.MaxValue };
                yield return new object[] { "SMALLINT", "-32768", short.MinValue };
                yield return new object[] { "SMALLINT UNSIGNED", "65535", ushort.MaxValue };
                yield return new object[] { "MEDIUMINT", "-8388608", (int)-8388608 };
                yield return new object[] { "MEDIUMINT UNSIGNED", "16777215", (uint)16777215 };
                yield return new object[] { "INT", "-2147483648", int.MinValue };
                yield return new object[] { "INT UNSIGNED", "4294967295", uint.MaxValue };
                yield return new object[] { "BIGINT", "-9223372036854775808", long.MinValue };
                yield return new object[] { "BIGINT UNSIGNED", "18446744073709551615", ulong.MaxValue };
                yield return new object[] { "DECIMAL(18,6)", "-345435.567894", -345435.567894m };
                yield return new object[] { "DECIMAL(18,6) UNSIGNED", "345435.567894", 345435.567894m };
                yield return new object[] { "FLOAT", "1.25e-5", 1.25e-5f };
                yield return new object[] { "DOUBLE", "345435.567894", 345435.567894d };
                yield return new object[] { "BIT(2)", "B'01'", ((ulong)1), };

                // time
                yield return new object[] { "DATE", "'0001-01-01'", new DateTime(1, 1, 1), };
                yield return new object[] { "TIME", "'13:40:45'", new DateTime(1, 1, 1, 13, 40, 45) };
                yield return new object[] { "DATETIME", "'2020-12-11 13:40:45'", new DateTime(2020, 12, 11, 13, 40, 45) };
                yield return new object[] { "TIMESTAMP", "'2020-12-11 13:40:45'", new DateTime(2020, 12, 11, 13, 40, 45) };
                yield return new object[] { "YEAR", "2020", new DateTime(2020, 1, 1) };

                // varchars
                yield return new object[] { "VARCHAR(20) CHARACTER SET utf8", "'some text'", "some text" };
                yield return new object[] { "CHAR(20) CHARACTER SET utf8", "'some text'", "some text"  };
                yield return new object[] { "TEXT CHARACTER SET utf8", "'some text'", "some text" };
                yield return new object[] { "TINYTEXT CHARACTER SET utf8", "'some text'", "some text" };
                yield return new object[] { "MEDIUMTEXT CHARACTER SET utf8", "'some text'", "some text" };
                yield return new object[] { "LONGTEXT CHARACTER SET utf8", "'some text'", "some text" };
                yield return new object[] { "BINARY(20)", "'some text'", "some text" };
                yield return new object[] { "VARBINARY(20)", "'some text'", "some text" };

                // enums & sets
                yield return new object[] { "ENUM('a','b','c') CHARACTER SET binary", "'a'", "a" };
                yield return new object[] { "SET('a', 'b', 'c', 'd')", "'d,a,d'", "a,d" };

                // blobs
                yield return new object[] { "BLOB", "X'01AF'", new byte[]{0x01,0xaf} };
                yield return new object[] { "TINYBLOB", "X'01AF'", new byte[] { 0x01, 0xaf } };
                yield return new object[] { "MEDIUMBLOB", "X'01AF'", new byte[] { 0x01, 0xaf } };
                yield return new object[] { "LONGBLOB", "X'01AF'", new byte[] { 0x01, 0xaf } };

                // geometry
                //yield return new object[] { "POINT", "POINT(1,1)", new MySqlPoint(1,1), };
                //yield return new object[] { "LINESTRING", "ST_LineStringFromText('LINESTRING(0 0,1 1,2 2)')", new MySqlLineString(new MySqlPoint(0,0), new MySqlPoint(1, 1), new MySqlPoint(2, 2)), };
                //yield return new object[] { "POLYGON", "ST_PolygonFromText('POLYGON((0 0,10 0,10 10,0 10,0 0),(5 5,7 5,7 7,5 7, 5 5))')", new MySqlPolygon(new MySqlLineString(new MySqlPoint(0,0), new MySqlPoint(10,0), new MySqlPoint(10,10), new MySqlPoint(0,10), new MySqlPoint(0,0)), new MySqlLineString(new MySqlPoint(5,5), new MySqlPoint(7,5), new MySqlPoint(7,7), new MySqlPoint(5,7), new MySqlPoint(5,5))),  };
                //yield return new object[] { "GEOMETRYCOLLECTION", "ST_GeomCollFromText('GEOMETRYCOLLECTION(POINT(1 1),LINESTRING(0 0,1 1,2 2,3 3,4 4))')", new MySqlGeometryCollection(new MySqlPoint(1,1), new MySqlLineString(new MySqlPoint(0,0), new MySqlPoint(1,1), new MySqlPoint(2,2), new MySqlPoint(3,3), new MySqlPoint(4,4))), };

                // json
                yield return new object[] { "JSON", "'{\"key1\": \"value1\", \"key2\": \"value2\"}'", "{\"key1\": \"value1\", \"key2\": \"value2\"}" };
            }


            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private MySQLConnection _cachedConnection = null;

        private MySQLConnection CachedConnection
        {
            get
            {
                if (_cachedConnection == null)
                {
                    var cc = new MySQLConnection(ConnectionString);
                    cc.Open();
                    _cachedConnection = cc;
                }

                return _cachedConnection;
            }
        }

        [Theory]
        [ClassData(typeof(FieldTypeTestData))]
        public void ExecuteScalarForAnyFieldType(string mySqlTypeDeclaration, string insertedLiteral, object expectedValue)
        {
           

            var c = CachedConnection;

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

    

        [Theory]
        [ClassData(typeof(FieldTypeTestData))]
        public void ExecuteDataReaderForAnyFieldType(string mySqlTypeDeclaration, string insertedLiteral, object expectedValue)
        {
            var c = CachedConnection;
            {
               

                using (var cmd1 = new MySQLCommand("DROP TABLE IF EXISTS number_type_test", c))
                    cmd1.ExecuteNonQuery();

                using (var cmd2 = new MySQLCommand($@"CREATE TABLE number_type_test ( id INT NOT NULL,COL_VALUE {mySqlTypeDeclaration},PRIMARY KEY (id));", c))
                    cmd2.ExecuteNonQuery();

                using (var cmd3 = new MySQLCommand($@"INSERT INTO number_type_test ( id ,COL_VALUE)values(0,{insertedLiteral});", c)) cmd3.ExecuteNonQuery();

                using (var cmd = new MySQLCommand("select id, COL_VALUE from number_type_test where id=0", c))
                {
                    var reader = cmd.ExecuteReader();
                    Assert.True(reader.Read());

                    {
                        var ordinal = reader.GetOrdinal("COL_VALUE");
                        var executeScalar = reader.GetValue(ordinal);

                        Assert.NotNull(executeScalar);
                        if (expectedValue == null)
                            Assert.Null(executeScalar);
                        else
                        {
                            Assert.NotNull(executeScalar);
                            Assert.IsType(expectedValue.GetType(), executeScalar);
                            if (expectedValue is string)
                                expectedValue = "'" + expectedValue.ToString() + "'";
                            if (executeScalar is string)
                                executeScalar = "'" + executeScalar.ToString() + "'";
                            Assert.Equal(expectedValue, executeScalar);
                        }
                    }

                    Assert.False(reader.Read());


                }
            }
        }

        public class PreparedFieldTypeTest
        {
            public string Declaration { get; }
            public MySQLParameter Parameter { get; }
            public object Value { get; }

            public PreparedFieldTypeTest(string declaration, MySQLParameter parameter, object value)
            {
                Declaration = declaration;
                Parameter = parameter;
                Value = value;
            }

            public override string ToString()
            {
                return Declaration;
            }
        }
        public class PreparedFieldTypeTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                foreach (var tuple in DataTypes())
                {

                    // with value
                    yield return new object[]{tuple};

                }

            }

            private IEnumerable<PreparedFieldTypeTest> DataTypes()
            {
                // numerics
                yield return new PreparedFieldTypeTest("TINYINT", new MySQLParameter("value", MySqlDbType.Byte, ParameterDirection.Input, (sbyte)-77), (sbyte)-77);
                yield return new PreparedFieldTypeTest( "TINYINT(1)", new MySQLParameter("value",MySqlDbType.Boolean,ParameterDirection.Input,true),  true );
                yield return new PreparedFieldTypeTest( "TINYINT UNSIGNED", new MySQLParameter("value", MySqlDbType.Byte, ParameterDirection.Input, byte.MaxValue), byte.MaxValue );
                yield return new PreparedFieldTypeTest( "SMALLINT", new MySQLParameter("value", MySqlDbType.Short, ParameterDirection.Input, short.MinValue), short.MinValue );
                yield return new PreparedFieldTypeTest( "SMALLINT UNSIGNED", new MySQLParameter("value", MySqlDbType.Short, ParameterDirection.Input, ushort.MaxValue), ushort.MaxValue );
                yield return new PreparedFieldTypeTest( "MEDIUMINT", new MySQLParameter("value", MySqlDbType.Int, ParameterDirection.Input, 16777215/2), 16777215/2 );
                yield return new PreparedFieldTypeTest( "MEDIUMINT UNSIGNED", new MySQLParameter("value", MySqlDbType.Int, ParameterDirection.Input, (uint)16777215), (uint)16777215 );
                yield return new PreparedFieldTypeTest( "INT", new MySQLParameter("value", MySqlDbType.Int, ParameterDirection.Input, int.MinValue), int.MinValue );
                yield return new PreparedFieldTypeTest( "INT UNSIGNED", new MySQLParameter("value", MySqlDbType.Int, ParameterDirection.Input, uint.MaxValue), uint.MaxValue );
                yield return new PreparedFieldTypeTest( "BIGINT", new MySQLParameter("value", MySqlDbType.BigInt, ParameterDirection.Input, long.MinValue), long.MinValue );
                yield return new PreparedFieldTypeTest( "BIGINT UNSIGNED", new MySQLParameter("value", MySqlDbType.BigInt, ParameterDirection.Input, ulong.MaxValue), ulong.MaxValue );
                yield return new PreparedFieldTypeTest( "DECIMAL(18,6)", new MySQLParameter("value", MySqlDbType.Decimal, ParameterDirection.Input, -345435.567894m), -345435.567894m );
                yield return new PreparedFieldTypeTest( "FLOAT", new MySQLParameter("value", MySqlDbType.Float, ParameterDirection.Input, 1.25e-5f), 1.25e-5f );
                yield return new PreparedFieldTypeTest( "DOUBLE", new MySQLParameter("value", MySqlDbType.Double, ParameterDirection.Input, 345435.567894d), 345435.567894d );
                yield return new PreparedFieldTypeTest( "BIT(2)", new MySQLParameter("value", MySqlDbType.Bit, ParameterDirection.Input, (ulong)1), ((ulong)1) );

                // time
                yield return new PreparedFieldTypeTest( "DATE", new MySQLParameter("value", MySqlDbType.Date, ParameterDirection.Input, new DateTime(1, 1, 1)), new DateTime(1, 1, 1));
                yield return new PreparedFieldTypeTest( "TIME", new MySQLParameter("value", MySqlDbType.Time, ParameterDirection.Input, new DateTime(1, 1, 1, 13, 40, 45)), new DateTime(1, 1, 1, 13, 40, 45) );
                yield return new PreparedFieldTypeTest( "DATETIME", new MySQLParameter("value", MySqlDbType.DateTime, ParameterDirection.Input, new DateTime(2020, 12, 11, 13, 40, 45)), new DateTime(2020, 12, 11, 13, 40, 45) );
                yield return new PreparedFieldTypeTest( "TIMESTAMP", new MySQLParameter("value", MySqlDbType.TimeStamp, ParameterDirection.Input, new DateTime(2020, 12, 11, 13, 40, 45)), new DateTime(2020, 12, 11, 13, 40, 45) );

        
                yield return new PreparedFieldTypeTest( "YEAR", new MySQLParameter("value", MySqlDbType.Year, ParameterDirection.Input, new DateTime(2020, 1, 1)), new DateTime(2020, 1, 1) );

                // varchars
                yield return new PreparedFieldTypeTest( "VARCHAR(20) CHARACTER SET utf8", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "some text"), "some text" );
                yield return new PreparedFieldTypeTest( "CHAR(20) CHARACTER SET utf8", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "some text"), "some text");
                yield return new PreparedFieldTypeTest( "TEXT CHARACTER SET utf8", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "some text"), "some text" );
                yield return new PreparedFieldTypeTest( "TINYTEXT CHARACTER SET utf8", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "some text"), "some text" );
                yield return new PreparedFieldTypeTest( "MEDIUMTEXT CHARACTER SET utf8", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "some text"), "some text" );
                yield return new PreparedFieldTypeTest( "LONGTEXT CHARACTER SET utf8", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "some text"), "some text" );
                yield return new PreparedFieldTypeTest( "BINARY(20)", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "some text"), "some text" );
                yield return new PreparedFieldTypeTest( "VARBINARY(20)", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "some text"), "some text" );

                // enums & sets
                yield return new PreparedFieldTypeTest( "ENUM('a','b','c') CHARACTER SET binary", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "a"), "a" );
                yield return new PreparedFieldTypeTest( "SET('a', 'b', 'c', 'd')", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "d,a,d"), "a,d" );

                // blobs
                yield return new PreparedFieldTypeTest( "BLOB",  new MySQLParameter("value", MySqlDbType.Binary, ParameterDirection.Input, new byte[] { 0x01, 0xaf }), new byte[] { 0x01, 0xaf }) ;
                yield return new PreparedFieldTypeTest( "TINYBLOB", new MySQLParameter("value", MySqlDbType.Binary, ParameterDirection.Input, new byte[] { 0x01, 0xaf }), new byte[] { 0x01, 0xaf });
                yield return new PreparedFieldTypeTest( "MEDIUMBLOB", new MySQLParameter("value", MySqlDbType.Binary, ParameterDirection.Input, new byte[] { 0x01, 0xaf }), new byte[] { 0x01, 0xaf });
                yield return new PreparedFieldTypeTest( "LONGBLOB", new MySQLParameter("value", MySqlDbType.Binary, ParameterDirection.Input, new byte[] { 0x01, 0xaf }), new byte[] { 0x01, 0xaf });

                // geometry
                //yield return new PreparedFieldTypeTest( "POINT", "POINT(1,1)", new MySqlPoint(1,1), );;
                //yield return new PreparedFieldTypeTest( "LINESTRING", "ST_LineStringFromText('LINESTRING(0 0,1 1,2 2)')", new MySqlLineString(new MySqlPoint(0,0), new MySqlPoint(1, 1), new MySqlPoint(2, 2)), );;
                //yield return new PreparedFieldTypeTest( "POLYGON", "ST_PolygonFromText('POLYGON((0 0,10 0,10 10,0 10,0 0),(5 5,7 5,7 7,5 7, 5 5))')", new MySqlPolygon(new MySqlLineString(new MySqlPoint(0,0), new MySqlPoint(10,0), new MySqlPoint(10,10), new MySqlPoint(0,10), new MySqlPoint(0,0)), new MySqlLineString(new MySqlPoint(5,5), new MySqlPoint(7,5), new MySqlPoint(7,7), new MySqlPoint(5,7), new MySqlPoint(5,5))),  );;
                //yield return new PreparedFieldTypeTest( "GEOMETRYCOLLECTION", "ST_GeomCollFromText('GEOMETRYCOLLECTION(POINT(1 1),LINESTRING(0 0,1 1,2 2,3 3,4 4))')", new MySqlGeometryCollection(new MySqlPoint(1,1), new MySqlLineString(new MySqlPoint(0,0), new MySqlPoint(1,1), new MySqlPoint(2,2), new MySqlPoint(3,3), new MySqlPoint(4,4))), );;

                // json
                yield return new PreparedFieldTypeTest( "JSON", new MySQLParameter("value", MySqlDbType.String, ParameterDirection.Input, "{\"key1\": \"value1\", \"key2\": \"value2\"}"), "{\"key1\": \"value1\", \"key2\": \"value2\"}" );
            }


            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [Fact]
        public void PreparedStatementOfBlobIsInserted()
        {
            var c = CachedConnection;

            using (var cmd1 = new MySQLCommand("DROP TABLE IF EXISTS number_type_test", c))
                cmd1.ExecuteNonQuery();

            using (var cmd2 = new MySQLCommand($@"CREATE TABLE number_type_test ( id INT NOT NULL,COL_VALUE BLOB,PRIMARY KEY (id));", c))
                cmd2.ExecuteNonQuery();

            using (var cmd3 = new MySQLCommand($@"INSERT INTO number_type_test ( id ,COL_VALUE)values(0, ?)", c))
            {
                cmd3.UsePreparedStatement = true;
                cmd3.Parameters.Add(new MySQLParameter("", MySqlDbType.Binary, ParameterDirection.Input, Encoding.UTF8.GetBytes("sure.")));
                cmd3.ExecuteNonQuery();
            }

            var expectedValue = "sure.";
            using (var cmd = new MySQLCommand("select id, to_base64(COL_VALUE) from number_type_test where id=0", c))
            {
                cmd.UsePreparedStatement = false;

                var reader = cmd.ExecuteReader();
                Assert.True(reader.Read());

                var ordinal = 1;
                var xx = reader.GetValue(ordinal);
                Assert.IsType<string>(xx);
                var executeScalar = Encoding.UTF8.GetString(Convert.FromBase64String((string)xx));

                Assert.NotNull(executeScalar);
                if (expectedValue == null)
                    Assert.Null(executeScalar);
                else
                {

                    Assert.IsType(expectedValue.GetType(), executeScalar);
                    if (expectedValue is string)
                        expectedValue = "'" + expectedValue.ToString() + "'";
                    if (executeScalar is string)
                        executeScalar = "'" + executeScalar.ToString() + "'";
                    Assert.Equal(expectedValue, executeScalar);
                }

                Assert.False(reader.Read());
            }
        }

        [Fact]
        public void PreparedStatementOfBlobIsFetched()
        {
            var expectedValue = "sure.";
            var c = CachedConnection;

            using (var cmd1 = new MySQLCommand("DROP TABLE IF EXISTS number_type_test", c))
                cmd1.ExecuteNonQuery();

            using (var cmd2 = new MySQLCommand($@"CREATE TABLE number_type_test ( id INT NOT NULL,COL_VALUE BLOB,PRIMARY KEY (id));", c))
                cmd2.ExecuteNonQuery();

            using (var cmd3 = new MySQLCommand($@"INSERT INTO number_type_test ( id ,COL_VALUE)values(0, from_base64('{Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedValue))}'))", c))
            {
                cmd3.UsePreparedStatement = false;
                cmd3.ExecuteNonQuery();
            }

  
            using (var cmd = new MySQLCommand("select id, COL_VALUE from number_type_test where id=0", c))
            {
                cmd.UsePreparedStatement = true;

                var reader = cmd.ExecuteReader();
                Assert.True(reader.Read());

                var ordinal = reader.GetOrdinal("COL_VALUE");
                var executeScalar = reader.GetValue(ordinal);
                Assert.IsType<byte[]>(executeScalar);
                var actualValue = Encoding.UTF8.GetString((byte[]) executeScalar);

                Assert.NotNull(actualValue);
                if (expectedValue == null)
                    Assert.Null(actualValue);
                else
                {

                    Assert.IsType(expectedValue.GetType(), actualValue);
                    if (expectedValue is string)
                        expectedValue = "'" + expectedValue.ToString() + "'";
                    if (actualValue is string)
                        actualValue = "'" + actualValue.ToString() + "'";
                    Assert.Equal(expectedValue, actualValue);
                }

                Assert.False(reader.Read());
            }
        }

        [Fact]
        public void NonPreparedStatementOfBitIsInserted()
        {
            var c = CachedConnection;

            using (var cmd1 = new MySQLCommand("DROP TABLE IF EXISTS number_type_test", c))
                cmd1.ExecuteNonQuery();

            using (var cmd2 = new MySQLCommand($@"CREATE TABLE number_type_test ( id INT NOT NULL,COL_VALUE BIT(2),PRIMARY KEY (id));", c))
                cmd2.ExecuteNonQuery();

            using (var cmd3 = new MySQLCommand($@"INSERT INTO number_type_test ( id ,COL_VALUE)values(0, B'01')", c))
            {
                cmd3.ExecuteNonQuery();
            }

            var expectedValue = "1";
            using (var cmd = new MySQLCommand("select id, HEX(COL_VALUE) from number_type_test where id=0", c))
            {
                cmd.UsePreparedStatement = false;

                var reader = cmd.ExecuteReader();
                Assert.True(reader.Read());

                var ordinal = 1;
                var executeScalar = reader.GetValue(ordinal);
                Assert.IsType<string>(executeScalar);
                var actualValue = (string)executeScalar;

                Assert.NotNull(actualValue);
                Assert.IsType(expectedValue.GetType(), actualValue);
                Assert.Equal(expectedValue, actualValue);

                Assert.False(reader.Read());
            }
        }

        [Fact]
        public void PreparedStatementOfBitIsInserted()
        {
            var c = CachedConnection;

            using (var cmd1 = new MySQLCommand("DROP TABLE IF EXISTS number_type_test", c))
                cmd1.ExecuteNonQuery();

            using (var cmd2 = new MySQLCommand($@"CREATE TABLE number_type_test ( id INT NOT NULL,COL_VALUE BIT(2),PRIMARY KEY (id));", c))
                cmd2.ExecuteNonQuery();

            using (var cmd3 = new MySQLCommand($@"INSERT INTO number_type_test ( id ,COL_VALUE)values(0, ?)", c))
            {
                cmd3.UsePreparedStatement = true;
                cmd3.Parameters.Add(new MySQLParameter("", MySqlDbType.Bit, ParameterDirection.Input, 1));
                cmd3.ExecuteNonQuery();
            }

            var expectedValue = "1";
            using (var cmd = new MySQLCommand("select id, HEX(COL_VALUE) from number_type_test where id=0", c))
            {
                cmd.UsePreparedStatement = false;

                var reader = cmd.ExecuteReader();
                Assert.True(reader.Read());

                var ordinal = 1;
                var executeScalar = reader.GetValue(ordinal);
                Assert.IsType<string>(executeScalar);
                var actualValue = (string)executeScalar;

                Assert.NotNull(actualValue);
                Assert.IsType(expectedValue.GetType(), actualValue);
                Assert.Equal(expectedValue, actualValue);

                Assert.False(reader.Read());
            }
        }

        [Fact]
        public void PreparedStatementOfBitIsFetched()
        {
            var expectedValue = (ulong)1;
            var c = CachedConnection;

            using (var cmd1 = new MySQLCommand("DROP TABLE IF EXISTS number_type_test", c))
                cmd1.ExecuteNonQuery();

            using (var cmd2 = new MySQLCommand($@"CREATE TABLE number_type_test ( id INT NOT NULL,COL_VALUE bit(2),PRIMARY KEY (id));", c))
                cmd2.ExecuteNonQuery();

            using (var cmd3 = new MySQLCommand($@"INSERT INTO number_type_test ( id ,COL_VALUE)values(0, B'01')", c))
            {
                cmd3.UsePreparedStatement = false;
                cmd3.ExecuteNonQuery();
            }


            using (var cmd = new MySQLCommand("select id, COL_VALUE from number_type_test where id=0", c))
            {
                cmd.UsePreparedStatement = true;

                var reader = cmd.ExecuteReader();
                Assert.True(reader.Read());

                var ordinal = reader.GetOrdinal("COL_VALUE");
                var executeScalar = reader.GetValue(ordinal);
                Assert.IsType<ulong>(executeScalar);
                var actualValue = (ulong) executeScalar;


                Assert.IsType(expectedValue.GetType(), actualValue);

                Assert.Equal(expectedValue, actualValue);

                Assert.False(reader.Read());
            }
        }


        [Theory]
        [ClassData(typeof(PreparedFieldTypeTestData))]
        public void ExecuteDataReaderForAnyFieldTypeWithPreparedStatement(PreparedFieldTypeTest test)
        {
            var mySqlTypeDeclaration = test.Declaration;
            var inputParameter = test.Parameter;
            var expectedValue = test.Value;
            var c = CachedConnection;

            using (var cmd1 = new MySQLCommand("DROP TABLE IF EXISTS number_type_test", c))
                cmd1.ExecuteNonQuery();

            using (var cmd2 = new MySQLCommand($@"CREATE TABLE number_type_test ( id INT NOT NULL,COL_VALUE {mySqlTypeDeclaration},PRIMARY KEY (id));", c))
                cmd2.ExecuteNonQuery();

            using (var cmd3 = new MySQLCommand($@"INSERT INTO number_type_test ( id ,COL_VALUE)values(0, ?)", c))
            {
                cmd3.UsePreparedStatement = true;
                cmd3.Parameters.Add(inputParameter);
                cmd3.ExecuteNonQuery();
            }

            using (var cmd = new MySQLCommand("select id, COL_VALUE from number_type_test where id=?", c))
            {
                cmd.UsePreparedStatement = true;
                var p = new MySQLParameter("id", MySqlDbType.Int);
                p.Value = 0;
                cmd.Parameters.Add(p);

                var reader = cmd.ExecuteReader();
                Assert.True(reader.Read());

                var ordinal = reader.GetOrdinal("COL_VALUE");
                var executeScalar = reader.GetValue(ordinal);

                Assert.NotNull(executeScalar);
                if (expectedValue == null)
                    Assert.Null(executeScalar);
                else
                {
                    Assert.NotNull(executeScalar);
                    Assert.IsType(expectedValue.GetType(), executeScalar);
                    if (expectedValue is string)
                        expectedValue = "'" + expectedValue.ToString() + "'";
                    if (executeScalar is string)
                        executeScalar = "'" + executeScalar.ToString() + "'";
                    Assert.Equal(expectedValue, executeScalar);
                }

                Assert.False(reader.Read());


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

        public void Dispose()
        {
            if (_cachedConnection != null)
            {
                _cachedConnection.Dispose();
                _cachedConnection = null;
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