// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     ICalendarComponentFactory interface.
	/// </summary>
	public class ComponentFactory : ICalendarComponentFactory
	{
		#region ICalendarComponentFactory Members

		/// <summary>
		///     Builds the specified object name.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="uninitialized">
		///     if set to <c>true</c> [uninitialized].
		/// </param>
		/// <returns></returns>
		public virtual ICalendarComponent Build( string objectName, bool uninitialized )
		{
			Type type;

			// Determine the type of component to build.
			switch ( objectName.ToUpper( ) )
			{
				case Components.ALARM:
					type = typeof ( Alarm );
					break;
				case Components.EVENT:
					type = typeof ( Event );
					break;
				case Components.FREEBUSY:
					type = typeof ( FreeBusy );
					break;
				case Components.JOURNAL:
					type = typeof ( Journal );
					break;
				case Components.TIMEZONE:
					type = typeof ( iCalTimeZone );
					break;
				case Components.TODO:
					type = typeof ( Todo );
					break;
				case Components.DAYLIGHT:
				case Components.STANDARD:
					type = typeof ( iCalTimeZoneInfo );
					break;
				default:
					type = typeof ( CalendarComponent );
					break;
			}

			ICalendarComponent c;

			if ( uninitialized )
			{
				// Create a new, uninitialized object (i.e. no constructor has been called).
				c = SerializationUtil.GetUninitializedObject( type ) as ICalendarComponent;
			}
			else
			{
				// Create a new, initialized object.
				c = Activator.CreateInstance( type ) as ICalendarComponent;
			}

			if ( c != null )
			{
				// Assign the name of this component.
				c.Name = objectName.ToUpper( );
			}

			return c;
		}

		#endregion
	}
}