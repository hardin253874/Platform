// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IRequestStatus interface.
	/// </summary>
	public interface IRequestStatus : IEncodableDataType
	{
		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		string Description
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the extra data.
		/// </summary>
		/// <value>
		///     The extra data.
		/// </value>
		string ExtraData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the status code.
		/// </summary>
		/// <value>
		///     The status code.
		/// </value>
		IStatusCode StatusCode
		{
			get;
			set;
		}
	}
}