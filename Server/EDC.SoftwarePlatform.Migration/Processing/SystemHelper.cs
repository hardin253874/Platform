// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     System helper.
	/// </summary>
	public static class SystemHelper
	{
        /// <summary>
        ///     Looks up an application in the global tenant by name or guid.
        /// </summary>
        /// <param name="applicationNameOrGuid"></param>
        /// <returns>Entity ID of the app resource.</returns>
        public static long GetGlobalApplicationIdByNameOrGuid( string applicationNameOrGuid )
        {
            Guid appGuid;

	        var result = Guid.TryParse( applicationNameOrGuid, out appGuid ) ? GetResourceIdByGuid( "app", 0, appGuid ) : GetResourceIdByName<App>( applicationNameOrGuid );

            return result;
        }

		/// <summary>
		///     Look up a application by name.
		/// </summary>
		/// <param name="applicationName">Name of the application</param>
		/// <returns>
		///     The application ID.
		/// </returns>
		/// <exception cref="System.Exception">Multiple application found with name  + applicationName</exception>
		public static long GetApplicationIdByName( string applicationName )
		{
            return GetResourceIdByName<Solution>( applicationName );
		}

		/// <summary>
		///     Gets the application identifier by its upgrade id.
		/// </summary>
		/// <param name="applicationGuid">The application upgrade id.</param>
		/// <returns></returns>
		public static long GetApplicationIdByGuid( Guid applicationGuid )
		{
            return GetResourceIdByGuid( "solution", RequestContext.TenantId, applicationGuid );
        }

        /// <summary>
        ///     Look up a application by name.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <returns>
        ///     The application ID.
        /// </returns>
        /// <exception cref="System.Exception">Multiple entries found with name  + applicationName</exception>
        public static long GetResourceIdByName<T>( string resourceName ) where T : class, IEntity
        {
            IEnumerable<T> matches = Entity.GetByName<T>( resourceName );

            IList<T> enumerable = matches as IList<T> ?? matches.ToList( );

			if ( enumerable.Count <= 0 )
            {
                return -1;
            }

            if ( enumerable.Count > 1 )
            {
                throw new Exception( "Multiple entries found with name: " + resourceName );
            }

            return enumerable.First( ).Id;
        }

        /// <summary>
        ///     Gets a resource by its guid.
        /// </summary>
        private static long GetResourceIdByGuid( string typeAlias, long tenantId, Guid upgradeId )
        {
			long applicationId = -1;

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				const string sql = @"
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @solution BIGINT = dbo.fnAliasNsId( 'solution', 'core', @tenantId )

SELECT
	e.Id
FROM
	Entity e
INNER JOIN
	Relationship r ON
		e.Id = r.FromId AND
		r.TenantId = e.TenantId AND
		r.TypeId = @isOfType AND
		r.ToId = @solution
WHERE
	e.UpgradeId = @upgradeId AND
	e.TenantId = @tenantId";

				using ( IDbCommand command = ctx.CreateCommand( sql ) )
				{
                    ctx.AddParameter( command, "@typeAlias", DbType.String, typeAlias );
                    ctx.AddParameter( command, "@upgradeId", DbType.Guid, upgradeId );
					ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );

					object applicationIdObject = command.ExecuteScalar( );

					if ( applicationIdObject != null && applicationIdObject != DBNull.Value )
					{
						applicationId = ( long ) applicationIdObject;
					}
				}
			}

			return applicationId;
		}

		/// <summary>
		///     Gets the application by unique identifier.
		/// </summary>
		/// <param name="applicationGuid">The application unique identifier.</param>
		/// <returns></returns>
		public static Solution GetApplicationByGuid( Guid applicationGuid )
		{
			long applicationId = GetApplicationIdByGuid( applicationGuid );

			if ( applicationId != -1 )
			{
				return Entity.Get<Solution>( applicationId, true, Solution.PackageId_Field, Solution.SolutionVersionString_Field, Solution.Name_Field, Solution.Description_Field, Solution.SolutionPublisher_Field, Solution.SolutionPublisherUrl_Field, Solution.SolutionReleaseDate_Field, Solution.SolutionVersionString_Field );
			}

			return null;
		}

		/// <summary>
		///     Gets the latest package identifier using the application name.
		/// </summary>
		/// <param name="applicationName">Name of the application.</param>
		/// <returns>
		///     Identifier of the latest package.
		/// </returns>
		public static long GetLatestPackageIdByName( string applicationName )
		{
			long packageId = -1;

			using ( new GlobalAdministratorContext( ) )
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					const string sql = @"
					DECLARE @name					BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )
					DECLARE @appVersionString		BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
					DECLARE @app					BIGINT = dbo.fnAliasNsId( 'app', 'core', DEFAULT )
					DECLARE @isOfType				BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', DEFAULT )
					DECLARE @packageForApplication	BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )

					SELECT
						TOP 1 ApplicationPackageId = ap.FromId
					FROM
						Data_NVarChar an
					INNER JOIN
						Relationship ar ON an.EntityId = ar.FromId AND ar.TenantId = 0 AND TypeId = @isOfType AND ToId = @app AND an.Data = @appName
					INNER JOIN
						Relationship ap ON ap.TenantId = 0 AND ar.FromId = ap.ToId AND ap.TypeId = @packageForApplication
					INNER JOIN
						Data_NVarChar pv ON ap.FromId = pv.EntityId AND pv.TenantId = 0 AND pv.FieldId = @appVersionString
					WHERE
						an.TenantId = 0
						AND an.FieldId = @name
					ORDER BY
						CAST( '/' + REPLACE( dbo.fnSanitiseVersion( pv.Data ), '.', '/' ) + '/' AS HIERARCHYID ) DESC";

					using ( IDbCommand command = ctx.CreateCommand( sql ) )
					{
						ctx.AddParameter( command, "@appName", DbType.String, applicationName );

						object packageIdObject = command.ExecuteScalar( );

						if ( packageIdObject != null && packageIdObject != DBNull.Value )
						{
							packageId = ( long ) packageIdObject;
						}
					}
				}
			}

			return packageId;
		}

		/// <summary>
		///     Look up a application by name within a specific tenant
		/// </summary>
		/// <param name="applicationName">Name of the application</param>
		/// <returns>
		///     The application ID.
		/// </returns>
		public static AppPackage GetLatestPackageByName( string applicationName )
		{
			long packageId = GetLatestPackageIdByName( applicationName );

			if ( packageId != -1 )
			{
				/////
				// Get the application package.
				/////
				return Entity.Get<AppPackage>( packageId );
			}

			return null;
		}

		/// <summary>
		///     Gets the latest package identifier by application upgrade id.
		/// </summary>
		/// <param name="applicationGuid">The application unique identifier.</param>
		/// <returns></returns>
		public static long GetLatestPackageIdByGuid( Guid applicationGuid )
		{
			long packageId = -1;

			using ( new GlobalAdministratorContext( ) )
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					const string sql = @"
DECLARE @applicationId BIGINT = dbo.fnAliasNsId('applicationId', 'core', DEFAULT)
DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId('packageForApplication', 'core', DEFAULT)
DECLARE @appVersionString		BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )

