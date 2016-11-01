// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Application
{
	/// <summary>
	///     Library Applications View Model.
	/// </summary>
	// ReSharper disable ExplicitCallerInfoArgument
	public class LibraryApplicationsViewModel : ViewModelBase
	{
		/// <summary>
		///     The application library applications.
		/// </summary>
		private List<AppLibraryApp> _appLibraryApps;

		/// <summary>
		/// The filtered application library apps
		/// </summary>
		private List<AppLibraryApp> _filteredAppLibraryApps;

		/// <summary>
		///     The plugin settings
		/// </summary>
		private IPluginSettings _pluginSettings;

		/// <summary>
		/// The application filter open
		/// </summary>
		private bool _applicationFilterOpen;

		/// <summary>
		/// The application filter open time
		/// </summary>
		private DateTime _applicationFilterOpenTime;

		/// <summary>
		/// The version filter open
		/// </summary>
		private bool _versionFilterOpen;

		/// <summary>
		/// The version filter open time
		/// </summary>
		private DateTime _versionFilterOpenTime;

		/// <summary>
		/// The publisher filter open
		/// </summary>
		private bool _publisherFilterOpen;

		/// <summary>
		/// The publisher filter open time
		/// </summary>
		private DateTime _publisherFilterOpenTime;

		/// <summary>
		/// The publisherUrl filter open
		/// </summary>
		private bool _publisherUrlFilterOpen;

		/// <summary>
		/// The publisherUrl filter open time
		/// </summary>
		private DateTime _publisherUrlFilterOpenTime;

		/// <summary>
		/// The latest versions only
		/// </summary>
		private bool _latestVersionsOnly;

		/// <summary>
		///     Initializes a new instance of the <see cref="LibraryApplicationsViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public LibraryApplicationsViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			RefreshCommand = new DelegateCommand( Refresh );
			CopyAppValueCommand = new DelegateCommand<AppLibraryApp>( CopyAppValueClick );
			CopyValueCommand = new DelegateCommand<AppLibraryApp>( CopyValueClick );
			CopyCommand = new DelegateCommand<AppLibraryApp>( CopyClick );
			FilterApplicationCommand = new DelegateCommand( FilterApplicationClick );
			FilterVersionCommand = new DelegateCommand( FilterVersionClick );
			FilterPublisherCommand = new DelegateCommand( FilterPublisherClick );
			FilterPublisherUrlCommand = new DelegateCommand( FilterPublisherUrlClick );
		}

		/// <summary>
		/// Gets or sets a value indicating whether [latest versions only].
		/// </summary>
		/// <value>
		///   <c>true</c> if [latest versions only]; otherwise, <c>false</c>.
		/// </value>
		public bool LatestVersionsOnly
		{
			get
			{
				return _latestVersionsOnly;
			}
			set
			{
				SetProperty( ref _latestVersionsOnly, value );

				LoadApplications( );
			}
		}

		/// <summary>
		/// Gets or sets the filter application command.
		/// </summary>
		/// <value>
		/// The filter application command.
		/// </value>
		public ICommand FilterApplicationCommand
		{
			get;
			set;
		}

		/// <summary>
		/// Filters the application click.
		/// </summary>
		private void FilterApplicationClick( )
		{
			ApplicationFilterOpen = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [application filter open].
		/// </summary>
		/// <value>
		/// <c>true</c> if [application filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool ApplicationFilterOpen
		{
			get
			{
				return _applicationFilterOpen;
			}
			set
			{
				if ( _applicationFilterOpen != value && _applicationFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _applicationFilterOpen, value );

					_applicationFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		/// Gets or sets the filter Version command.
		/// </summary>
		/// <value>
		/// The filter Version command.
		/// </value>
		public ICommand FilterVersionCommand
		{
			get;
			set;
		}

		/// <summary>
		/// Filters the Version click.
		/// </summary>
		private void FilterVersionClick( )
		{
			VersionFilterOpen = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [Version filter open].
		/// </summary>
		/// <value>
		/// <c>true</c> if [Version filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool VersionFilterOpen
		{
			get
			{
				return _versionFilterOpen;
			}
			set
			{
				if ( _versionFilterOpen != value && _versionFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _versionFilterOpen, value );

					_versionFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		/// Gets or sets the filter Publisher command.
		/// </summary>
		/// <value>
		/// The filter Publisher command.
		/// </value>
		public ICommand FilterPublisherCommand
		{
			get;
			set;
		}

		/// <summary>
		/// Filters the Publisher click.
		/// </summary>
		private void FilterPublisherClick( )
		{
			PublisherFilterOpen = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [Publisher filter open].
		/// </summary>
		/// <value>
		/// <c>true</c> if [Publisher filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool PublisherFilterOpen
		{
			get
			{
				return _publisherFilterOpen;
			}
			set
			{
				if ( _publisherFilterOpen != value && _publisherFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _publisherFilterOpen, value );

					_publisherFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		/// Gets or sets the filter PublisherUrl command.
		/// </summary>
		/// <value>
		/// The filter PublisherUrl command.
		/// </value>
		public ICommand FilterPublisherUrlCommand
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the application filters.
		/// </summary>
		/// <value>
		/// The application filters.
		/// </value>
		public List<FilterObject> ApplicationFilters
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the version filters.
		/// </summary>
		/// <value>
		/// The version filters.
		/// </value>
		public List<FilterObject> VersionFilters
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the publisher filters.
		/// </summary>
		/// <value>
		/// The publisher filters.
		/// </value>
		public List<FilterObject> PublisherFilters
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the publisher URL filters.
		/// </summary>
		/// <value>
		/// The publisher URL filters.
		/// </value>
		public List<FilterObject> PublisherUrlFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Filters the update.
		/// </summary>
		private void FilterUpdate( )
		{
			List<AppLibraryApp> filteredApps = new List<AppLibraryApp>( );

			foreach ( var app in AppLibraryApps )
			{
				FilterObject applicationFilterObject = ApplicationFilters.FirstOrDefault( f => f.Value.ToString( ) == app.Application );
				FilterObject versionFilterObject = VersionFilters.FirstOrDefault( f => f.Value.ToString( ) == app.Version );
				FilterObject publisherFilterObject = PublisherFilters.FirstOrDefault( f => f.Value.ToString( ) == app.Publisher );
				FilterObject publisherUrlFilterObject = PublisherUrlFilters.FirstOrDefault( f => f.Value.ToString( ) == app.PublisherUrl );

				if ( ( applicationFilterObject == null || applicationFilterObject.IsFiltered ) && ( versionFilterObject == null || versionFilterObject.IsFiltered ) && ( publisherFilterObject == null || publisherFilterObject.IsFiltered ) && ( publisherUrlFilterObject == null || publisherUrlFilterObject.IsFiltered ) )
				{
					filteredApps.Add( app );
				}
			}

			FilteredAppLibraryApps = filteredApps;
		}

		/// <summary>
		/// Filters the PublisherUrl click.
		/// </summary>
		private void FilterPublisherUrlClick( )
		{
			PublisherUrlFilterOpen = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [PublisherUrl filter open].
		/// </summary>
		/// <value>
		/// <c>true</c> if [PublisherUrl filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool PublisherUrlFilterOpen
		{
			get
			{
				return _publisherUrlFilterOpen;
			}
			set
			{
				if ( _publisherUrlFilterOpen != value && _publisherUrlFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _publisherUrlFilterOpen, value );

					_publisherUrlFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		///     Gets or sets the application library applications.
		/// </summary>
		/// <value>
		///     The application library applications.
		/// </value>
		private List<AppLibraryApp> AppLibraryApps
		{
			get
			{
				return _appLibraryApps;
			}
			set
			{
				SetProperty( ref _appLibraryApps, value );
			}
		}

		/// <summary>
		/// Gets or sets the filtered application library apps.
		/// </summary>
		/// <value>
		/// The filtered application library apps.
		/// </value>
		public List<AppLibraryApp> FilteredAppLibraryApps
		{
			get
			{
				return _filteredAppLibraryApps;
			}
			set
			{
				SetProperty( ref _filteredAppLibraryApps, value );
			}
		}

		/// <summary>
		///     Gets or sets the copy command.
		/// </summary>
		/// <value>
		///     The copy command.
		/// </value>
		public ICommand CopyCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the copy value command.
		/// </summary>
		/// <value>
		///     The copy value command.
		/// </value>
		public ICommand CopyValueCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the copy app value command.
		/// </summary>
		/// <value>
		///     The copy value command.
		/// </value>
		public ICommand CopyAppValueCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		public IPluginSettings PluginSettings
		{
			private get
			{
				return _pluginSettings;
			}
			set
			{
				_pluginSettings = value;

				LoadApplications( );
			}
		}

		/// <summary>
		///     Gets the refresh command.
		/// </summary>
		/// <value>
		///     The refresh command.
		/// </value>
		public ICommand RefreshCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Copies the value click.
		/// </summary>
		/// <param name="app">The application.</param>
		private void CopyAppValueClick( AppLibraryApp app )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, app.ApplicationId.ToString( "B" ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Value copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Copies the click.
		/// </summary>
		/// <param name="app">The application.</param>
		private void CopyClick( AppLibraryApp app )
		{
			const string format = @"Application:           {0}
Application Entity Id: {1}
Package Id:            {2}
Package Entity Id:     {3}
Version:               {4}
Application Id:        {5}
Publisher:             {6}
Publisher Url:         {7}
Release Date:          {8}";

			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, string.Format( format, app.Application, app.ApplicationEntityId, app.PackageId.ToString( "B" ), app.PackageEntityId, app.Version, app.ApplicationId.ToString( "B" ), app.Publisher, app.PublisherUrl, app.ReleaseDate ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Data copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Copies the value click.
		/// </summary>
		/// <param name="app">The application.</param>
		private void CopyValueClick( AppLibraryApp app )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, app.PackageId.ToString( "B" ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Value copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		/// Loads the applications.
		/// </summary>
		private void LoadApplications( )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - LoadApplications
SET NOCOUNT ON

DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', DEFAULT )
DECLARE @app BIGINT = dbo.fnAliasNsId( 'app', 'core', DEFAULT )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )
DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
DECLARE @appVersionString BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
DECLARE @appVerId BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )
DECLARE @applicationId BIGINT = dbo.fnAliasNsId( 'applicationId', 'core', DEFAULT )


SELECT
	Application = x.Application,
	ApplicationEntityId = x.ApplicationId,
	PackageId = pid.Data,
	PackageEntityId = p.PackageId,
	Version = p.Version,
	ApplicationId = aid.Data,
	Publisher = p1.Data,
	PublisherUrl = u.Data,
	ReleaseDate = c.Data
FROM
	(
	SELECT
		Application = n.Data,
		ApplicationId = n.EntityId
	FROM
		Relationship r
	JOIN
		Data_NVarChar n ON
			n.TenantId = r.TenantId
			AND r.FromId = n.EntityId
			AND n.FieldId = @name
	WHERE
		r.TenantId = 0
		AND r.TypeId = @isOfType
		AND r.ToId = @app
	) x
JOIN
(
	SELECT
		dt.PackageId,
		dt.ApplicationId,
		dt.Version,
		dt.RowNumber
	FROM
	(
		SELECT
			ROW_NUMBER( ) OVER
			(
				PARTITION BY
					r.ToId
				ORDER BY
					CAST( '/' + REPLACE( dbo.fnSanitiseVersion( v.Data ), '.', '/' ) + '/' AS HIERARCHYID ) DESC
			) AS 'RowNumber',
			PackageId = r.FromId,
			ApplicationId = r.ToId,
			Version = v.Data
		FROM
			Relationship r
		JOIN
			Data_NVarChar v ON
				v.TenantId = r.TenantId
				AND v.EntityId = r.FromId
				AND r.TypeId = @packageForApplication
				AND	v.FieldId = @appVersionString
		WHERE
			r.TenantId = 0
	) dt
) p ON
	x.ApplicationId = p.ApplicationId
JOIN
	Data_Guid pid ON
		pid.TenantId = 0
		AND pid.EntityId = p.PackageId
		AND	pid.FieldId = @appVerId
JOIN
	Data_Guid aid ON
		aid.TenantId = 0
		AND aid.EntityId = x.ApplicationId
		AND	aid.FieldId = @applicationId
LEFT JOIN
(
	SELECT
		p.EntityId,
		p.Data
	FROM
		Data_NVarChar p
	JOIN
		Data_Alias ap ON
			ap.TenantId = p.TenantId
			AND ap.EntityId = p.FieldId
			AND	ap.Data = 'publisher'
			AND	ap.Namespace = 'core'
	WHERE
		p.TenantId = 0
) p1 ON
	x.ApplicationId = p1.EntityId
LEFT JOIN
(
	SELECT
		u.EntityId,
		u.Data
	FROM
		Data_NVarChar u
	JOIN
		Data_Alias au ON
			au.TenantId = u.TenantId
			AND u.FieldId = au.EntityId
			AND au.Data = 'publisherUrl'
			AND	au.Namespace = 'core'
	WHERE
		u.TenantId = 0
) u ON
	x.ApplicationId = u.EntityId
LEFT JOIN
(
	SELECT
		c.EntityId,
		c.Data
	FROM
		Data_DateTime c
	JOIN
		Data_Alias ac ON
			ac.TenantId = c.TenantId
			AND ac.EntityId = c.FieldId
			AND ac.Data = 'releaseDate'
			AND	ac.Namespace = 'core'
	WHERE
		c.TenantId = 0
) c ON
	x.ApplicationId = c.EntityId";

			try
			{
				using ( IDbCommand command = databaseManager.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						var appLibraryApps = new List<AppLibraryApp>( );

						HashSet<string> applications = new HashSet<string>( );
						HashSet<string> versions = new HashSet<string>( );
						HashSet<string> publishers = new HashSet<string>( );
						HashSet<string> publisherUrls = new HashSet<string>( );

						Dictionary<Guid, AppLibraryApp> map = new Dictionary<Guid, AppLibraryApp>( );

						do
						{
							while ( reader.Read( ) )
							{
								var application = reader.GetString( 0, "Unnamed" );
								var applicationEntityId = reader.GetInt64( 1, -1 );
								var packageId = reader.GetGuid( 2 );
								var packageEntityId = reader.GetInt64( 3, -1 );
								var version = reader.GetString( 4, "" );
								var applicationId = reader.GetGuid( 5 );
								var publisher = reader.GetString( 6, "" );
								var publisherUrl = reader.GetString( 7, "" );
								var releaseDate = reader.GetDateTime( 8, DateTime.MinValue );

								var appLibraryApp = new AppLibraryApp( application, applicationEntityId, packageId, packageEntityId, version, applicationId, publisher, publisherUrl, releaseDate );

								if ( LatestVersionsOnly )
								{
									AppLibraryApp existingApp;

									if ( map.TryGetValue( applicationId, out existingApp ) )
									{
										if ( existingApp != null && !string.IsNullOrEmpty( existingApp.Version ) )
										{
											Version existingVersion;
											Version thisVersion;

											if ( Version.TryParse( existingApp.Version, out existingVersion ) && Version.TryParse( appLibraryApp.Version, out thisVersion ) )
											{
												if ( thisVersion > existingVersion )
												{
													map [ applicationId ] = appLibraryApp;
												}
											}
										}
									}
									else
									{
										map [ applicationId ] = appLibraryApp;
									}
								}
								else
								{
									appLibraryApps.Add( appLibraryApp );
								}
							}
						}
						while ( reader.NextResult( ) );

						if ( LatestVersionsOnly )
						{
							AppLibraryApps = map.Values.OrderBy( x => x.Application ).ToList( );
						}
						else
						{
							AppLibraryApps = appLibraryApps.OrderBy( x => x.Application ).ToList( );
						}

						applications.UnionWith( AppLibraryApps.Select( a => a.Application ) );
						versions.UnionWith( AppLibraryApps.Select( a => a.Version ) );
						publishers.UnionWith( AppLibraryApps.Select( a => a.Publisher ) );
						publisherUrls.UnionWith( AppLibraryApps.Select( a => a.PublisherUrl ) );

						ApplicationFilters = new List<FilterObject>( );

						foreach ( string sol in applications.OrderBy( k => k ) )
						{
							if ( !string.IsNullOrEmpty( sol ) )
							{
								ApplicationFilters.Add( new FilterObject( sol, true, ApplicationFilterUpdate ) );
							}
						}

						OnPropertyChanged( "ApplicationFilters" );

						VersionFilters = new List<FilterObject>( );

						foreach ( string ver in versions.OrderBy( k => k ) )
						{
							if ( !string.IsNullOrEmpty( ver ) )
							{
								VersionFilters.Add( new FilterObject( ver, true, VersionFilterUpdate ) );
							}
						}

						OnPropertyChanged( "VersionFilters" );

						PublisherFilters = new List<FilterObject>( );

						foreach ( string pub in publishers.OrderBy( k => k ) )
						{
							if ( !string.IsNullOrEmpty( pub ) )
							{
								PublisherFilters.Add( new FilterObject( pub, true, PublisherFilterUpdate ) );
							}
						}

						OnPropertyChanged( "PublisherFilters" );

						PublisherUrlFilters = new List<FilterObject>( );

						foreach ( string puburl in publisherUrls.OrderBy( k => k ) )
						{
							if ( !string.IsNullOrEmpty( puburl ) )
							{
								PublisherUrlFilters.Add( new FilterObject( puburl, true, PublisherUrlFilterUpdate ) );
							}
						}

						OnPropertyChanged( "PublisherUrlFilters" );

						FilteredAppLibraryApps = new List<AppLibraryApp>( AppLibraryApps );
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		/// Applications the filter update.
		/// </summary>
		private void ApplicationFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "ApplicationFilters" );
		}

		/// <summary>
		/// Versions the filter update.
		/// </summary>
		private void VersionFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "VersionFilters" );
		}

		/// <summary>
		/// Publishers the filter update.
		/// </summary>
		private void PublisherFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "PublisherFilters" );
		}

		/// <summary>
		/// Publishers the URL filter update.
		/// </summary>
		private void PublisherUrlFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "PublisherUrlFilters" );
		}

		/// <summary>
		///     Refreshes this instance.
		/// </summary>
		private void Refresh( )
		{
			LoadApplications( );
		}
	}
}