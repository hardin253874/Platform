// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents an RFC 5545 VJOURNAL component.
	/// </summary>
	[Serializable]
	public class Journal : RecurringComponent, IJournal
	{
		#region IJournal Members

		/// <summary>
		///     Gets or sets the status.
		/// </summary>
		/// <value>
		///     The status.
		/// </value>
		public JournalStatus Status
		{
			get
			{
				return Properties.Get<JournalStatus>( "STATUS" );
			}
			set
			{
				Properties.Set( "STATUS", value );
			}
		}

		#endregion

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			Name = Components.JOURNAL;
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
	}
}