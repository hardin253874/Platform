// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     List Extensions class.
	/// </summary>
	public static class ListExtensions
	{
		/// <summary>
		///     Adds the range.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <param name="values">The values.</param>
		public static void AddRange<T>( this IList<T> list, IEnumerable<T> values )
		{
			if ( values != null )
			{
				foreach ( T item in values )
				{
					list.Add( item );
				}
			}
		}
	}
}