SELECT
	TOP 1 r.FromId
FROM
	Data_Guid g
INNER JOIN
	Relationship r ON
		g.EntityId = r.ToId AND
		r.TenantId = 0 AND
		r.TypeId = @packageForApplication
INNER JOIN
	Data_NVarChar v ON
		r.FromId = v.EntityId AND
		v.TenantId = 0 AND
		v.FieldId = @appVersionString
WHERE
	g.FieldId = @applicationId AND
	g.Data = @appGuid AND
	g.TenantId = 0
ORDER BY
	CAST( '/' + REPLACE( dbo.fnSanitiseVersion( v.Data ), '.', '/' ) + '/' AS HIERARCHYID ) DESC";

					using ( IDbCommand command = ctx.CreateCommand( sql ) )
					{
						ctx.AddParameter( command, "@appGuid", DbType.Guid, applicationGuid );

						object packageIdObject = command.ExecuteScalar( );

						if ( packageIdObject != null && packageIdObject != DBNull.Value )
						{
							packageId = ( long ) packageIdObject;
						}
					}
				}
			}

			return packageId;
		}

		/// <summary>
		/// Gets the package identifier by unique identifier.
		/// </summary>
		/// <param name="applicationGuid">The application unique identifier.</param>
		/// <param name="minimumVersion">The minimum version.</param>
		/// <param name="maximumVersion">The maximum version.</param>
		/// <returns></returns>
		public static long GetPackageIdByGuid( Guid applicationGuid, Version minimumVersion, Version maximumVersion )
		{
			long selectedPackageId = -1;

			using ( new GlobalAdministratorContext( ) )
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					const string sql = @"
DECLARE @applicationId BIGINT = dbo.fnAliasNsId('applicationId', 'core', DEFAULT)
DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId('packageForApplication', 'core', DEFAULT)
DECLARE @appVersionString		BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )

