// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     FreeBusy class.
	/// </summary>
	public sealed class FreeBusy : UniqueComponent, IFreeBusy
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="FreeBusy" /> class.
		/// </summary>
		public FreeBusy( )
		{
			Name = Components.FREEBUSY;
		}

		#region IFreeBusy Members

		/// <summary>
		///     Gets or sets the entries.
		/// </summary>
		/// <value>
		///     The entries.
		/// </value>
		public IList<IFreeBusyEntry> Entries
		{
			get
			{
				return Properties.GetMany<IFreeBusyEntry>( "FREEBUSY" );
			}
			set
			{
				Properties.Set( "FREEBUSY", value );
			}
		}

		/// <summary>
		///     Gets or sets the DT start.
		/// </summary>
		/// <value>
		///     The DT start.
		/// </value>
		public IDateTime DtStart
		{
			get
			{
				return Properties.Get<IDateTime>( "DTSTART" );
			}
			set
			{
				Properties.Set( "DTSTART", value );
			}
		}

		/// <summary>
		///     Gets or sets the DT end.
		/// </summary>
		/// <value>
		///     The DT end.
		/// </value>
		public IDateTime DtEnd
		{
			get
			{
				return Properties.Get<IDateTime>( "DTEND" );
			}
			set
			{
				Properties.Set( "DTEND", value );
			}
		}

		/// <summary>
		///     Gets or sets the start.
		/// </summary>
		/// <value>
		///     The start.
		/// </value>
		public IDateTime Start
		{
			get
			{
				return Properties.Get<IDateTime>( "DTSTART" );
			}
			set
			{
				Properties.Set( "DTSTART", value );
			}
		}

		/// <summary>
		///     Gets or sets the end.
		/// </summary>
		/// <value>
		///     The end.
		/// </value>
		public IDateTime End
		{
			get
			{
				return Properties.Get<IDateTime>( "DTEND" );
			}
			set
			{
				Properties.Set( "DTEND", value );
			}
		}

		/// <summary>
		///     Gets the free busy status.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		public FreeBusyStatus GetFreeBusyStatus( IPeriod period )
		{
			var status = FreeBusyStatus.Free;
			if ( period != null )
			{
				foreach ( IFreeBusyEntry fbe in Entries )
				{
					if ( fbe.CollidesWith( period ) && status < fbe.Status )
					{
						status = fbe.Status;
					}
				}
			}
			return status;
		}

		/// <summary>
		///     Gets the free busy status.
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <returns></returns>
		public FreeBusyStatus GetFreeBusyStatus( IDateTime dt )
		{
			var status = FreeBusyStatus.Free;
			if ( dt != null )
			{
				foreach ( IFreeBusyEntry fbe in Entries )
				{
					if ( fbe.Contains( dt ) && status < fbe.Status )
					{
						status = fbe.Status;
					}
				}
			}
			return status;
		}

		#endregion

		#region IMergeable Members

		/// <summary>
		///     Merges this object with another.
		/// </summary>
		/// <param name="obj"></param>
		public void MergeWith( IMergeable obj )
		{
			var fb = obj as IFreeBusy;
			if ( fb != null )
			{
				foreach ( IFreeBusyEntry entry in fb.Entries )
				{
					if ( !Entries.Contains( entry ) )
					{
						Entries.Add( entry.Copy<IFreeBusyEntry>( ) );
					}
				}
			}
		}

		#endregion

		/// <summary>
		///     Creates the specified obj.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="freeBusyRequest">The free busy request.</param>
		/// <returns></returns>
		public static IFreeBusy Create( ICalendarObject obj, IFreeBusy freeBusyRequest )
		{
			var occurrencesTyped = obj as IGetOccurrencesTyped;

			if ( occurrencesTyped != null )
			{
				IGetOccurrencesTyped getOccurrences = occurrencesTyped;
				IList<Occurrence> occurrences = getOccurrences.GetOccurrences<IEvent>( freeBusyRequest.Start, freeBusyRequest.End );
				var contacts = new List<string>( );
				bool isFilteredByAttendees = false;

				if ( freeBusyRequest.Attendees != null &&
				     freeBusyRequest.Attendees.Count > 0 )
				{
					isFilteredByAttendees = true;
					contacts.AddRange( from attendee in freeBusyRequest.Attendees
					                   where attendee.Value != null
					                   select attendee.Value.OriginalString.Trim( ) );
				}

				var fb = freeBusyRequest.Copy<IFreeBusy>( );
				fb.Uid = new UidFactory( ).Build( );
				fb.Entries.Clear( );
				fb.DtStamp = iCalDateTime.Now;

				foreach ( Occurrence o in occurrences )
				{
					var uc = o.Source as IUniqueComponent;

					if ( uc != null )
					{
						var evt = uc as IEvent;
						bool accepted = false;
						var type = FreeBusyStatus.Busy;

						// We only accept events, and only "opaque" events.
						if ( evt != null && evt.Transparency != TransparencyType.Transparent )
						{
							accepted = true;
						}

						// If the result is filtered by attendees, then
						// we won't accept it until we find an event
						// that is being attended by this person.
						if ( accepted && isFilteredByAttendees )
						{
							accepted = false;
							foreach ( IAttendee a in uc.Attendees )
							{
								if ( a.Value != null && contacts.Contains( a.Value.OriginalString.Trim( ) ) )
								{
									if ( a.ParticipationStatus != ParticipationStatus.NotSpecified )
									{
										switch ( a.ParticipationStatus )
										{
											case ParticipationStatus.Tentative:
												accepted = true;
												type = FreeBusyStatus.BusyTentative;
												break;
											case ParticipationStatus.Accepted:
												accepted = true;
												type = FreeBusyStatus.Busy;
												break;
										}
									}
								}
							}
						}

						if ( accepted )
						{
							// If the entry was accepted, add it to our list!
							fb.Entries.Add( new FreeBusyEntry( o.Period, type ) );
						}
					}
				}

				return fb;
			}
			return null;
		}

		/// <summary>
		///     Creates the request.
		/// </summary>
		/// <param name="fromInclusive">From inclusive.</param>
		/// <param name="toExclusive">To exclusive.</param>
		/// <param name="organizer">The organizer.</param>
		/// <param name="contacts">The contacts.</param>
		/// <returns></returns>
		public static IFreeBusy CreateRequest( IDateTime fromInclusive, IDateTime toExclusive, IOrganizer organizer, IAttendee[] contacts )
		{
			var fb = new FreeBusy
				{
					DtStamp = iCalDateTime.Now,
					DtStart = fromInclusive,
					DtEnd = toExclusive
				};
			if ( organizer != null )
			{
				fb.Organizer = organizer.Copy<IOrganizer>( );
			}
			if ( contacts != null )
			{
				foreach ( IAttendee attendee in contacts )
				{
					fb.Attendees.Add( attendee.Copy<IAttendee>( ) );
				}
			}

			return fb;
		}
	}
}