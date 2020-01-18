using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MySQLDriverCS
{
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
}