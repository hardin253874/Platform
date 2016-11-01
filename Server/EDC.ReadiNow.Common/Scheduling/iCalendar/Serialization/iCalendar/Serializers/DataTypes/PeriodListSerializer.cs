// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     PeriodListSerializer class.
	/// </summary>
	public class PeriodListSerializer : EncodableDataTypeSerializer
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
				return typeof ( PeriodList );
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

			// Create the day specifier and associate it with a calendar object
			var rdt = CreateAndAssociate( ) as IPeriodList;
			var factory = GetService<ISerializerFactory>( );
			if ( rdt != null && factory != null )
			{
				// Decode the value, if necessary
				value = Decode( rdt, value );

				var dtSerializer = factory.Build( typeof ( IDateTime ), SerializationContext ) as IStringSerializer;
				var periodSerializer = factory.Build( typeof ( IPeriod ), SerializationContext ) as IStringSerializer;
				if ( dtSerializer != null && periodSerializer != null )
				{
					string[] values = value.Split( ',' );
					foreach ( string v in values )
					{
						var dt = dtSerializer.Deserialize( new StringReader( v ) ) as IDateTime;
						var p = periodSerializer.Deserialize( new StringReader( v ) ) as IPeriod;

						if ( dt != null )
						{
							dt.AssociatedObject = rdt.AssociatedObject;
							rdt.Add( dt );
						}
						else if ( p != null )
						{
							p.AssociatedObject = rdt.AssociatedObject;
							rdt.Add( p );
						}
					}
					return rdt;
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
			var rdt = obj as IPeriodList;
			var factory = GetService<ISerializerFactory>( );

			if ( rdt != null && factory != null )
			{
				var dtSerializer = factory.Build( typeof ( IDateTime ), SerializationContext ) as IStringSerializer;
				var periodSerializer = factory.Build( typeof ( IPeriod ), SerializationContext ) as IStringSerializer;
				if ( dtSerializer != null && periodSerializer != null )
				{
					var parts = new List<string>( );

					foreach ( IPeriod p in rdt )
					{
						if ( p.EndTime != null )
						{
							parts.Add( periodSerializer.SerializeToString( p ) );
						}
						else if ( p.StartTime != null )
						{
							parts.Add( dtSerializer.SerializeToString( p.StartTime ) );
						}
					}

					return Encode( rdt, string.Join( ",", parts.ToArray( ) ) );
				}
			}
			return null;
		}
	}
}