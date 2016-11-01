// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Linq;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     ParameterSerializer class.
	/// </summary>
	public class ParameterSerializer : SerializerBase
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ParameterSerializer" /> class.
		/// </summary>
		public ParameterSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ParameterSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public ParameterSerializer( ISerializationContext ctx )
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
				return typeof ( CalendarParameter );
			}
		}

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public override object Deserialize( TextReader tr )
		{
			// Create a lexer for our text stream
			var lexer = new iCalLexer( tr );
			var parser = new iCalParser( lexer );

			// Get our serialization context
			ISerializationContext ctx = SerializationContext;

			// Parse the component!
			ICalendarParameter p = parser.parameter( ctx, null );

			// Close our text stream
			tr.Close( );

			// Return the parsed parameter
			return p;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var p = obj as ICalendarParameter;
			if ( p != null )
			{
				string result = p.Name + "=";
				string value = string.Join( ",", p.Values.ToArray( ) );

				// "Section 3.2:  Property parameter values MUST NOT contain the DQUOTE character."
				// Therefore, let's strip any double quotes from the value.                
				value = value.Replace( "\"", string.Empty );

				// Surround the parameter value with double quotes, if the value
				// contains any problematic characters.
				if ( value.IndexOfAny( new[]
					{
						';', ':', ','
					} ) >= 0 )
				{
					value = "\"" + value + "\"";
				}
				return result + value;
			}
			return string.Empty;
		}
	}
}