// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.IO;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents the organizer of an event/todo/journal.
	/// </summary>
	[DebuggerDisplay( "{Value}" )]
	[Serializable]
	public class Organizer : EncodableDataType, IOrganizer
	{
		/// <summary>
		///     Gets or sets the sent by.
		/// </summary>
		/// <value>
		///     The sent by.
		/// </value>
		public virtual Uri SentBy
		{
			get
			{
				return new Uri( Parameters.Get( "SENT-BY" ) );
			}
			set
			{
				if ( value != null )
				{
					Parameters.Set( "SENT-BY", value.OriginalString );
				}
				else
				{
					Parameters.Set( "SENT-BY", ( string ) null );
				}
			}
		}

		/// <summary>
		///     Gets or sets the name of the common.
		/// </summary>
		/// <value>
		///     The name of the common.
		/// </value>
		public virtual string CommonName
		{
			get
			{
				return Parameters.Get( "CN" );
			}
			set
			{
				Parameters.Set( "CN", value );
			}
		}

		/// <summary>
		///     Gets or sets the directory entry.
		/// </summary>
		/// <value>
		///     The directory entry.
		/// </value>
		public virtual Uri DirectoryEntry
		{
			get
			{
				return new Uri( Parameters.Get( "DIR" ) );
			}
			set
			{
				if ( value != null )
				{
					Parameters.Set( "DIR", value.OriginalString );
				}
				else
				{
					Parameters.Set( "DIR", ( string ) null );
				}
			}
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		public virtual Uri Value
		{
			get;
			set;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Organizer" /> class.
		/// </summary>
		public Organizer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Organizer" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public Organizer( string value )
			: this( )
		{
			var serializer = new OrganizerSerializer( );
			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="System.Object" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var o = obj as IOrganizer;
			if ( o != null )
			{
				return Equals( Value, o.Value );
			}
// ReSharper disable BaseObjectEqualsIsObjectEquals
			return base.Equals( obj );
// ReSharper restore BaseObjectEqualsIsObjectEquals
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override sealed void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var o = obj as IOrganizer;
			if ( o != null )
			{
				Value = o.Value;
			}
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			int hash = 13;

			if ( Value != null )
			{
				hash = ( hash * 7 ) + Value.GetHashCode( );
			}

			return hash;
		}
	}
}