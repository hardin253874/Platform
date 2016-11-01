// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     ISerializationSettings interface.
	/// </summary>
	public interface ISerializationSettings
	{
		/// <summary>
		///     Gets or sets a value indicating whether [ensure accurate line numbers].
		/// </summary>
		/// <value>
		///     <c>true</c> if [ensure accurate line numbers]; otherwise, <c>false</c>.
		/// </value>
		bool EnsureAccurateLineNumbers
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the parsing mode.
		/// </summary>
		/// <value>
		///     The parsing mode.
		/// </value>
		ParsingModeType ParsingMode
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [store extra serialization data].
		/// </summary>
		/// <value>
		///     <c>true</c> if [store extra serialization data]; otherwise, <c>false</c>.
		/// </value>
		bool StoreExtraSerializationData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the type of the i calendar.
		/// </summary>
		/// <value>
		///     The type of the i calendar.
		/// </value>
// ReSharper disable InconsistentNaming
		Type iCalendarType
// ReSharper restore InconsistentNaming
		{
			get;
			set;
		}
	}

	/// <summary>
	///     ParsingModeType enumeration.
	/// </summary>
	public enum ParsingModeType
	{
		Strict,
		Loose
	}
}