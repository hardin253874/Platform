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
	///     Tenant Applications View Model.
	/// </summary>
	// ReSharper disable ExplicitCallerInfoArgument
	public class TenantApplicationsViewModel : ViewModelBase
	{
		/// <summary>
		///     The filtered tenant apps
		/// </summary>
		private List<TenantApp> _filteredTenantApps;

		/// <summary>
		///     The plugin settings
		/// </summary>
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The publisher filter open
		/// </summary>
		private bool _publisherFilterOpen;

		/// <summary>
		///     The Publisher filter open time
		/// </summary>
		private DateTime _publisherFilterOpenTime;

		/// <summary>
		///     The publisher URL filter open
		/// </summary>
		private bool _publisherUrlFilterOpen;

        /// <summary>
        /// The protected filter open
        /// </summary>
        private bool _protectedFilterOpen;

        /// <summary>
        ///     The PublisherUrl filter open time
        /// </summary>
        private DateTime _publisherUrlFilterOpenTime;

        /// <summary>
        /// The protected filter open time
        /// </summary>
        private DateTime _protectedFilterOpenTime;

        /// <summary>
        ///     The solution filter open
        /// </summary>
        private bool _solutionFilterOpen;

		/// <summary>
		///     The tenant filter open time
		/// </summary>
		private DateTime _solutionFilterOpenTime;

		/// <summary>
		///     The tenant applications
		/// </summary>
		private List<TenantApp> _tenantApps;

		/// <summary>
		///     The tenant filter open
		/// </summary>
		private bool _tenantFilterOpen;

		/// <summary>
		///     The tenant filter open time
		/// </summary>
		private DateTime _tenantFilterOpenTime;

		/// <summary>
		///     The version filter open
		/// </summary>
		private bool _versionFilterOpen;

		/// <summary>
		///     The Version filter open time
		/// </summary>
		private DateTime _versionFilterOpenTime;

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantApplicationsViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public TenantApplicationsViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			RefreshCommand = new DelegateCommand( Refresh );
			CopyIdCommand = new DelegateCommand<TenantApp>( CopyIdClick );
			CopyValueCommand = new DelegateCommand<TenantApp>( CopyValueClick );
			CopyPkgValueCommand = new DelegateCommand<TenantApp>( CopyPkgValueClick );
			CopyCommand = new DelegateCommand<TenantApp>( CopyClick );
			FilterTenantCommand = new DelegateCommand( FilterTenantClick );
			FilterSolutionCommand = new DelegateCommand( FilterSolutionClick );
			FilterVersionCommand = new DelegateCommand( FilterVersionClick );
			FilterPublisherCommand = new DelegateCommand( FilterPublisherClick );
			FilterPublisherUrlCommand = new DelegateCommand( FilterPublisherUrlClick );
            FilterProtectedCommand = new DelegateCommand( FilterProtectedClick );
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
		///     Gets or sets the copy identifier command.
		/// </summary>
		/// <value>
		///     The copy identifier command.
		/// </value>
		public ICommand CopyIdCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the copy PKG value command.
		/// </summary>
		/// <value>
		///     The copy PKG value command.
		/// </value>
		public ICommand CopyPkgValueCommand
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
		///     Gets or sets the filtered tenant applications.
		/// </summary>
		/// <value>
		///     The filtered accounts.
		/// </value>
		public List<TenantApp> FilteredTenantApps
		{
			get
			{
				return _filteredTenantApps;
			}
			set
			{
				SetProperty( ref _filteredTenantApps, value );
			}
		}

		/// <summary>
		///     Gets or sets the filter Publisher command.
		/// </summary>
		/// <value>
		///     The filter Publisher command.
		/// </value>
		public ICommand FilterPublisherCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the filter PublisherUrl command.
		/// </summary>
		/// <value>
		///     The filter PublisherUrl command.
		/// </value>
		public ICommand FilterPublisherUrlCommand
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the filter protected command.
        /// </summary>
        /// <value>
        /// The filter protected command.
        /// </value>
        public ICommand FilterProtectedCommand
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the filter solution command.
        /// </summary>
        /// <value>
        ///     The filter solution command.
        /// </value>
        public ICommand FilterSolutionCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the filter tenant command.
		/// </summary>
		/// <value>
		///     The filter tenant command.
		/// </value>
		public ICommand FilterTenantCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the filter Version command.
		/// </summary>
		/// <value>
		///     The filter Version command.
		/// </value>
		public ICommand FilterVersionCommand
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

				Refresh( );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [Publisher filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [Publisher filter open]; otherwise, <c>false</c>.
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
		///     Gets or sets the Publisher filters.
		/// </summary>
		/// <value>
		///     The Publisher filters.
		/// </value>
		public List<FilterObject> PublisherFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [PublisherUrl filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [PublisherUrl filter open]; otherwise, <c>false</c>.
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
        /// Gets or sets a value indicating whether [protected filter open].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [protected filter open]; otherwise, <c>false</c>.
        /// </value>
        public bool ProtectedFilterOpen
	    {
            get
            {
                return _protectedFilterOpen;
            }
            set
            {
                if (_protectedFilterOpen != value && _protectedFilterOpenTime.AddMilliseconds(500) < DateTime.UtcNow)
                {
                    SetProperty(ref _protectedFilterOpen, value);

                    _protectedFilterOpenTime = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the PublisherUrl filters.
        /// </summary>
        /// <value>
        ///     The PublisherUrl filters.
        /// </value>
        public List<FilterObject> PublisherUrlFilters
		{
			get;
			set;
		}

        /// <summary>
		///     Gets or sets the Protected filters.
		/// </summary>
		/// <value>
		///     The Protected filters.
		/// </value>
		public List<FilterObject> ProtectedFilters
        {
            get;
            set;
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
		///     Gets or sets a value indicating whether [tenant filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [tenant filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool SolutionFilterOpen
		{
			get
			{
				return _solutionFilterOpen;
			}
			set
			{
				if ( _solutionFilterOpen != value && _solutionFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _solutionFilterOpen, value );

					_solutionFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		///     Gets or sets the solution filters.
		/// </summary>
		/// <value>
		///     The solution filters.
		/// </value>
		public List<FilterObject> SolutionFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [tenant filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [tenant filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool TenantFilterOpen
		{
			get
			{
				return _tenantFilterOpen;
			}
			set
			{
				if ( _tenantFilterOpen != value && _tenantFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _tenantFilterOpen, value );

					_tenantFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		///     Gets or sets the tenant filters.
		/// </summary>
		/// <value>
		///     The tenant filters.
		/// </value>
		public List<FilterObject> TenantFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [Version filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [Version filter open]; otherwise, <c>false</c>.
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
		///     Gets or sets the Version filters.
		/// </summary>
		/// <value>
		///     The Version filters.
		/// </value>
		public List<FilterObject> VersionFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the tenant applications.
		/// </summary>
		/// <value>
		///     The tenant applications.
		/// </value>
		private List<TenantApp> TenantApps
		{
			get
			{
				return _tenantApps;
			}
			set
			{
				SetProperty( ref _tenantApps, value );
			}
		}

		/// <summary>
		///     Copies the click.
		/// </summary>
		/// <param name="app">The application.</param>
		private void CopyClick( TenantApp app )
		{
			const string format = @"Tenant Id:             {0}
Tenant Name:           {1}
Solution:              {2}
Solution Entity Id:    {3}
Solution Version:      {4}
Package Id:            {5}
Package Entity Id:     {6}
Package Version:       {7}
Application Entity Id: {8}
Application Id:        {9}
Publisher:             {10}
Publisher Url:         {11}
Release Date:          {12}";

			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, string.Format( format, app.TenantId, app.TenantName, app.Solution, app.SolutionEntityId, app.SolutionVersion, app.PackageId.ToString( "B" ), app.PackageEntityId, app.PackageVersion, app.ApplicationEntityId, app.ApplicationId.ToString( "B" ), app.Publisher, app.PublisherUrl, app.ReleaseDate ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Data copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Copies the identifier click.
		/// </summary>
		/// <param name="app">The application.</param>
		private void CopyIdClick( TenantApp app )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, app.SolutionEntityId.ToString( ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Id copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Copies the PKG value click.
		/// </summary>
		/// <param name="app">The application.</param>
		private void CopyPkgValueClick( TenantApp app )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, app.PackageId.ToString( "B" ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Value copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Copies the value click.
		/// </summary>
		/// <param name="app">The application.</param>
		private void CopyValueClick( TenantApp app )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, app.ApplicationId.ToString( "B" ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Value copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Filters the Publisher click.
		/// </summary>
		private void FilterPublisherClick( )
		{
			PublisherFilterOpen = true;
		}

		/// <summary>
		///     Filters the PublisherUrl click.
		/// </summary>
		private void FilterPublisherUrlClick( )
		{
			PublisherUrlFilterOpen = true;
		}

        /// <summary>
        /// Filters the protected click.
        /// </summary>
        private void FilterProtectedClick()
	    {
	        ProtectedFilterOpen = true;
	    }

        /// <summary>
        ///     Filters the solution click.
        /// </summary>
        private void FilterSolutionClick( )
		{
			SolutionFilterOpen = true;
		}

		/// <summary>
		///     Filters the tenant click.
		/// </summary>
		private void FilterTenantClick( )
		{
			TenantFilterOpen = true;
		}

		/// <summary>
		///     Filters the update.
		/// </summary>
		private void FilterUpdate( )
		{
			List<TenantApp> filteredApps = new List<TenantApp>( );

			foreach ( var app in TenantApps )
			{
				FilterObject tenantFilterObject = TenantFilters.FirstOrDefault( f => f.Value.ToString( ) == app.TenantName );
				FilterObject solutionFilterObject = SolutionFilters.FirstOrDefault( f => f.Value.ToString( ) == app.Solution );
				FilterObject versionFilterObject = VersionFilters.FirstOrDefault( f => f.Value.ToString( ) == app.SolutionVersion );
				FilterObject publisherFilterObject = PublisherFilters.FirstOrDefault( f => f.Value.ToString( ) == app.Publisher );
				FilterObject publisherUrlFilterObject = PublisherUrlFilters.FirstOrDefault( f => f.Value.ToString( ) == app.PublisherUrl );
                FilterObject protectedFilterObject = ProtectedFilters.FirstOrDefault(f => (bool) f.Value == app.IsProtected);

                if ((tenantFilterObject == null || tenantFilterObject.IsFiltered) && (solutionFilterObject == null || solutionFilterObject.IsFiltered) && (versionFilterObject == null || versionFilterObject.IsFiltered) && (publisherFilterObject == null || publisherFilterObject.IsFiltered) && (publisherUrlFilterObject == null || publisherUrlFilterObject.IsFiltered) && (protectedFilterObject == null || protectedFilterObject.IsFiltered) )
				{
					filteredApps.Add( app );
				}
			}

			FilteredTenantApps = filteredApps;
		}

		/// <summary>
		///     Filters the Version click.
		/// </summary>
		private void FilterVersionClick( )
		{
			VersionFilterOpen = true;
		}

		/// <summary>
		///     Loads the applications.
		/// </summary>
		private void LoadApplications( )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - LoadApplications
SET NOCOUNT ON

DECLARE @tenantId BIGINT
DECLARE @applicationGuid UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'

DECLARE cur CURSOR FORWARD_ONLY FOR
SELECT 0
UNION
SELECT Id FROM _vTenant

OPEN cur

FETCH NEXT FROM cur
INTO @tenantId

WHILE @@FETCH_STATUS = 0
BEGIN

	DECLARE @tenantName NVARCHAR(MAX) = ISNULL( dbo.fnNameAlias( @tenantId, 0 ), 'Global' )
	DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )
	DECLARE @appVersionString BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
	DECLARE @appVerId BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )
	DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
	DECLARE @solutionVersionString BIGINT = dbo.fnAliasNsId( 'solutionVersionString', 'core', @tenantId )
	DECLARE @name_tenantSpecific BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
	DECLARE @packageId BIGINT = dbo.fnAliasNsId( 'packageId', 'core', @tenantId )
	DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
	DECLARE @solution BIGINT = dbo.fnAliasNsId( 'solution', 'core', @tenantId )
    DECLARE @canModifyApplication BIGINT = dbo.fnAliasNsId( 'canModifyApplication', 'core', @tenantId )

	SELECT
		TenantId = @tenantId,
		Tenant = @tenantName,
		Solution = solName.Data,
		SolutionEntityId = s.FromId,
		SolutionVersion = solVer.Data,
		PackageId = pkgId.Data,
		PackageEntityId = p.EntityId,
		Version = pkgVer.Data,
		ApplicationEntityId = pkgApp.ToId,
		ApplicationId = aid.UpgradeId,
		Publisher = p1.Data,
		PublisherUrl = u.Data,
		ReleaseDate = c.Data,
        CanModifyApplication = ISNULL(protected.Data, 0)
	FROM
		Relationship s   -- isOfType solution
	JOIN
		Data_NVarChar solVer ON
			solVer.TenantId = s.TenantId
			AND solVer.EntityId = s.FromId
			AND solVer.FieldId = @solutionVersionString
	JOIN
		Data_NVarChar solName ON
			solName.TenantId = s.TenantId
			AND solName.EntityId = s.FromId
			AND solName.FieldId = @name_tenantSpecific
	JOIN
		Data_Guid pkgId ON
			pkgId.TenantId = s.TenantId
			AND pkgId.EntityId = s.FromId
			AND pkgId.FieldId = @packageId
	LEFT JOIN
		Data_Guid p ON
			p.TenantId = 0
			AND p.Data = pkgId.Data
			AND p.FieldId = @appVerId
	LEFT JOIN
		Relationship pkgApp ON
			pkgApp.TenantId = 0
			AND pkgApp.FromId = p.EntityId
			AND pkgApp.TypeId = @packageForApplication
	LEFT JOIN
		Data_NVarChar pkgVer ON
			pkgVer.TenantId = 0
			AND pkgVer.EntityId = p.EntityId
			AND pkgVer.FieldId = @appVersionString
    LEFT JOIN
		Data_Bit protected ON
			protected.TenantId = s.TenantId
			AND protected.EntityId = s.FromId
			AND protected.FieldId = @canModifyApplication
	LEFT JOIN
	(
		SELECT
			p.EntityId,
			p.Data
		FROM
			Data_NVarChar p
		JOIN
			Data_Alias solPub ON
				solPub.TenantId = p.TenantId
				AND solPub.EntityId = p.FieldId
				AND solPub.Data = 'solutionPublisher'
				AND solPub.Data = 'solutionPublisher'
				AND solPub.Namespace = 'core'
		WHERE
			p.TenantId = @tenantId
	) p1 ON
		p1.EntityId = s.FromId
	LEFT JOIN
	(
		SELECT
			u.EntityId,
			u.Data
		FROM
			Data_NVarChar u
		JOIN
			Data_Alias solUrl ON
				solUrl.TenantId = u.TenantId
				AND solUrl.EntityId = u.FieldId
				AND solUrl.Data = 'solutionPublisherUrl'
				AND solUrl.Namespace = 'core'
		WHERE
			u.TenantId = @tenantId
	) u ON
		u.EntityId = s.FromId
	LEFT JOIN
	(
		SELECT
			c.EntityId,
			c.Data
		FROM
			Data_DateTime c
		JOIN
			Data_Alias solRelDate ON
				solRelDate.TenantId = c.TenantId
				AND solRelDate.EntityId = c.FieldId
				AND solRelDate.Data = 'solutionReleaseDate'
				AND solRelDate.Namespace = 'core'
		WHERE
			c.TenantId = @tenantId
	) c ON
		c.EntityId = s.FromId
	JOIN
		Entity aid ON
			aid.TenantId = s.TenantId
			AND aid.Id = s.FromId
	WHERE
		s.TenantId = @tenantId
		AND s.TypeId = @isOfType
		AND s.ToId = @solution
		AND
		(
			p.EntityId = pkgApp.FromId
			OR
			p.EntityId IS NULL
		)
		AND
		(
			@applicationGuid = CONVERT(UNIQUEIDENTIFIER, '00000000-0000-0000-0000-000000000000')
			OR
			@applicationGuid = aid.UpgradeId
		)
	ORDER BY
		solName.Data,
		pkgVer.Data

	FETCH NEXT FROM cur 
	INTO @tenantId
END 
CLOSE cur;
DEALLOCATE cur;";

			try
			{
				using ( IDbCommand command = databaseManager.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						var tenantApps = new List<TenantApp>( );

						HashSet<string> tenants = new HashSet<string>( );
						HashSet<string> solutions = new HashSet<string>( );
						HashSet<string> versions = new HashSet<string>( );
						HashSet<string> publishers = new HashSet<string>( );
						HashSet<string> publisherUrls = new HashSet<string>( );
                        HashSet<bool> isProtectedValues = new HashSet<bool>();

                        do
						{
							do
							{
								while ( reader.Read( ) )
								{
									var tenantId = reader.GetInt64( 0 );
									var tenantName = reader.GetString( 1 );
									var solution = reader.GetString( 2, "Unnamed" );
									var solutionEntityId = reader.GetInt64( 3, -1 );
									var solutionVersion = reader.GetString( 4, "" );
									var packageId = reader.GetGuid( 5 );
									var packageEntityId = reader.GetInt64( 6, -1 );
									var packageVersion = reader.GetString( 7, "" );
									var applicationEntityId = reader.GetInt64( 8, -1 );
									var applicationId = reader.GetGuid( 9 );
									var publisher = reader.GetString( 10, "" );
									var publisherUrl = reader.GetString( 11, "" );
									var releaseDate = reader.GetDateTime( 12, DateTime.MinValue );
								    var canModifyApplication = reader.GetBoolean(13);

                                    var tenantApp = new TenantApp(tenantId, tenantName, solution, solutionEntityId, solutionVersion, packageId, packageEntityId, packageVersion, applicationEntityId, applicationId, publisher, publisherUrl, releaseDate, !canModifyApplication);

									tenants.Add( tenantName );
									solutions.Add( solution );
									versions.Add( solutionVersion );
									publishers.Add( publisher );
									publisherUrls.Add( publisherUrl );
								    isProtectedValues.Add(canModifyApplication);

                                    tenantApps.Add( tenantApp );
								}
							}
							while ( reader.NextResult( ) );
						}
						while ( reader.NextResult( ) );

						TenantApps = tenantApps.OrderBy( x => x.TenantId ).ThenBy( x => x.Solution ).ToList( );

						TenantFilters = new List<FilterObject>( );

						foreach ( string ten in tenants.OrderBy( k => k ) )
						{
							if ( !string.IsNullOrEmpty( ten ) )
							{
								TenantFilters.Add( new FilterObject( ten, true, TenantFilterUpdate ) );
							}
						}

						OnPropertyChanged( "TenantFilters" );

						SolutionFilters = new List<FilterObject>( );

						foreach ( string sol in solutions.OrderBy( k => k ) )
						{
							if ( !string.IsNullOrEmpty( sol ) )
							{
								SolutionFilters.Add( new FilterObject( sol, true, SolutionFilterUpdate ) );
							}
						}

						OnPropertyChanged( "SolutionFilters" );

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

                        ProtectedFilters = new List<FilterObject>();

                        foreach (bool value in isProtectedValues.OrderBy(k => k))
                        {
                            ProtectedFilters.Add(new FilterObject(value, true, ProtectedFilterUpdate));
                        }                        

					    OnPropertyChanged( "ProtectedFilters" );

                        FilteredTenantApps = new List<TenantApp>( TenantApps );
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		///     Publishers the filter update.
		/// </summary>
		private void PublisherFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "PublisherFilters" );
		}

		/// <summary>
		///     PublisherUrls the filter update.
		/// </summary>
		private void PublisherUrlFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "PublisherUrlFilters" );
		}

        /// <summary>
		///     Protected filter update.
		/// </summary>
		private void ProtectedFilterUpdate()
        {
            FilterUpdate();

            OnPropertyChanged("ProtectedFilters");
        }

        /// <summary>
        ///     Refreshes this instance.
        /// </summary>
        private void Refresh( )
		{
			LoadApplications( );
		}

		/// <summary>
		///     Solutions the filter update.
		/// </summary>
		private void SolutionFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "SolutionFilters" );
		}

		/// <summary>
		///     Tenants the filter update.
		/// </summary>
		private void TenantFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "TenantFilters" );
		}

		/// <summary>
		///     Versions the filter update.
		/// </summary>
		private void VersionFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "VersionFilters" );
		}
	}
}