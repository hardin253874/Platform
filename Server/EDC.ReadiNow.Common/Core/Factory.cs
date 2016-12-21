// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model.CacheInvalidation;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using EDC.IO;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Services.Console.WorkflowActions;
using ReadiNow.Connector;
using ReadiNow.Expressions.CalculatedFields;
using ReadiNow.Expressions;
using ReadiNow.DocGen;
using ReadiNow.Database;
using ReadiNow.EntityGraph;
using EDC.ReadiNow.Core.FeatureSwitch;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter;
using EDC.ReadiNow.IO.RemoteFileFetcher;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.SecuredData;
using EDC.ReadiNow.BackgroundTasks;
using ReadiNow.ImportExport;

namespace EDC.ReadiNow.Core
{
    /// <summary>
    /// Autofac container for inversion of control.
    /// </summary>
    public static class Factory
    {
        public const string NonCachedKey = "NonCached";
        public const string TaskGroupName = "Tasks";

        /// <summary>
        /// Global IoC container.
        /// </summary>
        private static Lazy<IContainer> _globalContainer = new Lazy<IContainer>( Build, LazyThreadSafetyMode.ExecutionAndPublication );

        /// <summary>
        /// Current scope container on local thread.
        /// </summary>
        [ThreadStatic]
        static ILifetimeScope _current;

        /// <summary>
        /// This is the preferred way for code to access the container.
        /// It will eventually respect scope.
        /// </summary>
        public static ILifetimeScope Current
        {
            get
            {
                if (_current == null)
                {
                    _current = Global;
                }
                return _current;
            } 
        }

        /// <summary>
        /// Use global if you want to access the root scope rather than the current scope.
        /// </summary>
        public static ILifetimeScope Global
        {
            get { return _globalContainer.Value; }
        }

        /// <summary>
        /// Use global if you want to access the root scope rather than the current scope.
        /// </summary>
        public static IDisposable SetCurrentScope(ILifetimeScope scope)
        {
            if ( scope == null )
                throw new ArgumentNullException( "scope" );

            return new CurrentScope( scope );
        }

        /// <summary>
        /// Register types.
        /// </summary>
        /// <returns></returns>
        private static IContainer Build( )
        {
            var builder = new ContainerBuilder( );

            RegisterAssemblyModules(builder, "ReadiNow.Connector, Version=1.0.0.0, Culture=neutral");
            RegisterAssemblyModules(builder, "ReadiNow.DocGen, Version=1.0.0.0, Culture=neutral");
            RegisterAssemblyModules(builder, "ReadiNow.EntityGraph, Version=1.0.0.0, Culture=neutral");
            RegisterAssemblyModules(builder, "ReadiNow.Expressions, Version=1.0.0.0, Culture=neutral");
            RegisterAssemblyModules(builder, "ReadiNow.QueryEngine, Version=1.0.0.0, Culture=neutral");
            RegisterAssemblyModules(builder, "EDC.ReadiNow.Common, Version=1.0.0.0, Culture=neutral");
            RegisterAssemblyModules(builder, "EDC.ReadiNow.CAST, Version=1.0.0.0, Culture=neutral");
            RegisterAssemblyModules(builder, "ReadiNow.Activities, Version=1.0.0.0, Culture=neutral");
            RegisterAssemblyModules(builder, "EDC.SoftwarePlatform.Services, Version=1.0.0.0, Culture=neutral");
			RegisterAssemblyModules(builder, "EDC.SoftwarePlatform.Migration, Version=1.0.0.0, Culture=neutral" );
			RegisterAssemblyModules(builder, "ReadiNow.Integration, Version=1.0.0.0, Culture=neutral");

            var container = builder.Build( );
            return container;
        }

        /// <summary>
        /// Register the modules in an assembly.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="assemblyName">The strong name of the assembly.</param>
        private static void RegisterAssemblyModules( ContainerBuilder builder, string assemblyName )
        {
            try
            {
                Assembly asm = Assembly.Load( assemblyName );
                if ( asm == null )
                    throw new Exception( "Assembly did not load." );

                builder.RegisterAssemblyModules( asm );
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError( "Failed to register assembly components.\n{0}\n{1}", assemblyName, ex.ToString() );
            }
        }

        /// <summary>
        /// Get an <see cref="ICalculatedFieldProvider"/>.
        /// </summary>
        public static ICalculatedFieldProvider CalculatedFieldProvider
        {
            get { return Current.Resolve<ICalculatedFieldProvider>(); }
        }

