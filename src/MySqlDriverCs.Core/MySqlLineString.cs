using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MySQLDriverCS
{
    public class MySQLLineString : IMySQLGeometryObject, IEquatable<MySQLLineString>, IReadOnlyList<MySQLPoint>
    {
        private readonly MySQLPoint[] _points;

        public MySQLLineString(params MySQLPoint[] points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            _points = points.ToArray();
        }

        public int Count => _points.Length;

        public MySQLPoint this[int index]
        {
            get { return _points[index]; }
        }

        public static bool operator !=(MySQLLineString left, MySQLLineString right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(MySQLLineString left, MySQLLineString right)
        {
            return Equals(left, right);
        }

        public bool Equals(MySQLLineString other)
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
            return Equals((MySQLLineString)obj);
        }

        public IEnumerator<MySQLPoint> GetEnumerator()
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
}