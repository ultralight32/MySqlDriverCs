using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MySQLDriverCS
{
    public class MySQLPolygon : IMySQLGeometryObject, IEquatable<MySQLPolygon>, IReadOnlyList<MySQLLineString>
    {
        private readonly MySQLLineString[] _segments;

        public MySQLPolygon(params MySQLLineString[] segments)
        {
            _segments = segments ?? throw new ArgumentNullException(nameof(segments));
        }

        public int Count => _segments.Length;

        public MySQLLineString this[int index]
        {
            get { return _segments[index]; }
        }

        public static bool operator !=(MySQLPolygon left, MySQLPolygon right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(MySQLPolygon left, MySQLPolygon right)
        {
            return Equals(left, right);
        }

        public bool Equals(MySQLPolygon other)
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
            return Equals((MySQLPolygon)obj);
        }

        public IEnumerator<MySQLLineString> GetEnumerator()
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