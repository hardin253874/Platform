// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     An iCalendar component that recurs.
	/// </summary>
	/// <remarks>
	///     This component automatically handles
	///     RRULEs, RDATE, EXRULEs, and EXDATEs, as well as the DTSTART
	///     for the recurring item (all recurring items must have a DTSTART).
	/// </remarks>
	[Serializable]
	public class RecurringComponent : UniqueComponent, IRecurringComponent
	{
		/// <summary>
		///     Sorts the by date.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static IEnumerable<IRecurringComponent> SortByDate( IEnumerable<IRecurringComponent> list )
		{
			return SortByDate<IRecurringComponent>( list );
		}

		/// <summary>
		///     Sorts the by date.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static IEnumerable<T> SortByDate<T>( IEnumerable<T> list )
		{
			List<IRecurringComponent> items = list.OfType<IRecurringComponent>( ).ToList( );

			// Sort the list by date
			items.Sort( new RecurringComponentDateSorter( ) );

			return items.Select( rc => ( T ) rc );
		}

		/// <summary>
		///     Gets a value indicating whether [evaluation includes reference date].
		/// </summary>
		/// <value>
		///     <c>true</c> if [evaluation includes reference date]; otherwise, <c>false</c>.
		/// </value>
		protected virtual bool EvaluationIncludesReferenceDate
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		///     Gets or sets the attachments.
		/// </summary>
		/// <value>
		///     The attachments.
		/// </value>
		public virtual IList<IAttachment> Attachments
		{
			get
			{
				return Properties.GetMany<IAttachment>( "ATTACH" );
			}
			set
			{
				Properties.Set( "ATTACH", value );
			}
		}

		/// <summary>
		///     Gets or sets the categories.
		/// </summary>
		/// <value>
		///     The categories.
		/// </value>
		public virtual IList<string> Categories
		{
			get
			{
				return Properties.GetMany<string>( "CATEGORIES" );
			}
			set
			{
				Properties.Set( "CATEGORIES", value );
			}
		}

		/// <summary>
		///     Gets or sets the class.
		/// </summary>
		/// <value>
		///     The class.
		/// </value>
		public virtual string Class
		{
			get
			{
				return Properties.Get<string>( "CLASS" );
			}
			set
			{
				Properties.Set( "CLASS", value );
			}
		}

		/// <summary>
		///     Gets or sets the contacts.
		/// </summary>
		/// <value>
		///     The contacts.
		/// </value>
		public virtual IList<string> Contacts
		{
			get
			{
				return Properties.GetMany<string>( "CONTACT" );
			}
			set
			{
				Properties.Set( "CONTACT", value );
			}
		}

		/// <summary>
		///     Gets or sets the created.
		/// </summary>
		/// <value>
		///     The created.
		/// </value>
		public virtual IDateTime Created
		{
			get
			{
				return Properties.Get<IDateTime>( "CREATED" );
			}
			set
			{
				Properties.Set( "CREATED", value );
			}
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public virtual string Description
		{
			get
			{
				return Properties.Get<string>( "DESCRIPTION" );
			}
			set
			{
				Properties.Set( "DESCRIPTION", value );
			}
		}

		/// <summary>
		///     The start date/time of the component.
		/// </summary>
		public virtual IDateTime DtStart
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
		///     Gets or sets the exception dates.
		/// </summary>
		/// <value>
		///     The exception dates.
		/// </value>
		public virtual IList<IPeriodList> ExceptionDates
		{
			get
			{
				return Properties.GetMany<IPeriodList>( "EXDATE" );
			}
			set
			{
				Properties.Set( "EXDATE", value );
			}
		}

		/// <summary>
		///     Gets or sets the exception rules.
		/// </summary>
		/// <value>
		///     The exception rules.
		/// </value>
		public virtual IList<IRecurrencePattern> ExceptionRules
		{
			get
			{
				return Properties.GetMany<IRecurrencePattern>( "EXRULE" );
			}
			set
			{
				Properties.Set( "EXRULE", value );
			}
		}

		/// <summary>
		///     Gets or sets the last modified.
		/// </summary>
		/// <value>
		///     The last modified.
		/// </value>
		public virtual IDateTime LastModified
		{
			get
			{
				return Properties.Get<IDateTime>( "LAST-MODIFIED" );
			}
			set
			{
				Properties.Set( "LAST-MODIFIED", value );
			}
		}

		/// <summary>
		///     Gets or sets the priority.
		/// </summary>
		/// <value>
		///     The priority.
		/// </value>
		public virtual int Priority
		{
			get
			{
				return Properties.Get<int>( "PRIORITY" );
			}
			set
			{
				Properties.Set( "PRIORITY", value );
			}
		}

		/// <summary>
		///     Gets or sets the recurrence dates.
		/// </summary>
		/// <value>
		///     The recurrence dates.
		/// </value>
		public virtual IList<IPeriodList> RecurrenceDates
		{
			get
			{
				return Properties.GetMany<IPeriodList>( "RDATE" );
			}
			set
			{
				Properties.Set( "RDATE", value );
			}
		}

		/// <summary>
		///     Gets or sets the recurrence rules.
		/// </summary>
		/// <value>
		///     The recurrence rules.
		/// </value>
		public virtual IList<IRecurrencePattern> RecurrenceRules
		{
			get
			{
				return Properties.GetMany<IRecurrencePattern>( "RRULE" );
			}
			set
			{
				Properties.Set( "RRULE", value );
			}
		}

		/// <summary>
		///     Gets or sets the recurrence ID.
		/// </summary>
		/// <value>
		///     The recurrence ID.
		/// </value>
		public virtual IDateTime RecurrenceId
		{
			get
			{
				return Properties.Get<IDateTime>( "RECURRENCE-ID" );
			}
			set
			{
				Properties.Set( "RECURRENCE-ID", value );
			}
		}

		/// <summary>
		///     Gets or sets the related components.
		/// </summary>
		/// <value>
		///     The related components.
		/// </value>
		public virtual IList<string> RelatedComponents
		{
			get
			{
				return Properties.GetMany<string>( "RELATED-TO" );
			}
			set
			{
				Properties.Set( "RELATED-TO", value );
			}
		}

		/// <summary>
		///     Gets or sets the sequence.
		/// </summary>
		/// <value>
		///     The sequence.
		/// </value>
		public virtual int Sequence
		{
			get
			{
				return Properties.Get<int>( "SEQUENCE" );
			}
			set
			{
				Properties.Set( "SEQUENCE", value );
			}
		}

		/// <summary>
		///     An alias to the DTStart field (i.e. start date/time).
		/// </summary>
		public virtual IDateTime Start
		{
			get
			{
				return DtStart;
			}
			set
			{
				DtStart = value;
			}
		}

		/// <summary>
		///     Gets or sets the summary.
		/// </summary>
		/// <value>
		///     The summary.
		/// </value>
		public virtual string Summary
		{
			get
			{
				return Properties.Get<string>( "SUMMARY" );
			}
			set
			{
				Properties.Set( "SUMMARY", value );
			}
		}

		/// <summary>
		///     A list of <see cref="Alarm" />s for this recurring component.
		/// </summary>
		public virtual ICalendarObjectList<IAlarm> Alarms
		{
			get
			{
				return new CalendarObjectListProxy<IAlarm>( Children );
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RecurringComponent" /> class.
		/// </summary>
		public RecurringComponent( )
		{
			Initialize( );
			EnsureProperties( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RecurringComponent" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public RecurringComponent( string name )
			: base( name )
		{
			Initialize( );
			EnsureProperties( );
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			SetService( new RecurringEvaluator( this ) );
		}

		/// <summary>
		///     Ensures the properties.
		/// </summary>
		private void EnsureProperties( )
		{
			if ( !Properties.ContainsKey( "SEQUENCE" ) )
			{
				Sequence = 0;
			}
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

		#region IRecurringComponent Members

		/// <summary>
		///     Clears a previous evaluation, usually because one of the
		///     key elements used for evaluation has changed
		///     (Start, End, Duration, recurrence rules, exceptions, etc.).
		/// </summary>
		public virtual void ClearEvaluation( )
		{
			RecurrenceUtil.ClearEvaluation( this );
		}

		/// <summary>
		///     Returns all occurrences of this component that start on the date provided.
		///     All components starting between 12:00:00AM and 11:59:59 PM will be
		///     returned.
		///     <note>
		///         This will first Evaluate() the date range required in order to
		///         determine the occurrences for the date provided, and then return
		///         the occurrences.
		///     </note>
		/// </summary>
		/// <param name="dt">The date for which to return occurrences.</param>
		/// <returns>
		///     A list of Periods representing the occurrences of this object.
		/// </returns>
		public virtual IList<Occurrence> GetOccurrences( IDateTime dt )
		{
			return RecurrenceUtil.GetOccurrences( this, dt, EvaluationIncludesReferenceDate );
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public virtual IList<Occurrence> GetOccurrences( DateTime dt )
		{
			return RecurrenceUtil.GetOccurrences( this, new iCalDateTime( dt ), EvaluationIncludesReferenceDate );
		}

		/// <summary>
		///     Returns all occurrences of this component that start within the date range provided.
		///     All components occurring between <paramref name="startTime" /> and <paramref name="endTime" />
		///     will be returned.
		/// </summary>
		/// <param name="startTime">The starting date range</param>
		/// <param name="endTime">The ending date range</param>
		/// <returns></returns>
		public virtual IList<Occurrence> GetOccurrences( IDateTime startTime, IDateTime endTime )
		{
			return RecurrenceUtil.GetOccurrences( this, startTime, endTime, EvaluationIncludesReferenceDate );
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <returns></returns>
		public virtual IList<Occurrence> GetOccurrences( DateTime startTime, DateTime endTime )
		{
			return RecurrenceUtil.GetOccurrences( this, new iCalDateTime( startTime ), new iCalDateTime( endTime ), EvaluationIncludesReferenceDate );
		}

		/// <summary>
		///     Polls the alarms.
		/// </summary>
		/// <returns></returns>
		public virtual IList<AlarmOccurrence> PollAlarms( )
		{
			return PollAlarms( null, null );
		}

		/// <summary>
		///     Polls the alarms.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <returns></returns>
		public virtual IList<AlarmOccurrence> PollAlarms( IDateTime startTime, IDateTime endTime )
		{
			var occurrences = new List<AlarmOccurrence>( );

			if ( Alarms != null )
			{
				foreach ( IAlarm alarm in Alarms )
				{
					occurrences.AddRange( alarm.Poll( startTime, endTime ) );
				}
			}

			return occurrences;
		}

		#endregion
	}

	/// <summary>
	///     Sorts recurring components by their start dates
	/// </summary>
	public class RecurringComponentDateSorter : IComparer<IRecurringComponent>
	{
		#region IComparer<RecurringComponent> Members

		public int Compare( IRecurringComponent x, IRecurringComponent y )
		{
			return x.Start.CompareTo( y.Start );
		}

		#endregion
	}
}