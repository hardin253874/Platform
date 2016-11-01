// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.Common
{
    /// <summary>
    /// A three-element tuple.
    /// </summary>
    /// <remarks>Intended to overcome limitations that we have with System.Tuple such as serialization.</remarks>
    public class ProtectedTuple<T1, T2, T3> : IEquatable<ProtectedTuple<T1, T2, T3>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <param name="item3"></param>
        protected ProtectedTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        /// <summary>
        /// Item 1.
        /// </summary>
        protected T1 Item1 { get; set; }

        /// <summary>
        /// Item 2.
        /// </summary>
        protected T2 Item2 { get; set; }

        /// <summary>
        /// Item 3.
        /// </summary>
        protected T3 Item3 { get; set; }

        /// <summary>
        /// Typed equality check.
        /// </summary>
        public bool Equals(ProtectedTuple<T1, T2, T3> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3);
        }

        /// <summary>
        /// Untyped equality check.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProtectedTuple<T1, T2, T3>)obj);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = EqualityComparer<T1>.Default.GetHashCode(Item1);
                hashCode = (hashCode * 397) ^ EqualityComparer<T2>.Default.GetHashCode(Item2);
                hashCode = (hashCode * 397) ^ EqualityComparer<T3>.Default.GetHashCode(Item3);
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(ProtectedTuple<T1, T2, T3> left, ProtectedTuple<T1, T2, T3> right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(ProtectedTuple<T1, T2, T3> left, ProtectedTuple<T1, T2, T3> right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Default string representation.
        /// </summary>
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", Item1, Item2, Item3);
        }
    }
}
