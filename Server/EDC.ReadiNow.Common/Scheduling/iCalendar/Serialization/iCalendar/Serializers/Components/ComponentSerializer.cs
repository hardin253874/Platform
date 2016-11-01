// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     ComponentSerializer class.
	/// </summary>
	public class ComponentSerializer : SerializerBase
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ComponentSerializer" /> class.
		/// </summary>
		public ComponentSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ComponentSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public ComponentSerializer( ISerializationContext ctx )
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
				return typeof ( CalendarComponent );
			}
		}

		/// <summary>
		///     Gets the property sorter.
		/// </summary>
		/// <value>
		///     The property sorter.
		/// </value>
		protected virtual IComparer<ICalendarProperty> PropertySorter
		{
			get
			{
				return new PropertyAlphabetizer( );
			}
		}

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public override object Deserialize( TextReader tr )
		{
			if ( tr != null )
			{
				// Normalize the text before parsing it
				tr = TextUtil.Normalize( tr, SerializationContext );

				// Create a lexer for our text stream
				var lexer = new iCalLexer( tr );
				var parser = new iCalParser( lexer );

				// Get our serialization context
				ISerializationContext ctx = SerializationContext;

				// Get a serializer factory from our serialization services
				var sf = GetService<ISerializerFactory>( );

				// Get a calendar component factory from our serialization services
				var cf = GetService<ICalendarComponentFactory>( );

				// Parse the component!
				ICalendarComponent component = parser.component( ctx, sf, cf, null );

				// Close our text stream
				tr.Close( );

				// Return the parsed component
				return component;
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
			var c = obj as ICalendarComponent;
			if ( c != null )
			{
				var sb = new StringBuilder( );
				sb.Append( TextUtil.WrapLines( "BEGIN:" + c.Name.ToUpper( ) ) );

				// Get a serializer factory
				var sf = GetService<ISerializerFactory>( );

				// Sort the calendar properties in alphabetical order before
				// serializing them!
				var properties = new List<ICalendarProperty>( c.Properties );

				// FIXME: remove this try/catch
				properties.Sort( PropertySorter );

				// Serialize properties
				foreach ( ICalendarProperty p in properties )
				{
					// Get a serializer for each property.
					var serializer = sf.Build( p.GetType( ), SerializationContext ) as IStringSerializer;
					if ( serializer != null )
					{
						sb.Append( serializer.SerializeToString( p ) );
					}
				}

				// Serialize child objects
				if ( sf != null )
				{
					foreach ( ICalendarObject child in c.Children )
					{
						// Get a serializer for each child object.
						var serializer = sf.Build( child.GetType( ), SerializationContext ) as IStringSerializer;
						if ( serializer != null )
						{
							sb.Append( serializer.SerializeToString( child ) );
						}
					}
				}

				sb.Append( TextUtil.WrapLines( "END:" + c.Name.ToUpper( ) ) );
				return sb.ToString( );
			}
			return null;
		}

		/// <summary>
		///     PropertyAlphabetizer class.
		/// </summary>
		public class PropertyAlphabetizer : IComparer<ICalendarProperty>
		{
			#region IComparer<ICalendarProperty> Members

			/// <summary>
			///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			///     Value
			///     Condition
			///     Less than zero
			///     <paramref name="x" /> is less than <paramref name="y" />.
			///     Zero
			///     <paramref name="x" /> equals <paramref name="y" />.
			///     Greater than zero
			///     <paramref name="x" /> is greater than <paramref name="y" />.
			/// </returns>
			public int Compare( ICalendarProperty x, ICalendarProperty y )
			{
				if ( x == y || ( x == null && y == null ) )
				{
					return 0;
				}

				if ( x == null )
				{
					return -1;
				}

				if ( y == null )
				{
					return 1;
				}

				return String.Compare( x.Name, y.Name, StringComparison.OrdinalIgnoreCase );
			}

			#endregion
		}
	}
}