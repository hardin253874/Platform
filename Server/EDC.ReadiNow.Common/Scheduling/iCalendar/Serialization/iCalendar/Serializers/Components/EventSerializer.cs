// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     EventSerializer class.
	/// </summary>
	public class EventSerializer : UniqueComponentSerializer
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EventSerializer" /> class.
		/// </summary>
		public EventSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EventSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public EventSerializer( ISerializationContext ctx )
			: base( ctx )
		{
		}

		/// <summary>
		///     Gets the type of the target.
		/// </summary>
		/// <value>
		///     The type of the target.
		/// </value>
		public override Type TargetType
		{
			get
			{
				return typeof ( Event );
			}
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			if ( obj is IEvent )
			{
				var evt = ( ( IEvent ) obj ).Copy<IEvent>( );

				// NOTE: DURATION and DTEND cannot co-exist on an event.
				// Some systems do not support DURATION, so we serialize
				// all events using DTEND instead.
				if ( evt.Properties.ContainsKey( "DURATION" ) && evt.Properties.ContainsKey( "DTEND" ) )
				{
					evt.Properties.Remove( "DURATION" );
				}

				return base.SerializeToString( evt );
			}
			return base.SerializeToString( obj );
		}
	}
}