// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Sources;
using EDC.SoftwarePlatform.Migration.Storage;
using EDC.SoftwarePlatform.Migration.Targets;
using ReadiNow.Database;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Tenant Manager
	/// </summary>
	public static class TenantManager
	{
        /// <summary>
        ///     Exports the tenant.
        /// </summary>
        /// <param name="tenantName">Name of the tenant.</param>
        /// <param name="packagePath">The package path.</param>
        /// <param name="metadataOnly">If true, exclude user data.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     tenantName
        ///     or
        ///     tenantStorePath
        /// </exception>
        public static void ExportTenant( string tenantName, string packagePath, bool metadataOnly, IProcessingContext context = null )
		{
			if ( string.IsNullOrEmpty( tenantName ) )
			{
				throw new ArgumentNullException( "tenantName" );
			}

			if ( string.IsNullOrEmpty( packagePath ) )
			{
				throw new ArgumentNullException( "packagePath" );
			}

			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			context.Report.StartTime = DateTime.Now;

			long tenantId = TenantHelper.GetTenantId( tenantName, true );

            IDataSource source = metadataOnly
                ? (IDataSource)new TenantMetadataSource { TenantId = tenantId, TenantName = tenantName }
                : (IDataSource)new TenantSource { TenantId = tenantId, TenantName = tenantName };

            /////
            // Create source to load app data from tenant
            /////
            using ( source )
            {
                Format packageFormat = FileManager.GetExportFileFormat( packagePath );
                
                /////
                 // Create target to write to SQLite database
                 /////
                using ( var target = FileManager.CreateDataTarget( packageFormat, packagePath ) )
				{
					/////
					// Copy the data
					/////
					var processor = new CopyProcessor( source, target, context );
					processor.MigrateData( );
				}
			}

			context.Report.EndTime = DateTime.Now;
        }

        /// <summary>
        ///     Gets the tenants.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetTenants( )
		{
			return GetTenants( null );
		}

		/// <summary>
		///     Gets the tenants.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public static IEnumerable<string> GetTenants( IProcessingContext context )
		{
			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			context.Report.StartTime = DateTime.Now;

			List<string> tenantNames;

			using ( new GlobalAdministratorContext( ) )
			{
				IEnumerable<Tenant> tenants = TenantHelper.GetAll();

				tenantNames = tenants.Select( tenant => tenant.Name ).ToList( );
			}

			context.Report.EndTime = DateTime.Now;

			return tenantNames;
		}

		/// <summary>
		///     Gets the users.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns></returns>
		public static IEnumerable<string> GetUsers( string tenantName )
		{
			return GetUsers( tenantName, null );
		}

		/// <summary>
		///     Gets the users.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public static IEnumerable<string> GetUsers( string tenantName, IProcessingContext context )
		{
			if ( string.IsNullOrEmpty( tenantName ) )
			{
				throw new ArgumentNullException( "tenantName" );
			}

			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			context.Report.StartTime = DateTime.Now;

			List<string> userNames;

			using ( new TenantAdministratorContext( tenantName ) )
			{
				IEnumerable<UserAccount> users = Entity.GetInstancesOfType<UserAccount>( false, "isOfType.id, name" );

				userNames = users.Select( user => user.Name ).ToList( );
			}

			context.Report.EndTime = DateTime.Now;

			return userNames;
		}

		/// <summary>
		///     Imports the tenant.
		/// </summary>
		/// <param name="packagePath">The package path.</param>'
        /// <returns>the tenant id</returns>
		public static long ImportTenant( string packagePath )
		{
			return ImportTenant( packagePath, null, null );
		}

		/// <summary>
		///     Imports the tenant.
		/// </summary>
		/// <param name="packagePath">The package path.</param>
		/// <param name="context">The context.</param>
        /// <returns>the tenant id</returns>
		public static long ImportTenant( string packagePath, IProcessingContext context )
		{
			return ImportTenant_Impl( packagePath, null, false, context );
		}

		/// <summary>
		///     Imports the tenant.
		/// </summary>
		/// <param name="packagePath">The package path.</param>
		/// <param name="tenantName">Name of the tenant.</param>
        /// <returns>the tenant id</returns>
		public static long ImportTenant( string packagePath, string tenantName )
		{
			return ImportTenant( packagePath, tenantName, null );
		}

		/// <summary>
		///     Imports the tenant.
		/// </summary>
		/// <param name="packagePath">The package path.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="context">The context.</param>
        /// <returns>the tenant id</returns>
		public static long ImportTenant( string packagePath, string tenantName, IProcessingContext context )
		{
			return ImportTenant_Impl( packagePath, tenantName, false, context );
        }

        /// <summary>
        ///     Imports the tenant.
        /// </summary>
        /// <returns>the tenant id</returns>
        public static void InstallGlobalTenant( )
        {
            IProcessingContext context = new ProcessingContext( );

            context.Report.Action = AppLibraryAction.InstallGlobal;
            context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant Id", "Global" ) );

            IDictionary<Guid, Guid> appToAppVer = new Dictionary<Guid, Guid>( );

            // Get application versions
            // It is assumed that if there is no global tenant, then there is only one version of each of the core apps
            using ( IDatabaseContext dbContext = DatabaseContext.GetContext( false ) )
            using ( IDbCommand command = dbContext.CreateCommand())
            {
                command.CommandText = "select FromUid AppUid, AppVerUid from AppRelationship where ToUid = @solution and TypeUid = @isOfType";
                command.AddParameterWithValue( "@solution", Guids.Solution );
                command.AddParameterWithValue( "@isOfType", Guids.IsOfType );

                using ( IDataReader reader = command.ExecuteReader( ) )
                {
                    // Load appVerId for apps from app library
                    while ( reader.Read( ) )
                    {
                        Guid appId = reader.GetGuid( 0 );
                        Guid appVerId = reader.GetGuid( 1 );
                        appToAppVer[ appId ] = appVerId;
                    }
                }
            }

            // Install apps
            Guid[ ] coreApps = new[ ]
            {
                Guids.CoreSolution,
                Guids.ConsoleSolution,
                Guids.CoreDataSolution,
                Guids.SystemSolution
            };

            // Copy system application content from the app library into the global tenant
            foreach ( Guid appId in coreApps )
            {
                // Get the AppVerId
                Guid appVerId;
                if ( !appToAppVer.TryGetValue( appId, out appVerId ) )
                    throw new Exception( $"Aborting: System app {0} was not present in app library." );

                context.WriteInfo( $"Installing app {appId} package {appVerId} to global tenant." );

                IDataSource empty = new EmptySource( );
                IDataSource source = new LibraryAppSource
                {
                    AppId = appId,
                    AppVerId = appVerId,
                    AppName = appId.ToString( )
                };
                TenantMergeTarget target = new TenantMergeTarget
                {
                    TenantId = 0
                };

                using ( empty )
                using ( source )
                using ( target )
                {
                    MergeProcessor processor = new MergeProcessor( context )
                    {
                        OldVersion = empty,
                        NewVersion = source,
                        Target = target
                    };
                    processor.MergeData( );

                    target.Commit( );
                }
            }


            // Copy metadata from the tenant back into the application library
            foreach ( Guid appId in coreApps )
            {
                Guid appVerId = appToAppVer[ appId ];

                long solutionEntityId = Entity.GetIdFromUpgradeId( appId );
                IDataSource metadataSource = new TenantAppSource
                {
                    SolutionId = solutionEntityId,
                    TenantId = 0
                };
                LibraryAppTarget metadataTarget = new LibraryAppTarget
                {
                    ApplicationVersionId = appVerId
                };
                using ( metadataSource )
                using ( metadataTarget )
                {
                    CopyProcessor processor = new CopyProcessor( metadataSource, metadataTarget, context );
                    processor.CopyMetadataOnly = true;
                    processor.MigrateData( );

                    metadataTarget.Commit( );
                }

                context.WriteInfo( $"Installed app {appId} to global tenant." );
            }
        }

        /// <summary>
        ///     Imports the tenant.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="tenantName">Name of the tenant.</param>
        /// <param name="overwrite">
        ///     if set to <c>true</c> [overwrite].
        /// </param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentNullException">tenantStorePath</exception>
        /// <exception cref="System.InvalidOperationException">
        ///     The package does not contain a tenant.
        ///     or
        /// </exception>
        /// <exception cref="System.ArgumentException">The package is corrupt.</exception>
        /// <remarks>
        ///     If a tenant with the same name already exists, an exception will be thrown.
        ///     To overwrite an existing tenant with the same name, call OverwriteTenant.
        /// </remarks>
        /// <returns>the tenant id</returns>
        private static long ImportTenant_Impl( string packagePath, string tenantName, bool overwrite, IProcessingContext context )
		{
			if ( string.IsNullOrEmpty( packagePath ) )
			{
				throw new ArgumentNullException( "packagePath" );
			}

			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			context.Report.StartTime = DateTime.Now;

			try
			{
				/////
				// Create source to load app data from tenant
				/////
				using ( var source = FileManager.CreateDataSource( packagePath ) )
				{
					Metadata metadata = source.GetMetadata( context );

					if ( metadata.Type == SourceType.AppPackage )
					{
						throw new InvalidOperationException( "The package does not contain a tenant." );
					}

					if ( string.IsNullOrEmpty( tenantName ) )
					{
						tenantName = metadata.Name;
					}

					if ( string.IsNullOrEmpty( tenantName ) )
					{
						throw new ArgumentException( "The package is corrupt." );
					}

					bool deleteExistingTenant = false;

					if ( TenantExists( tenantName ) )
					{
						if ( overwrite )
						{
							deleteExistingTenant = true;
						}
						else
						{
							throw new InvalidOperationException( string.Format( "The tenant '{0}' already exists.", tenantName ) );
						}
					}

					long userId;
					RequestContext.TryGetUserId( out userId );

					/////
					// Create target to write to SQLite database
					/////
					using ( var target = new TenantCopyTarget( ) )
					{
                        Tenant tenant;

						/////
						// Create the tenant.
						/////
						using ( new GlobalAdministratorContext( ) )
						{
							if ( deleteExistingTenant )
							{
								Tenant existingTenant = Entity.GetByName<Tenant>( tenantName ).FirstOrDefault( );

								if ( existingTenant != null )
								{
									using ( DatabaseContextInfo.SetContextInfo( "Delete existing tenant" ) )
									using ( IDbCommand command = target.CreateCommand( ) )
									{
										command.CommandText = "spDeleteTenant";
										command.CommandType = CommandType.StoredProcedure;
										command.Parameters.Add( new SqlParameter( "@tenantId", existingTenant.Id ) );
										command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );
										command.ExecuteNonQuery( );
									}
								}
							}

							tenant = Entity.Create<Tenant>( );
							tenant.Name = tenantName;
							tenant.Description = tenantName + " tenant";
							tenant.Save( );

							target.TenantId = tenant.Id;
						}

						/////
						// Copy the data
						/////
						var processor = new CopyProcessor( source, target, context );
						processor.MigrateData( );

						/////
						// Commit the changes.
						/////
						target.Commit( );

                        return tenant.Id;
					}
				}
			}
			finally
			{
				ForeignKeyHelper.Trust( );

				context.Report.EndTime = DateTime.Now;
			}
		}

		/// <summary>
		///     Overwrites the tenant.
		/// </summary>
		/// <param name="packagePath">The package path.</param>
		public static void OverwriteTenant( string packagePath )
		{
			OverwriteTenant( packagePath, null, null );
		}

		/// <summary>
		///     Overwrites the tenant.
		/// </summary>
		/// <param name="packagePath">The package path.</param>
		/// <param name="context">The context.</param>
		public static void OverwriteTenant( string packagePath, IProcessingContext context )
		{
			ImportTenant_Impl( packagePath, null, true, context );
		}

		/// <summary>
		///     Overwrites the tenant.
		/// </summary>
		/// <param name="packagePath">The package path.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		public static void OverwriteTenant( string packagePath, string tenantName )
		{
			OverwriteTenant( packagePath, tenantName, null );
		}

		/// <summary>
		///     Overwrites the tenant.
		/// </summary>
		/// <param name="packagePath">The package path.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="context">The context.</param>
		public static void OverwriteTenant( string packagePath, string tenantName, IProcessingContext context )
		{
			ImportTenant_Impl( packagePath, tenantName, true, context );
		}

		/// <summary>
		///     Sets the user password.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		public static bool SetUserPassword( string userName, string tenantName, string password )
		{
			if ( string.IsNullOrEmpty( userName ) )
			{
				throw new ArgumentNullException( "userName" );
			}

			if ( string.IsNullOrEmpty( tenantName ) )
			{
				throw new ArgumentNullException( "tenantName" );
			}

			long tenantId = TenantHelper.GetTenantId( tenantName );

			if ( tenantId == -1 )
			{
				return false;
			}

			using ( new TenantAdministratorContext( tenantId ) )
			{
				/////
				// Fetch the user account.
				/////
				UserAccount userAccount = Entity.GetByField<UserAccount>( userName, true, new EntityRef( "core", "name" ) ).FirstOrDefault( );

				if ( userAccount == null )
				{
					return false;
				}

				userAccount.Password = password;

				userAccount.Save( );
			}

			return true;
		}

		/// <summary>
		/// Resets the account.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// userName
		/// or
		/// tenantName
		/// </exception>
		public static bool ResetAccount( string userName, string tenantName )
		{
			if ( string.IsNullOrEmpty( userName ) )
			{
				throw new ArgumentNullException( "userName" );
			}

			if ( string.IsNullOrEmpty( tenantName ) )
			{
				throw new ArgumentNullException( "tenantName" );
			}

			long tenantId = TenantHelper.GetTenantId( tenantName );

			if ( tenantId == -1 )
			{
				return false;
			}

			using ( new TenantAdministratorContext( tenantId ) )
			{
				/////
				// Fetch the user account.
				/////
				UserAccount userAccount = Entity.GetByField<UserAccount>( userName, true, new EntityRef( "core", "name" ) ).FirstOrDefault( );

				if ( userAccount == null )
				{
					return false;
				}

				userAccount.BadLogonCount = 0;

				userAccount.Save( );
			}

			return true;
		}

		/// <summary>
		/// Enables the account.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// userName
		/// or
		/// tenantName
		/// </exception>
		public static bool EnableAccount( string userName, string tenantName )
		{
			return SetAccountStatus( userName, tenantName, UserAccountStatusEnum_Enumeration.Active );
		}

		/// <summary>
		/// Disables the account.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// userName
		/// or
		/// tenantName
		/// </exception>
		public static bool DisableAccount( string userName, string tenantName )
		{
			return SetAccountStatus( userName, tenantName, UserAccountStatusEnum_Enumeration.Disabled );
		}

		/// <summary>
		/// Locks the account.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// userName
		/// or
		/// tenantName
		/// </exception>
		public static bool LockAccount( string userName, string tenantName )
		{
			return SetAccountStatus( userName, tenantName, UserAccountStatusEnum_Enumeration.Locked );
		}

		/// <summary>
		/// Sets the account status.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="status">The status.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// userName
		/// or
		/// tenantName
		/// </exception>
		private static bool SetAccountStatus( string userName, string tenantName, UserAccountStatusEnum_Enumeration status )
		{
			if ( string.IsNullOrEmpty( userName ) )
			{
				throw new ArgumentNullException( "userName" );
			}

			if ( string.IsNullOrEmpty( tenantName ) )
			{
				throw new ArgumentNullException( "tenantName" );
			}

			long tenantId = TenantHelper.GetTenantId( tenantName );

			if ( tenantId == -1 )
			{
				return false;
			}

			using ( new TenantAdministratorContext( tenantId ) )
			{
				/////
				// Fetch the user account.
				/////
				UserAccount userAccount = Entity.GetByField<UserAccount>( userName, true, new EntityRef( "core", "name" ) ).FirstOrDefault( );

				if ( userAccount == null )
				{
					return false;
				}

				userAccount.AccountStatus_Enum = status;

				userAccount.Save( );
			}

			return true;
		}


		/// <summary>
		///     Tenants the exists.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static bool TenantExists( string name )
		{
			return TenantExists( name, null );
		}

		/// <summary>
		///     Tenants the exists.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">name</exception>
		public static bool TenantExists( string name, IProcessingContext context )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				throw new ArgumentNullException( "name" );
			}

			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			context.Report.StartTime = DateTime.Now;

			Tenant tenant;

			using ( new GlobalAdministratorContext( ) )
			{
				tenant = Entity.GetByName<Tenant>( name ).FirstOrDefault( );
			}

			context.Report.EndTime = DateTime.Now;

			return tenant != null;
		}

		/// <summary>
		///     User exists.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="tenant">The tenant.</param>
		/// <returns></returns>
		public static bool UserExists( string name, string tenant )
		{
			return UserExists( name, tenant, null );
		}

		/// <summary>
		///     Tenants the exists.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="tenant">The tenant.</param>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">name</exception>
		public static bool UserExists( string name, string tenant, IProcessingContext context )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				throw new ArgumentNullException( "name" );
			}

			if ( string.IsNullOrEmpty( tenant ) )
			{
				throw new ArgumentNullException( "tenant" );
			}

			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			context.Report.StartTime = DateTime.Now;

			UserAccount user;

			using ( new TenantAdministratorContext( tenant ) )
			{
				user = Entity.GetByName<UserAccount>( name ).FirstOrDefault( );
			}

			context.Report.EndTime = DateTime.Now;

			return user != null;
		}
	}
}