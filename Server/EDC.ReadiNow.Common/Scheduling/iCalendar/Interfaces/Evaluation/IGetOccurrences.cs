// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IGetOccurrences interface.
	/// </summary>
	public interface IGetOccurrences
	{
		/// <summary>
		///     Clears a previous evaluation, usually because one of the
		///     key elements used for evaluation has changed
		///     (Start, End, Duration, recurrence rules, exceptions, etc.).
		/// </summary>
		void ClearEvaluation( );

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
		/// <returns>A list of Periods representing the occurrences of this object.</returns>
		IList<Occurrence> GetOccurrences( IDateTime dt );

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		IList<Occurrence> GetOccurrences( DateTime dt );

		/// <summary>
		///     Returns all occurrences of this component that start within the date range provided.
		///     All components occurring between <paramref name="startTime" /> and <paramref name="endTime" />
		///     will be returned.
		/// </summary>
		/// <param name="startTime">The starting date range</param>
		/// <param name="endTime">The ending date range</param>
		IList<Occurrence> GetOccurrences( IDateTime startTime, IDateTime endTime );

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <returns></returns>
		IList<Occurrence> GetOccurrences( DateTime startTime, DateTime endTime );
	}

	/// <summary>
	///     IGetOccurrencesTyped interface.
	/// </summary>
	public interface IGetOccurrencesTyped : IGetOccurrences
	{
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
		/// <param name="dt">The date for which to return occurrences.</param>
		/// <returns>A list of Periods representing the occurrences of this object.</returns>
		IList<Occurrence> GetOccurrences<T>( IDateTime dt ) where T : IRecurringComponent;

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		IList<Occurrence> GetOccurrences<T>( DateTime dt ) where T : IRecurringComponent;

		/// <summary>
		///     Returns all occurrences of components of type T that start within the date range provided.
		///     All components occurring between <paramref name="startTime" /> and <paramref name="endTime" />
		///     will be returned.
		/// </summary>
		/// <param name="startTime">The starting date range</param>
		/// <param name="endTime">The ending date range</param>
		IList<Occurrence> GetOccurrences<T>( IDateTime startTime, IDateTime endTime ) where T : IRecurringComponent;

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <returns></returns>
		IList<Occurrence> GetOccurrences<T>( DateTime startTime, DateTime endTime ) where T : IRecurringComponent;
	}
}