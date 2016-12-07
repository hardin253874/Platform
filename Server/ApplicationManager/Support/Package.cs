// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Data;
using ApplicationManager.Core;
using EDC.Database;
using EDC.ReadiNow.Database;

namespace ApplicationManager.Support
{
	/// <summary>
	///     Package.
	/// </summary>
	public class Package : ViewModelBase
	{
		/// <summary>
		///     Selected tenant.
		/// </summary>
		private Tenant _selectedTenant;

		/// <summary>
		///     Tenant collection.
		/// </summary>
		private ObservableCollection<Tenant> _tenantCollection;

		/// <summary>
		///     Tenants
		/// </summary>
		private CollectionViewSource _tenants;

		/// <summary>
		///     Gets the app version id.
		/// </summary>
		/// <value>
		///     The app version id.
		/// </value>
		public Guid AppVerId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the id.
		/// </summary>
		/// <value>
		///     The id.
		/// </value>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the release date.
		/// </summary>
		/// <value>
		///     The release date.
		/// </value>
		public DateTime ReleaseDate
		{
			get;
			set;
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
					SetProperty( ref _selectedTenant, value );
				}
			}
		}

		/// <summary>
		///     Gets the tenant collection.
		/// </summary>
		/// <value>
		///     The tenant collection.
		/// </value>
		public ObservableCollection<Tenant> TenantCollection
		{
			get
			{
				if ( _tenantCollection == null )
				{
					_tenantCollection = new ObservableCollection<Tenant>( );

					LoadTenants( );
				}

				return _tenantCollection;
			}
		}

		/// <summary>
		///     Gets the tenants.
		/// </summary>
		/// <value>
		///     The tenants.
		/// </value>
		public CollectionViewSource Tenants => _tenants ?? ( _tenants = new CollectionViewSource
		                                       {
			                                       Source = TenantCollection
		                                       } );

		/// <summary>
		///     Gets or sets the version.
		/// </summary>
		/// <value>
		///     The version.
		/// </value>
		public string Version
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the package.
		/// </summary>
		/// <param name="appVerId">The app version id.</param>
		/// <returns></returns>
		public static Package GetPackage( Guid appVerId )
		{
			var databaseInfo = new SqlDatabaseInfo( Config.ServerName, Config.DatabaseName, DatabaseAuthentication.Integrated, null, 60, 30, 300 );

			/////
			// Set the base properties
			/////

			using ( DatabaseContext ctx = DatabaseContext.GetContext( databaseInfo: databaseInfo ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( ) )
				{
					command.CommandText = @"
SELECT
	PackageId = g.EntityId,
	PackageName = n.Data,
	PackageDescritpion = d.Data,
	PackageVersion = v.Data,
	AppVerId = @appVerId,
	PublishDate = dt.Data
FROM
	Data_Guid g
JOIN
	Data_NVarChar n ON
		g.TenantId = n.TenantId AND
		g.EntityId = n.EntityId
JOIN
	Data_Alias a ON
		a.TenantId = n.TenantId AND
		n.FieldId = a.EntityId AND
		a.Data = 'name' AND
		a.Namespace = 'core'
JOIN
	Data_NVarChar d ON
		d.TenantId = g.TenantId AND
		g.EntityId = d.EntityId
JOIN
	Data_Alias ad ON
		ad.TenantId = d.TenantId AND
		d.FieldId = ad.EntityId AND
		ad.Data = 'description' AND
		ad.Namespace = 'core'
JOIN
	Data_NVarChar v ON
		v.TenantId = g.TenantId AND
		g.EntityId = v.EntityId
JOIN
	Data_Alias av ON
		av.TenantId = v.TenantId AND
		v.FieldId = av.EntityId AND
		av.Data = 'appVersionString' AND
		av.Namespace = 'core'
LEFT JOIN
(
	SELECT
		dt.EntityId,
		dt.TenantId,
		dt.Data
	FROM
		Data_DateTime dt
	JOIN
		Data_Alias adt ON
			dt.FieldId = adt.EntityId AND
			adt.Data = 'publishDate' AND
			adt.Namespace = 'core'
	WHERE
		dt.TenantId = 0
) dt ON
	dt.TenantId = g.TenantId AND
	g.EntityId = dt.EntityId
WHERE
	g.Data = @appVerId AND
	g.TenantId = 0";

					ctx.AddParameter( command, "@appVerId", DbType.Guid, appVerId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						if ( reader.Read( ) )
						{
							return new Package
							{
								Id = reader.GetInt64( 0 ),
								Name = reader.GetString( 1 ),
								Description = reader.GetString( 2, null ),
								Version = reader.GetString( 3, null ),
								AppVerId = reader.GetGuid( 4 ),
								ReleaseDate = reader.GetDateTime( 5, DateTime.MinValue )
							};
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		///     Gets the applications.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Package> GetPackages( Guid appId )
		{
			var packages = new List<Package>( );

			var databaseInfo = new SqlDatabaseInfo( Config.ServerName, Config.DatabaseName, DatabaseAuthentication.Integrated, null, 60, 30, 300 );

			/////
			// Set the base properties
			/////

			using ( DatabaseContext ctx = DatabaseContext.GetContext( databaseInfo: databaseInfo ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( ) )
				{
					command.CommandText = @"
SELECT
	PackageId = ar.FromId,
	PackageName = n.Data,
	PackageDescription = d.Data,
	PackageVersion = v.Data,
	AppVerId = g.Data,
	PublishDate = dt.Data
FROM
	dbgData_Guid eg
JOIN
	Relationship ar ON
		eg.EntityId = ar.ToId AND
		eg.Data = @appId AND
		eg.TenantId = 0
JOIN
	Data_Alias aeg ON
		eg.TenantId = eg.TenantId AND
		eg.FieldId = aeg.EntityId AND
		aeg.Data = 'applicationId' AND
		aeg.Namespace = 'core'
JOIN
	Data_Alias a ON
		a.TenantId = ar.TenantId AND
		ar.TypeId = a.EntityId AND
		a.Data = 'packageForApplication' AND
		a.Namespace = 'core'
JOIN
	Data_Guid g ON
		g.TenantId = ar.TenantId AND
		ar.FromId = g.EntityId
JOIN
	Data_Alias ga ON
		ga.TenantId = g.TenantId AND
		g.FieldId = ga.EntityId AND
		ga.Data = 'appVerId' AND
		ga.Namespace = 'core'
LEFT JOIN
(
	SELECT
		n.EntityId,
		n.TenantId,
		n.Data
	FROM
		Data_NVarChar n
	JOIN
		Data_Alias a ON
			a.TenantId = n.TenantId AND
			n.FieldId = a.EntityId AND
			a.Data = 'name' AND
			a.Namespace = 'core'
	WHERE
		n.TenantId = 0
) n ON
	n.TenantId = ar.TenantId AND
	ar.FromId = n.EntityId
LEFT JOIN
(
	SELECT
		d.EntityId,
		d.TenantId,
		d.Data
	FROM
		Data_NVarChar d
	JOIN
		Data_Alias a ON
			d.TenantId = a.TenantId AND
			d.FieldId = a.EntityId AND
			a.Data = 'description' AND
			a.Namespace = 'core'
	WHERE
		d.TenantId = 0
) d ON
	d.TenantId = ar.TenantId AND
	ar.FromId = d.EntityId
LEFT JOIN
(
	SELECT
		v.EntityId,
		v.TenantId,
		v.Data
	FROM
		Data_NVarChar v
	JOIN
		Data_Alias a ON
			v.TenantId = a.TenantId AND
			v.FieldId = a.EntityId AND
			a.Data = 'appVersionString' AND
			a.Namespace = 'core'
	WHERE
		v.TenantId = 0
) v ON
	v.TenantId = ar.TenantId AND
	ar.FromId = v.EntityId
LEFT JOIN
(
	SELECT
		dt.EntityId,
		dt.TenantId,
		dt.Data
	FROM
		Data_DateTime dt
	JOIN
		Data_Alias adt ON
			dt.TenantId = adt.TenantId AND
			dt.FieldId = adt.EntityId AND
			adt.Data = 'publishDate' AND
			adt.Namespace = 'core'
	WHERE
		dt.TenantId = 0
) dt ON
	dt.TenantId = ar.TenantId AND
	ar.FromId = dt.EntityId
WHERE
	eg.TenantId = 0
ORDER BY
	v.Data";

					ctx.AddParameter( command, "@appId", DbType.Guid, appId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							var package = new Package
							{
								Id = reader.GetInt64( 0 ),
								Name = reader.GetString( 1 ),
								Description = reader.GetString( 2, null ),
								Version = reader.GetString( 3, null ),
								AppVerId = reader.GetGuid( 4 ),
								ReleaseDate = reader.GetDateTime( 5, DateTime.MinValue )
							};

							packages.Add( package );
						}
					}
				}
			}

			return packages;
		}

		/// <summary>
		///     Loads the tenants.
		/// </summary>
		private void LoadTenants( )
		{
			IEnumerable<Tenant> tenants = Tenant.GetTenants( );

			foreach ( Tenant tenant in tenants )
			{
				_tenantCollection.Add( tenant );
			}
		}
	}
}