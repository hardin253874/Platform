// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     SerializerFactory class.
	/// </summary>
	public class SerializerFactory : ISerializerFactory
	{
		/// <summary>
		///     Data Type Serializer Factory
		/// </summary>
		private readonly ISerializerFactory _dataTypeSerializerFactory;

		/// <summary>
		///     Initializes a new instance of the <see cref="SerializerFactory" /> class.
		/// </summary>
		public SerializerFactory( )
		{
			_dataTypeSerializerFactory = new DataTypeSerializerFactory( );
		}

		/// <summary>
		///     Returns a serializer that can be used to serialize and object
		///     of type <paramref name="objectType" />.
		///     <note>
		///         TODO: Add support for caching.
		///     </note>
		/// </summary>
		/// <param name="objectType">The type of object to be serialized.</param>
		/// <param name="ctx">The serialization context.</param>
		public virtual ISerializer Build( Type objectType, ISerializationContext ctx )
		{
			if ( objectType != null )
			{
				ISerializer s;

				if ( typeof ( IICalendar ).IsAssignableFrom( objectType ) )
				{
					s = new iCalendarSerializer( );
				}
				else if ( typeof ( ICalendarComponent ).IsAssignableFrom( objectType ) )
				{
					if ( typeof ( IEvent ).IsAssignableFrom( objectType ) )
					{
						s = new EventSerializer( );
					}
					else if ( typeof ( IUniqueComponent ).IsAssignableFrom( objectType ) )
					{
						s = new UniqueComponentSerializer( );
					}
					else
					{
						s = new ComponentSerializer( );
					}
				}
				else if ( typeof ( ICalendarProperty ).IsAssignableFrom( objectType ) )
				{
					s = new PropertySerializer( );
				}
				else if ( typeof ( ICalendarParameter ).IsAssignableFrom( objectType ) )
				{
					s = new ParameterSerializer( );
				}
				else if ( typeof ( string ).IsAssignableFrom( objectType ) )
				{
					s = new StringSerializer( );
				}
				else if ( objectType.IsEnum )
				{
					s = new EnumSerializer( objectType );
				}
				else if ( typeof ( TimeSpan ).IsAssignableFrom( objectType ) )
				{
					s = new TimeSpanSerializer( );
				}
				else if ( typeof ( int ).IsAssignableFrom( objectType ) )
				{
					s = new IntegerSerializer( );
				}
				else if ( typeof ( Uri ).IsAssignableFrom( objectType ) )
				{
					s = new UriSerializer( );
				}
				else if ( typeof ( ICalendarDataType ).IsAssignableFrom( objectType ) )
				{
					s = _dataTypeSerializerFactory.Build( objectType, ctx );
				}
					// Default to a string serializer, which simply calls
					// ToString() on the value to serialize it.
				else
				{
					s = new StringSerializer( );
				}

				// Set the serialization context
				if ( s != null )
				{
					s.SerializationContext = ctx;
				}

				return s;
			}
			return null;
		}
	}
}