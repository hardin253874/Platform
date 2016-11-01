// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     StringSerializer class.
	/// </summary>
	public class StringSerializer : EncodableDataTypeSerializer
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="StringSerializer" /> class.
		/// </summary>
		public StringSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="StringSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public StringSerializer( ISerializationContext ctx )
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
				return typeof ( string );
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
				string value = tr.ReadToEnd( );

				// NOTE: this can deserialize into an IList<string> or simply a string,
				// depending on the input text.  Anything that uses this serializer should
				// be prepared to receive either a string, or an IList<string>.

				bool serializeAsList = false;

				// Determine if we can serialize this property
				// with multiple values per line.
				var co = SerializationContext.Peek( ) as ICalendarObject;
				if ( co is ICalendarProperty )
				{
					serializeAsList = GetService<IDataTypeMapper>( ).GetPropertyAllowsMultipleValues( co );
				}

				value = TextUtil.Normalize( value, SerializationContext ).ReadToEnd( );

				// Try to decode the string
				EncodableDataType dt = null;
				if ( co != null )
				{
					dt = new EncodableDataType
						{
							AssociatedObject = co
						};
				}

				var escapedValues = new List<string>( );
				var values = new List<string>( );

				int i = 0;
				if ( serializeAsList )
				{
					MatchCollection matches = Regex.Matches( value, @"[^\\](,)" );
					foreach ( Match match in matches )
					{
// ReSharper disable ConditionIsAlwaysTrueOrFalse
						string newValue = dt != null ? Decode( dt, value.Substring( i, match.Index - i + 1 ) ) : value.Substring( i, match.Index - i + 1 );
// ReSharper restore ConditionIsAlwaysTrueOrFalse
						escapedValues.Add( newValue );
						values.Add( Unescape( newValue ) );
						i = match.Index + 2;
					}
				}

				if ( i < value.Length )
				{
					string newValue = dt != null ? Decode( dt, value.Substring( i, value.Length - i ) ) : value.Substring( i, value.Length - i );
					escapedValues.Add( newValue );
					values.Add( Unescape( newValue ) );
				}

				var calendarProperty = co as ICalendarProperty;

				if ( calendarProperty != null )
				{
					// Determine if our we're supposed to store extra information during
					// the serialization process.  If so, let's store the escaped value.
					var settings = GetService<ISerializationSettings>( );
					if ( settings != null &&
					     settings.StoreExtraSerializationData )
					{
						// Store the escaped value
						calendarProperty.SetService( "EscapedValue", escapedValues.Count == 1 ?
							                                             escapedValues[ 0 ] :
							                                             ( object ) escapedValues );
					}
				}

				// Return either a single value, or the entire list.
				if ( values.Count == 1 )
				{
					return values[ 0 ];
				}

				return values;
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
			if ( obj != null )
			{
				GetService<ISerializationSettings>( );

				var values = new List<string>( );

				var item = obj as string;

				if ( item != null )
				{
					// Object to be serialized is a string already
					values.Add( item );
				}
				else
				{
					var children = obj as IEnumerable;

					if ( children != null )
					{
						// Object is a list of objects (probably IList<string>).
						values.AddRange( from object child in children
						                 select child.ToString( ) );
					}
					else
					{
						// Serialize the object as a string.
						values.Add( obj.ToString( ) );
					}
				}

				var co = SerializationContext.Peek( ) as ICalendarObject;
				if ( co != null )
				{
					// Encode the string as needed.
					var dt = new EncodableDataType
						{
							AssociatedObject = co
						};
					for ( int i = 0; i < values.Count; i++ )
					{
						values[ i ] = Encode( dt, Escape( values[ i ] ) );
					}

					return string.Join( ",", values.ToArray( ) );
				}

				for ( int i = 0; i < values.Count; i++ )
				{
					values[ i ] = Escape( values[ i ] );
				}
				return string.Join( ",", values.ToArray( ) );
			}
			return null;
		}

		/// <summary>
		///     Escapes the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected virtual string Escape( string value )
		{
			// added null check - you can't call .Replace on a null
			// string, but you can just return null as a string
			if ( value != null )
			{
				// NOTE: fixed a bug that caused text parsing to fail on
				// programmatically entered strings.
				// SEE unit test SERIALIZE25().
				value = value.Replace( "\r\n", @"\n" );
				value = value.Replace( "\r", @"\n" );
				value = value.Replace( "\n", @"\n" );
				value = value.Replace( ";", @"\;" );
				value = value.Replace( ",", @"\," );
			}
			return value;
		}

		/// <summary>
		///     Unescapes the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected virtual string Unescape( string value )
		{
			// added null check - you can't call .Replace on a null
			// string, but you can just return null as a string
			if ( value != null )
			{
				value = value.Replace( @"\n", "\n" );
				value = value.Replace( @"\N", "\n" );
				value = value.Replace( @"\;", ";" );
				value = value.Replace( @"\,", "," );
				// NOTE: double quotes aren't escaped in RFC2445, but are in Mozilla Sunbird (0.5-)
				value = value.Replace( "\\\"", "\"" );

				// Replace all single-backslashes with double-backslashes.
				value = Regex.Replace( value, @"(?<!\\)\\(?!\\)", "\\\\" );

				// Unescape double backslashes
				value = value.Replace( @"\\", @"\" );
			}
			return value;
		}
	}
}