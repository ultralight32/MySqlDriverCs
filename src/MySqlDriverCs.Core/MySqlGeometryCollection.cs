using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MySQLDriverCS
{
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
}