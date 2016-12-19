// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace EDC.Collections.Generic
{
    public class EnumeratorEqualityComparer<T>: IEqualityComparer<IEnumerator<T>>
    {
        /// <summary>
        /// Create a new <see cref="EnumeratorEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="equalityComparer">
        /// An optional EqualityComparer for comparing elements.
        /// </param>
        public EnumeratorEqualityComparer(IEqualityComparer<T> equalityComparer = null)
        {
            EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Used to compare elements.
        /// </summary>
        public IEqualityComparer<T> EqualityComparer { get; }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <typeparamref name="T"/> to compare.</param>
        /// <param name="y">The second object of type <typeparamref name="T"/> to compare.</param>
        public bool Equals(IEnumerator<T> x, IEnumerator<T> y)
        {
            bool finished;
            bool equal;
            bool xAfterEnd;
            bool yAfterEnd;

            equal = false;
            finished = false;

            if (x == null && y == null)
            {
                equal = true;
                finished = true;
            }
            if ((y != null && x == null) || (y == null && x != null))
            {
                equal = false;
                finished = true;
            }

            if (!finished)
            {
                x.Reset();
                y.Reset();
                while (!finished)
                {
                    xAfterEnd = !x.MoveNext();
                    yAfterEnd = !y.MoveNext();
                    if (xAfterEnd != yAfterEnd)
                    {
                        equal = false;
                        finished = true;
                    }
                    else if (xAfterEnd && yAfterEnd)
                    {
                        equal = true;
                        finished = true;
                    }
                    else
                    {
                        equal = EqualityComparer.Equals(x.Current, y.Current);
                        finished = !equal;
                    }
                }
            }

            return equal;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(IEnumerator<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}
