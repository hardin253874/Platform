// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// A test equality comparer for <see cref="StructuredQuery"/> objects. Not a complete or 
    /// full equality test but good enough for most uses.
    /// </summary>
    public class StructuredQueryEqualityComparer: IEqualityComparer<StructuredQuery>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">
        /// The first <see cref="StructuredQuery"/> to compare.
        /// </param>
        /// <param name="y">
        /// The second <see cref="StructuredQuery"/> to compare.
        /// </param>
        public bool Equals(StructuredQuery x, StructuredQuery y)
        {
            bool result;

            if (x == null && y == null)
            {
                result = true;
            }
            else if (x == null || y == null)
            {
                result = false;
            }
            else
            {
                result = x.Conditions.Count() == y.Conditions.Count()
                         && x.Distinct == y.Distinct
                         && x.OrderBy.Count() == y.OrderBy.Count()
                         && x.RootEntity.EntityId == y.RootEntity.EntityId
                         && x.SelectColumns.Count() == y.SelectColumns.Count();
            }

            return result;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">
        /// The <see cref="T:System.Object"/> for which a hash code is to be returned.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="obj"/> cannot be null.
        /// </exception>
        public int GetHashCode(StructuredQuery obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return obj.GetHashCode();
        }
    }
}