        /// <summary>
        /// Get an <see cref="ICalculatedFieldProvider"/>.
        /// </summary>
        public static ICalculatedFieldMetadataProvider CalculatedFieldMetadataProvider
        {
            get { return Current.Resolve<ICalculatedFieldMetadataProvider>(); }
        }

        /// <summary>
        /// Get an <see cref="IConnectorService"/>, used for data connector.
        /// </summary>
        public static IConnectorService ConnectorService
        {
            get { return Current.Resolve<IConnectorService>( ); }
        }

        /// <summary>
        /// Get an <see cref="IDatabaseProvider"/>, for connecting to the database.
        /// </summary>
        public static IDatabaseProvider DatabaseProvider
        {
            get { return Current.Resolve<IDatabaseProvider>(); }
        }

        /// <summary>
        /// Get an <see cref="IDocumentGenerator"/>, used for creating document template reports.
        /// </summary>
        public static IDocumentGenerator DocumentGenerator
        {
            get { return Current.Resolve<IDocumentGenerator>(); }
        }

        /// <summary>
        /// Get an <see cref="IEntityRepository"/>, used for creating and getting entities.
        /// </summary>
        public static IEntityRepository EntityRepository
        {
            get { return Current.Resolve<IEntityRepository>( ); }
        }

        /// <summary>
        /// Get an <see cref="IEntityRepository"/>, used for creating and getting entities.
        /// </summary>
        public static ICacheInvalidator MultiCacheInvalidator
        {
            get { return Current.ResolveNamed<ICacheInvalidator>( EDC.ReadiNow.Model.CacheInvalidation.MultiCacheInvalidator.Autofac_Key ); }
        }

        /// <summary>
        /// Get an <see cref="IEntityRepository"/>, used for creating and getting entities.
        /// </summary>
        public static IEntityRepository GraphEntityRepository
        {
            get { return Current.ResolveNamed<IEntityRepository>( "Graph" ); }
        }

        /// <summary>
        /// Preferred IReportToQueryConverter (i.e. with caching)
        /// </summary>
        public static IReportToQueryConverter ReportToQueryConverter
        {
            get { return Current.Resolve<IReportToQueryConverter>(); }
        }

        /// <summary>
        /// ReportToQueryConverter without caching.
        /// </summary>
        public static IReportToQueryConverter NonCachedReportToQueryConverter
        {
            get { return Current.ResolveNamed<IReportToQueryConverter>( NonCachedKey ); }
        }

        /// <summary>
        /// Get an <see cref="IQueryRepository"/>, used for getting the security queries.
        /// </summary>
        public static IQueryRepository QueryRepository
        {
            get { return Current.ResolveNamed<IQueryRepository>(EntityAccessControlModule.DefaultQueryRepository); }
        }

        /// <summary>
        /// Get an <see cref="IQueryRepository"/>, used for getting the system security queries.
        /// </summary>
        public static IQueryRepository SystemQueryRepository
        {
            get { return Current.ResolveNamed<IQueryRepository>(EntityAccessControlModule.SystemQueryRepository); }
        }

        /// <summary>
        /// Get an <see cref="IRequestParser"/>, used for parsing entity-graph style request strings.
        /// </summary>
        public static IRequestParser RequestParser
        {
            get { return Current.Resolve<IRequestParser>(); }
        }

        /// <summary>
        /// Get an <see cref="IRuleRepository"/>, used for getting the security rules.
        /// </summary>
        public static IRuleRepository RuleRepository
        {
            get { return Current.Resolve<IRuleRepository>( ); }
        }

        /// <summary>
        /// Get an <see cref="IUserRuleSetProvider"/>, used for getting a key that refers to the set of rules applicable to a user.
        /// </summary>
        public static IUserRuleSetProvider UserRuleSetProvider
        {
            get { return Current.Resolve<IUserRuleSetProvider>( ); }
        }

        /// <summary>
        /// Preferred IQuerySqlBuilder (i.e. with caching)
        /// </summary>
        public static IQuerySqlBuilder QuerySqlBuilder
        {
            get { return Current.Resolve<IQuerySqlBuilder>( ); }
        }

        /// <summary>
        /// QuerySqlBuilder without caching.
        /// </summary>
        public static IQuerySqlBuilder NonCachedQuerySqlBuilder
        {
            get { return Current.ResolveNamed<IQuerySqlBuilder>( NonCachedKey ); }
        }

        /// <summary>
        /// Get an <see cref="IEntityAccessControlService"/>, used for high-level access control operations.
        /// </summary>
        public static IEntityAccessControlService EntityAccessControlService
        {
            get { return Current.Resolve<IEntityAccessControlService>(); }
        }