SELECT
	r.FromId,
	v.Data
FROM
	Data_Guid g
INNER JOIN
	Relationship r ON
		g.EntityId = r.ToId AND
		r.TenantId = 0 AND
		r.TypeId = @packageForApplication
INNER JOIN
	Data_NVarChar v ON
		r.FromId = v.EntityId AND
		v.TenantId = 0 AND
		v.FieldId = @appVersionString
WHERE
	g.FieldId = @applicationId AND
	g.Data = @appGuid AND
	g.TenantId = 0";

					using ( IDbCommand command = ctx.CreateCommand( sql ) )
					{
						ctx.AddParameter( command, "@appGuid", DbType.Guid, applicationGuid );

						using ( IDataReader reader = command.ExecuteReader( ) )
						{
							Version selectedPackageVersion = new Version( 0, 0, 0, 0);

							while ( reader.Read( ) )
							{
								long packageId = reader.GetInt64( 0 );
								string versionString = reader.GetString( 1, null );

								if ( !string.IsNullOrEmpty( versionString ) )
								{
									Version version;

									if ( Version.TryParse( versionString, out version ) )
									{
										if ( (minimumVersion != null && version < minimumVersion ) || ( maximumVersion != null && version > maximumVersion ) )
										{
											continue;
										}

										if ( version > selectedPackageVersion )
										{
											selectedPackageId = packageId;
											selectedPackageVersion = version;
										}
									}
								}
							}
						}
					}
				}
			}

			return selectedPackageId;
		}

		/// <summary>
		///     Gets the latest package by unique identifier.
		/// </summary>
		/// <param name="applicationGuid">The application unique identifier.</param>
		/// <returns></returns>
		public static AppPackage GetLatestPackageByGuid( Guid applicationGuid )
		{
			long packageId = GetLatestPackageIdByGuid( applicationGuid );

			if ( packageId != -1 )
			{
				/////
				// Get the application package.
				/////
				return Entity.Get<AppPackage>( packageId );
			}

			return null;
		}

		/// <summary>
		///     Gets the package identifier by name and version.
		/// </summary>
		/// <param name="applicationName">Name of the application.</param>
		/// <param name="version">The version.</param>
		/// <returns>
		///     The package identifier.
		/// </returns>
		public static long GetPackageIdByNameAndVersion( string applicationName, string version )
		{
			long packageId = -1;

			using ( new GlobalAdministratorContext( ) )
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					const string sql = @"
DECLARE @appVersionString		BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
DECLARE @app					BIGINT = dbo.fnAliasNsId( 'app', 'core', DEFAULT )
DECLARE @isOfType				BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', DEFAULT )
DECLARE @packageForApplication	BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
DECLARE @name					BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )

SELECT
	TOP 1 ApplicationPackageId = ap.FromId
FROM
	Relationship r
INNER JOIN
	Data_NVarChar an ON an.TenantId = 0 AND r.FromId = an.EntityId AND an.FieldId = @name
INNER JOIN
	Relationship ap ON ap.TenantId = 0 AND r.FromId = ap.ToId AND ap.TypeId = @packageForApplication
INNER JOIN
	Data_NVarChar pv ON pv.TenantId = 0 AND ap.FromId = pv.EntityId AND pv.FieldId = @appVersionString
WHERE
	r.TenantId = 0
	AND
		r.TypeId = @isOfType
	AND
		r.ToId = @app
	AND
		pv.Data LIKE @appVersion + '%'
	AND
		an.Data = @appName
