// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IStatusCode interface.
	/// </summary>
	public interface IStatusCode : IEncodableDataType
	{
		/// <summary>
		///     Gets or sets the parts.
		/// </summary>
		/// <value>
		///     The parts.
		/// </value>
		int[] Parts
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the primary.
		/// </summary>
		/// <value>
		///     The primary.
		/// </value>
		int Primary
		{
			get;
		}

		/// <summary>
		///     Gets the secondary.
		/// </summary>
		/// <value>
		///     The secondary.
		/// </value>
		int Secondary
		{
			get;
		}

		/// <summary>
		///     Gets the tertiary.
		/// </summary>
		/// <value>
		///     The tertiary.
		/// </value>
		int Tertiary
		{
			get;
		}
	}
}