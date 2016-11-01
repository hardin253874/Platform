// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Scheduling.iCalendar.Utility;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Attendee class.
	/// </summary>
	[Serializable]
	public sealed class Attendee : EncodableDataType, IAttendee
	{
		#region IAttendee Members

		/// <summary>
		///     Gets or sets the sent by.
		/// </summary>
		/// <value>
		///     The sent by.
		/// </value>
		public Uri SentBy
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
		public string CommonName
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
		public Uri DirectoryEntry
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
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		public string Type
		{
			get
			{
				return Parameters.Get( "CUTYPE" );
			}
			set
			{
				Parameters.Set( "CUTYPE", value );
			}
		}

		/// <summary>
		///     Gets or sets the members.
		/// </summary>
		/// <value>
		///     The members.
		/// </value>
		public IList<string> Members
		{
			get
			{
				return Parameters.GetMany( "MEMBER" );
			}
			set
			{
				Parameters.Set( "MEMBER", value );
			}
		}

		/// <summary>
		///     Gets or sets the role.
		/// </summary>
		/// <value>
		///     The role.
		/// </value>
		public string Role
		{
			get
			{
				return Parameters.Get( "ROLE" );
			}
			set
			{
				Parameters.Set( "ROLE", value );
			}
		}

		/// <summary>
		///     Gets or sets the participation status.
		/// </summary>
		/// <value>
		///     The participation status.
		/// </value>
		public ParticipationStatus ParticipationStatus
		{
			get
			{
				string partStatus = Parameters.Get( "PARTSTAT" );

				if ( !string.IsNullOrEmpty( partStatus ) )
				{
					return partStatus.FromDescription<ParticipationStatus>( );
				}

				return ParticipationStatus.NotSpecified;
			}
			set
			{
				if ( value == ParticipationStatus.NotSpecified )
				{
					Parameters.Remove( "PARTSTAT" );
				}
				else
				{
					Parameters.Set( "PARTSTAT", value.ToDescription( ) );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="Attendee" /> is RSVP.
		/// </summary>
		/// <value>
		///     <c>true</c> if RSVP; otherwise, <c>false</c>.
		/// </value>
		public bool Rsvp
		{
			get
			{
				bool val;
				string rsvp = Parameters.Get( "RSVP" );
				if ( rsvp != null && bool.TryParse( rsvp, out val ) )
				{
					return val;
				}
				return false;
			}
			set
			{
				string val = value.ToString( ).ToUpper( );
				Parameters.Set( "RSVP", val );
			}
		}

		/// <summary>
		///     Gets or sets the delegated to.
		/// </summary>
		/// <value>
		///     The delegated to.
		/// </value>
		public IList<string> DelegatedTo
		{
			get
			{
				return Parameters.GetMany( "DELEGATED-TO" );
			}
			set
			{
				Parameters.Set( "DELEGATED-TO", value );
			}
		}

		/// <summary>
		///     Gets or sets the delegated from.
		/// </summary>
		/// <value>
		///     The delegated from.
		/// </value>
		public IList<string> DelegatedFrom
		{
			get
			{
				return Parameters.GetMany( "DELEGATED-FROM" );
			}
			set
			{
				Parameters.Set( "DELEGATED-FROM", value );
			}
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		[DataMember( Order = 1 )]
		public Uri Value
		{
			get;
			set;
		}

		#endregion

		/// <summary>
		///     Initializes a new instance of the <see cref="Attendee" /> class.
		/// </summary>
		public Attendee( )
		{
		}

		public Attendee( Uri attendee )
		{
			Value = attendee;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Attendee" /> class.
		/// </summary>
		/// <param name="attendeeUri">The attendee URI.</param>
		/// <exception cref="System.ArgumentException">attendeeUri</exception>
		public Attendee( string attendeeUri )
		{
			if ( !Uri.IsWellFormedUriString( attendeeUri, UriKind.Absolute ) )
			{
				throw new ArgumentException( "attendeeUri" );
			}
			Value = new Uri( attendeeUri );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Attendee" /> class.
		/// </summary>
		/// <param name="emailAddress">The email address.</param>
		/// <param name="commonName">Name of the common.</param>
		public Attendee( string emailAddress, string commonName )
			: this( emailAddress, commonName, ParticipantRole.Unknown )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Attendee" /> class.
		/// </summary>
		/// <param name="emailAddress">The email address.</param>
		/// <param name="role">The role.</param>
		public Attendee( string emailAddress, ParticipantRole role )
			: this( emailAddress, null, role )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Attendee" /> class.
		/// </summary>
		/// <param name="emailAddress">The email address.</param>
		/// <param name="commonName">Name of the common.</param>
		/// <param name="role">The role.</param>
		public Attendee( string emailAddress, string commonName, ParticipantRole role )
			: this( "MAILTO:" + emailAddress )
		{
			if ( commonName != null )
			{
				CommonName = commonName;
			}

			switch ( role )
			{
				case ParticipantRole.Chair:
					Role = "CHAIR";
					break;
				case ParticipantRole.NonParticipant:
					Role = "NON-PARTICIPANT";
					break;
				case ParticipantRole.OptionalParticipant:
					Role = "OPT-PARTICIPANT";
					break;
				case ParticipantRole.RequiredParticipant:
					Role = "REQ-PARTICIPANT";
					break;
			}
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
			var a = obj as IAttendee;
			if ( a != null )
			{
				return Equals( Value, a.Value );
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
		public override void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var a = obj as IAttendee;
			if ( a != null )
			{
				Value = a.Value;
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