        /// <summary>
        /// Preferred IQueryRunner (i.e. with caching)
        /// </summary>
        public static IQueryRunner QueryRunner
        {
            get { return Current.Resolve<IQueryRunner>( ); }
        }

        /// <summary>
        /// IQueryRunner without caching.
        /// </summary>
        public static IQueryRunner NonCachedQueryRunner
        {
            get { return Current.ResolveNamed<IQueryRunner>( NonCachedKey ); }
        }

        /// <summary>
        /// The default <see cref="IActivityLogWriter"/>.
        /// </summary>
        public static IActivityLogWriter ActivityLogWriter
        {
            get { return Current.Resolve<IActivityLogWriter>(); }
        }

        /// <summary>
        /// Preferred IExpressionCompiler (i.e. eventually with caching)
        /// </summary>
        public static IExpressionCompiler ExpressionCompiler
        {
            get { return Current.Resolve<IExpressionCompiler>( ); }
        }

        /// <summary>
        /// Preferred IExpressionRunner
        /// </summary>
        public static IExpressionRunner ExpressionRunner
        {
            get { return Current.Resolve<IExpressionRunner>( ); }
        }


        /// <summary>
        /// Preferred IDistributedMemoryManager
        /// </summary>
        public static IDistributedMemoryManager DistributedMemoryManager
        {
            get { return Current.Resolve<IDistributedMemoryManager>(); }
        }

        /// <summary>
        /// Preferred IScriptNameResolver
        /// </summary>
        public static IScriptNameResolver ScriptNameResolver
        {
            get { return Current.Resolve<IScriptNameResolver>(); }
        }

        /// <summary>
        /// Preferred IEntityXmlExporter
        /// </summary>
        public static IEntityXmlExporter EntityXmlExporter
        {
            get { return Current.Resolve<IEntityXmlExporter>( ); }
        }

        /// <summary>
        /// Preferred IEntityXmlImporter
        /// </summary>
        public static IEntityXmlImporter EntityXmlImporter
        {
            get { return Current.Resolve<IEntityXmlImporter>( ); }
        }



        /// <summary>
        /// Prefered IResourceTriggerFilterPolicyCache
        /// </summary>
        public static IResourceTriggerFilterPolicyCache ResourceTriggerFilterPolicyCache
        {
            get { return Current.Resolve<IResourceTriggerFilterPolicyCache>(); }

        }

        /// <summary>
        /// Prefered IUpgradeIdProvider
        /// </summary>
        public static IUpgradeIdProvider UpgradeIdProvider => Current.Resolve<IUpgradeIdProvider>();

        /// <summary>
        /// Preferred TaskManager
        /// </summary>
        public static ITaskManager WorkflowRunTaskManager
        {
            get 
            { 
                return Current.Resolve<ITaskManager>(
                    new NamedParameter("memoryManager", DistributedMemoryManager),      // This should be injected
                    new NamedParameter("taskGroupName", TaskGroupName)); 
            }
        }

        /// <summary>
        /// Get an <see cref="IWorkflowActionsFactory"/>, used for fetching workflow that apply as actions.
        /// </summary>
        public static IWorkflowActionsFactory WorkflowActionsFactory
        {
            get { return Current.Resolve<IWorkflowActionsFactory>(); }
        }

        /// <summary>
        /// Get an <see cref="IFeatureSwitch"/>, used for fetching feature switch.
        /// </summary>
        public static IFeatureSwitch FeatureSwitch
        {
            get { return Current.Resolve<IFeatureSwitch>(); }
        }

        /// <summary>
        /// Get the application library <see cref="IFileRepository"/>
        /// </summary>
        public static IFileRepository AppLibraryFileRepository
        {
            get { return Current.ResolveNamed<FileRepository>(FileRepositoryModule.ApplicationLibraryFileRepositoryName); }
        }

        /// <summary>
        /// Get the binary <see cref="IFileRepository"/>
        /// </summary>
        public static IFileRepository BinaryFileRepository
        {
            get { return Current.ResolveNamed<FileRepository>(FileRepositoryModule.BinaryFileRepositoryName); }
        }

        /// <summary>
        /// Get the temp <see cref="IFileRepository"/>
        /// </summary>
        public static IFileRepository TemporaryFileRepository
        {
            get { return Current.ResolveNamed<FileRepository>(FileRepositoryModule.TemporaryFileRepositoryName); }
        }

        /// <summary>
        /// Get the document <see cref="IFileRepository"/>
        /// </summary>
        public static IFileRepository DocumentFileRepository
        {
            get { return Current.ResolveNamed<FileRepository>(FileRepositoryModule.DocumentFileRepositoryName); }
        }


