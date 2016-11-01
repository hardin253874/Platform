// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;
using ApplicationManager.Core;
using ApplicationManager.Properties;
using ApplicationManager.Support;

namespace ApplicationManager
{
	/// <summary>
	///     View Model for the main window.
	/// </summary>
	public class MainWindowViewModel : ViewModelBase
	{
		/// <summary>
		///     Dialog result.
		/// </summary>
		private bool? _closeWindow;

		/// <summary>
		///     Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
		/// </summary>
		public MainWindowViewModel( )
		{
			SearchCommand = new DelegateCommand< string >( Search );
			CloseCommand = new DelegateCommand( ( ) => CloseWindow = true );
			ImportCommand = new DelegateCommand( Import );
			SettingsCommand = new DelegateCommand( ShowSettings );

			Applications = new CollectionViewSource( );
			ApplicationCollection = new ObservableCollection< Application >( );
			Applications.Source = ApplicationCollection;
			Applications.SortDescriptions.Add( new System.ComponentModel.SortDescription( "Name", System.ComponentModel.ListSortDirection.Ascending ) );
			ApplicationCache = new List< Application >( );

			LoadApplications( );
		}

		/// <summary>
		///     Gets or sets the application collection.
		/// </summary>
		/// <value>
		///     The application collection.
		/// </value>
		public ObservableCollection< Application > ApplicationCollection
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the applications.
		/// </summary>
		/// <value>
		///     The applications.
		/// </value>
		public CollectionViewSource Applications
		{
			get;
			private set;
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

		public ICommand ImportCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the search command.
		/// </summary>
		/// <value>
		///     The search command.
		/// </value>
		public ICommand SearchCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the settings command.
		/// </summary>
		/// <value>
		///     The settings command.
		/// </value>
		public ICommand SettingsCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the title.
		/// </summary>
		/// <value>
		///     The title.
		/// </value>
		public string Title
		{
			get
			{
				return string.Format( Resources.MainWindowTitle, Assembly.GetExecutingAssembly( ).GetName( ).Version.ToString( 2 ) );
			}
		}

		/// <summary>
		///     Gets or sets the application cache.
		/// </summary>
		/// <value>
		///     The application cache.
		/// </value>
		private List< Application > ApplicationCache
		{
			get;
			set;
		}

		/// <summary>
		///     Imports this instance.
		/// </summary>
		private void Import( )
		{
			var import = new ImportApplication( );
			import.ShowDialog( );

			LoadApplications( );
		}

		/// <summary>
		///     Loads the applications.
		/// </summary>
		private void LoadApplications( )
		{
			ApplicationCollection.Clear( );
			ApplicationCache.Clear( );

			foreach ( Application app in Application.GetApplications( ) )
			{
				app.RefreshAll = LoadApplications;

				ApplicationCollection.Add( app );
				ApplicationCache.Add( app );
			}
		}

		/// <summary>
		///     Searches this instance.
		/// </summary>
		/// <param name="searchText">The search text.</param>
		private void Search( string searchText )
		{
			ApplicationCollection.Clear( );

			bool cleared = string.IsNullOrEmpty( searchText );

			foreach ( Application app in ApplicationCache )
			{
				if ( cleared )
				{
					ApplicationCollection.Add( app );
				}
				else
				{
					if ( app.Name.Contains( searchText ) )
					{
						ApplicationCollection.Add( app );
					}
				}
			}
		}

		/// <summary>
		///     Shows the settings.
		/// </summary>
		private void ShowSettings( )
		{
			var settings = new SettingsWindow( );
			settings.ShowDialog( );

			LoadApplications( );
		}
	}
}