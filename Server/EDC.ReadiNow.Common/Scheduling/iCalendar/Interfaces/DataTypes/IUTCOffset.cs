// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IUTCOffset interface.
	/// </summary>
	public interface IUtcOffset : IEncodableDataType
	{
		/// <summary>
		///     Gets or sets the hours.
		/// </summary>
		/// <value>
		///     The hours.
		/// </value>
		int Hours
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the minutes.
		/// </summary>
		/// <value>
		///     The minutes.
		/// </value>
		int Minutes
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="IUtcOffset" /> is positive.
		/// </summary>
		/// <value>
		///     <c>true</c> if positive; otherwise, <c>false</c>.
		/// </value>
		bool Positive
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the seconds.
		/// </summary>
		/// <value>
		///     The seconds.
		/// </value>
		int Seconds
		{
			get;
			set;
		}

		/// <summary>
		///     To the local.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		DateTime ToLocal( DateTime dt );

		/// <summary>
		///     To the UTC.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		DateTime ToUtc( DateTime dt );
	}
}