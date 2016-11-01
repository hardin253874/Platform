// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     iCalendarSerializer class.
	/// </summary>
// ReSharper disable InconsistentNaming
	public class iCalendarSerializer : ComponentSerializer
// ReSharper restore InconsistentNaming
	{
		/// <summary>
		///     iCalendar.
		/// </summary>
		private readonly IICalendar _iCalendar;

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalendarSerializer" /> class.
		/// </summary>
		public iCalendarSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalendarSerializer" /> class.
		/// </summary>
		/// <param name="iCal">The i cal.</param>
		public iCalendarSerializer( IICalendar iCal )
		{
			_iCalendar = iCal;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalendarSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public iCalendarSerializer( ISerializationContext ctx )
			: base( ctx )
		{
		}

		/// <summary>
		///     Gets the property sorter.
		/// </summary>
		/// <value>
		///     The property sorter.
		/// </value>
		protected override IComparer<ICalendarProperty> PropertySorter
		{
			get
			{
				return new CalendarPropertySorter( );
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

				// Parse the iCalendar(s)!
				IICalendarCollection iCalendars = parser.icalendar( SerializationContext );

				// Close our text stream
				tr.Close( );

				// Return the parsed iCalendar(s)
				return iCalendars;
			}
			return null;
		}

		/// <summary>
		///     Serializes the specified filename.
		/// </summary>
		/// <param name="filename">The filename.</param>
		[Obsolete( "Use the Serialize(IICalendar iCal, string filename) method instead." )]
		public virtual void Serialize( string filename )
		{
			if ( _iCalendar != null )
			{
				Serialize( _iCalendar, filename );
			}
		}

		/// <summary>
		///     Serializes the specified i cal.
		/// </summary>
		/// <param name="iCal">The i cal.</param>
		/// <param name="filename">The filename.</param>
		public virtual void Serialize( IICalendar iCal, string filename )
		{
			using ( var fs = new FileStream( filename, FileMode.Create ) )
			{
				Serialize( iCal, fs, new UTF8Encoding( ) );
			}
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <returns></returns>
		[Obsolete( "Use the SerializeToString(ICalendarObject obj) method instead." )]
		public virtual string SerializeToString( )
		{
			return SerializeToString( _iCalendar );
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var iCal = obj as IICalendar;
			if ( iCal != null )
			{
				// Ensure VERSION and PRODUCTID are both set,
				// as they are required by RFC5545.
				var copy = iCal.Copy<IICalendar>( );
				if ( string.IsNullOrEmpty( copy.Version ) )
				{
					copy.Version = CalendarVersions.v2_0;
				}
				if ( string.IsNullOrEmpty( copy.ProductId ) )
				{
					copy.ProductId = CalendarProductIDs.Default;
				}

				return base.SerializeToString( copy );
			}

			return base.SerializeToString( obj );
		}

		/// <summary>
		///     CalendarPropertySorter class.
		/// </summary>
		private class CalendarPropertySorter : IComparer<ICalendarProperty>
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

				// Alphabetize all properties except VERSION, which should appear first. 
				if ( string.Equals( "VERSION", x.Name, StringComparison.InvariantCultureIgnoreCase ) )
				{
					return -1;
				}

				if ( string.Equals( "VERSION", y.Name, StringComparison.InvariantCultureIgnoreCase ) )
				{
					return 1;
				}
				return String.CompareOrdinal( x.Name, y.Name );
			}

			#endregion
		}
	}
}