﻿// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.AddIn;
using System.Windows;
using JetBrains.Annotations;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Database Health plugin
	/// </summary>
	[AddIn( "Database Health Plugin", Version = "1.0.0.0" )]
	[UsedImplicitly]
	public class DatabaseHealthPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The options view model
		/// </summary>
		private DatabaseHealthOptionsViewModel _optionsViewModel;

		/// <summary>
		///     The user interface
		/// </summary>
		private DatabaseHealth _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseHealthPlugin" /> class.
		/// </summary>
		public DatabaseHealthPlugin( )
		{
			SectionOrdinal = 5;
			SectionName = "Database";
			EntryName = "Health Check";
			EntryOrdinal = 5;
			HasOptionsUserInterface = true;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the options view model.
		/// </summary>
		/// <value>
		///     The options view model.
		/// </value>
		private DatabaseHealthOptionsViewModel OptionsViewModel => _optionsViewModel ?? ( _optionsViewModel = new DatabaseHealthOptionsViewModel( ) );

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new DatabaseHealth( Settings ) );
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetOptionsUserInterface( )
		{
			return new DatabaseHealthOptions( OptionsViewModel );
		}

		/// <summary>
		///     Saves the options.
		/// </summary>
		public override void SaveOptions( )
		{
			_optionsViewModel?.OnSave( );
		}

		/// <summary>
		///     Receives the message.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <returns></returns>
		public override string Invoke( string argument )
		{
			var strings = argument?.Split( ':' );

			if ( strings?.Length > 0 )
			{
				if ( strings [ 0 ] == "run" )
				{
					DatabaseHealthViewModel model = new DatabaseHealthViewModel( Settings );

					if ( strings.Length > 1 )
					{
						int maxResults;

						if ( int.TryParse( strings [ 1 ], out maxResults ) )
						{
							model.MaxResults = maxResults;
						}
						else
						{
							model.MaxResults = null;
						}
					}
					else
					{
						model.MaxResults = null;
					}

					DateTime start = DateTime.UtcNow;
					model.OnRunTests( true );
					DateTime end = DateTime.UtcNow;

					return model.GenerateJUnitReport( start, end );
				}
			}

			return null;
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			var viewModel = _userInterface?.DataContext as DatabaseHealthViewModel;

			if ( viewModel != null )
			{
				viewModel.PluginSettings = Settings;
			}
		}
	}
}