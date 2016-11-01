// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.Xml;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.Metadata.Solutions
{
	/// <summary>
	///     Provides helper methods to implement the ISolutionInstaller interface.
	/// </summary>
	public static class SolutionInstallerHelper
	{
		/// <summary>
		///     Deploys an entity-bases solution.
		/// </summary>
		/// <param name="solution">The solution info</param>
		/// <param name="tenantId">The tenant unique identifier.</param>
		public static void DeactivateEntitySolution( SolutionInfo solution, long tenantId )
		{
			EventLog.Application.WriteInformation( "Starting DeactivateEntitySolution for solution={0} tenant={1}", solution.Name, tenantId );

			IEntity solutionEntity = Entity.GetByName<Solution>( solution.Name ).FirstOrDefault( );

			if ( solutionEntity == null )
			{
				EventLog.Application.WriteWarning( "Solution could not be found." );
			}
			else
			{
				// Delete solution entity. CascadeDelete will take care of everything else.
				DeactivateEntitySolution( solutionEntity.Id, tenantId );
			}
		}

		/// <summary>
		///     Deploys an entity-bases solution.
		/// </summary>
		/// <param name="solutionId">The solution unique identifier.</param>
		/// <param name="tenantId">The tenant unique identifier.</param>
		public static void DeactivateEntitySolution( long solutionId, long tenantId )
		{
			EventLog.Application.WriteInformation( "Starting DeactivateEntitySolution for solution={0} tenant={1}", solutionId, tenantId );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				Entity.Delete( solutionId );
			}

			EventLog.Application.WriteInformation( "Ending DeactivateEntitySolution" );
		}

		/// <summary>
		///     Gets the list of solutions contained within the specified configuration file.
		/// </summary>
		/// <param name="path">
		///     A string containing the path to the configuration file.
		/// </param>
		/// <returns>
		///     A collection of solution information.
		/// </returns>
		public static List<SolutionInfo> GetSolutions( string path )
		{
			if ( string.IsNullOrEmpty( path ) )
			{
				throw new ArgumentException( "The specified path parameter is null." );
			}

			if ( !File.Exists( path ) )
			{
				throw new FileNotFoundException( "The specified configuration file does not exist or cannot be opened." );
			}

			var document = new XmlDocument( );
			document.Load( path );

			XmlNodeList solutionNodes = null;

			if ( XmlHelper.EvaluateNodes( document.DocumentElement, "/resources/resource" ) )
			{
				solutionNodes = XmlHelper.SelectNodes( document.DocumentElement, "/resources/resource" );
			}
			else if ( XmlHelper.EvaluateSingleNode( document.DocumentElement, "/resource" ) )
			{
				solutionNodes = XmlHelper.SelectNodes( document.DocumentElement, "/resource" );
			}

			// Iterate through the solutions
			return ( from XmlNode solutionNode in solutionNodes
			         let id = XmlHelper.ReadElementGuid( solutionNode, "id", Guid.Empty )
			         let name = XmlHelper.ReadElementString( solutionNode, "name", String.Empty )
			         let description = XmlHelper.ReadElementString( solutionNode, "description", String.Empty )
			         where ( id != Guid.Empty ) && ( !String.IsNullOrEmpty( name ) )
			         select new SolutionInfo( id, name, description ) ).ToList( );
		}

		/// <summary>
		///     Installs, upgrades or uninstalls the solution using the specified XML configuration set.
		/// </summary>
		/// <param name="node">
		///     An object containing the solution's configuration set.
		/// </param>
		/// <param name="configPath">
		///     A string containing the path to the configuration directory.
		/// </param>
		/// <param name="action">
		///     An enumeration describing the solution install action.
		/// </param>
		public static void Install( XmlNode node, string configPath, SolutionInstallAction action )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( configPath ) )
			{
				throw new ArgumentException( "The specified configPath parameter is invalid." );
			}            
            
            switch ( action )
			{
				case SolutionInstallAction.Install:
					{
					    EventLog.Application.WriteInformation("Installing Solution");
						// Process the database tables (if specified)
						XmlNodeList rawSqlNodes = XmlHelper.SelectNodes( node, "configuration/database/rawSql" );
						ProcessDatabaseRawSql( rawSqlNodes, configPath );

						XmlNodeList bulkNodes = XmlHelper.SelectNodes( node, "configuration/database/bulk" );
						ProcessDatabaseBulk( bulkNodes, configPath, action );

						long userId;
						RequestContext.TryGetUserId( out userId );

						// Creates an open connection with a transaction
						using ( new DeferredChannelMessageContext( ) )
						using ( DatabaseContextInfo.SetContextInfo( "Install bootstrap" ) )
						using ( DatabaseContext databaseContext = DatabaseContext.GetContext( true ) )
						{
							DatabaseChangeTracking.CreateRestorePoint( DatabaseContextInfo.GetMessageChain( userId ) );

							// Process the database tables (if specified)
							XmlNodeList scriptNodes = XmlHelper.SelectNodes( node, "configuration/database/scripts" );
							ProcessDatabaseScripts( scriptNodes, configPath, action );

							// Process the metadata entities (if specified)
							XmlNodeList entityNodes = XmlHelper.SelectNodes( node, "configuration/metadata/entities" );
							if ( entityNodes.Count > 0 )
							{
								ProcessEntityNodes( entityNodes, configPath );
							}

							// Process the metadata entities (if specified)
							XmlNodeList upgradeMapNodes = XmlHelper.SelectNodes( node, "configuration/metadata/upgradeMap" );
							if ( upgradeMapNodes.Count > 0 )
							{
								ProcessUpgradeMapNodes( upgradeMapNodes, configPath );
							}

							// Commit the changes
							databaseContext.CommitTransaction( );
						}

						break;
					}

				case SolutionInstallAction.Upgrade:
					{						
                        /////
                        // Creates an open connection with a transaction
                        /////
                        using ( new DeferredChannelMessageContext( ))
                        using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
                        {
							// Upgrade the metadata entities (if specified)
							XmlNodeList upgradeMapNodes = XmlHelper.SelectNodes( node, "configuration/metadata/upgradeMap" );
							if ( upgradeMapNodes.Count > 0 )
							{
								IDictionary<string, Guid> upgradeMap = ProcessUpgradeMapNodesForUpgrade( upgradeMapNodes, configPath );

								// Upgrade; the metadata entities (if specified)
								XmlNodeList entityNodes = XmlHelper.SelectNodes( node, "configuration/metadata/entities" );
								if ( entityNodes.Count > 0 )
								{
									ProcessEntityNodesForUpgrade( entityNodes, configPath, upgradeMap );
								}
							}
						}

						break;
					}

				case SolutionInstallAction.Uninstall:
					{
                        // ToDo: Add log entry

                        // Creates an open connection with a transaction
                        using ( new DeferredChannelMessageContext( ))
                        using ( DatabaseContext databaseContext = DatabaseContext.GetContext( true ) )
						{
							// Process the database tables (if specified)
							XmlNodeList tableNodes = XmlHelper.SelectNodes( node, "configuration/database/scripts" );
							ProcessDatabaseScripts( tableNodes, configPath, action );

							databaseContext.CommitTransaction( );
						}

						break;
					}
			}

			ForeignKeyHelper.Trust( );
		}

		/// <summary>
		///     Installs, upgrades or uninstalls a solution using the specified XML instruction set.
		/// </summary>
		/// <param name="node">
		///     An object containing the solution's XML instruction set.
		/// </param>
		/// <param name="configPath">
		///     A string containing the path to the configuration directory.
		/// </param>
		/// <param name="action">
		///     An enumeration describing the solution installation action.
		/// </param>
		public static void InstallSolution( XmlNode node, string configPath, SolutionInstallAction action )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( configPath ) )
			{
				throw new ArgumentException( "The specified configPath parameter is invalid." );
			}

			// Check if the solution XML is internal or external
			string format = XmlHelper.ReadAttributeString( node, "@format", string.Empty );
			if ( String.Compare( format, "external", StringComparison.OrdinalIgnoreCase ) == 0 )
			{
				string solutionPath = XmlHelper.ReadAttributeString( node, "@path" );
				node = ResolveNode( node, "/resource", configPath );
				configPath = Path.GetDirectoryName( Path.Combine( configPath, solutionPath ) );
			    EventLog.Application.WriteInformation("Install Solution configuration path", configPath);
			}

			// Note: some database operations (such as those creating db schema) cannot always be performed in
			// a single transaction.

			// Install, upgrade or uninstall the solution
			Install( node, configPath, action );
		}

		/// <summary>
		///     Installs the solution(s) from the specified configuration file.
		/// </summary>
		/// <param name="path">
		///     A string containing the path to the configuration file.
		/// </param>
		public static void InstallSolutions( string path )
		{
			InstallSolutions( path, SolutionInstallAction.Install );
		}

		/// <summary>
		///     Installs, upgrades or uninstalls the solution(s) using the specified configuration file.
		/// </summary>
		/// <param name="path">
		///     A string containing the path to the configuration file.
		/// </param>
		/// <param name="action">
		///     An enumeration describing the solution installation action.
		/// </param>
		public static void InstallSolutions( string path, SolutionInstallAction action )
		{
			if ( String.IsNullOrEmpty( path ) )
			{
				throw new ArgumentException( "The specified path parameter is null." );
			}

			EventLog.Application.WriteInformation( "Starting {0} of {1}", action, path );
			try
			{
				// Check that the file exists
				if ( !File.Exists( path ) )
				{
					throw new FileNotFoundException( string.Format( "The specified solution configuration file does not exist (Path: {0}.)", path ) );
				}

				var document = new XmlDocument( );
				document.Load( path );

				// Get the path to the configuration directory
				string configPath = Path.GetDirectoryName( path );

				XmlNodeList solutionNodes = null;

				if ( XmlHelper.EvaluateNodes( document.DocumentElement, "/resources/resource" ) )
				{
					solutionNodes = XmlHelper.SelectNodes( document.DocumentElement, "/resources/resource" );
				}
				else if ( XmlHelper.EvaluateSingleNode( document.DocumentElement, "/resource" ) )
				{
					solutionNodes = XmlHelper.SelectNodes( document.DocumentElement, "/resource" );
				}

				// Install, upgrade or uninstall each solution
				if ( solutionNodes != null )
				{
					foreach ( XmlNode solutionNode in solutionNodes )
					{
						InstallSolution( solutionNode, configPath, action );
					}
				}

				EventLog.Application.WriteInformation( "Completed {0} of {1}", action, path );
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteError( ex.ToString( ) );
				throw;
			}
		}

		/// <summary>
		///     Processes the database bulk.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="configPath">The configuration path.</param>
		/// <param name="action">The action.</param>
		public static void ProcessDatabaseBulk( XmlNodeList nodes, string configPath, SolutionInstallAction action )
		{
			if ( nodes == null )
			{
				throw new ArgumentNullException( "nodes" );
			}

			if ( string.IsNullOrEmpty( configPath ) )
			{
				throw new ArgumentException( "The specified configPath parameter is invalid." );
			}

			/////
			// Select the bulkInsert nodes
			/////
			IEnumerable<XmlNode> bulkInsertNodes = from XmlNode node in nodes
			                                       from XmlNode scriptNode in ResolveNodes( node, "/bulkInsert", configPath )
			                                       select scriptNode;

			/////
			// Check for valid nodes
			/////
			DateTime startTime = DateTime.Now;
			EventLog.Application.WriteInformation( "Start processing bulk inserts (Start Time: {0})", startTime );


			using ( DatabaseContext context = DatabaseContext.GetContext( ) )
			{
				/////
				// Iterate through the bulkInsert nodes
				/////
				foreach ( XmlNode bulkInsertNode in bulkInsertNodes )
				{
					XmlNode metadata = XmlHelper.SelectSingleNode( bulkInsertNode, "metadata" );
					BulkProviderMetadata providerMetadata;

					try
					{
						providerMetadata = new BulkProviderMetadata( metadata );
					}
					catch
					{
						continue;
					}

					XmlNode sourceNode = XmlHelper.SelectSingleNode( bulkInsertNode, "source" );

					if ( sourceNode == null )
					{
						EventLog.Application.WriteError( "Unable to perform bulk insert. The source is missing. (Configuration: {0}, Xml: {1}).", configPath, bulkInsertNode.OuterXml );
						continue;
					}

					string providerType = XmlHelper.ReadAttributeString( sourceNode, "@provider" );

					/////
					// Get the bulk provider.
					/////
					IDataReader provider = GetProvider( providerType, sourceNode, providerMetadata );

					if ( provider == null )
					{
						EventLog.Application.WriteError( "Unable to perform bulk insert. The source provider is missing or unknown. (Config: {0}, Xml: {1}).", configPath, bulkInsertNode.OuterXml );
						continue;
					}

					/////
					// Bulk load.
					/////
					using ( var copy = new SqlBulkCopy( ( SqlConnection ) context.GetUnderlyingConnection( ) ) )
					{
						copy.DestinationTableName = providerMetadata.TableName;
						copy.BatchSize = providerMetadata.BatchSize;
						copy.WriteToServer( provider );
					}
				}
			}

			DateTime endTime = DateTime.Now;
			TimeSpan timeTaken = endTime - startTime;
			EventLog.Application.WriteInformation( "End processing database scripts (End Time: {0}, Processing Time: {1})", endTime, timeTaken );
		}

		/// <summary>
		///     Uninstalls the solution(s) associated with the specified configuration file.
		/// </summary>
		/// <param name="path">
		///     A string containing the path to the configuration file.
		/// </param>
		public static void UninstallSolutions( string path )
		{
			InstallSolutions( path, SolutionInstallAction.Uninstall );
		}

		/// <summary>
		///     Upgrades the solution(s) from the specified configuration file.
		/// </summary>
		/// <param name="path">
		///     A string containing the path to the configuration file.
		/// </param>
		public static void UpgradeSolutions( string path )
		{
			InstallSolutions( path, SolutionInstallAction.Upgrade );
		}

		/// <summary>
		///     Gets the provider.
		/// </summary>
		/// <param name="providerType">Type of the provider.</param>
		/// <param name="sourceNode">The source node.</param>
		/// <param name="metadata">The metadata.</param>
		/// <returns></returns>
		private static IDataReader GetProvider( string providerType, XmlNode sourceNode, BulkProviderMetadata metadata )
		{
			if ( string.IsNullOrEmpty( providerType ) )
			{
				return null;
			}

			switch ( providerType.ToLowerInvariant( ) )
			{
				case "csv":
					return new CsvBulkProvider( sourceNode, metadata );
				default:
					return null;
			}
		}

		/// <summary>
		///     Mechanism for processing large blocks of unstructured SQL. E.g. a SQL export of a large number of objects.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="configPath">The config path.</param>
		private static void ProcessDatabaseRawSql( XmlNodeList nodes, string configPath )
		{
			IEnumerable<string> sqlCommands = from XmlNode node in nodes
			                                  let sqlBlock = ResolveText( node, configPath )
			                                  from sql in SplitSqlStatements( sqlBlock )
			                                  select sql;

			using ( DatabaseContext databaseContext = DatabaseContext.GetContext( ) )
			{
				// Read each script
				// Note: Typical scenario only has one large script.
				foreach ( string sql in sqlCommands )
				{
					string sqlValue = sql.Replace( "{{SOFTWAREPLATFORMDATABASE}}", databaseContext.DatabaseInfo.Database );

					// Read the first line of script for diagnostic purposes
					EventLog.Application.WriteTrace( "Executing raw script: " + sqlValue );

					try
					{
						// And execute it 
						using ( IDbCommand command = databaseContext.CreateCommand( sqlValue ) )
						{
							command.ExecuteNonQuery( );
						}
					}
					catch ( Exception ex )
					{
						EventLog.Application.WriteError( "Failed to run raw script: {0}\r\n\r\nScript:\r\n{1}", ex.ToString( ), sqlValue );
					}
				}


				// Store database info
				try
				{
					const string sql = "insert into PlatformInfo (InstallDate, BookmarkScheme) values (getdate(), convert(int,rand() * 1000000))";
					using ( IDbCommand command = databaseContext.CreateCommand( sql ) )
					{
						command.ExecuteNonQuery( );
					}
				}
				catch ( Exception ex )
				{
					EventLog.Application.WriteError( "Failed to set database info: {0}", ex.ToString( ) );
				}
			}
		}

		/// <summary>
		///     Process the database scripts from the specified XML instruction set.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="configPath">A string containing the path to the configuration directory.</param>
		/// <param name="action">An enumeration describing the solution install action.</param>
		/// <exception cref="System.ArgumentNullException">node</exception>
		/// <exception cref="System.ArgumentException">The specified configPath parameter is invalid.</exception>
		private static void ProcessDatabaseScripts( XmlNodeList nodes, string configPath, SolutionInstallAction action )
		{
			if ( nodes == null )
			{
				throw new ArgumentNullException( "nodes" );
			}

			if ( string.IsNullOrEmpty( configPath ) )
			{
				throw new ArgumentException( "The specified configPath parameter is invalid." );
			}


			// Select the script nodes
			IEnumerable<XmlNode> scriptNodes = from XmlNode node in nodes
			                                   from XmlNode scriptNode in ResolveNodes( node, "/scripts/script", configPath )
			                                   select scriptNode;

			// Check for valid nodes
			DateTime startTime = DateTime.Now;
			EventLog.Application.WriteInformation( "Start processing database scripts (Start Time: {0})", startTime );


			using ( DatabaseContext databaseContext = DatabaseContext.GetContext( ) )
			{
				// Iterate through the script nodes
				foreach ( XmlNode scriptNode in scriptNodes )
				{
					string name = XmlHelper.ReadElementString( scriptNode, "name" );
					XmlNodeList queryNodes = XmlHelper.SelectNodes( scriptNode, "queries/query" );
					bool match = false;

					// Iterate through the queries
					foreach ( XmlNode queryNode in queryNodes )
					{
						try
						{
							// Match the query provider
							// Retrieve the query type
							string queryAction = XmlHelper.ReadAttributeString( queryNode, "@type", string.Empty );

							// Attempt to match the action and query type (limited options)
							switch ( action )
							{
								case SolutionInstallAction.Install:
									if ( ( String.Compare( queryAction, "install", StringComparison.OrdinalIgnoreCase ) == 0 ) || ( String.Compare( queryAction, "install,upgrade", StringComparison.OrdinalIgnoreCase ) == 0 ) )
									{
										match = true;
									}
									break;
								case SolutionInstallAction.Upgrade:
									if ( ( String.Compare( queryAction, "upgrade", StringComparison.OrdinalIgnoreCase ) == 0 ) || ( String.Compare( queryAction, "install,upgrade", StringComparison.OrdinalIgnoreCase ) == 0 ) )
									{
										match = true;
									}
									break;
								case SolutionInstallAction.Uninstall:
									if ( String.Compare( queryAction, "uninstall", StringComparison.OrdinalIgnoreCase ) == 0 )
									{
										match = true;
									}
									break;
							}

							if ( match )
							{
								string commandText = XmlHelper.ReadElementString( queryNode, "command", string.Empty );
								if ( !string.IsNullOrEmpty( commandText ) )
								{
									// Execute the script
									EventLog.Application.WriteTrace( "Executing database script '{0}'.", name );
									using ( IDbCommand command = databaseContext.CreateCommand( commandText ) )
									{
										command.ExecuteNonQuery( );
									}
								}
							}
						}
						catch ( Exception exception )
						{
							EventLog.Application.WriteError( "Unable to execute the specified database script (Name: {0}, Xml: {1}, Exception: {2}).", name, queryNode.OuterXml, exception.ToString( ) );
						}
					}
				}
			}

			DateTime endTime = DateTime.Now;
			TimeSpan timeTaken = endTime - startTime;
			EventLog.Application.WriteInformation( "End processing database scripts (End Time: {0}, Processing Time: {1})", endTime, timeTaken );
		}


		/// <summary>
		///		Processes solution nodes that point to entity config files.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="configPath">The configuration path.</param>
		private static void ProcessEntityNodes( XmlNodeList nodes, string configPath )
		{
			// We receive a list of nodes of the form:
			// <entities format="external" path="Bootstrap_Types.xml" />

			// Get the paths
			IEnumerable<string> paths =
				from XmlNode node in nodes
				select ResolvePath( node, configPath );

			EntityInstaller.ImportBootstrapEntityConfig( paths );
		}

		/// <summary>
		/// Processes the entity nodes for upgrade.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="configPath">The configuration path.</param>
		/// <param name="upgradeMap">The upgrade map.</param>
		private static void ProcessEntityNodesForUpgrade( XmlNodeList nodes, string configPath, IDictionary<string, Guid> upgradeMap )
		{
			// We receive a list of nodes of the form:
			// <entities format="external" path="Bootstrap_Types.xml" />

			// Get the paths
			IEnumerable<string> paths =
				from XmlNode node in nodes
				select ResolvePath( node, configPath );

			EntityInstaller.UpgradeBootstrapEntityConfig( paths, upgradeMap );
		}


		/// <summary>
		///		Processes solution nodes that point to upgrade map config files.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="configPath">The configuration path.</param>
		/// <remarks>
		///		The upgradeId should be used to correlate entities between distinct versions/installations of the platform.
		///		It maps to alias, within the current install, which is required on configured entities, which in turn can be mapped to Id.
		///		Note: this allows alias to be changed between versions, so long as they share the same upgra
		/// </remarks>
		private static void ProcessUpgradeMapNodes( XmlNodeList nodes, string configPath )
		{
			// We receive a list (i.e. 1) of nodes of the form:
			// <upgradeMap format="external" path="UpgradeMap.xml" />

			// The upgrade map files are of the form.
			// <upgradeMap>
			//   <entity alias="fullNamespace:alias" upgradeId="e2845b3a-32e3-4876-8af8-8b558f1548c3" />
			//   <entity...

			// Get the paths
			IEnumerable<string> paths =
				from XmlNode node in nodes
				let path = XmlHelper.ReadAttributeString( node, "@path" )
				select Path.Combine( configPath, path );

			using ( DatabaseContext databaseContext = DatabaseContext.GetContext( ) )
			{
				var table = new DataTable( );
				table.Columns.Add( new DataColumn( "Namespace", typeof ( string ) ) );
				table.Columns.Add( new DataColumn( "Alias", typeof( string ) ) );
				table.Columns.Add( new DataColumn( "UpgradeId", typeof( Guid ) ) );

				/////
				// Visit all map entries in all files
				/////
				IEnumerable<XElement> entityElems = from path in paths
					                                let xElement = XDocument.Load( path ).Root
					                                where xElement != null
					                                from entity in xElement.Elements( "entity" )
					                                select entity;

				/////
				// Insert into UpgradeMap table
				/////
				foreach ( XElement entityElem in entityElems )
				{
					string [ ] aliasParts = entityElem.Attribute( "alias" ).Value.Split( ':' );
					string upgradeId = entityElem.Attribute( "upgradeId" ).Value;

					DataRow row = table.NewRow( );

					row[ 0 ] = aliasParts[ 0 ];
					row [ 1 ] = aliasParts [ 1 ];
					row [ 2 ] = new Guid( upgradeId );

					table.Rows.Add( row );
				}

				if ( table.Rows.Count > 0 )
				{
					var connection = databaseContext.GetUnderlyingConnection( ) as SqlConnection;

					using ( var bulk = new SqlBulkCopy( connection ) )
					{
						bulk.DestinationTableName = "UpgradeMap";
						bulk.BatchSize = 10000;
						bulk.WriteToServer( table );
					}
				}
			}

			// Copy UpgradeIds into Entity table
			// TODO: Just insert them directly into here and ditch UpgradeMap completely
			const string sql2 = @"
				IF ( @context IS NOT NULL )
				BEGIN
					DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
					SET CONTEXT_INFO @contextInfo
				END

                update e
	                set UpgradeId = um.UpgradeId
	                from Entity e
	                join Data_Alias da on e.Id = da.EntityId and da.TenantId = 0
	                join UpgradeMap um on um.Alias = da.Data and um.Namespace = da.Namespace
	                where e.TenantId = 0";

			long userId;
			RequestContext.TryGetUserId( out userId );

			using ( DatabaseContextInfo.SetContextInfo( "Process upgrade map nodes" ) )
			using ( DatabaseContext databaseContext = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = databaseContext.CreateCommand( sql2 ) )
				{
					command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

					command.ExecuteNonQuery( );
				}
			}
		}

		/// <summary>
		/// Processes the upgrade map nodes for upgrade.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="configPath">The configuration path.</param>
		/// <returns></returns>
		private static IDictionary<string, Guid> ProcessUpgradeMapNodesForUpgrade( XmlNodeList nodes, string configPath )
		{
			// We receive a list (i.e. 1) of nodes of the form:
			// <upgradeMap format="external" path="UpgradeMap.xml" />

			// The upgrade map files are of the form.
			// <upgradeMap>
			//   <entity alias="fullNamespace:alias" upgradeId="e2845b3a-32e3-4876-8af8-8b558f1548c3" />
			//   <entity...

			// Get the paths
			IEnumerable<string> paths =
				from XmlNode node in nodes
				let path = XmlHelper.ReadAttributeString( node, "@path" )
				select Path.Combine( configPath, path );

			/////
			// Visit all map entries in all files
			/////
			IEnumerable<XElement> entityElements = from path in paths
												let xElement = XDocument.Load( path ).Root
												where xElement != null
												from entity in xElement.Elements( "entity" )
												select entity;

			IDictionary<string, Guid> upgradeMap = new Dictionary<string, Guid>( );

			/////
			// Insert into UpgradeMap table
			/////
			foreach ( XElement entityElem in entityElements )
			{
				var aliasAttribute = entityElem.Attribute( "alias" );

				if ( aliasAttribute != null )
				{
					string alias = aliasAttribute.Value;

					var upgradeAttribute = entityElem.Attribute( "upgradeId" );

					if ( upgradeAttribute != null )
					{
						string upgradeId = upgradeAttribute.Value;

						upgradeMap [ alias ] = new Guid( upgradeId );
					}
				}
			}

			return upgradeMap;
		}

		/// <summary>
		///     Resolve the first node matching the XPath expression.
		/// </summary>
		/// <param name="node">
		///     An object containing the database instruction set.
		/// </param>
		/// <param name="xpath">
		///     A string containing the XPath expression
		/// </param>
		/// <param name="configPath">
		///     A string containing the path to the configuration directory.
		/// </param>
		/// <returns>
		///     The first node matching the XPath query.
		/// </returns>
		private static XmlNode ResolveNode( XmlNode node, string xpath, string configPath )
		{
			XmlNodeList nodes = ResolveNodes( node, xpath, configPath );

			if ( ( nodes == null ) || ( nodes.Count <= 0 ) )
			{
				throw new ArgumentException( string.Format( "The XML node does not contain the specified node (xpath: {0})", xpath ) );
			}

			return nodes[ 0 ];
		}

		/// <summary>
		///     Resolve the list of nodes matching the XPath expression.
		/// </summary>
		/// <param name="node">
		///     An object containing the database instruction set.
		/// </param>
		/// <param name="xpath">
		///     A string containing the XPath expression
		/// </param>
		/// <param name="configPath">
		///     A string containing the path to the configuration directory.
		/// </param>
		/// <returns>
		///     A collection of nodes matching the XPath query.
		/// </returns>
		private static XmlNodeList ResolveNodes( XmlNode node, string xpath, string configPath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			if ( string.IsNullOrEmpty( configPath ) )
			{
				throw new ArgumentException( "The specified configPath parameter is invalid." );
			}

			bool external = false;

			// Check if the XML is internal or external
			string format = XmlHelper.ReadAttributeString( node, "@format", string.Empty );
			if ( String.Compare( format, "external", StringComparison.OrdinalIgnoreCase ) == 0 )
			{
				external = true;
			}

			XmlNodeList nodes;

			// Load the nodes
			if ( external )
			{
				string path = ResolvePath( node, configPath );

				// Load the XML file
				var document = new XmlDocument( );
				document.Load( path );

				// Select the list of nodes matching the XPath expression
				nodes = XmlHelper.SelectNodes( document.DocumentElement, xpath );
			}
			else
			{
				// Select the list of nodes matching the XPath expression
				nodes = XmlHelper.SelectNodes( node, xpath );
			}

			return nodes;
		}

		/// <summary>
		///     Resolve the text content referred to by the node.
		/// </summary>
		private static string ResolveText( XmlNode node, string configPath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			bool external = false;

			// Check if the XML is internal or external
			string format = XmlHelper.ReadAttributeString( node, "@format", string.Empty );
			if ( String.Compare( format, "external", StringComparison.OrdinalIgnoreCase ) == 0 )
			{
				external = true;
			}

			// Load the nodes
			if ( external )
			{
				var path = ResolvePath( node, configPath );

				using ( TextReader reader = new StreamReader( path ) )
				{
					return reader.ReadToEnd( );
				}
			}

			return node.InnerText;
		}

		/// <summary>
		/// Resolves the path.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="configPath">The configuration path.</param>
		/// <returns></returns>
		/// <exception cref="System.IO.FileNotFoundException">
		/// </exception>
		private static string ResolvePath( XmlNode node, string configPath )
		{
			string path = XmlHelper.ReadAttributeString( node, "@path" );
			string subPath = path;

			if ( !File.Exists( path ) )
			{
				path = Path.Combine( configPath, path );

				if ( !File.Exists( path ) )
				{
					string designTimeFolder = XmlHelper.ReadAttributeString( node, "@designTimeFolder", null );

					if ( !string.IsNullOrEmpty( designTimeFolder ) )
					{
						path = Path.Combine( configPath, designTimeFolder, subPath );

						if ( !File.Exists( path ) )
						{
							EventLog.Application.WriteError( "The specified config file does not exist (Path: {0}).", path );
							throw new FileNotFoundException( string.Format( "The specified config file does not exist (Path: {0}).", path ) );
						}
					}
					else
					{
						EventLog.Application.WriteError( "The specified config file does not exist (Path: {0}).", path );
						throw new FileNotFoundException( string.Format( "The specified config file does not exist (Path: {0}).", path ) );
					}
				}
			}
			return path;
		}

		/// <summary>
		///     Splits the SQL statements by 'GO' keyword.
		/// </summary>
		/// <param name="sql">The SQL.</param>
		/// <returns></returns>
		private static IEnumerable<string> SplitSqlStatements( string sql )
		{
			return sql.Split( new[]
				{
					"\r\nGO\r\n"
				}, StringSplitOptions.RemoveEmptyEntries );
		}
	}
}