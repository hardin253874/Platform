// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents an RFC 5545 VTODO component.
	/// </summary>
	[DebuggerDisplay( "{Summary} - {Status}" )]
	[Serializable]
	public class Todo : RecurringComponent, ITodo
	{
		/// <summary>
		///     Evaluator.
		/// </summary>
		private TodoEvaluator _evaluator;

		/// <summary>
		///     The date/time the todo was completed.
		/// </summary>
		public virtual IDateTime Completed
		{
			get
			{
				return Properties.Get<IDateTime>( "COMPLETED" );
			}
			set
			{
				Properties.Set( "COMPLETED", value );
			}
		}

		/// <summary>
		///     The start date/time of the todo item.
		/// </summary>
		public override IDateTime DtStart
		{
			get
			{
				return base.DtStart;
			}
			set
			{
				base.DtStart = value;
				ExtrapolateTimes( );
			}
		}

		/// <summary>
		///     The due date of the todo item.
		/// </summary>
		public virtual IDateTime Due
		{
			get
			{
				return Properties.Get<IDateTime>( "DUE" );
			}
			set
			{
				Properties.Set( "DUE", value );
				ExtrapolateTimes( );
			}
		}

		/// <summary>
		///     The duration of the todo item.
		/// </summary>
		// NOTE: Duration is not supported by all systems,
		// (i.e. iPhone) and cannot co-exist with Due.
		// RFC 5545 states:
		//
		//      ; either 'due' or 'duration' may appear in
		//      ; a 'todoprop', but 'due' and 'duration'
		//      ; MUST NOT occur in the same 'todoprop'
		//
		// Therefore, Duration is not serialized, as Due
		// should always be extrapolated from the duration.
		public virtual TimeSpan Duration
		{
			get
			{
				return Properties.Get<TimeSpan>( "DURATION" );
			}
			set
			{
				Properties.Set( "DURATION", value );
				ExtrapolateTimes( );
			}
		}

		/// <summary>
		///     The geographic location (lat/long) of the todo item.
		/// </summary>
		public virtual IGeographicLocation GeographicLocation
		{
			get
			{
				return Properties.Get<IGeographicLocation>( "GEO" );
			}
			set
			{
				Properties.Set( "GEO", value );
			}
		}

		/// <summary>
		///     The location of the todo item.
		/// </summary>
		public virtual string Location
		{
			get
			{
				return Properties.Get<string>( "LOCATION" );
			}
			set
			{
				Properties.Set( "LOCATION", value );
			}
		}

		/// <summary>
		///     A number between 0 and 100 that represents
		///     the percentage of completion of this item.
		/// </summary>
		public virtual int PercentComplete
		{
			get
			{
				return Properties.Get<int>( "PERCENT-COMPLETE" );
			}
			set
			{
				Properties.Set( "PERCENT-COMPLETE", value );
			}
		}

		/// <summary>
		///     A list of resources associated with this todo item.
		/// </summary>
		public virtual IList<string> Resources
		{
			get
			{
				return Properties.GetMany<string>( "RESOURCES" );
			}
			set
			{
				Properties.Set( "RESOURCES", value );
			}
		}

		/// <summary>
		///     The status of the todo item.
		/// </summary>
		public virtual TodoStatus Status
		{
			get
			{
				return Properties.Get<TodoStatus>( "STATUS" );
			}
			set
			{
				if ( Status != value )
				{
					// Automatically set/unset the Completed time, once the
					// component is fully loaded (When deserializing, it shouldn't
					// automatically set the completed time just because the
					// status was changed).
					if ( IsLoaded )
					{
						Completed = value == TodoStatus.Completed ? iCalDateTime.Now : null;
					}

					Properties.Set( "STATUS", value );
				}
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Todo" /> class.
		/// </summary>
		public Todo( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			Name = Components.TODO;

			_evaluator = new TodoEvaluator( this );
			SetService( _evaluator );
		}

		/// <summary>
		///     Use this method to determine if a todo item has been completed.
		///     This takes into account recurrence items and the previous date
		///     of completion, if any.
		///     <note>
		///         This method evaluates the recurrence pattern for this TODO
		///         as necessary to ensure all relevant information is taken
		///         into account to give the most accurate result possible.
		///     </note>
		/// </summary>
		/// <returns>True if the todo item has been completed</returns>
		public virtual bool IsCompleted( IDateTime currDt )
		{
			if ( Status == TodoStatus.Completed )
			{
				if ( Completed == null ||
				     Completed.GreaterThan( currDt ) )
				{
					return true;
				}

				// Evaluate to the previous occurrence.
				_evaluator.EvaluateToPreviousOccurrence( Completed, currDt );

				// The item has recurred after it was completed
				// and the current date is after or on the recurrence date.
				return _evaluator.Periods.Cast<Period>( ).All( p => !p.StartTime.GreaterThan( Completed ) || !currDt.GreaterThanOrEqual( p.StartTime ) );
			}
			return false;
		}

		/// <summary>
		///     Returns 'True' if the todo item is Active as of <paramref name="currDt" />.
		///     An item is Active if it requires action of some sort.
		/// </summary>
		/// <param name="currDt">The date and time to test.</param>
		/// <returns>
		///     True if the item is Active as of <paramref name="currDt" />, False otherwise.
		/// </returns>
		public virtual bool IsActive( IDateTime currDt )
		{
			if ( DtStart == null )
			{
				return !IsCompleted( currDt ) && !IsCancelled( );
			}

			if ( currDt.GreaterThanOrEqual( DtStart ) )
			{
				return !IsCompleted( currDt ) && !IsCancelled( );
			}

			return false;
		}

		/// <summary>
		///     Returns True if the todo item was cancelled.
		/// </summary>
		/// <returns>True if the todo was cancelled, False otherwise.</returns>
		public virtual bool IsCancelled( )
		{
			return Status == TodoStatus.Cancelled;
		}

		/// <summary>
		///     Gets a value indicating whether the evaluation includes a reference date.
		/// </summary>
		/// <value>
		///     <c>true</c> if the evaluation includes a reference date; otherwise, <c>false</c>.
		/// </value>
		protected override bool EvaluationIncludesReferenceDate
		{
			get
			{
				return true;
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

		/// <summary>
		///     Extrapolates the times.
		/// </summary>
		private void ExtrapolateTimes( )
		{
			if ( Due == null && DtStart != null && Duration != default( TimeSpan ) )
			{
				Due = DtStart.Add( Duration );
			}
			else if ( Duration == default( TimeSpan ) && DtStart != null && Due != null )
			{
				Duration = Due.Subtract( DtStart );
			}
			else if ( DtStart == null && Duration != default( TimeSpan ) && Due != null )
			{
				DtStart = Due.Subtract( Duration );
			}
		}
	}
}