// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using ApplicationManager.Core;
using ApplicationManager.Support;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Migration.Processing;

namespace ApplicationManager
{
	/// <summary>
	///     Delete Application View Model.
	/// </summary>
	public class DeleteApplicationViewModel : ViewModelBase
	{
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
		///     Initializes a new instance of the <see cref="DeleteApplicationViewModel" /> class.
		/// </summary>
		/// <param name="application">The application.</param>
		public DeleteApplicationViewModel( Application application )
		{
			Application = application;

			SearchCommand = new DelegateCommand<string>( Search );
			CloseCommand = new DelegateCommand( ( ) => CloseWindow = true );
			DeleteCommand = new DelegateCommand( Delete );

			Packages = new CollectionViewSource( );
			PackageCollection = new ObservableCollection<Package>( );
			Packages.Source = PackageCollection;
			PackageCache = new List<Package>( );

			LoadPackages( );
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
		///     Gets or sets the delete command.
		/// </summary>
		/// <value>
		///     The delete command.
		/// </value>
		public ICommand DeleteCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets a value indicating whether [delete enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [delete enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool DeleteEnabled => SelectedPackage != null;

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
		///     Gets or sets a value indicating whether [packages deleted].
		/// </summary>
		/// <value>
		///     <c>true</c> if [packages deleted]; otherwise, <c>false</c>.
		/// </value>
		public bool PackagesDeleted
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
					OnPropertyChanged( "DeleteEnabled" );
				}
			}
		}

		/// <summary>
		///     Gets the title.
		/// </summary>
		/// <value>
		///     The title.
		/// </value>
		public string Title => $"Delete '{Application.Name}'";

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
		///     Deletes this instance.
		/// </summary>
		private void Delete( )
		{
			if ( SelectedPackage == null )
			{
				return;
			}

			IsBusy = true;

			var workerThread = new Thread( DeleteAsynchronous );
			workerThread.Start( );
		}

		/// <summary>
		///     Deletes asynchronously.
		/// </summary>
		private void DeleteAsynchronous( )
		{
			using ( new TenantAdministratorContext( 0 ) )
			{
				AppManager.DeleteApp( SelectedPackage.AppVerId );
			}

			IsBusy = false;

			PackagesDeleted = true;

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