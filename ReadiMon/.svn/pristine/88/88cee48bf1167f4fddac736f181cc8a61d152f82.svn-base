// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Workflow Monitor Options.
	/// </summary>
	public class WorkflowMonitorOptionsViewModel : OptionsViewModelBase
	{
		/// <summary>
		///     The refresh duration
		/// </summary>
		private int _refreshDuration;

		/// <summary>
		///     Whether to show completed runs.
		/// </summary>
		private bool _showCompletedRuns;

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
		public bool ShowCompletedRuns
		{
			get
			{
				return _showCompletedRuns;
			}
			set
			{
				SetProperty( ref _showCompletedRuns, value );
			}
		}

		/// <summary>
		///     Called when loading.
		/// </summary>
		public override void OnLoad( )
		{
			RefreshDuration = Settings.Default.WorkflowRefreshDuration;
			ShowCompletedRuns = Settings.Default.WorkflowShowCompletedRuns;
		}

		/// <summary>
		///     Called when saving.
		/// </summary>
		public override void OnSave( )
		{
			Settings.Default.WorkflowRefreshDuration = RefreshDuration;
			Settings.Default.WorkflowShowCompletedRuns = ShowCompletedRuns;
			Settings.Default.Save( );
		}
	}
}