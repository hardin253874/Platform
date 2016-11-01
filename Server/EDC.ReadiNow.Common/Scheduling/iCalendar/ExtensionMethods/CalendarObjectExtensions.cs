// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     CalendarObjectExtensions class.
	/// </summary>
	public static class CalendarObjectExtensions
	{
		/// <summary>
		///     Adds the child.
		/// </summary>
		/// <typeparam name="TItem">The type of the item.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="child">The child.</param>
		public static void AddChild<TItem>( this ICalendarObject obj, TItem child ) where TItem : ICalendarObject
		{
			obj.Children.Add( child );
		}

		/// <summary>
		///     Removes the child.
		/// </summary>
		/// <typeparam name="TItem">The type of the item.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="child">The child.</param>
		public static void RemoveChild<TItem>( this ICalendarObject obj, TItem child ) where TItem : ICalendarObject
		{
			obj.Children.Remove( child );
		}
	}
}