// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     FIXME: implement this.
	/// </summary>
	public class ComponentPropertyConsolidator : ISerializationProcessor<ICalendarComponent>
	{
		#region ISerializationProcessor<ICalendarComponent> Members

		/// <summary>
		///     Pres the serialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void PreSerialization( ICalendarComponent obj )
		{
		}

		/// <summary>
		///     Posts the serialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void PostSerialization( ICalendarComponent obj )
		{
		}

		/// <summary>
		///     Pres the deserialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void PreDeserialization( ICalendarComponent obj )
		{
		}

		/// <summary>
		///     Posts the deserialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void PostDeserialization( ICalendarComponent obj )
		{
		}

		#endregion
	}
}