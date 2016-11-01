// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     PropertySerializer class.
	/// </summary>
	public class PropertySerializer : SerializerBase
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="PropertySerializer" /> class.
		/// </summary>
		public PropertySerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PropertySerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public PropertySerializer( ISerializationContext ctx )
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
				return typeof ( CalendarProperty );
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

				// Parse the component!
				ICalendarProperty p = parser.property( ctx, null );

				// Close our text stream
				tr.Close( );

				// Return the parsed property
				return p;
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
			var prop = obj as ICalendarProperty;
			if ( prop != null )
			{
				// Don't serialize the property if the value is null                

				// Push this object on the serialization context.
				SerializationContext.Push( prop );

				var mapper = GetService<IDataTypeMapper>( );
				Type serializedType = mapper.GetPropertyMapping( prop );

				// Get a serializer factory that we can use to serialize
				// the property and parameter values
				var sf = GetService<ISerializerFactory>( );

				var result = new StringBuilder( );
				if ( prop.Values != null &&
				     prop.Values.Any( ) )
				{
					foreach ( object v in prop.Values )
					{
						// Only serialize the value to a string if it
						// is non-null.
						if ( v != null )
						{
							Type valueType = v.GetType( );

							// Use the determined type of the value if the property
							// mapping didn't yield any results.
							if ( serializedType == null )
							{
								serializedType = valueType;
							}

							Type genericListOfSerializedType = typeof ( IEnumerable<> ).MakeGenericType( new[]
								{
									serializedType
								} );
							if ( genericListOfSerializedType.IsAssignableFrom( valueType ) )
							{
								// Serialize an enumerable list of properties
								foreach ( object value in ( IEnumerable ) v )
								{
									SerializeValue( sf, prop, serializedType, value, result );
								}
							}
							else
							{
								// Serialize an individual value
								SerializeValue( sf, prop, valueType, v, result );
							}
						}
					}
				}
				else
				{
					// If there was no value, then we need to preserve an 'empty' value                    
					result.Append( TextUtil.WrapLines( prop.Name + ":" ) );
				}

				// Pop the object off the serialization context.
				SerializationContext.Pop( );

				return result.ToString( );
			}
			return null;
		}

		/// <summary>
		///     Serializes the value.
		/// </summary>
		/// <param name="sf">The sf.</param>
		/// <param name="prop">The prop.</param>
		/// <param name="valueType">Type of the value.</param>
		/// <param name="v">The v.</param>
		/// <param name="result">The result.</param>
		protected void SerializeValue( ISerializerFactory sf, ICalendarProperty prop, Type valueType, object v, StringBuilder result )
		{
			// Get a serializer to serialize the property's value.
			// If we can't serialize the property's value, the next step is worthless anyway.
			var valueSerializer = sf.Build( valueType, SerializationContext ) as IStringSerializer;
			if ( valueSerializer != null )
			{
				// Iterate through each value to be serialized,
				// and give it a property (with parameters).
				// FIXME: this isn't always the way this is accomplished.
				// Multiple values can often be serialized within the
				// same property.  How should we fix this?

				// NOTE:
				// We Serialize the property's value first, as during 
				// serialization it may modify our parameters.
				// FIXME: the "parameter modification" operation should
				// be separated from serialization. Perhaps something
				// like PreSerialize(), etc.
				string value = valueSerializer.SerializeToString( v );

				// Get the list of parameters we'll be serializing
				ICalendarParameterCollection parameterList = prop.Parameters;

				var calendarDataType = v as ICalendarDataType;

				if ( calendarDataType != null )
				{
					parameterList = calendarDataType.Parameters;
				}

				var sb = new StringBuilder( prop.Name );
				if ( parameterList.Count > 0 )
				{
					// Get a serializer for parameters
					var parameterSerializer = sf.Build( typeof ( ICalendarParameter ), SerializationContext ) as IStringSerializer;
					if ( parameterSerializer != null )
					{
						// Serialize each parameter
						// Separate parameters with semicolons
						sb.Append( ";" );
						sb.Append( string.Join( ";", parameterList.Select( parameterSerializer.SerializeToString ).ToArray( ) ) );
					}
				}
				sb.Append( ":" );
				sb.Append( value );

				result.Append( TextUtil.WrapLines( sb.ToString( ) ) );
			}
		}
	}
}