        /// <summary>
        /// Preferred FileFetcher
        /// </summary>
        public static IRemoteFileFetcher RemoteFileFetcher
        {
            get { return Current.Resolve<IRemoteFileFetcher>(); }
        }

        /// <summary>
        /// Gets the identity provider request context cache.
        /// </summary>
        /// <value>The identity provider request context cache.</value>
        public static IIdentityProviderRequestContextCache IdentityProviderRequestContextCache
        {
            get { return Current.Resolve<IIdentityProviderRequestContextCache>(); }
        }

        /// <summary>
        /// Preferred secured data service
        /// </summary>
        public static ISecuredData SecuredData
        {
            get { return Current.Resolve<ISecuredData>(); }
        }

        /// <summary>
        /// Preferred secured data service
        /// </summary>
        public static ISecuredDataSaveHelper SecuredDataSaveHelper
        {
            get { return Current.Resolve<ISecuredDataSaveHelper>(); }
        }

        /// <summary>
        /// Preferred background task runner
        /// </summary>
        public static IBackgroundTaskManager BackgroundTaskManager
        {
            get { return Current.Resolve<IBackgroundTaskManager>(); }
        }

        /// <summary>
        /// Preferred background task runner
        /// </summary>
        public static IBackgroundTaskController BackgroundTaskController
        {
            get { return Current.Resolve<IBackgroundTaskController>(); }
        }



        /// <summary>
        /// Static constructor
        /// </summary>
        static Factory( )
        {
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        /// <summary>
        /// Handles the DomainUnload event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            // Cleans up all instances managed by Autofac that implement IDispose
            Global.Dispose();
        }

        /// <summary>
        /// Handle binding events
        /// </summary>
        /// <remarks>
        /// The problem: Autofac.Extras.Attributed.dll refers to ver 3.4.0.0 of Autofac.dll
        /// However, the published version is Autofac is 3.5.0.0
        /// The NuGet package lays down assembly redirects in app.config, which are annoying
        /// (and more specifically, don't work when we try to access the Entity model in the MSI Custom Action package).
        /// So catch all bindings for Autofac.dll and redirect them to the version that EDC.ReadiNow.Common references.
        /// </remarks>
        private static Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            var name = new AssemblyName( args.Name );

            if ( name.Name == "Autofac" )
            {
                return typeof( ContainerBuilder ).Assembly;
            }

			var installPath = SpecialFolder.GetSpecialFolderPath( SpecialMachineFolders.Install );

	        /////
			// Probe the tests folder (if found)
			/////
			var assembly = ProbeAssembly( Path.Combine( installPath, "Tests" ), name );

			if ( assembly != null )
			{
				return assembly;
			}

			/////
			// Probe the tools folder (if found)
			/////
			assembly = ProbeAssembly( Path.Combine( installPath, "Tools" ), name );

			if ( assembly != null )
			{
				return assembly;
			}

			/////
			// Probe the spapi bin folder (if found)
			/////
			assembly = ProbeAssembly( Path.Combine( installPath, "SpApi\\Bin" ), name );

	        return assembly;
        }

		/// <summary>
		///		Attempt to locate the assembly at the specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="assemblyName"></param>
		/// <returns></returns>
		private static Assembly ProbeAssembly( string path, AssemblyName assemblyName )
		{
			string assemblyFilename = assemblyName.Name + ".dll";
			
			string assemblyPath = Path.Combine( path, assemblyFilename );

			if ( File.Exists( assemblyPath ) )
			{
				try
				{
					Assembly assembly = Assembly.LoadFrom( assemblyPath );

					if ( assembly != null )
					{
						AssemblyName foundName = assembly.GetName( );

						if ( string.Equals( foundName.Name, assemblyName.Name, StringComparison.InvariantCultureIgnoreCase )  && foundName.Version == assemblyName.Version && string.Equals( foundName.CultureName, assemblyName.CultureName, StringComparison.InvariantCultureIgnoreCase ) )
						{
							return assembly;
						}
					}
				}
				catch
				{
            return null;
        }
			}

			return null;
		}

        private class CurrentScope : IDisposable
        {
            ILifetimeScope NewScope;
            ILifetimeScope OldScope;

            public CurrentScope( ILifetimeScope newScope )
            {
                NewScope = newScope;
                OldScope = Current;
                _current = newScope;
            }

            public void Dispose( )
            {
                if (NewScope != null)
                    NewScope.Dispose();
                _current = OldScope;
            }
        }
    }
}
