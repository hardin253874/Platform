// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Windows.Input;
using ApplicationManager.Core;

namespace ApplicationManager
{
	/// <summary>
	///     Settings view model.
	/// </summary>
	public class SettingsWindowViewModel : ViewModelBase
	{
		/// <summary>
		///     Dialog result.
		/// </summary>
		private bool? _closeWindow;

		/// <summary>
		///     Repository Database name.
		/// </summary>
		private string _databaseName = "SoftwarePlatform";

		/// <summary>
		///     Repository Server name
		/// </summary>
		private string _serverName = Environment.MachineName;

		/// <summary>
		///     Initializes a new instance of the <see cref="SettingsWindowViewModel" /> class.
		/// </summary>
		public SettingsWindowViewModel( )
		{
			CloseCommand = new DelegateCommand( ( ) => CloseWindow = true );
			SaveCommand = new DelegateCommand( Save );

			Load( );
		}

		/// <summary>
		///     Gets or sets the close command.
		/// </summary>
		/// <value>
		///     The close command.
		/// </value>
		public ICommand CloseCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the close window.
		/// </summary>
		/// <value>
		///     The close window.
		/// </value>
		public bool? CloseWindow
		{
			get
			{
				return _closeWindow;
			}
			set
			{
				if ( _closeWindow != value )
				{
					_closeWindow = value;
					RaisePropertyChanged( "CloseWindow" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the name of the database.
		/// </summary>
		/// <value>
		///     The name of the database.
		/// </value>
		public string DatabaseName
		{
			get
			{
				return _databaseName;
			}
			set
			{
				if ( _databaseName != value )
				{
					_databaseName = value;
					RaisePropertyChanged( "DatabaseName" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the save command.
		/// </summary>
		/// <value>
		///     The save command.
		/// </value>
		public ICommand SaveCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the server.
		/// </summary>
		/// <value>
		///     The name of the server.
		/// </value>
		public string ServerName
		{
			get
			{
				return _serverName;
			}
			set
			{
				if ( _serverName != value )
				{
					_serverName = value;
					RaisePropertyChanged( "ServerName" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the title.
		/// </summary>
		/// <value>
		///     The title.
		/// </value>
		public string Title
		{
			get
			{
				return "Settings";
			}
		}

		/// <summary>
		///     Loads this instance.
		/// </summary>
		private void Load( )
		{
			ServerName = Config.ServerName;
			DatabaseName = Config.DatabaseName;
		}

		/// <summary>
		///     Saves this instance.
		/// </summary>
		private void Save( )
		{
			Config.ServerName = ServerName;
			Config.DatabaseName = DatabaseName;

			CloseWindow = true;
		}
	}
}