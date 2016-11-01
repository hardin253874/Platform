// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IGeographicLocation interface.
	/// </summary>
	public interface IGeographicLocation : IEncodableDataType
	{
		/// <summary>
		///     Gets or sets the latitude.
		/// </summary>
		/// <value>
		///     The latitude.
		/// </value>
		double Latitude
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the longitude.
		/// </summary>
		/// <value>
		///     The longitude.
		/// </value>
		double Longitude
		{
			get;
			set;
		}
	}
}