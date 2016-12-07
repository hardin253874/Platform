// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using ApplicationManager.Core;
using ApplicationManager.Support;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Migration.Processing;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace ApplicationManager
{
	/// <summary>
	///     Import application view model.
	/// </summary>
	public class ImportApplicationViewModel : ViewModelBase
	{
		/// <summary>
		///     The dialog.
		/// </summary>
		private static OpenFileDialog _dialog;

		/// <summary>
		///     The _sync root.
		/// </summary>
		private static readonly object SyncRoot = new object( );

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
		///     Initializes a new instance of the <see cref="ImportApplicationViewModel" /> class.
		/// </summary>
		public ImportApplicationViewModel( )
		{
			SearchCommand = new DelegateCommand<string>( Search );
			CloseCommand = new DelegateCommand( ( ) => CloseWindow = true );

			Packages = new CollectionViewSource( );
			PackageCollection = new ObservableCollection<Package>( );
			Packages.Source = PackageCollection;
			PackageCache = new List<Package>( );
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
		///     Gets the title.
		/// </summary>
		/// <value>
		///     The title.
		/// </value>
		public string Title => "Import Application";

		/// <summary>
		///     Gets or sets the open dialog.
		/// </summary>
		/// <value>
		///     The open dialog.
		/// </value>
		private static OpenFileDialog OpenDialog
		{
			get
			{
				if ( _dialog == null )
				{
					lock ( SyncRoot )
					{
						if ( _dialog == null )
						{
							_dialog = new OpenFileDialog
							{
								InitialDirectory = Config.ApplicationCache,
								Filter = "Xml Application (*.xml)|*.xml|SqLite Database (*.db)|*.db",
								Title = "Import Package",
								Multiselect = true
							};
						}
					}
				}

				return _dialog;
			}
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
		///     Dialogs the loaded.
		/// </summary>
		public void DialogLoaded( )
		{
			Import( );
		}

		private void AddPackage( Package package )
		{
			Application.Current.Dispatcher.VerifyAccess( );
			PackageCollection.Add( package );
			PackageCache.Add( package );
		}

		/// <summary>
		///     Imports this instance.
		/// </summary>
		private void Import( )
		{
			bool? result = OpenDialog.ShowDialog( );

			if ( result == true )
			{
				OpenDialog.InitialDirectory = string.Empty;

				IsBusy = true;

				var workerThread = new Thread( ImportAsynchronous );
				workerThread.Start( OpenDialog.FileNames );
			}
			else
			{
				CloseWindow = true;
			}
		}

		/// <summary>
		///     Runs the import method asynchronously.
		/// </summary>
		/// <param name="state">The state.</param>
		private void ImportAsynchronous( object state )
		{
			var context = new RoutedProcessingContext( message => BusyMessage = message, message => BusyMessage = message, message => BusyMessage = message, message => BusyMessage = message );

			var packagePaths = ( string[ ] ) state;

			foreach ( string packagePath in packagePaths )
			{
				Guid appVerId;
				using ( new TenantAdministratorContext( 0 ) )
				{
					appVerId = AppManager.ImportAppPackage( packagePath, context );
				}

				Package package = Package.GetPackage( appVerId );

				if ( package != null )
				{
					Application.Current.Dispatcher.Invoke( new Action<Package>( AddPackage ), package );
				}
			}

			IsBusy = false;
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