// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Misc
{
	/// <summary>
	///     Clipboard Monitor Option sViewModel
	/// </summary>
	public class ClipboardMonitorOptionsViewModel : OptionsViewModelBase
	{
		/// <summary>
		///     The balloon timeout
		/// </summary>
		private int _balloonTimeout;

	    /// <summary>
	    ///     The monitor perf log
	    /// </summary>
	    private bool _monitorPerfLog;

		/// <summary>
		///     The monitor long
		/// </summary>
		private bool _monitorAlias;

		/// <summary>
		///     The monitor long
		/// </summary>
		private bool _monitorGuid;

		/// <summary>
		///     The monitor long
		/// </summary>
		private bool _monitorLong;

		/// <summary>
		///     Gets or sets the balloon timeout.
		/// </summary>
		/// <value>
		///     The balloon timeout.
		/// </value>
		public int BalloonTimeout
		{
			get
			{
				return _balloonTimeout;
			}
			set
			{
				SetProperty( ref _balloonTimeout, value );
			}
		}

        /// <summary>
        ///     Gets or sets a value indicating whether to monitor for perf logs.
        /// </summary>
	    public bool MonitorPerfLog
	    {
            get
            {
                return _monitorPerfLog;
            }
            set
            {
                SetProperty(ref _monitorPerfLog, value);
            }
	    }

		/// <summary>
		///     Gets or sets a value indicating whether to monitor for aliases.
		/// </summary>
		/// <value>
		///     <c>true</c> if monitoring for aliases; otherwise, <c>false</c>.
		/// </value>
		public bool MonitorAlias
		{
			get
			{
				return _monitorAlias;
			}
			set
			{
				SetProperty( ref _monitorAlias, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether to monitor for GUIDs.
		/// </summary>
		/// <value>
		///     <c>true</c> if monitoring for GUIDs; otherwise, <c>false</c>.
		/// </value>
		public bool MonitorGuid
		{
			get
			{
				return _monitorGuid;
			}
			set
			{
				SetProperty( ref _monitorGuid, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether to monitor for longs.
		/// </summary>
		/// <value>
		///     <c>true</c> if monitoring for longs; otherwise, <c>false</c>.
		/// </value>
		public bool MonitorLong
		{
			get
			{
				return _monitorLong;
			}
			set
			{
				SetProperty( ref _monitorLong, value );
			}
		}

		/// <summary>
		///     Called when loading.
		/// </summary>
		public override void OnLoad( )
		{
			MonitorLong = Settings.Default.MonitorLong;
			MonitorGuid = Settings.Default.MonitorGuid;
			MonitorAlias = Settings.Default.MonitorAlias;
		    MonitorPerfLog = Settings.Default.MonitorPerfLog;
			BalloonTimeout = Settings.Default.BalloonTimeout;
		}

		/// <summary>
		///     Called when saving.
		/// </summary>
		public override void OnSave( )
		{
			Settings.Default.MonitorLong = MonitorLong;
			Settings.Default.MonitorGuid = MonitorGuid;
			Settings.Default.MonitorAlias = MonitorAlias;
		    Settings.Default.MonitorPerfLog = MonitorPerfLog;
			Settings.Default.BalloonTimeout = BalloonTimeout;
			Settings.Default.Save( );
		}
	}
}