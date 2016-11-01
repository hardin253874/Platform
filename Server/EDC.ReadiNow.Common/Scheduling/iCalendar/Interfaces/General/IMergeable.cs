// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IMergeable interface.
	/// </summary>
	public interface IMergeable
	{
		/// <summary>
		///     Merges this object with another.
		/// </summary>
		void MergeWith( IMergeable obj );
	}
}