// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ReadiMon.Plugin.Graphs.TenantsTrend
{
	/// <summary>
	///     TenantsTrendSeries class.
	/// </summary>
	/// <seealso cref="INotifyPropertyChanged" />
	public class TenantsTrendSeries : INotifyPropertyChanged
	{
		private string _displayName;

		/// <summary>
		///     Gets or sets the display name.
		/// </summary>
		/// <value>
		///     The display name.
		/// </value>
		public string DisplayName
		{
			get
			{
				return _displayName;
			}
			set
			{
				_displayName = value;
				if ( PropertyChanged != null )
				{
					PropertyChanged( this, new PropertyChangedEventArgs( "DisplayName" ) );
				}
			}
		}

		/// <summary>
		///     Gets or sets the items.
		/// </summary>
		/// <value>
		///     The items.
		/// </value>
		public ObservableCollection<TenantsTrendData> Items
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