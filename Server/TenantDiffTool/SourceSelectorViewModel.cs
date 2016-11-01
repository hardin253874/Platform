// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using TenantDiffTool.Core;
using TenantDiffTool.SupportClasses;
using File = TenantDiffTool.SupportClasses.File;

namespace TenantDiffTool
{
	/// <summary>
	/// </summary>
	public class SourceSelectorViewModel : ViewModelBase, ISourceProvider
	{
		/// <summary>
		///     App Library selected.
		/// </summary>
		private bool _appLibrarySelected;

		/// <summary>
		///     Dialog result.
		/// </summary>
		private bool? _closeWindow;

		/// <summary>
		///     Empty selected.
		/// </summary>
		private bool _emptySelected;

		/// <summary>
		///     File Selected.
		/// </summary>
		private bool _fileSelected;

		/// <summary>
		///     Database found
		/// </summary>
		private bool _foundDatabase;

		/// <summary>
		///     Selected application library app
		/// </summary>
		private ApplicationLibraryApp _selectedApplicationLibraryApp;

		/// <summary>
		///     Selected file.
		/// </summary>
		private File _selectedFile;

		/// <summary>
		///     Selected File Path.
		/// </summary>
		private string _selectedFilePath;

		/// <summary>
		///     Selected tenant.
		/// </summary>
		private Tenant _selectedTenant;

		/// <summary>
		///     Selected tenant app.
		/// </summary>
		private TenantApp _selectedTenantApp;

		/// <summary>
		///     Selected tenant app tenant.
		/// </summary>
		private Tenant _selectedTenantAppTenant;

		/// <summary>
		///     Source.
		/// </summary>
		private ISource _source;

		/// <summary>
		///     Tenant application selected.
		/// </summary>
		private bool _tenantApplicationSelected;

		/// <summary>
		///     Tenants.
		/// </summary>
		private List<Tenant> _tenants;

		/// <summary>
		///     Tenant selected.
		/// </summary>
		private bool _tenantSelected;

		/// <summary>
		///     Initializes a new instance of the <see cref="SourceSelectorViewModel" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="parent">The parent.</param>
		/// <exception cref="System.InvalidOperationException">Existing source cannot be null.</exception>
		public SourceSelectorViewModel( ISource source, Window parent )
			: base( parent )
		{
			if ( source == null )
			{
				throw new InvalidOperationException( "Existing source cannot be null." );
			}

			FileSelectCommand = new DelegateCommand( FileSelect );

			OkCommand = new DelegateCommand( ( ) =>
			{
				OkClicked = true;
				CloseWindow = true;
			} );

			CloseCommand = new DelegateCommand( ( ) =>
			{
				OkClicked = false;
				CloseWindow = true;
			} );

			AppLibraryDoubleClickCommand = new DelegateCommand( ( ) =>
			{
				if ( SelectedApplicationLibraryApp != null )
				{
					OkClicked = true;
					CloseWindow = true;
				}
			} );

			TenantAppDoubleClickCommand = new DelegateCommand( ( ) =>
			{
				if ( SelectedTenantApp != null )
				{
					OkClicked = true;
					CloseWindow = true;
				}
			} );

			ServerCommand = new DelegateCommand( ServerSelect );

			ApplicationLibraryApps = new CollectionViewSource( );
			ApplicationLibraryAppCollection = new ObservableCollection<ApplicationLibraryApp>( );
			ApplicationLibraryApps.Source = ApplicationLibraryAppCollection;

			TenantApps = new CollectionViewSource( );
			TenantAppCollection = new ObservableCollection<TenantApp>( );
			TenantApps.Source = TenantAppCollection;

			Source = source;
		}

