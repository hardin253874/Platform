// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     The ThreadMonitorOptionsViewModel class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.OptionsViewModelBase" />
	public class ThreadMonitorOptionsViewModel : OptionsViewModelBase
	{
		/// <summary>
		///     The refresh duration
		/// </summary>
		private int _refreshDuration;

		/// <summary>
		///     Whether to show unmanaged threads.
		/// </summary>
		private bool _showUnmanagedThreads;

		/// <summary>
		///     Gets or sets the duration of the refresh.
		/// </summary>
		/// <value>
		///     The duration of the refresh.
		/// </value>
		public int RefreshDuration
		{
			get
			{
				return _refreshDuration;
			}
			set
			{
				if ( value < 1000 )
				{
					value = 1000;
				}

				if ( value > 10000 )
				{
					value = 10000;
				}

				SetProperty( ref _refreshDuration, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [show unmanaged threads].
		/// </summary>
		/// <value>
		///     <c>true</c> if [show unmanaged threads]; otherwise, <c>false</c>.
		/// </value>
		public bool ShowUnmanagedThreads
		{
			get
			{
				return _showUnmanagedThreads;
			}
			set
			{
				SetProperty( ref _showUnmanagedThreads, value );
			}
		}

		/// <summary>
		///     Called when loading.
		/// </summary>
		public override void OnLoad( )
		{
			RefreshDuration = Settings.Default.RefreshDuration;
			ShowUnmanagedThreads = Settings.Default.ShowUnmanagedThreads;
		}

		/// <summary>
		///     Called when saving.
		/// </summary>
		public override void OnSave( )
		{
			Settings.Default.RefreshDuration = RefreshDuration;
			Settings.Default.ShowUnmanagedThreads = ShowUnmanagedThreads;
			Settings.Default.Save( );
		}
	}
}