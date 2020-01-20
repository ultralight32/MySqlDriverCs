using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MySQLDriverCS
{
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