		/// <summary>
		///     Gets or sets the app library double click command.
		/// </summary>
		/// <value>
		///     The app library double click command.
		/// </value>
		public ICommand AppLibraryDoubleClickCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [app library selected].
		/// </summary>
		/// <value>
		///     <c>true</c> if [app library selected]; otherwise, <c>false</c>.
		/// </value>
		public bool AppLibrarySelected
		{
			get
			{
				return _appLibrarySelected;
			}
			set
			{
				if ( _appLibrarySelected != value )
				{
					_appLibrarySelected = value;
					RaisePropertyChanged( "AppLibrarySelected" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the application library app collection.
		/// </summary>
		/// <value>
		///     The application library app collection.
		/// </value>
		public ObservableCollection<ApplicationLibraryApp> ApplicationLibraryAppCollection
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the application library apps.
		/// </summary>
		/// <value>
		///     The application library apps.
		/// </value>
		public CollectionViewSource ApplicationLibraryApps
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

		/// <summary>
		///     Gets or sets a value indicating whether [empty selected].
		/// </summary>
		/// <value>
		///     <c>true</c> if [empty selected]; otherwise, <c>false</c>.
		/// </value>
		public bool EmptySelected
		{
			get
			{
				return _emptySelected;
			}
			set
			{
				if ( _emptySelected != value )
				{
					_emptySelected = value;
					RaisePropertyChanged( "EmptySelected" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the file select command.
		/// </summary>
		/// <value>
		///     The file select command.
		/// </value>
		public ICommand FileSelectCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [file selected].
		/// </summary>
		/// <value>
		///     <c>true</c> if [file selected]; otherwise, <c>false</c>.
		/// </value>
		public bool FileSelected
		{
			get
			{
				return _fileSelected;
			}
			set
			{
				if ( _fileSelected != value )
				{
					_fileSelected = value;
					RaisePropertyChanged( "FileSelected" );
				}
			}
		}

		public bool FoundDatabase
		{
			get
			{
				return _foundDatabase;
			}
			set
			{
				if ( _foundDatabase != value )
				{
					_foundDatabase = value;
					RaisePropertyChanged( "FoundDatabase" );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [ok clicked].
		/// </summary>
		/// <value>
		///     <c>true</c> if [ok clicked]; otherwise, <c>false</c>.
		/// </value>
		public bool OkClicked
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the ok command.
		/// </summary>
		/// <value>
		///     The ok command.
		/// </value>
		public ICommand OkCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the selected application library app.
		/// </summary>
		/// <value>
		///     The selected application library app.
		/// </value>
		public ApplicationLibraryApp SelectedApplicationLibraryApp
		{
			get
			{
				return _selectedApplicationLibraryApp;
			}
			set
			{
				if ( _selectedApplicationLibraryApp != value )
				{
					_selectedApplicationLibraryApp = value;

					RaisePropertyChanged( "SelectedApplicationLibraryApp" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected file path.
		/// </summary>
		/// <value>
		///     The selected file path.
		/// </value>
		public File SelectedFile
		{
			get
			{
				return _selectedFile;
			}
			set
			{
				if ( _selectedFile != value )
				{
					_selectedFile = value;
					RaisePropertyChanged( "SelectedFile" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected file path.
		/// </summary>
		/// <value>
		///     The selected file path.
		/// </value>
		public string SelectedFilePath
		{
			get
			{
				return _selectedFilePath;
			}
			set
			{
				if ( _selectedFilePath != value )
				{
					_selectedFilePath = value;
					RaisePropertyChanged( "SelectedFilePath" );

					FileInfo info = new FileInfo( value );

					if ( string.Equals( info.Extension, ".xml", StringComparison.OrdinalIgnoreCase ) )
					{
						SelectedFile = new XmlFile( value, Source.Context );
					}
					else
					{
						SelectedFile = new SqliteFile( value, Source.Context );
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected tenant.
		/// </summary>
		/// <value>
		///     The selected tenant.
		/// </value>
		public Tenant SelectedTenant
		{
			get
			{
				return _selectedTenant;
			}
			set
			{
				if ( _selectedTenant != value )
				{
					_selectedTenant = value;
					RaisePropertyChanged( "SelectedTenant" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected tenant app.
		/// </summary>
		/// <value>
		///     The selected tenant app.
		/// </value>
		public TenantApp SelectedTenantApp
		{
			get
			{
				return _selectedTenantApp;
			}
			set
			{
				if ( _selectedTenantApp != value )
				{
					_selectedTenantApp = value;

					RaisePropertyChanged( "SelectedTenantApp" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected tenant app tenant.
		/// </summary>
		/// <value>
		///     The selected tenant app tenant.
		/// </value>
		public Tenant SelectedTenantAppTenant
		{
			get
			{
				return _selectedTenantAppTenant;
			}
			set
			{
				if ( _selectedTenantAppTenant != value )
				{
					_selectedTenantAppTenant = value;
					RaisePropertyChanged( "SelectedTenantAppTenant" );

					LoadTenantApps( value );
				}
			}
		}

		/// <summary>
		///     Gets or sets the server command.
		/// </summary>
		/// <value>
		///     The server command.
		/// </value>
		public ICommand ServerCommand
		{
			get;
			set;
		}

		public ISource Source
		{
			get
			{
				return _source;
			}
			set
			{
				if ( !Equals( _source, value ) )
				{
					_source = value;

					if ( _source != null )
					{
						Load( );
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the tenant app collection.
		/// </summary>
		/// <value>
		///     The tenant app collection.
		/// </value>
		public ObservableCollection<TenantApp> TenantAppCollection
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the tenant app double click command.
		/// </summary>
		/// <value>
		///     The tenant app double click command.
		/// </value>
		public ICommand TenantAppDoubleClickCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [tenant application selected].
		/// </summary>
		/// <value>
		///     <c>true</c> if [tenant application selected]; otherwise, <c>false</c>.
		/// </value>
		public bool TenantApplicationSelected
		{
			get
			{
				return _tenantApplicationSelected;
			}
			set
			{
				if ( _tenantApplicationSelected != value )
				{
					_tenantApplicationSelected = value;
					RaisePropertyChanged( "TenantApplicationSelected" );
				}
			}
		}

		/// <summary>
		///     Gets the tenant apps.
		/// </summary>
		/// <value>
		///     The tenant apps.
		/// </value>
		public CollectionViewSource TenantApps
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [tenant selected].
		/// </summary>
		/// <value>
		///     <c>true</c> if [tenant selected]; otherwise, <c>false</c>.
		/// </value>
		public bool TenantSelected
		{
			get
			{
				return _tenantSelected;
			}
			set
			{
				if ( _tenantSelected != value )
				{
					_tenantSelected = value;
					RaisePropertyChanged( "TenantSelected" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the tenants.
		/// </summary>
		/// <value>
		///     The tenants.
		/// </value>
		public List<Tenant> Tenants
		{
			get
			{
				return _tenants;
			}
			set
			{
				if ( _tenants != value )
				{
					_tenants = value;
					RaisePropertyChanged( "Tenants" );
				}
			}
		}

		/// <summary>
		///     Gets the source.
		/// </summary>
		/// <returns></returns>
		public ISource GetSource( )
		{
			if ( TenantSelected && !string.IsNullOrEmpty( SelectedTenant?.Name ) )
			{
				return SelectedTenant;
			}

			if ( FileSelected && !string.IsNullOrEmpty( SelectedFile?.Path ) )
			{
				return SelectedFile;
			}

			if ( AppLibrarySelected && SelectedApplicationLibraryApp != null )
			{
				return SelectedApplicationLibraryApp;
			}

			if ( TenantApplicationSelected && SelectedTenantApp != null )
			{
				return SelectedTenantApp;
			}

			return new Empty( Source.Context );
		}

		/// <summary>
		///     Sets the existing source.
		/// </summary>
		/// <param name="source">The source.</param>
		public void SetExistingSource( ISource source )
		{
			var tenant = source as Tenant;

			if ( tenant != null )
			{
				TenantSelected = true;
				SelectedTenant = Tenants.FirstOrDefault( t => t.Id == tenant.Id );
				return;
			}

			var file = source as File;

			if ( file != null )
			{
				TenantSelected = false;
				FileSelected = true;
				SelectedFilePath = file.Path;
				return;
			}

			var app = source as ApplicationLibraryApp;

			if ( app != null )
			{
				TenantSelected = false;
				AppLibrarySelected = true;
				SelectedApplicationLibraryApp = ApplicationLibraryAppCollection.FirstOrDefault( a => a.Id == app.Id );
				return;
			}

			var tenantApp = source as TenantApp;

			if ( tenantApp != null )
			{
				TenantSelected = false;
				TenantApplicationSelected = true;
				SelectedTenantAppTenant = Tenants.FirstOrDefault( t => t.Id == tenantApp.Tenant.Id );
				SelectedTenantApp = TenantAppCollection.FirstOrDefault( t => t.Id == tenantApp.Id );
				return;
			}

			var empty = source as Empty;

			if ( empty != null )
			{
				TenantSelected = false;
				EmptySelected = true;
			}
		}

		/// <summary>
		///     Files the select.
		/// </summary>
		private void FileSelect( )
		{
			var openDialog = new OpenFileDialog
			{
				Filter = "Xml Application (*.xml)|*.xml|SqLite Database (*.db)|*.db",
				Title = "Import Package",
				Multiselect = false
			};

			bool? result = openDialog.ShowDialog( );

			if ( result == true )
			{
				SelectedFilePath = openDialog.FileName;
			}
		}

		/// <summary>
		///     Loads this instance.
		/// </summary>
		private void Load( )
		{
			try
			{
				LoadApplicationLibraryApps( );

				LoadTenants( );
			}
			catch ( DbException )
			{
			}
		}

		/// <summary>
		///     Loads the application library apps.
		/// </summary>
		private void LoadApplicationLibraryApps( )
		{
			ApplicationLibraryAppCollection.Clear( );

			foreach ( ApplicationLibraryApp app in ApplicationLibraryApp.GetApplicationLibraryApps( Source.Context ) )
			{
				ApplicationLibraryAppCollection.Add( app );
			}
		}

		/// <summary>
		///     Loads the tenant apps.
		/// </summary>
		/// <param name="tenant">The tenant.</param>
		private void LoadTenantApps( Tenant tenant )
		{
			if ( tenant == null )
			{
				return;
			}

			TenantAppCollection.Clear( );

			foreach ( TenantApp app in tenant.GetApps( ) )
			{
				TenantAppCollection.Add( app );
			}
		}

		/// <summary>
		///     Loads the tenants.
		/// </summary>
		private void LoadTenants( )
		{
			var tenants = new List<Tenant>( );

			using ( IDbCommand command = Source.Context.CreateCommand( SqlQueries.GetTenants ) )
			{
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						long id = reader.GetInt64( 0 );
						string tenant = reader.GetString( 1, null );

						if ( tenant != null )
						{
							tenants.Add( new Tenant( id, tenant, Source.Context ) );
						}
					}
				}
			}

			Tenants = tenants;

			if ( Tenants != null && Tenants.Any( ) )
			{
				SelectedTenantAppTenant = Tenants.First( );
				SelectedTenant = Tenants.First( );
				FoundDatabase = true;
			}
			else
			{
				FoundDatabase = false;
			}
		}

		/// <summary>
		///     Servers the select.
		/// </summary>
		private void ServerSelect( )
		{
			var properties = new DatabaseProperties( );

			var viewModel = properties.DataContext as DatabasePropertiesViewModel;

			if ( viewModel == null )
			{
				return;
			}

			viewModel.SetExistingServer( Source.Context );

			properties.Owner = ParentWindow;
			properties.ShowDialog( );

			if ( viewModel.OkClicked )
			{
				Source.Context = viewModel.DatabaseInfo;

				Load( );
			}
		}
	}
}