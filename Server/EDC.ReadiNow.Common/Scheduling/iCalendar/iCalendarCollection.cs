// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A list of iCalendars.
	/// </summary>
	[Serializable]
// ReSharper disable InconsistentNaming
	public class iCalendarCollection : List<IICalendar>, IICalendarCollection
// ReSharper restore InconsistentNaming
	{
		/// <summary>
		///     Clears a previous evaluation, usually because one of the
		///     key elements used for evaluation has changed
		///     (Start, End, Duration, recurrence rules, exceptions, etc.).
		/// </summary>
		public void ClearEvaluation( )
		{
			foreach ( IICalendar iCal in this )
			{
				iCal.ClearEvaluation( );
			}
		}

		/// <summary>
		///     Returns all occurrences of this component that start on the date provided.
		///     All components starting between 12:00:00AM and 11:59:59 PM will be
		///     returned.
		///     <note>
		///         This will first Evaluate() the date range required in order to
		///         determine the occurrences for the date provided, and then return
		///         the occurrences.
		///     </note>
		/// </summary>
		/// <param name="dt">The date for which to return occurrences.</param>
		/// <returns>
		///     A list of Periods representing the occurrences of this object.
		/// </returns>
		public IList<Occurrence> GetOccurrences( IDateTime dt )
		{
			var occurrences = new List<Occurrence>( );
			foreach ( IICalendar iCal in this )
			{
				occurrences.AddRange( iCal.GetOccurrences( dt ) );
			}
			occurrences.Sort( );
			return occurrences;
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public IList<Occurrence> GetOccurrences( DateTime dt )
		{
			var occurrences = new List<Occurrence>( );
			foreach ( IICalendar iCal in this )
			{
				occurrences.AddRange( iCal.GetOccurrences( dt ) );
			}
			occurrences.Sort( );
			return occurrences;
		}

		/// <summary>
		///     Returns all occurrences of this component that start within the date range provided.
		///     All components occurring between <paramref name="startTime" /> and <paramref name="endTime" />
		///     will be returned.
		/// </summary>
		/// <param name="startTime">The starting date range</param>
		/// <param name="endTime">The ending date range</param>
		/// <returns></returns>
		public IList<Occurrence> GetOccurrences( IDateTime startTime, IDateTime endTime )
		{
			var occurrences = new List<Occurrence>( );
			foreach ( IICalendar iCal in this )
			{
				occurrences.AddRange( iCal.GetOccurrences( startTime, endTime ) );
			}
			occurrences.Sort( );
			return occurrences;
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <returns></returns>
		public IList<Occurrence> GetOccurrences( DateTime startTime, DateTime endTime )
		{
			var occurrences = new List<Occurrence>( );
			foreach ( IICalendar iCal in this )
			{
				occurrences.AddRange( iCal.GetOccurrences( startTime, endTime ) );
			}
			occurrences.Sort( );
			return occurrences;
		}

		/// <summary>
		///     Returns all occurrences of components of type T that start on the date provided.
		///     All components starting between 12:00:00AM and 11:59:59 PM will be
		///     returned.
		///     <note>
		///         This will first Evaluate() the date range required in order to
		///         determine the occurrences for the date provided, and then return
		///         the occurrences.
		///     </note>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dt">The date for which to return occurrences.</param>
		/// <returns>
		///     A list of Periods representing the occurrences of this object.
		/// </returns>
		public IList<Occurrence> GetOccurrences<T>( IDateTime dt ) where T : IRecurringComponent
		{
			var occurrences = new List<Occurrence>( );
			foreach ( IICalendar iCal in this )
			{
				occurrences.AddRange( iCal.GetOccurrences<T>( dt ) );
			}
			occurrences.Sort( );
			return occurrences;
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public IList<Occurrence> GetOccurrences<T>( DateTime dt ) where T : IRecurringComponent
		{
			var occurrences = new List<Occurrence>( );
			foreach ( IICalendar iCal in this )
			{
				occurrences.AddRange( iCal.GetOccurrences<T>( dt ) );
			}
			occurrences.Sort( );
			return occurrences;
		}

		/// <summary>
		///     Returns all occurrences of components of type T that start within the date range provided.
		///     All components occurring between <paramref name="startTime" /> and <paramref name="endTime" />
		///     will be returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startTime">The starting date range</param>
		/// <param name="endTime">The ending date range</param>
		/// <returns></returns>
		public IList<Occurrence> GetOccurrences<T>( IDateTime startTime, IDateTime endTime ) where T : IRecurringComponent
		{
			var occurrences = new List<Occurrence>( );
			foreach ( IICalendar iCal in this )
			{
				occurrences.AddRange( iCal.GetOccurrences<T>( startTime, endTime ) );
			}
			occurrences.Sort( );
			return occurrences;
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <returns></returns>
		public IList<Occurrence> GetOccurrences<T>( DateTime startTime, DateTime endTime ) where T : IRecurringComponent
		{
			var occurrences = new List<Occurrence>( );
			foreach ( IICalendar iCal in this )
			{
				occurrences.AddRange( iCal.GetOccurrences<T>( startTime, endTime ) );
			}
			occurrences.Sort( );
			return occurrences;
		}

		/// <summary>
		///     Combines the free busy.
		/// </summary>
		/// <param name="main">The main.</param>
		/// <param name="current">The current.</param>
		/// <returns></returns>
		private IFreeBusy CombineFreeBusy( IFreeBusy main, IFreeBusy current )
		{
			if ( main != null )
			{
				main.MergeWith( current );
			}
			return current;
		}

		#region IGetFreeBusy Members

		/// <summary>
		///     Gets the free busy.
		/// </summary>
		/// <param name="freeBusyRequest">The free busy request.</param>
		/// <returns></returns>
		public IFreeBusy GetFreeBusy( IFreeBusy freeBusyRequest )
		{
			return this.Aggregate<IICalendar, IFreeBusy>( null, ( current, iCal ) => CombineFreeBusy( current, iCal.GetFreeBusy( freeBusyRequest ) ) );
		}

		/// <summary>
		///     Gets the free busy.
		/// </summary>
		/// <param name="fromInclusive">From inclusive.</param>
		/// <param name="toExclusive">To exclusive.</param>
		/// <returns></returns>
		public IFreeBusy GetFreeBusy( IDateTime fromInclusive, IDateTime toExclusive )
		{
			return this.Aggregate<IICalendar, IFreeBusy>( null, ( current, iCal ) => CombineFreeBusy( current, iCal.GetFreeBusy( fromInclusive, toExclusive ) ) );
		}

		/// <summary>
		///     Gets the free busy.
		/// </summary>
		/// <param name="organizer">The organizer.</param>
		/// <param name="contacts">The contacts.</param>
		/// <param name="fromInclusive">From inclusive.</param>
		/// <param name="toExclusive">To exclusive.</param>
		/// <returns></returns>
		public IFreeBusy GetFreeBusy( IOrganizer organizer, IAttendee[] contacts, IDateTime fromInclusive, IDateTime toExclusive )
		{
			return this.Aggregate<IICalendar, IFreeBusy>( null, ( current, iCal ) => CombineFreeBusy( current, iCal.GetFreeBusy( organizer, contacts, fromInclusive, toExclusive ) ) );
		}

		#endregion
	}
}