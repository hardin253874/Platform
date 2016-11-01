// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

// http://stackoverflow.com/questions/4901320/is-there-any-kind-of-referencecomparer-in-net

namespace EDC
{
    /// <summary>
    /// Class to force reference equality on classes that have overriden Equality tests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReferenceComparer<T> : IEqualityComparer<T> where T : class
    {
        private static ReferenceComparer<T> _instance;

        public static ReferenceComparer<T> Instance
        {
            get
            {
                return _instance ?? (_instance = new ReferenceComparer<T>());
            }
        }

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
