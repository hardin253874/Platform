// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Represents a unique component, a component with a unique UID,
	///     which can be used to uniquely identify the component.
	/// </summary>
	[Serializable]
	public class UniqueComponent : CalendarComponent, IUniqueComponent
	{
		// TODO: Add AddRelationship() public method.
		// This method will add the UID of a related component
		// to the Related_To property, along with any "RELTYPE"
		// parameter ("PARENT", "CHILD", "SIBLING", or other)
		// TODO: Add RemoveRelationship() public method.        

		/// <summary>
		///     Initializes a new instance of the <see cref="UniqueComponent" /> class.
		/// </summary>
		public UniqueComponent( )
		{
			Initialize( );
			EnsureProperties( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="UniqueComponent" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public UniqueComponent( string name )
			: base( name )
		{
			Initialize( );
			EnsureProperties( );
		}

		/// <summary>
		///     Ensures the properties.
		/// </summary>
		private void EnsureProperties( )
		{
			if ( string.IsNullOrEmpty( Uid ) )
			{
				// Create a new UID for the component
				Uid = new UidFactory( ).Build( );
			}

			// NOTE: removed setting the 'CREATED' property here since it breaks serialization.
			// See https://sourceforge.net/projects/dday-ical/forums/forum/656447/topic/3754354
			if ( DtStamp == null )
			{
				// Here, we don't simply set to DateTime.Now because DateTime.Now contains milliseconds, and
				// the iCalendar standard doesn't care at all about milliseconds.  Therefore, when comparing
				// two calendars, one generated, and one loaded from file, they may be functionally identical,
				// but be determined to be different due to millisecond differences.
				//
				// NOTE: also ensure we're in UTC, so our CLR implementation closely matches the RFC.
				// See bug #3485766.
				DateTime now = DateTime.UtcNow;
				DtStamp = new iCalDateTime( now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second )
					{
						IsUniversalTime = true
					};
			}
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			Properties.ItemAdded += Properties_ItemAdded;
			Properties.ItemRemoved += Properties_ItemRemoved;
		}

		/// <summary>
		///     Gets or sets the attendees.
		/// </summary>
		/// <value>
		///     The attendees.
		/// </value>
		public virtual IList<IAttendee> Attendees
		{
			get
			{
				return Properties.GetMany<IAttendee>( "ATTENDEE" );
			}
			set
			{
				Properties.Set( "ATTENDEE", value );
			}
		}

		/// <summary>
		///     Gets or sets the comments.
		/// </summary>
		/// <value>
		///     The comments.
		/// </value>
		public virtual IList<string> Comments
		{
			get
			{
				return Properties.GetMany<string>( "COMMENT" );
			}
			set
			{
				Properties.Set( "COMMENT", value );
			}
		}

		/// <summary>
		///     Gets or sets the DT stamp.
		/// </summary>
		/// <value>
		///     The DT stamp.
		/// </value>
		public virtual IDateTime DtStamp
		{
			get
			{
				return Properties.Get<IDateTime>( "DTSTAMP" );
			}
			set
			{
				Properties.Set( "DTSTAMP", value );
			}
		}

		/// <summary>
		///     Gets or sets the organizer.
		/// </summary>
		/// <value>
		///     The organizer.
		/// </value>
		public virtual IOrganizer Organizer
		{
			get
			{
				return Properties.Get<IOrganizer>( "ORGANIZER" );
			}
			set
			{
				Properties.Set( "ORGANIZER", value );
			}
		}

		/// <summary>
		///     Gets or sets the request statuses.
		/// </summary>
		/// <value>
		///     The request statuses.
		/// </value>
		public virtual IList<IRequestStatus> RequestStatuses
		{
			get
			{
				return Properties.GetMany<IRequestStatus>( "REQUEST-STATUS" );
			}
			set
			{
				Properties.Set( "REQUEST-STATUS", value );
			}
		}

		/// <summary>
		///     Gets or sets the URL.
		/// </summary>
		/// <value>
		///     The URL.
		/// </value>
		public virtual Uri Url
		{
			get
			{
				return Properties.Get<Uri>( "URL" );
			}
			set
			{
				Properties.Set( "URL", value );
			}
		}

		/// <summary>
		///     Properties_s the item removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void Properties_ItemRemoved( object sender, ObjectEventArgs<ICalendarProperty, int> e )
		{
			if ( e.First != null &&
			     e.First.Name != null &&
			     string.Equals( e.First.Name.ToUpper( ), "UID" ) )
			{
				OnUidChanged( e.First.Values.Cast<string>( ).FirstOrDefault( ), null );
				e.First.ValueChanged -= Object_ValueChanged;
			}
		}

		/// <summary>
		///     Properties_s the item added.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void Properties_ItemAdded( object sender, ObjectEventArgs<ICalendarProperty, int> e )
		{
			if ( e.First != null &&
			     e.First.Name != null &&
			     string.Equals( e.First.Name.ToUpper( ), "UID" ) )
			{
				OnUidChanged( null, e.First.Values.Cast<string>( ).FirstOrDefault( ) );
				e.First.ValueChanged += Object_ValueChanged;
			}
		}

		/// <summary>
		///     Object_s the value changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void Object_ValueChanged( object sender, ValueChangedEventArgs<object> e )
		{
			string oldValue = e.RemovedValues.OfType<string>( ).FirstOrDefault( );
			string newValue = e.AddedValues.OfType<string>( ).FirstOrDefault( );
			OnUidChanged( oldValue, newValue );
		}

		/// <summary>
		///     Called when deserializing.
		/// </summary>
		/// <param name="context">The context.</param>
		protected override void OnDeserializing( StreamingContext context )
		{
			base.OnDeserializing( context );

			Initialize( );
		}

		/// <summary>
		///     Called when deserialized.
		/// </summary>
		/// <param name="context">The context.</param>
		protected override void OnDeserialized( StreamingContext context )
		{
			base.OnDeserialized( context );

			EnsureProperties( );
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
			if ( obj is RecurringComponent &&
			     obj != this )
			{
				var r = ( RecurringComponent ) obj;
				if ( Uid != null )
				{
					return Uid.Equals( r.Uid );
				}

				return Uid == r.Uid;
			}
			return base.Equals( obj );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			if ( Uid != null )
			{
				return Uid.GetHashCode( );
			}
			return base.GetHashCode( );
		}

		#region IUniqueComponent Members

		/// <summary>
		///     Occurs when the UID changed.
		/// </summary>
		public virtual event EventHandler<ObjectEventArgs<string, string>> UidChanged;

		/// <summary>
		///     Called when the UID changed.
		/// </summary>
		/// <param name="oldUid">The old UID.</param>
		/// <param name="newUid">The new UID.</param>
		protected virtual void OnUidChanged( string oldUid, string newUid )
		{
			EventHandler<ObjectEventArgs<string, string>> onUidChanged = UidChanged;

			if ( onUidChanged != null )
			{
				onUidChanged( this, new ObjectEventArgs<string, string>( oldUid, newUid ) );
			}
		}

		/// <summary>
		///     Gets or sets the UID.
		/// </summary>
		/// <value>
		///     The UID.
		/// </value>
		public virtual string Uid
		{
			get
			{
				return Properties.Get<string>( "UID" );
			}
			set
			{
				Properties.Set( "UID", value );
			}
		}

		#endregion
	}
}