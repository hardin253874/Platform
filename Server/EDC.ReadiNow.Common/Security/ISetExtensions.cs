// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Security
{
	/// <summary>
	///     Set-based methods.
	/// </summary>
	// ReSharper disable InconsistentNaming
	public static class ISetExtensions
	{
		/// <summary>
		///     Efficient set-based intersection that returns as soon as any element appears in both .
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		public static bool IntersectAny<T>( this ISet<T> a, ISet<T> b )
		{
			if ( a == null && b == null )
			{
				return true;
			}

			if ( a == null || b == null )
			{
				return false;
			}

			ISet<T> outer = b;
			ISet<T> inner = a;

			/////
			// Determine the smaller of the two tables.
			/////
			if ( a.Count <= b.Count )
			{
				outer = a;
				inner = b;
			}

			return outer.Any( value => inner.Contains( value ) );
		}
	}

	// ReSharper restore InconsistentNaming
}