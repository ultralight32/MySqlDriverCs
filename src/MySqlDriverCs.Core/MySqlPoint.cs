using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MySQLDriverCS
{
    public class MySQLPoint : IMySQLGeometryObject, IEquatable<MySQLPoint>, IReadOnlyList<double>
    {
        private readonly double[] _coordinates;

        public MySQLPoint(double x, double y)
        {
            var coordinates = new double[]{x,y};
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            _coordinates = coordinates.ToArray();
        }

        public int Count => _coordinates.Length;
        public double this[int index] => _coordinates[index];

        public static bool operator !=(MySQLPoint left, MySQLPoint right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(MySQLPoint left, MySQLPoint right)
        {
            return Equals(left, right);
        }

        public bool Equals(MySQLPoint other)
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
            return Equals((MySQLPoint)obj);
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
}