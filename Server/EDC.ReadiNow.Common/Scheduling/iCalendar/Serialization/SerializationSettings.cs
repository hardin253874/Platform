// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     SerializationSettings class.
	/// </summary>
	public class SerializationSettings : ISerializationSettings
	{
		/// <summary>
		///     Calendar type.
		/// </summary>
		private Type _iCalendarType = typeof ( iCalendar );

		/// <summary>
		///     Parsing mode.
		/// </summary>
		private ParsingModeType _parsingMode = ParsingModeType.Strict;

		/// <summary>
		///     Gets or sets the type of the i calendar.
		/// </summary>
		/// <value>
		///     The type of the i calendar.
		/// </value>
		public virtual Type iCalendarType
		{
			get
			{
				return _iCalendarType;
			}
			set
			{
				_iCalendarType = value;
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [ensure accurate line numbers].
		/// </summary>
		/// <value>
		///     <c>true</c> if [ensure accurate line numbers]; otherwise, <c>false</c>.
		/// </value>
		public virtual bool EnsureAccurateLineNumbers
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
		public virtual ParsingModeType ParsingMode
		{
			get
			{
				return _parsingMode;
			}
			set
			{
				_parsingMode = value;
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [store extra serialization data].
		/// </summary>
		/// <value>
		///     <c>true</c> if [store extra serialization data]; otherwise, <c>false</c>.
		/// </value>
		public virtual bool StoreExtraSerializationData
		{
			get;
			set;
		}
	}
}