/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.
-
	This file is part of MySQLDriverCS.

    MySQLDriverCS is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    MySQLDriverCS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MySQLDriverCS; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

	***** Update MySQLDataReader in order suport close connection after close DataReader ****

	Items update by Omar del Valle Rodríguez (01/18/2004)
	- Constrctor MySQLDataReader now support bool parameter to close connection after close DataReader.
	- Update method Close in order support CommandBehavior.CloseConnection

*/

using MySQLDriverCS.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MySQLDriverCS
{
    public interface IMySqlGeometryObject
    {
    }

    public class MySQLRealQueryDataReader : DbDataReader
    {
        readonly MySQLConnection _connection;
         readonly NativeStatement _stmt;
     
         bool m_CloseConnection = false;

       
         bool _closed;

      
         uint m_fieldCount;

       
         IMySqlField[] m_fields;

      
         MYSQL_BIND[] m_row;

    
         int MYSQL_NO_DATA = 100;

    
        public MySQLRealQueryDataReader(uint fieldCount, NativeStatement stmt, MySQLParameterCollection parameters, MySQLConnection connection, bool closeConnection)
        {
            _connection = connection;
            _stmt = stmt;
            m_CloseConnection = closeConnection;

            _closed = false;
           
            m_fieldCount = fieldCount;

            IntPtr fields;
            if (_connection.NativeConnection.ClientVersion.CompareTo("6.0.0") > 0)
            {
                MYSQL_STMT_6_1 mysql_stmt = (MYSQL_STMT_6_1)Marshal.PtrToStructure(stmt.stmt, typeof(MYSQL_STMT_6_1));

                fields = mysql_stmt.result.data;
            }
            else
            {
                MYSQL_STMT mysql_stmt = (MYSQL_STMT)Marshal.PtrToStructure(stmt.stmt, typeof(MYSQL_STMT));

                fields = mysql_stmt.result.data;
            }

            m_fields = (IMySqlField[])Array.CreateInstance(new MYSQL_FIELD_VERSION_5_64().GetType(), m_fieldCount);

            long pointer = fields.ToInt64();
            int index;
            m_row = new MYSQL_BIND[m_fieldCount];
            for (index = 0; index < m_fieldCount; index++)
            {
                IMySqlField fieldMetadata = new MYSQL_FIELD_VERSION_5_64();
                IntPtr ptr = new IntPtr(pointer);
                Marshal.PtrToStructure(ptr, fieldMetadata);
                pointer += Marshal.SizeOf(fieldMetadata);
                m_fields[index] = fieldMetadata;
                m_row[index] = new MYSQL_BIND();

                if (fieldMetadata.Type == (uint)enum_field_types.MYSQL_TYPE_BLOB)
                {
                    fieldMetadata.MaxLength = 1024;
                }
                else if (fieldMetadata.Type == (uint)enum_field_types.MYSQL_TYPE_NULL && parameters != null && parameters.Count > index)//Caso select distinct donde mysql_stmt_bind_param3 mapea erroneamente a NULL
                {
                    fieldMetadata.Type = PreparedStatement.DbtoMysqlType(parameters[index].DbType);
                }
                m_row[index].InitForBind(fieldMetadata);
            }

            sbyte code = stmt.mysql_stmt_bind_result64(m_row);
            if (code != 0)
                throw new MySqlException(stmt);
        }

        /// <summary>
        /// Closes this reader
        /// </summary>
        public new void Dispose()
        {
            Close();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        public void RowDispose(int i)
        {
            m_row[i].Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool RowIsNull(int i)
        {
            return m_row[i].IsNull;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public uint RowType(int i)
        {
            return m_row[i].Type;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object RowValue(int i)
        {
            return m_row[i].Value;
        }

       
        /// <summary>
        /// returns 1
        /// </summary>
        public override int Depth => 1;

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        public override bool IsClosed => _closed;

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        /// <remarks>The RecordsAffected property is not set until all rows are read and you close the MySQLDataReader.</remarks>
        public override int RecordsAffected => (int) _stmt.mysql_stmt_affected_rows();

        /// <summary>
        /// Closes the MySQLDataReader 0bject.
        /// </summary>
        public override void Close()
        {
            if (!_closed)
            {
                //Releases memory associated with the result set produced by execution of the prepared statement.
                //If there is a cursor open for the statement, mysql_stmt_free_result() closes it.
                int errorCode = _stmt.mysql_stmt_free_result();
                if (errorCode != 0)//Error occurred
                {
                    throw new MySqlException(_stmt);
                }
                _closed = true;
                for (int i = 0; i < m_fieldCount; i++)
                {
                    RowDispose(i);
                }
                if (_connection != null && m_CloseConnection)
                    _connection.Close();
            }

            
        }

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the MySQLDataReader and it's values.
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchemaTable()
        {
            throw new MySqlException("Operation not supported");
        }

        /// <summary>
        /// No more results expected
        /// </summary>
        /// <returns></returns>
        public override bool NextResult()
        {
            // MLVL 24/05/2003 ->
            // throw new MySQLException("Operation not supported");
            // <-
            return false;
        }

        /// <summary>
        /// Advances the MySQLDataReader to the next record.
        /// </summary>
        /// <returns></returns>
        public override bool Read()
        {
            if (IsClosed) return false;
            var errorCode = _stmt.mysql_stmt_fetch();
            if (errorCode == MYSQL_NO_DATA)//No more rows/data exists
            {
                return false;
            }
            else if (errorCode == 1)//Error occurred
            {
                throw new MySqlException(_stmt);
            }
            else
            {
                //0	Successful, the data has been fetched to application data buffers.
                //MYSQL_DATA_TRUNCATED 	Data truncation occurred
                return true;
            }
        }

        /// <summary>
        /// Number of fields returned
        /// </summary>
        public override int FieldCount { get { if (IsClosed) return 0; else return (int)m_fieldCount; } }

      
        public override bool HasRows => _stmt.mysql_stmt_affected_rows() > 0;

        /// <summary>
        /// Get value by name
        /// </summary>
        public override object this[string name]
        {
            get
            {
                if (IsClosed) throw new MySqlException("Reader must be open");
                for (int i = 0; i < m_fields.Length; i++)
                {
                    if (m_fields[i].Name == name)
                        return this[i];
                }
                throw new MySqlException("Invalid column name");
            }
        }

        /// <summary>
        /// Get value by index
        /// </summary>
        public override object this[int i]
        {
            get
            {
                if (IsClosed) throw new MySqlException("Reader must be open");
                if (RowType(i) == (uint)FieldTypes.MYSQL_TYPE_BLOB)
                {
                    return getAllBytes(i);
                }
                else
                {
                    return RowValue(i);
                }
            }
        }

        /// <summary>
        /// Get as boolean
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override bool GetBoolean(int i) { return (Convert.ToByte(this[i]) > 0) ? true : false; }

        /// <summary>
        /// Get as byte
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override byte GetByte(int i) { return Convert.ToByte(this[i]); }

        /// <summary>
        /// Unsupported
        /// </summary>
        /// <param name="i"></param> he zero-based column ordinal
        /// <param name="fieldOffset"></param> The index within the field from which to begin the read operation
        /// <param name="buffer"></param> The buffer into which to read the stream of bytes
        /// <param name="bufferoffset"></param>The index for buffer to begin the read operation
        /// <param name="length"></param>The maximum length to copy into the buffer
        /// <returns></returns>The actual number of bytes rea
        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            if (m_row[i].Length > m_row[i].BufferLength)//data truncation
            {
                MYSQL_BIND[] newbind = new MYSQL_BIND[1];
                newbind[0] = new MYSQL_BIND();
                IMySqlField ft = new MYSQL_FIELD_VERSION_5_64();
                ft.Type = (uint)enum_field_types.MYSQL_TYPE_BLOB;
                ft.MaxLength = (uint)length;
                newbind[0].InitForBind(ft);

                sbyte errorCode = _stmt.mysql_stmt_fetch_column64(newbind, (uint)i, (uint)fieldOffset);
                if (errorCode != 0)
                    throw new MySqlException(_stmt);

                long result = Math.Min(length, newbind[0].Length - fieldOffset);
                newbind[0].GetBytes(buffer, (uint)result);
                newbind[0].Dispose();
                return result;
            }
            else
            {
                m_row[i].GetBytes(buffer, (uint)length);
                return m_row[i].Length;
            }
        }

        /// <summary>
        /// Get as char
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override char GetChar(int i) { return Convert.ToChar(this[i]); }

        /// <summary>
        /// Unsupported
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldoffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) { throw new MySqlException("Operation not supported"); }

        /// <summary>
        /// Unsupported
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override string GetDataTypeName(int i) { throw new MySqlException("Operation not supported"); }

        /// <summary>
        /// Get as DateTime
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override DateTime GetDateTime(int i)
        {
            return (DateTime)RowValue(i);
        }

        /// <summary>
        /// Get as decimal
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        /// Modified by Claudia Murialdo (07/24/04) in order to work with
        /// culture-independent format of numeric values in a stmt.
        public override decimal GetDecimal(int i) { return Convert.ToDecimal(this[i], CultureInfo.InvariantCulture.NumberFormat); }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override System.Collections.IEnumerator GetEnumerator()
        {
            return dt.Rows.GetEnumerator();
        }

        /// <summary>
        /// Unsupported
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override Guid GetGuid(int i) { throw new MySqlException("Operation not supported"); }

        /// <summary>
        /// Get as Int16
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override short GetInt16(int i) { return Convert.ToInt16(this[i]); }

        /// <summary>
        /// Get as Int32
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override int GetInt32(int i) { return Convert.ToInt32(this[i]); }

        /// <summary>
        /// Get as Int64
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override long GetInt64(int i) { return Convert.ToInt64(this[i]); }

        /// <summary>
        /// Get as string
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        /// <remarks>Remake by "Christophe Ravier" (c.ravier@laposte.net) 2003-11-27</remarks>
        public override string GetString(int i)
        {
            //Convert Text field to string
            object val = this[i];
            if (val is byte[])
                return System.Text.Encoding.Default.GetString((byte[])val);
            else
                return Convert.ToString(val);
        }

        /// <summary>
        /// Get as object
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override object GetValue(int i)
        {
            return this[i];
        }

        /// <summary>
        /// Get full row as array of object
        /// </summary>
        /// <param name="values"></param>
        /// <returns>Fields retrived, if it fails returns -1.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <listheader><item><description>Bugfixed:</description></item></listheader>
        /// <item><term>2002-10-28</term><description>Values was ignored</description></item>
        /// </list>
        /// </remarks>
        public override int GetValues(object[] values)
        {
            if (values.Length < FieldCount)
                return -1;
            for (int i = 0; i < FieldCount; i++)
                values[i] = GetValue(i);
            return FieldCount;
        }

        /// <summary>
        /// Unsupported
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected override DbDataReader GetDbDataReader(int i) { throw new MySqlException("Operation not supported"); }

        /// <summary>
        /// Get as double
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override double GetDouble(int i) { return (double)this[i]; }

        /// <summary>
        /// Get field type (class Type)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override Type GetFieldType(int i) { return MySQLUtils.MySQLToNetType(m_fields[i].Type); }

        /// <summary>
        /// Get as float
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override float GetFloat(int i) { return (float)this[i]; }

        /// <summary>
        /// Get name of field by index
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override string GetName(int i) { return m_fields[i].Name; }

        /// <summary>
        /// Get index of field by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override int GetOrdinal(string name)
        {
            for (int i = 0; i < m_fields.Length; i++)
            {
                if (m_fields[i].Name == name)
                    return i;
            }
            throw new MySqlException("Field not found");
        }

        /// <summary>
        /// Returns ok if the field is null in database
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override bool IsDBNull(int i)
        {
            return RowIsNull(i);
        }

        private object getAllBytes(int id)
        {
            if (RowIsNull(id))
            {
                return DBNull.Value;
            }
            else
            {
                MemoryStream fs;
                BinaryWriter bw;
                int bufferSize = 1024;
                byte[] outbyte = new byte[bufferSize];
                long retval;
                long startIndex = 0;
                try
                {
                    // Reset the starting byte for the new BLOB.
                    startIndex = 0;

                    retval = GetBytes(id, startIndex, outbyte, 0, bufferSize);

                    if (retval == 0)
                        return new byte[0];

                    fs = new MemoryStream();

                    bw = new BinaryWriter(fs);

                    // Continue reading and writing while there are bytes beyond the size of the buffer.
                    while (retval == bufferSize)
                    {
                        bw.Write(outbyte);
                        bw.Flush();
                        // Reposition the start index to the end of the last buffer and fill the buffer.
                        startIndex += bufferSize;
                        retval = GetBytes(id, startIndex, outbyte, 0, bufferSize);
                    }
                    if (retval >= 0)
                    {
                        // Write the remaining buffer.
                        bw.Write(outbyte, 0, (int)retval);
                    }
                    bw.Flush();

                    // Close the output file.
                    bw.Close();
                    fs.Close();
                }
                catch (IOException e)
                {
                    throw e;
                }
                return fs.ToArray();
            }
        }
    }
    public class MySqlGeometryCollection : IMySqlGeometryObject, IEquatable<MySqlGeometryCollection>, IReadOnlyList<IMySqlGeometryObject>
    {
        private readonly IMySqlGeometryObject[] _objects;

        public MySqlGeometryCollection(params IMySqlGeometryObject[] objects)
        {
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            this._objects = objects.ToArray();
        }

        public int Count => _objects.Length;

        public IMySqlGeometryObject this[int index]
        {
            get { return _objects[index]; }
        }

        public static bool operator !=(MySqlGeometryCollection left, MySqlGeometryCollection right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(MySqlGeometryCollection left, MySqlGeometryCollection right)
        {
            return Equals(left, right);
        }

        public bool Equals(MySqlGeometryCollection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _objects.SequenceEqual(other._objects);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MySqlGeometryCollection)obj);
        }

        public IEnumerator<IMySqlGeometryObject> GetEnumerator()
        {
            foreach (var point in _objects)
            {
                yield return point;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _objects.Aggregate(0, (current, coordinate) => current ^ coordinate.GetHashCode());
        }
        public override string ToString()
        {
            return "GEOMETRYCOLLECTION(" + string.Join(", ", _objects.Select(x => x.ToString())) + ")";
        }
    }

    public class MySqlLineString : IMySqlGeometryObject, IEquatable<MySqlLineString>, IReadOnlyList<MySqlPoint>
    {
        private readonly MySqlPoint[] _points;

        public MySqlLineString(params MySqlPoint[] points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            _points = points.ToArray();
        }

        public int Count => _points.Length;

        public MySqlPoint this[int index]
        {
            get { return _points[index]; }
        }

        public static bool operator !=(MySqlLineString left, MySqlLineString right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(MySqlLineString left, MySqlLineString right)
        {
            return Equals(left, right);
        }

        public bool Equals(MySqlLineString other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _points.SequenceEqual(other._points);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MySqlLineString)obj);
        }

        public IEnumerator<MySqlPoint> GetEnumerator()
        {
            foreach (var point in _points)
            {
                yield return point;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _points.Aggregate(0, (current, coordinate) => current ^ coordinate.GetHashCode());
        }
        public override string ToString()
        {
            return "LINESTRING(" + string.Join(", ", _points.Select(x => x.ToString())) + ")";
        }
    }

    public class MySqlPoint : IMySqlGeometryObject, IEquatable<MySqlPoint>, IReadOnlyList<double>
    {
        private readonly double[] _coordinates;

        public MySqlPoint(params double[] coordinates)
        {
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            _coordinates = coordinates.ToArray();
        }

        public int Count => _coordinates.Length;
        public double this[int index] => _coordinates[index];

        public static bool operator !=(MySqlPoint left, MySqlPoint right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(MySqlPoint left, MySqlPoint right)
        {
            return Equals(left, right);
        }

        public bool Equals(MySqlPoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _coordinates.SequenceEqual(other._coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MySqlPoint)obj);
        }

        public IEnumerator<double> GetEnumerator()
        {
            return ((IEnumerable<double>)_coordinates).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _coordinates.Aggregate(0, (current, coordinate) => current ^ coordinate.GetHashCode());
        }
        public override string ToString()
        {
            return "POINT(" + string.Join(", ", _coordinates.Select(x => x.ToString(CultureInfo.InvariantCulture))) + ")";
        }
    }
    public class MySqlPolygon : IMySqlGeometryObject, IEquatable<MySqlPolygon>, IReadOnlyList<MySqlLineString>
    {
        private readonly MySqlLineString[] _segments;

        public MySqlPolygon(params MySqlLineString[] segments)
        {
            _segments = segments ?? throw new ArgumentNullException(nameof(segments));
        }

        public int Count => _segments.Length;

        public MySqlLineString this[int index]
        {
            get { return _segments[index]; }
        }

        public static bool operator !=(MySqlPolygon left, MySqlPolygon right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(MySqlPolygon left, MySqlPolygon right)
        {
            return Equals(left, right);
        }

        public bool Equals(MySqlPolygon other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _segments.SequenceEqual(other._segments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MySqlPolygon)obj);
        }

        public IEnumerator<MySqlLineString> GetEnumerator()
        {
            foreach (var point in _segments)
            {
                yield return point;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _segments.Aggregate(0, (current, coordinate) => current ^ coordinate.GetHashCode());
        }
        public override string ToString()
        {
            return "POLYGON(" + string.Join(", ", _segments.Select(x => x.ToString())) + ")";
        }
    }
}