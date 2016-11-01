// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using ApplicationManager.Core;
using ApplicationManager.Support;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Migration.Processing;

namespace ApplicationManager
{
	public class RepairApplicationViewModel : ViewModelBase
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
		///     Initializes a new instance of the <see cref="RepairApplicationViewModel" /> class.
		/// </summary>
		/// <param name="application">The application.</param>
		public RepairApplicationViewModel( Application application )
		{
			Application = application;

			SearchCommand = new DelegateCommand<string>( Search );
			CloseCommand = new DelegateCommand( ( ) => CloseWindow = true );
			RepairCommand = new DelegateCommand( Repair );

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
					_busyMessage = value;
					RaisePropertyChanged( "BusyMessage" );
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
					_closeWindow = value;
					RaisePropertyChanged( "CloseWindow" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the repair command.
		/// </summary>
		/// <value>
		///     The repair command.
		/// </value>
		public ICommand RepairCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [button enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [button enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool RepairEnabled
		{
			get
			{
				return SelectedPackage != null;
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
					_isBusy = value;
					RaisePropertyChanged( "IsBusy" );
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
			private set;
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
			private set;
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
					_selectedPackage = value;

					RaisePropertyChanged( "SelectedPackage" );
					RaisePropertyChanged( "RepairEnabled" );
				}
			}
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
				return string.Format( "Repair '{0}'", Application.Name );
			}
		}

		/// <summary>
		///     Gets or sets the application.
		/// </summary>
		/// <value>
		///     The application.
		/// </value>
		private Application Application
		{
			get;
			set;
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
			set;
		}

		/// <summary>
		///     Repairs this instance.
		/// </summary>
		private void Repair( )
		{
			IsBusy = true;

			var workerThread = new Thread( RepairAsync );
			workerThread.Start( SelectedPackage );
		}

		/// <summary>
		///     Deploys the application asynchronously.
		/// </summary>
		/// <param name="state">The state.</param>
		private void RepairAsync( object state )
		{
			var package = state as Package;

			var context = new RoutedProcessingContext( message => BusyMessage = message, message => BusyMessage = message, message => BusyMessage = message, message => BusyMessage = message );

			using ( new TenantAdministratorContext( 0 ) )
			{
				if ( package != null )
				{
					AppManager.RepairApp( package.SelectedTenant.EntityId, package.AppVerId, context );
				}
			}

			IsBusy = false;

			System.Windows.Application.Current.Dispatcher.Invoke( LoadPackages );
		}

		/// <summary>
		///     Loads the packages.
		/// </summary>
		private void LoadPackages( )
		{
			PackageCollection.Clear( );
			PackageCache.Clear( );

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