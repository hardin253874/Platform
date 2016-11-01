// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ITrigger interface.
	/// </summary>
	public interface ITrigger : IEncodableDataType
	{
		/// <summary>
		///     Gets or sets the date time.
		/// </summary>
		/// <value>
		///     The date time.
		/// </value>
		IDateTime DateTime
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the duration.
		/// </summary>
		/// <value>
		///     The duration.
		/// </value>
		TimeSpan? Duration
		{
			get;
			set;
		}

		/// <summary>
		///     Gets a value indicating whether this instance is relative.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is relative; otherwise, <c>false</c>.
		/// </value>
		bool IsRelative
		{
			get;
		}

		/// <summary>
		///     Gets or sets the related.
		/// </summary>
		/// <value>
		///     The related.
		/// </value>
		TriggerRelation Related
		{
			get;
			set;
		}
	}
}