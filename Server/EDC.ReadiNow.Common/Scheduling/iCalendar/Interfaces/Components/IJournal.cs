// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IJournal interface.
	/// </summary>
	public interface IJournal : IRecurringComponent
	{
		/// <summary>
		///     Gets or sets the status.
		/// </summary>
		/// <value>
		///     The status.
		/// </value>
		JournalStatus Status
		{
			get;
			set;
		}
	}
}