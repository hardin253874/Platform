// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using ApplicationManager.Core;
using ApplicationManager.Support;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing;
using Microsoft.Win32;

namespace ApplicationManager
{
	/// <summary>
	///     Export view model.
	/// </summary>
	public class ExportApplicationViewModel : ViewModelBase
	{
		/// <summary>
		///     Busy message.
		/// </summary>
		private string _busyMessage = "Please wait";

		/// <summary>
		///     Dialog result.
		/// </summary>
		private bool? _closeWindow;

		/// <summary>
		///     Is Busy.
		/// </summary>
		private bool _isBusy;

		/// <summary>
		///     Selected package
		/// </summary>
		private Package _selectedPackage;

		/// <summary>
		///     Initializes a new instance of the <see cref="ExportApplicationViewModel" /> class.
		/// </summary>
		/// <param name="application">The application.</param>
		public ExportApplicationViewModel( Application application )
		{
			Application = application;

			SearchCommand = new DelegateCommand<string>( Search );
			CloseCommand = new DelegateCommand( ( ) => CloseWindow = true );
			ExportCommand = new DelegateCommand( Export );

			Packages = new CollectionViewSource( );
			PackageCollection = new ObservableCollection<Package>( );
			Packages.Source = PackageCollection;
			PackageCache = new List<Package>( );

			LoadPackages( );
		}

		/// <summary>
		///     Gets or sets the busy message.
		/// </summary>
		/// <value>
		///     The busy message.
		/// </value>
		public string BusyMessage
		{
			get
			{
				return _busyMessage;
			}
			set
			{
				if ( _busyMessage != value )
				{
					SetProperty( ref _busyMessage, value );
				}
			}
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
					SetProperty( ref _closeWindow, value );
				}
			}
		}

		/// <summary>
		///     Gets or sets the export command.
		/// </summary>
		/// <value>
		///     The export command.
		/// </value>
		public ICommand ExportCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets a value indicating whether [export enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [export enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool ExportEnabled => SelectedPackage != null;

		/// <summary>
		///     Gets or sets a value indicating whether this instance is busy.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is busy; otherwise, <c>false</c>.
		/// </value>
		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}
			set
			{
				if ( _isBusy != value )
				{
					SetProperty( ref _isBusy, value );
				}
			}
		}

		/// <summary>
		///     Gets or sets the package collection.
		/// </summary>
		/// <value>
		///     The package collection.
		/// </value>
		public ObservableCollection<Package> PackageCollection
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the packages.
		/// </summary>
		/// <value>
		///     The packages.
		/// </value>
		public CollectionViewSource Packages
		{
			get;
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
		///     Gets or sets the selected package.
		/// </summary>
		/// <value>
		///     The selected package.
		/// </value>
		public Package SelectedPackage
		{
			get
			{
				return _selectedPackage;
			}
			set
			{
				if ( _selectedPackage != value )
				{
					SetProperty( ref _selectedPackage, value );

					// ReSharper disable once ExplicitCallerInfoArgument
					OnPropertyChanged( "ExportEnabled" );
				}
			}
		}

		/// <summary>
		///     Gets the title.
		/// </summary>
		/// <value>
		///     The title.
		/// </value>
		public string Title => $"Export '{Application.Name}'";

		/// <summary>
		///     Gets or sets the application.
		/// </summary>
		/// <value>
		///     The application.
		/// </value>
		private Application Application
		{
			get;
		}

		/// <summary>
		///     Gets or sets the package cache.
		/// </summary>
		/// <value>
		///     The package cache.
		/// </value>
		private List<Package> PackageCache
		{
			get;
		}

		/// <summary>
		///     Exports this instance.
		/// </summary>
		private void Export( )
		{
			string version = SelectedPackage.Version;

			var saveDialog = new SaveFileDialog
			{
				Filter = "Xml Application (*.xml)|*.xml|SqLite Database (*.db)|*.db",
				Title = "Export Package",
				FileName = Application.Name + ( string.IsNullOrEmpty( version ) ? Application.ApplicationId.ToString( "B" ) : " " + version )
			};
			bool? result = saveDialog.ShowDialog( );

			if ( result == true )
			{
				IsBusy = true;

				var workerThread = new Thread( ExportAsynchronous );
				workerThread.Start( saveDialog.FileName );
			}
		}

		/// <summary>
		///     Runs the export asynchronously.
		/// </summary>
		/// <param name="state">The state.</param>
		private void ExportAsynchronous( object state )
		{
			var context = new RoutedProcessingContext( message => BusyMessage = message, message => BusyMessage = message, message => BusyMessage = message, message => BusyMessage = message );

			FileInfo info = new FileInfo( ( string ) state );

			Format format = Format.Undefined;

			if ( info.Extension.Equals( ".db", StringComparison.OrdinalIgnoreCase ) )
			{
				format = Format.Sqlite;
			}

			AppManager.ExportAppPackage( SelectedPackage.AppVerId, ( string ) state, format, context );

			IsBusy = false;
		}

		/// <summary>
		///     Loads the packages.
		/// </summary>
		private void LoadPackages( )
		{
			foreach ( Package package in Package.GetPackages( Application.ApplicationId ) )
			{
				PackageCollection.Add( package );
				PackageCache.Add( package );
			}

			if ( PackageCollection.Count == 1 )
			{
				SelectedPackage = PackageCollection[ 0 ];
			}
		}

		/// <summary>
		///     Searches this instance.
		/// </summary>
		/// <param name="searchText">The search text.</param>
		private void Search( string searchText )
		{
			PackageCollection.Clear( );

			bool cleared = string.IsNullOrEmpty( searchText );

			foreach ( Package package in PackageCache )
			{
				if ( cleared )
				{
					PackageCollection.Add( package );
				}
				else
				{
					if ( package.Version.Contains( searchText ) )
					{
						PackageCollection.Add( package );
					}
				}
			}
		}
	}
}