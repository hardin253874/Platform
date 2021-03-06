// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     PeriodSerializer class.
	/// </summary>
	public class PeriodSerializer : EncodableDataTypeSerializer
	{
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
				return typeof ( Period );
			}
		}

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public override object Deserialize( TextReader tr )
		{
			string value = tr.ReadToEnd( );

			var p = CreateAndAssociate( ) as IPeriod;
			var factory = GetService<ISerializerFactory>( );
			if ( p != null && factory != null )
			{
				var dtSerializer = factory.Build( typeof ( IDateTime ), SerializationContext ) as IStringSerializer;
				var durationSerializer = factory.Build( typeof ( TimeSpan ), SerializationContext ) as IStringSerializer;
				if ( dtSerializer != null && durationSerializer != null )
				{
					// Decode the value as necessary
					value = Decode( p, value );

					string[] values = value.Split( '/' );
					if ( values.Length != 2 )
					{
						return false;
					}

					p.StartTime = dtSerializer.Deserialize( new StringReader( values[ 0 ] ) ) as IDateTime;
					p.EndTime = dtSerializer.Deserialize( new StringReader( values[ 1 ] ) ) as IDateTime;
					if ( p.EndTime == null )
					{
						p.Duration = ( TimeSpan ) durationSerializer.Deserialize( new StringReader( values[ 1 ] ) );
					}

					// Only return an object if it has been deserialized correctly.
					if ( p.StartTime != null )
					{
						return p;
					}
				}
			}

			return null;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var p = obj as IPeriod;
			var factory = GetService<ISerializerFactory>( );

			if ( p != null && factory != null )
			{
				// Push the period onto the serialization context stack
				SerializationContext.Push( p );

				try
				{
					var dtSerializer = factory.Build( typeof ( IDateTime ), SerializationContext ) as IStringSerializer;
					var timeSpanSerializer = factory.Build( typeof ( TimeSpan ), SerializationContext ) as IStringSerializer;
					if ( dtSerializer != null && timeSpanSerializer != null )
					{
						var sb = new StringBuilder( );

						// Serialize the start time                    
						sb.Append( dtSerializer.SerializeToString( p.StartTime ) );

						// Serialize the duration
						sb.Append( "/" );
						sb.Append( timeSpanSerializer.SerializeToString( p.Duration ) );

						// Encode the value as necessary
						return Encode( p, sb.ToString( ) );
					}
				}
				finally
				{
					// Pop the period off the serialization context stack
					SerializationContext.Pop( );
				}
			}
			return null;
		}
	}
}