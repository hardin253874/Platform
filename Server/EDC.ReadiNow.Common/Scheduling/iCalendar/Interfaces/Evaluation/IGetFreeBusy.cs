// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IGetFreeBusy interface.
	/// </summary>
	public interface IGetFreeBusy
	{
		/// <summary>
		///     Gets the free busy.
		/// </summary>
		/// <param name="freeBusyRequest">The free busy request.</param>
		/// <returns></returns>
		IFreeBusy GetFreeBusy( IFreeBusy freeBusyRequest );

		/// <summary>
		///     Gets the free busy.
		/// </summary>
		/// <param name="fromInclusive">From inclusive.</param>
		/// <param name="toExclusive">To exclusive.</param>
		/// <returns></returns>
		IFreeBusy GetFreeBusy( IDateTime fromInclusive, IDateTime toExclusive );

		/// <summary>
		///     Gets the free busy.
		/// </summary>
		/// <param name="organizer">The organizer.</param>
		/// <param name="contacts">The contacts.</param>
		/// <param name="fromInclusive">From inclusive.</param>
		/// <param name="toExclusive">To exclusive.</param>
		/// <returns></returns>
		IFreeBusy GetFreeBusy( IOrganizer organizer, IAttendee[] contacts, IDateTime fromInclusive, IDateTime toExclusive );
	}
}