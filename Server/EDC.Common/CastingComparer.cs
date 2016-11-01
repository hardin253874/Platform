// Copyright 2011-2016 Global Software Innovation Pty Ltd
using ReadiNow.Annotations;
using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace

namespace EDC.Common
{
	/// <summary>
	///     Helper class to act as a comparer of one type by performing a transform on it.
	/// </summary>
	/// <example>
	///     Sample Usage:
	///     <code>
	/// var comparer = new CastingComparer[Resource, string](resource => resource.Name);
	/// List[Resource] list1, list2, result;
	/// result = list1.Except(list2, comparer);
	/// </code>
	/// </example>
	/// <typeparam name="TComparer">The type that this comparer is going to compare.</typeparam>
	/// <typeparam name="TTransform">The transformed type that will actually be compared.</typeparam>
	public class CastingComparer<TComparer, TTransform> : IEqualityComparer<TComparer>
	{
		private readonly Func<TComparer, TTransform> _converter;
        private readonly IEqualityComparer<TTransform> _targetComparer;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="converter">A delegate to convert the source type to the target type that will actually be compared.</param>
        /// <param name="targetComparer">A comparer to apply to the target type.</param>
        public CastingComparer( Func<TComparer, TTransform> converter, [CanBeNull] IEqualityComparer<TTransform> targetComparer = null )
        {
            if ( converter == null )
                throw new ArgumentNullException( nameof( converter ) );
            _converter = converter;
            _targetComparer = targetComparer ?? EqualityComparer<TTransform>.Default;
        }


        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        ///     True if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals( TComparer x, TComparer y )
		{
			return _targetComparer.Equals( _converter( x ), _converter( y ) );
		}


		/// <summary>
		///     Returns a hash code for the instance being compared that is compatible with the equality test.
		/// </summary>
		public int GetHashCode( TComparer obj )
		{
			return _targetComparer.GetHashCode( _converter( obj ) );
		}
	}
}

// ReSharper restore CheckNamespace