ORDER BY
	CAST( '/' + REPLACE( dbo.fnSanitiseVersion( pv.Data ), '.', '/' ) + '/' AS HIERARCHYID ) DESC";

					using ( IDbCommand command = ctx.CreateCommand( sql ) )
					{
						ctx.AddParameter( command, "@appName", DbType.String, applicationName );
						ctx.AddParameter( command, "@appVersion", DbType.String, version );

						object packageIdObject = command.ExecuteScalar( );

						if ( packageIdObject != null && packageIdObject != DBNull.Value )
						{
							packageId = ( long ) packageIdObject;
						}
					}
				}
			}

			return packageId;
		}

		/// <summary>
		///     Gets the package identifier by unique identifier and version.
		/// </summary>
		/// <param name="applicationGuid">The application unique identifier.</param>
		/// <param name="version">The version.</param>
		/// <returns>
		///     The package identifier.
		/// </returns>
		public static long GetPackageIdByGuidAndVersion( Guid applicationGuid, string version )
		{
			long packageId = -1;

			using ( new GlobalAdministratorContext( ) )
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					const string sql = @"
DECLARE @appVersionString		BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
DECLARE @packageForApplication	BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
DECLARE @applicationId			BIGINT = dbo.fnAliasNsId( 'applicationId', 'core', DEFAULT )

SELECT
	TOP 1 ApplicationPackageId = r.FromId
FROM
	Data_Guid g
INNER JOIN
	Relationship r ON
		g.EntityId = r.ToId AND
		r.TenantId = 0 AND
		r.TypeId = @packageForApplication
INNER JOIN
	Data_NVarChar v ON v.TenantId = 0 AND r.FromId = v.EntityId AND v.FieldId = @appVersionString
WHERE
	g.Data = @appGuid AND
	g.TenantId = 0 AND
	g.FieldId = @applicationId AND
	v.Data LIKE @appVersion + '%'
ORDER BY
	CAST( '/' + REPLACE( dbo.fnSanitiseVersion( v.Data ), '.', '/' ) + '/' AS HIERARCHYID ) DESC";

					using ( IDbCommand command = ctx.CreateCommand( sql ) )
					{
						ctx.AddParameter( command, "@appGuid", DbType.Guid, applicationGuid );
						ctx.AddParameter( command, "@appVersion", DbType.String, version );

						object packageIdObject = command.ExecuteScalar( );

						if ( packageIdObject != null && packageIdObject != DBNull.Value )
						{
							packageId = ( long ) packageIdObject;
						}
					}
				}
			}

			return packageId;
		}

		/// <summary>
		///     Gets the package by name and version.
		/// </summary>
		/// <param name="applicationName">Name of the application.</param>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		public static AppPackage GetPackageByNameAndVersion( string applicationName, string version )
		{
			long packageId = GetPackageIdByNameAndVersion( applicationName, version );

			if ( packageId != -1 )
			{
				/////
				// Get the application package.
				/////
				return Entity.Get<AppPackage>( packageId );
			}

			return null;
		}

		/// <summary>
		///     Gets the package by unique identifier and version.
		/// </summary>
		/// <param name="applicationGuid">The application unique identifier.</param>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		public static AppPackage GetPackageByGuidAndVersion( Guid applicationGuid, string version )
		{
			long packageId = GetPackageIdByGuidAndVersion( applicationGuid, version );

			if ( packageId != -1 )
			{
				/////
				// Get the application package.
				/////
				return Entity.Get<AppPackage>( packageId );
			}

			return null;
		}

		/// <summary>
		///     Gets the package identifier by ver identifier.
		/// </summary>
		/// <param name="appVerId">The application ver identifier.</param>
		/// <returns>
		///     The package identifier.
		/// </returns>
		public static long GetPackageIdByVerId( Guid appVerId )
		{
			long packageId = -1;

			using ( new GlobalAdministratorContext( ) )
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					const string sql = @"
DECLARE @appVerIdField BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', 0 )

SELECT
	EntityId
FROM
	Data_Guid
WHERE
	TenantId = 0
	AND FieldId = @appVerIdField
	AND Data = @appVerId";

					using ( IDbCommand command = ctx.CreateCommand( sql ) )
					{
						ctx.AddParameter( command, "@appVerId", DbType.Guid, appVerId );

						object packageIdObject = command.ExecuteScalar( );

						if ( packageIdObject != null && packageIdObject != DBNull.Value )
						{
							packageId = ( long ) packageIdObject;
						}
					}
				}
			}

			return packageId;
		}

		/// <summary>
		///     Gets the package by version id.
		/// </summary>
		/// <param name="appVerId">The app version id.</param>
		/// <returns></returns>
		public static AppPackage GetPackageByVerId( Guid appVerId )
		{
			long packageId = GetPackageIdByVerId( appVerId );

			if ( packageId != -1 )
			{
				/////
				// Get the application package.
				/////
				return Entity.Get<AppPackage>( packageId );
			}

			return null;
		}

		/// <summary>
		///     Look up a application id by name within a specific tenant
		/// </summary>
		/// <param name="tenantId">The ID of the tenant to find the application within.</param>
		/// <param name="applicationName">Name of the application</param>
		/// <returns>
		///     The application ID.
		/// </returns>
		public static long GetTenantApplicationIdByName( long tenantId, string applicationName )
		{
			using ( new TenantAdministratorContext( tenantId ) )
			{
				return GetApplicationIdByName( applicationName );
			}
		}

		/// <summary>
		///     Gets the tenant application id by guid.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="applicationGuid">The application identifier.</param>
		/// <returns></returns>
		public static long GetTenantApplicationIdByGuid( long tenantId, Guid applicationGuid )
		{
			using ( new TenantAdministratorContext( tenantId ) )
			{
				return GetApplicationIdByGuid( applicationGuid );
			}
		}

		/// <summary>
		///     Gets the tenant application by unique identifier.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="applicationGuid">The application unique identifier.</param>
		/// <returns></returns>
		public static Solution GetTenantApplicationByGuid( long tenantId, Guid applicationGuid )
		{
			using ( new TenantAdministratorContext( tenantId ) )
			{
				return GetApplicationByGuid( applicationGuid );
			}
		}

		/// <summary>
		///     Gets the tenant current package identifier by name.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="applicationName">Name of the application.</param>
		/// <returns>The tenants current package identifier.</returns>
		public static long GetTenantCurrentPackageIdByName( long tenantId, string applicationName )
		{
			long packageId = -1;

			using ( new GlobalAdministratorContext( ) )
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					const string sql = @"
DECLARE @app					BIGINT = dbo.fnAliasNsId( 'app', 'core', DEFAULT )
DECLARE @isOfType				BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', DEFAULT )
DECLARE @packageForApplication	BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
DECLARE @appVerId				BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )
DECLARE @packageId				BIGINT = dbo.fnAliasNsId( 'packageId', 'core', @tenantId )

