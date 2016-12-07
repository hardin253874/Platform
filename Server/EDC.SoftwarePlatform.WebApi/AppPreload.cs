// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using Autofac;
using EDC.ReadiNow.CAST;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.MessageQueue;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.SoftwarePlatform.Migration;
using EDC.SoftwarePlatform.Services.FileManager;

namespace EDC.SoftwarePlatform.WebApi
{
	/// <summary>
	///     Class representing application preload for WebApi.
	/// </summary>
	/// <seealso cref="System.Web.Hosting.IProcessHostPreloadClient" />
	public class AppPreload : IProcessHostPreloadClient
	{
		/// <summary>
		///     Gets the tenants to preload.
		/// </summary>
		/// <value>
		///     The tenants to preload.
		/// </value>
		/// <exception cref="System.Exception">Could not load tenants</exception>
		private static List<Tenant> TenantsToPreload
		{
			get
			{
				List<Tenant> tenantEntities = TenantHelper.GetAll( ).ToList( );

				if ( tenantEntities.Count <= 0 )
				{
					throw new Exception( "Could not load tenants" );
				}

				var prewarmConfiguration = ConfigurationSettings.GetPrewarmConfigurationSection( );

				if ( prewarmConfiguration != null && prewarmConfiguration.ElementInformation.IsPresent )
				{
					if ( prewarmConfiguration.Tenants != null && prewarmConfiguration.Tenants.ElementInformation.IsPresent )
					{
						bool allTenants = false;
						List<string> tenantNames = new List<string>( );

						foreach ( PrewarmTenant tenant in prewarmConfiguration.Tenants )
						{
							if ( !string.IsNullOrEmpty( tenant.Name ) )
							{
								/////
								// If wild card is specified, pre-warm all tenants (same as not having specified any tenants)
								/////
								if ( tenant.Name.Trim( ) == "*" )
								{
									allTenants = true;
									break;
								}

								tenantNames.Add( tenant.Name.ToLowerInvariant( ).Trim( ) );
							}
						}

						if ( !allTenants )
						{
							if ( tenantNames.Count > 0 )
							{
								tenantEntities = tenantEntities.Where( t => t.Name == null || tenantNames.Contains( t.Name.ToLowerInvariant( ).Trim( ) ) ).ToList( );
							}
							else
							{
								tenantEntities.Clear( );
							}
						}
					}
				}
				return tenantEntities;
			}
		}

		/// <summary>
		///     Provides initialization data that is required in order to preload the application.
		/// </summary>
		/// <param name="parameters">Data to initialize the application. This parameter can be null or an empty array.</param>
		public void Preload( string[ ] parameters )
		{
			try
			{
				using ( DatabaseContext.GetContext( ) )
				{
					EventLog.Application.WriteInformation( "EDC.SoftwarePlatform.WebApi Preload starting..." );

					LoadMigrationAssembly( );

					/////
					// NOTE* Preloading of tenants is currently not supported due to issues with AutoFac and the GAC
					/////
					//PreloadTenants( );

					PreloadApplication( );

					EventLog.Application.WriteInformation( "EDC.SoftwarePlatform.WebApi Preload complete." );
				}
			}
			catch ( TypeInitializationException )
			{
				/////
				// During install, a type initialization error may occur since not all dependencies are loaded yet.
				/////
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( "An error occurred preloading EDC.SoftwarePlatform.WebApi. {0}", exc );
			}
		}

		/// <summary>
		///     Loads the migration assembly.
		/// </summary>
		private static void LoadMigrationAssembly( )
		{
// HACK HACK HACK
			// This code is here to bring in the migration assembly early.
			// Without this fileload exceptions are raised when the AppManagerController is accessed
			// after first install.
			// For some reason assemblies are being loaded into different load contexts.
			// The web api assembly is being loaded into the default context on first load and the migration assembly is loaded in a loadfrom context.
			// As the migration assembly is in a different load context it is not being made available to the web api assembly.
// ReSharper disable UnusedVariable
			var g = Applications.ConsoleApplicationId;
			var ud = FileManagerService.UploadDirectory;
// ReSharper restore UnusedVariable
		}

		/// <summary>
		///     Preloads the tenant.
		/// </summary>
		/// <param name="tenant">The tenant.</param>
		private static void PreloadTenant( KeyValuePair<long, string> tenant )
		{
			try
			{
				EventLog.Application.WriteInformation( $"EDC.SoftwarePlatform.WebApi Preload tenant '{tenant.Value}' starting..." );

				RequestContext.SetTenantAdministratorContext( tenant.Key );

				BulkPreloader.TenantWarmup( );

				RequestContext.FreeContext( );

				EventLog.Application.WriteInformation( $"EDC.SoftwarePlatform.WebApi Preload tenant '{tenant.Value}' complete." );
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( $"Failed to preload tenant '{tenant.Value}'. {exc}" );
			}
		}

		/// <summary>
		///     Preloads the tenants.
		/// </summary>
		/// <exception cref="System.Exception">Could not load tenants</exception>
		private static void PreloadTenants( )
		{
			using ( Profiler.Measure( "EDC.SoftwarePlatform.WebApi Preload" ) )
			{
				/////
				// Set system administrators context to retrieve the tenants.
				/////
				RequestContext.SetSystemAdministratorContext( );

				Dictionary<long, string> tenants = TenantsToPreload.Where( t => t.Id != 0 ).ToDictionary( t => t.Id, t => t.Name );

				RequestContext.FreeContext( );

				foreach ( var tenant in tenants )
				{
					PreloadTenant( tenant );
				}
			}
		}

		/// <summary>
		///     Register Autofac modules in the WebApi assembly
		/// </summary>
		internal static void RegisterModules( )
		{
            if ( _modulesRegistered )
                return;

            Assembly assembly = MethodBase.GetCurrentMethod( ).DeclaringType.Assembly;
			ContainerBuilder containerBuilder = new ContainerBuilder( );
			containerBuilder.RegisterAssemblyModules( assembly );
			containerBuilder.Update( ( IContainer ) Factory.Global );

            _modulesRegistered = true;
        }
        private static bool _modulesRegistered = false;

		/// <summary>
		///     Preloads the application.
		/// </summary>
		private void PreloadApplication( )
		{
			var scheduler = SchedulingHelper.Instance; // Start the scheduler (suspended, not running)

			// Pub-Sub Cache Invalidation Channels
			TenantHelper.InitializeMessageChannel( );
			PerTenantEntityTypeCache.InitializeMessageChannel( );

			RegisterModules( );
		}
	}
}