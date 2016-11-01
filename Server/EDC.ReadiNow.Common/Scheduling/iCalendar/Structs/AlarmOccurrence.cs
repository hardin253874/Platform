// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents a specific occurrence of an <see cref="Alarm" />.
	/// </summary>
	/// <remarks>
	///     The <see cref="AlarmOccurrence" /> contains the <see cref="Period" /> when
	///     the alarm occurs, the <see cref="Alarm" /> that fired, and the
	///     component on which the alarm fired.
	/// </remarks>
	[Serializable]
	public struct AlarmOccurrence : IComparable<AlarmOccurrence>
	{
		/// <summary>
		///     Period.
		/// </summary>
		private IPeriod _period;

		/// <summary>
		///     Component.
		/// </summary>
		private IRecurringComponent _component;

		/// <summary>
		///     Alarm.
		/// </summary>
		private IAlarm _alarm;

		/// <summary>
		///     Gets or sets the period.
		/// </summary>
		/// <value>
		///     The period.
		/// </value>
		public IPeriod Period
		{
			get
			{
				return _period;
			}
			set
			{
				_period = value;
			}
		}

		/// <summary>
		///     Gets or sets the component.
		/// </summary>
		/// <value>
		///     The component.
		/// </value>
		public IRecurringComponent Component
		{
			get
			{
				return _component;
			}
			set
			{
				_component = value;
			}
		}

		/// <summary>
		///     Gets or sets the alarm.
		/// </summary>
		/// <value>
		///     The alarm.
		/// </value>
		public IAlarm Alarm
		{
			get
			{
				return _alarm;
			}
			set
			{
				_alarm = value;
			}
		}

		/// <summary>
		///     Gets or sets the date time.
		/// </summary>
		/// <value>
		///     The date time.
		/// </value>
		public IDateTime DateTime
		{
			get
			{
				return Period.StartTime;
			}
			set
			{
				Period = new Period( value );
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="AlarmOccurrence" /> structure.
		/// </summary>
		/// <param name="ao">The occurrence.</param>
		public AlarmOccurrence( AlarmOccurrence ao )
		{
			_period = ao.Period;
			_component = ao.Component;
			_alarm = ao.Alarm;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="AlarmOccurrence" /> structure.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="dt">The date time.</param>
		/// <param name="rc">The recurring component.</param>
		public AlarmOccurrence( IAlarm a, IDateTime dt, IRecurringComponent rc )
		{
			_alarm = a;
			_period = new Period( dt );
			_component = rc;
		}

		#region IComparable<AlarmOccurrence> Members

		/// <summary>
		///     Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
		///     Value
		///     Meaning
		///     Less than zero
		///     This object is less than the <paramref name="other" /> parameter.
		///     Zero
		///     This object is equal to <paramref name="other" />.
		///     Greater than zero
		///     This object is greater than <paramref name="other" />.
		/// </returns>
		public int CompareTo( AlarmOccurrence other )
		{
			return Period.CompareTo( other.Period );
		}

		#endregion
	}
}