SELECT
	p.EntityId
FROM
	Data_NVarChar an
INNER JOIN
	Relationship ar ON an.EntityId = ar.FromId AND ar.TenantId = 0 AND TypeId = @isOfType AND ToId = @app AND an.Data = @appName
INNER JOIN
	Relationship ap ON ar.FromId = ap.ToId AND ap.TypeId = @packageForApplication AND ap.TenantId = ar.TenantId
INNER JOIN
	Data_Guid p ON ap.FromId = p.EntityId AND p.FieldId = @appVerId AND p.TenantId = ap.TenantId
INNER JOIN
	Data_Guid sp ON p.Data = sp.Data AND sp.FieldId = @packageId AND sp.TenantId = @tenantId
WHERE
	an.TenantId = 0";

					using ( IDbCommand command = ctx.CreateCommand( sql ) )
					{
						ctx.AddParameter( command, "@appName", DbType.String, applicationName );
						ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );

						object packageIdObject = command.ExecuteScalar( );

						if ( packageIdObject != null && packageIdObject != DBNull.Value )
						{
							packageId = ( long ) packageIdObject;
						}
					}
				}
			}

			return packageId;
		}

		/// <summary>
		///     Gets the tenant current package identifier by unique identifier.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="applicationGuid">The application unique identifier.</param>
		/// <returns></returns>
		public static long GetTenantCurrentPackageIdByGuid( long tenantId, Guid applicationGuid )
		{
			long packageId = -1;

			using ( new GlobalAdministratorContext( ) )
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					const string sql = @"
DECLARE @packageForApplication	BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
DECLARE @appVerId				BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )
DECLARE @applicationId			BIGINT = dbo.fnAliasNsId( 'applicationId', 'core', DEFAULT)
DECLARE @packageId				BIGINT = dbo.fnAliasNsId( 'packageId', 'core', @tenantId )

