// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	public sealed class FreeBusyEntry :
		Period,
		IFreeBusyEntry
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="FreeBusyEntry" /> class.
		/// </summary>
		public FreeBusyEntry( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="FreeBusyEntry" /> class.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="status">The status.</param>
		public FreeBusyEntry( IPeriod period, FreeBusyStatus status )
		{
			Initialize( );
			CopyFrom( period );
			Status = status;
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var fb = obj as IFreeBusyEntry;
			if ( fb != null )
			{
				Status = fb.Status;
			}
		}

		#region IFreeBusyEntry Members

		/// <summary>
		///     Gets or sets the status.
		/// </summary>
		/// <value>
		///     The status.
		/// </value>
		public FreeBusyStatus Status
		{
			get;
			set;
		}

		#endregion

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			Status = FreeBusyStatus.Busy;
		}
	}
}