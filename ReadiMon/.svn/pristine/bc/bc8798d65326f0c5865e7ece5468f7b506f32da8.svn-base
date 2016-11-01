// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     Field Comparer.
	/// </summary>
	public class FieldComparer : IComparer<IFieldInfo>
	{
		/// <summary>
		///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in
		///     the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero
		///     <paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than
		///     <paramref name="y" />.
		/// </returns>
		public int Compare( IFieldInfo x, IFieldInfo y )
		{
			return String.Compare( x.Name, y.Name, StringComparison.Ordinal );
		}
	}
}