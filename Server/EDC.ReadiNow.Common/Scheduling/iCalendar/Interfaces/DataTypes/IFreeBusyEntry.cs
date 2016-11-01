// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IFreeBusyEntry interface.
	/// </summary>
	public interface IFreeBusyEntry : IPeriod
	{
		/// <summary>
		///     Gets or sets the status.
		/// </summary>
		/// <value>
		///     The status.
		/// </value>
		FreeBusyStatus Status
		{
			get;
			set;
		}
	}
}