// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;

namespace ReadiMon.Plugin.Graphs.TenantsTrend
{
	/// <summary>
	///     TenantsTrendData class.
	/// </summary>
	/// <seealso cref="INotifyPropertyChanged" />
	public class TenantsTrendData : INotifyPropertyChanged
	{
		private int _count;

		/// <summary>
		///     Gets or sets the count.
		/// </summary>
		/// <value>
		///     The count.
		/// </value>
		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				_count = value;
				if ( PropertyChanged != null )
				{
					PropertyChanged( this, new PropertyChangedEventArgs( "Count" ) );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="TenantsTrendData" /> is disabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if disabled; otherwise, <c>false</c>.
		/// </value>
		public bool Disabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the label.
		/// </summary>
		/// <value>
		///     The label.
		/// </value>
		public string Label
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the time stamp.
		/// </summary>
		/// <value>
		///     The time stamp.
		/// </value>
		public DateTime Timestamp
		{
			get;
			set;
		}

		/// <summary>
		///     Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
	}
}