SELECT
	ag.EntityId
FROM
	Entity e
INNER JOIN
	Data_Guid g ON
		e.UpgradeId = g.Data AND
		g.FieldId = @applicationId AND
		g.TenantId = 0
INNER JOIN
	Relationship r ON
		g.EntityId = r.ToId AND
		r.TenantId = 0 AND
		r.TypeId = @packageForApplication
INNER JOIN
	Data_Guid ag ON
		r.FromId = ag.EntityId AND
		ag.FieldId = @appVerId AND
		ag.TenantId = 0
INNER JOIN
	Data_Guid tg ON
		tg.Data = ag.Data AND
		tg.FieldId = @packageId AND
		tg.TenantId = @tenantId
WHERE
	e.UpgradeId = @appGuid AND
	e.TenantId = @tenantId";

					using ( IDbCommand command = ctx.CreateCommand( sql ) )
					{
						ctx.AddParameter( command, "@appGuid", DbType.Guid, applicationGuid );
						ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );

						object packageIdObject = command.ExecuteScalar( );

						if ( packageIdObject != null && packageIdObject != DBNull.Value )
						{
							packageId = ( long ) packageIdObject;
						}
					}
				}
			}

			return packageId;
		}

		/// <summary>
		///     Look up a application by name within a specific tenant
		/// </summary>
		/// <param name="tenantId">The tenant id.</param>
		/// <param name="applicationName">Name of the application</param>
		/// <returns>
		///     The application ID.
		/// </returns>
		public static AppPackage GetTenantCurrentPackageByName( long tenantId, string applicationName )
		{
			long packageId = GetTenantCurrentPackageIdByName( tenantId, applicationName );

			if ( packageId != -1 )
			{
				/////
				// Get the application package.
				/////
				return Entity.Get<AppPackage>( packageId );
			}

			return null;
		}

		/// <summary>
		///     Gets the tenant current package by unique identifier.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="applicationGuid">The application unique identifier.</param>
		/// <returns></returns>
		public static AppPackage GetTenantCurrentPackageByGuid( long tenantId, Guid applicationGuid )
		{
			long packageId = GetTenantCurrentPackageIdByGuid( tenantId, applicationGuid );

			if ( packageId != -1 )
			{
				/////
				// Get the application package.
				/////
				return Entity.Get<AppPackage>( packageId );
			}

			return null;
		}

        /// <summary>
	    /// Sets the documentation settings.
	    /// </summary>
	    /// <param name="docoSettings">Dictionary of doc settings. Key name is the field alias name.</param>
	    /// <returns></returns>
	    /// <exception cref="ArgumentNullException"></exception>
	    public static bool SetDocumentationSettings(Dictionary<string, string> docoSettings)
        {
            if (docoSettings == null)
            {
                throw new ArgumentNullException(nameof(docoSettings));
            }

            using (new GlobalAdministratorContext())
            {                
                var docoSettingsEntity = Entity.Get<SystemDocumentationSettings>("core:systemDocumentationSettingsInstance", true);
                
                var allowedFields = new HashSet<string>
                {
                    "core:navHeaderDocumentationUrl",
                    "core:documentationUrl",
                    "core:releaseNotesUrl",
                    "core:contactSupportUrl",
                    "core:documentationUserName",
                    "core:documentationUserPassword"
                };

                foreach (var kvp in docoSettings)
                {
                    var alias = kvp.Key;
                    var value = kvp.Value;

                    if (alias == null || value == null)
                    {
                        continue;
                    }

                    if (!allowedFields.Contains(alias))
                    {
                        throw new ArgumentException(
                            $@"The field with alias {alias} does not apply to the system documentation settings type.", nameof(docoSettings));
                    }

                    docoSettingsEntity.SetField(alias, value);
                }

                docoSettingsEntity.Save();
            }

            return true;
        }
    }
}