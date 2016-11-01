// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Globalization;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Database Health Options View Model
	/// </summary>
	public class DatabaseHealthOptionsViewModel : OptionsViewModelBase
	{
		/// <summary>
		///     The command timeout
		/// </summary>
		private string _commandTimeout;

		/// <summary>
		///     The simultaneous tests
		/// </summary>
		private string _simTests;

		/// <summary>
		///     Gets or sets the command timeout.
		/// </summary>
		/// <value>
		///     The command timeout.
		/// </value>
		public string CommandTimeout
		{
			get
			{
				return _commandTimeout;
			}
			set
			{
				SetProperty( ref _commandTimeout, value );
			}
		}

		/// <summary>
		///     Gets or sets the simultaneous tests.
		/// </summary>
		/// <value>
		///     The simultaneous tests.
		/// </value>
		public string SimTests
		{
			get
			{
				return _simTests;
			}
			set
			{
				SetProperty( ref _simTests, value );
			}
		}

		/// <summary>
		///     Called when loading.
		/// </summary>
		public override void OnLoad( )
		{
			CommandTimeout = Settings.Default.CommandTimeout.ToString( CultureInfo.InvariantCulture );
			SimTests = Settings.Default.SimultaneousTests.ToString( CultureInfo.InvariantCulture );
		}

		/// <summary>
		///     Called when saving.
		/// </summary>
		public override void OnSave( )
		{
			int timeout;

			if ( int.TryParse( CommandTimeout, out timeout ) )
			{
				if ( timeout <= 0 )
				{
					timeout = 30000;
				}

				Settings.Default.CommandTimeout = timeout;
			}

			int simultaneousTests;

			if ( int.TryParse( SimTests, out simultaneousTests ) )
			{
				if ( simultaneousTests <= 0 )
				{
					simultaneousTests = 3;
				}

				Settings.Default.SimultaneousTests = simultaneousTests;
			}

			Settings.Default.Save( );
		}
	}
}