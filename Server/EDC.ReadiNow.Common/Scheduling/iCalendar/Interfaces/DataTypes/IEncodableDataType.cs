// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IEncodableDataType interface.
	/// </summary>
	public interface IEncodableDataType : ICalendarDataType
	{
		/// <summary>
		///     Gets or sets the encoding.
		/// </summary>
		/// <value>
		///     The encoding.
		/// </value>
		string Encoding
		{
			get;
			set;
		}
	}
}