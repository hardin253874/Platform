// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using EDC.Common;
using EDC.IO;
using EDC.ReadiNow.Common.ConfigParser;
using EDC.ReadiNow.Common.ConfigParser.Containers;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.Xml;
using Entity = EDC.ReadiNow.Common.ConfigParser.Containers.Entity;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using Relationship = EDC.ReadiNow.Common.ConfigParser.Containers.Relationship;

namespace EDC.SoftwarePlatform.Migration.Sources
{
	/// <summary>
	///     Represents an application package source that exists as config xml. (i.e. Core and Console)
	/// </summary>
	internal class XmlSource : IDataSource
	{
		/// <summary>
		///     Default is on type.
		/// </summary>
		private readonly Dictionary<Entity, List<FieldData>> _defaultsOnType = new Dictionary<Entity, List<FieldData>>( );

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     Alias resolver.
		/// </summary>
		private volatile IAliasResolver _aliasResolver;

		/// <summary>
		///     Binaries
		/// </summary>
		private volatile IList<KeyValuePair<string, string>> _binaries;

		/// <summary>
		///     Configuration folder.
		/// </summary>
		private volatile string _configFolder;

		/// <summary>
		///     Xml Document
		/// </summary>
		private volatile XmlDocument _document;

		/// <summary>
		///     Document element,
		/// </summary>
		private volatile XmlNode _documentElement;

		/// <summary>
		///     Entity list.
		/// </summary>
		private volatile IList<Entity> _entities;

		/// <summary>
		///     Field data.
		/// </summary>
		private volatile Dictionary<string, Dictionary<Entity, List<FieldData>>> _fieldData;

		/// <summary>
		///     Field type mapping.
		/// </summary>
		private volatile Dictionary<Entity, Func<string, object>> _fieldTypeInfo;

		/// <summary>
		///     Relationship list.
		/// </summary>
		private volatile IEnumerable<Relationship> _relationships;

		/// <summary>
		///     Schema resolver.
		/// </summary>
		private volatile ISchemaResolver _schema;

		/// <summary>
		///     Gets or sets the active solution.
		/// </summary>
		/// <value>
		///     The active solution.
		/// </value>
		public string ActiveSolution
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the alias resolver.
		/// </summary>
		/// <value>
		///     The alias resolver.
		/// </value>
		public IAliasResolver AliasResolver
		{
			get
			{
				if ( _aliasResolver == null )
				{
					lock ( _syncRoot )
					{
						if ( _aliasResolver == null )
						{
							_aliasResolver = new EntityStreamAliasResolver( Entities );
						}
					}
				}

				return _aliasResolver;
			}
		}

		/// <summary>
		///     Gets the available solutions.
		/// </summary>
		/// <value>
		///     The available solutions.
		/// </value>
		public IEnumerable<string> AvailableSolutions
		{
			get
			{
				if ( SolutionNode == null )
				{
					throw new ArgumentException( "Invalid SolutionNode." );
				}

                XmlNodeList nodes = XmlHelper.SelectNodes(SolutionNode, "configuration/solutions/solution/text()");
                var result = nodes.Cast<XmlText>().Select(textNode => textNode.Value).ToList();
                return result;
            }
		}

		/// <summary>
		///     Gets the binaries.
		/// </summary>
		/// <value>
		///     The binaries.
		/// </value>
		private IEnumerable<KeyValuePair<string, string>> Binaries
		{
			get
			{
				if ( _binaries == null )
				{
					lock ( _syncRoot )
					{
						if ( _binaries == null )
						{
							/////
							// Process the metadata entities (if specified)
							/////
							XmlNodeList entityNodes = XmlHelper.SelectNodes( SolutionNode, "configuration/metadata/entities" );

							if ( entityNodes.Count > 0 )
							{
								/////
								// Get the paths of the embedded xml files.
								/////
								IEnumerable<string> paths =
									from XmlNode node in entityNodes
									let path = XmlHelper.ReadAttributeString( node, "@path" )
									let subFolder = XmlHelper.ReadAttributeString( node, "@designTimeFolder", string.Empty )
									let fullPath = Path.Combine( Path.Combine( ConfigFolder, subFolder ), path )
									where File.Exists( fullPath )
									select fullPath;

								/////
								// Store the entities into a list so the enumeration does not occur more than once.
								/////
								IList<KeyValuePair<string, string>> binaryList = new List<KeyValuePair<string, string>>( XmlParser.ReadBinaries( paths ) );

								return _binaries = binaryList;
							}
						}
					}
				}

				return _binaries;
			}
		}

		/// <summary>
		///     Gets the configuration folder.
		/// </summary>
		/// <value>
		///     The configuration folder.
		/// </value>
		private string ConfigFolder
		{
			get
			{
				if ( _configFolder == null )
				{
					lock ( _syncRoot )
					{
						if ( _configFolder == null )
						{
							_configFolder = Path.GetDirectoryName( SolutionPath );
						}
					}
				}

				return _configFolder;
			}
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <value>
		///     The entities.
		/// </value>
		private IEnumerable<Entity> Entities
		{
			get
			{
				if ( _entities == null )
				{
					lock ( _syncRoot )
					{
						if ( _entities == null )
						{
							/////
							// Process the metadata entities (if specified)
							/////
							XmlNodeList entityNodes = XmlHelper.SelectNodes( SolutionNode, "configuration/metadata/entities" );

							if ( entityNodes.Count > 0 )
							{
								/////
								// Get the paths of the embedded xml files.
								/////
								IEnumerable<string> paths =
									from XmlNode node in entityNodes
									let path = XmlHelper.ReadAttributeString( node, "@path" )
									let subFolder = XmlHelper.ReadAttributeString( node, "@designTimeFolder", string.Empty )
									let fullPath = Path.Combine( Path.Combine( ConfigFolder, subFolder ), path )
									where File.Exists( fullPath )
									select fullPath;

								/////
								// Read out all of the entities.
								/////
								IEnumerable<Entity> entityStream = XmlParser.ReadEntities( paths, Enumerable.Repeat<Func<XElement, Entity, List<KeyValuePair<string, object>>>>( ExpandBinarySource, 1 ) );

								/////
								// Store the entities into a list so the enumeration does not occur more than once.
								/////
								IList<Entity> entityList = entityStream as IList<Entity> ?? entityStream.ToList( );

								/////
								// Add additional information to entities
								/////
								Decorator.DecorateEntities( entityList );

								/////
								// Apply the upgrade ids.
								/////
								ApplyUpgradeIds( entityList );

								return _entities = entityList;
							}
						}
					}
				}

				return _entities;
			}
		}

		/// <summary>
		///     Gets the field data.
		/// </summary>
		/// <value>
		///     The field data.
		/// </value>
		private Dictionary<string, Dictionary<Entity, List<FieldData>>> FieldData
		{
			get
			{
				if ( _fieldData == null )
				{
					lock ( _syncRoot )
					{
						if ( _fieldData == null )
						{
							var fieldData = new Dictionary<string, Dictionary<Entity, List<FieldData>>>( );

							/////
							// Comparer for equating two field data objects by their field.
							/////
							var comparer = new CastingComparer<FieldData, Entity>( fd => fd.Field );

							/////
							// Visit each entity
							/////
							foreach ( Entity entity in Entities )
							{
								/////
								// Combine declared values with defaults
								/////
								IEnumerable<FieldData> definedFields = SchemaResolver.GetAllEntityFields( entity );
								IEnumerable<FieldData> defaultFields = GetFieldsWithDefaults( AliasResolver[ entity.Type ] );
								IList<FieldData> fieldDatas = definedFields as IList<FieldData> ?? definedFields.ToList( );
								IEnumerable<FieldData> fieldsToSave = fieldDatas.Concat( defaultFields.Except( fieldDatas, comparer ) );

								// Process each field
								foreach ( FieldData fieldDataValue in fieldsToSave )
								{
									Func<string, object> fieldType = FieldTypeInfo[ AliasResolver[ fieldDataValue.Field.Type ] ];

									fieldType( fieldDataValue.Value );

									Member dbFieldType = fieldDataValue.Field.Type.Entity.Members.FirstOrDefault( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "dbFieldTable" );

									if ( dbFieldType != null )
									{
										Dictionary<Entity, List<FieldData>> dict;

										string type = ConvertDbFieldTableToFieldDataTable( dbFieldType.Value );

										/////
										// Find the dictionary entry for this type.
										/////
										if ( !fieldData.TryGetValue( type, out dict ) )
										{
											/////
											// Create a new dictionary entry for this type.
											/////
											dict = new Dictionary<Entity, List<FieldData>>( );
											fieldData[ type ] = dict;
										}

										List<FieldData> list;

										/////
										// Find the entity that this field belongs to.
										/////
										if ( !dict.TryGetValue( entity, out list ) )
										{
											/////
											// Create a new list of fields for this entity.
											/////
											list = new List<FieldData>( );
											dict[ entity ] = list;
										}

										list.Add( fieldDataValue );
									}
								}
							}

							_fieldData = fieldData;
						}
					}
				}

				return _fieldData;
			}
		}

		/// <summary>
		///     Gets the field type info.
		/// </summary>
		/// <value>
		///     The field type info.
		/// </value>
		private Dictionary<Entity, Func<string, object>> FieldTypeInfo
		{
			get
			{
				if ( _fieldTypeInfo == null )
				{
					lock ( _syncRoot )
					{
						if ( _fieldTypeInfo == null )
						{
							/////
							// Create dictionary to hold per-field-type info
							/////
							var fieldTypeInfo = new Dictionary<Entity, Func<string, object>>( );

							/////
							// Some aliases we'll need
							/////
							Aliases.CoreAlias( "dbFieldTable" );
							Aliases.CoreAlias( "dbType" );
							Entity rootFieldType = AliasResolver[ Aliases.Field ];

							/////
							// Get the entities for all field types
							/////
							IEnumerable<Entity> fieldTypes = SchemaResolver.GetInstancesOfType( AliasResolver[ Aliases.FieldType ] );

							/////
							// Process each field type
							/////
							foreach ( Entity fieldType in fieldTypes )
							{
								/////
								// Except the abstract root type
								/////
								if ( fieldType == rootFieldType )
								{
									continue;
								}

								/////
								// Create type-specific XML parser
								/////
								Func<string, object> parser = GetFieldTypeParser( fieldType );

								fieldTypeInfo[ fieldType ] = parser;
							}

							_fieldTypeInfo = fieldTypeInfo;
						}
					}
				}

				return _fieldTypeInfo;
			}
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <value>
		///     The relationships.
		/// </value>
		private IEnumerable<Relationship> Relationships
		{
			get
			{
				if ( _relationships == null )
				{
					lock ( _syncRoot )
					{
						if ( _relationships == null )
						{
							IEnumerable<Relationship> relationships = SchemaResolver.GetAllRelationships( );

							IList<Relationship> relationshipList = relationships as IList<Relationship> ?? relationships.ToList( );

							return _relationships = relationshipList;
						}
					}
				}

				return _relationships;
			}
		}

		/// <summary>
		///     Gets the schema resolver.
		/// </summary>
		/// <value>
		///     The schema resolver.
		/// </value>
		public ISchemaResolver SchemaResolver
		{
			get
			{
				if ( _schema == null )
				{
					lock ( _syncRoot )
					{
						if ( _schema == null )
						{
							var schema = new SchemaResolver( Entities, AliasResolver );
							schema.Initialize( );

							_schema = schema;
						}
					}
				}

				return _schema;
			}
		}

		/// <summary>
		///     Gets the solution document.
		/// </summary>
		/// <value>
		///     The solution document.
		/// </value>
		/// <exception cref="System.ArgumentException">The specified path parameter is null.</exception>
		/// <exception cref="System.IO.FileNotFoundException"></exception>
		private XmlDocument SolutionDocument
		{
			get
			{
				if ( _document == null )
				{
					lock ( _syncRoot )
					{
						if ( _document == null )
						{
							if ( String.IsNullOrEmpty( SolutionPath ) )
							{
								throw new ArgumentException( "The specified path parameter is null." );
							}

							/////
							// Ensure that the solution path exists.
							/////
							if ( !File.Exists( SolutionPath ) )
							{
								throw new FileNotFoundException( $"The specified solution configuration file does not exist (Path: {SolutionPath}.)" );
							}

							var doc = new XmlDocument( );
							doc.Load( SolutionPath );

							return _document = doc;
						}
					}
				}

				return _document;
			}
		}

		/// <summary>
		///     Gets the solution node.
		/// </summary>
		/// <value>
		///     The solution node.
		/// </value>
		private XmlNode SolutionNode
		{
			get
			{
				if ( _documentElement == null )
				{
					lock ( _syncRoot )
					{
						if ( _documentElement == null )
						{
							_documentElement = SolutionDocument.SelectSingleNode( "/resource" );
						}
					}
				}

				return _documentElement;
			}
		}

		/// <summary>
		///     Gets or sets the solution path.
		/// </summary>
		/// <value>
		///     The solution path.
		/// </value>
		public string SolutionPath
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the binary data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<BinaryDataEntry> GetBinaryData( IProcessingContext context )
		{
            var tokenProvider = new Sha256FileTokenProvider();

			/////
			// Get the binaries that belong to the active solution and convert them into the BinaryDataEntry format.
			/////
			return Binaries.Where( pair => pair.Key == ActiveSolution ).Select(pair =>
			{
                string hash;

			    using (var stream = File.OpenRead(pair.Value))
			    {
			        hash = tokenProvider.ComputeToken(stream);
			    }

			    return new BinaryDataEntry
			    {
			        Data = File.ReadAllBytes(pair.Value),
			        DataHash = hash
                };
			});
		}

		public IEnumerable<DocumentDataEntry> GetDocumentData( IProcessingContext context )
		{
			return Enumerable.Empty<DocumentDataEntry>( );
		}

		/// <summary>
		///     Load entities.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public IEnumerable<EntityEntry> GetEntities( IProcessingContext context )
		{
			if ( string.IsNullOrEmpty( ActiveSolution ) )
			{
				throw new InvalidOperationException( "No 'ActiveSolution' has been specified." );
			}

			/////
			// Get all entities.
			/////
			IEnumerable<Entity> allEntities = Entities.Concat( SchemaResolver.GetImpliedRelationshipEntites( ) );

			/////
			// Get the entities that belong to the active solution.
			/////
			IEnumerable<Entity> activeSolutionEntities = allEntities.Where( e => e.Members.Any( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "inSolution" && m.Value == ActiveSolution ) )
				.Where( e => !e.Members.Any( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "systemTenantOnly" ) );

			IList<Entity> solutionEntities = activeSolutionEntities as IList<Entity> ?? activeSolutionEntities.ToList( );
			IEnumerable<Entity> enumerable = solutionEntities.Where( e => e.Type?.Entity != null && e.Type.Entity.Members.Any( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "systemTenantOnly" ) );

			/////
			// Remove the entities whose type is system tenant only.
			/////
			IEnumerable<Entity> except = solutionEntities.Except( enumerable );

			return except.Select( e => new EntityEntry
			{
				EntityId = e.Guid,
				State = DataState.Added
			} );
		}

		/// <summary>
		///     Load field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that bits are represented as Booleans
		///     - ensure that XML is transformed so that entityRefs contain Upgrade ids
		///     - or where XML contains an alias, translate it to uprgadeId|alias   (as the alias may be changed in the target)
		///     - ensure that aliases export their namespace and direction marker.
		/// </remarks>
		public IEnumerable<DataEntry> GetFieldData( string dataTable, IProcessingContext context )
		{
			if ( !FieldData.ContainsKey( dataTable ) )
			{
				return Enumerable.Empty<DataEntry>( );
			}

			Dictionary<Entity, List<FieldData>> typedValues = FieldData[ dataTable ];

			if ( dataTable == "Alias" )
			{
				Alias aliasMarkerIdField = Aliases.CoreAlias( "aliasMarkerId" );

				return ( from pair in typedValues
					from field in pair.Value
					let alias = SchemaResolver.GetAliasFieldValue( pair.Key, field.Field.Alias )
					let intFieldValue = SchemaResolver.GetIntFieldValue( field.Field, aliasMarkerIdField )
					where intFieldValue != null
					where pair.Key.Members != null && pair.Key.Members.Any( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "inSolution" && m.Value == ActiveSolution )
					where pair.Key.Members != null && !pair.Key.Members.Any( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "systemTenantOnly" )
					let markerId = intFieldValue.Value
					select new DataEntry
					{
						AliasMarkerId = markerId,
						Data = alias.Value,
						EntityId = pair.Key.Guid,
						FieldId = field.Field.Guid,
						Namespace = alias.Namespace,
						State = DataState.Added
					} );
			}

			/////
			// Non-Alias
			/////
			return ( from pair in typedValues
				from field in pair.Value
				where pair.Key.Members != null && pair.Key.Members.Any( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "inSolution" && m.Value == ActiveSolution )
				where pair.Key.Members != null && !pair.Key.Members.Any( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "systemTenantOnly" )
				select new DataEntry
				{
					Data = FormatData( field ),
					EntityId = pair.Key.Guid,
					FieldId = field.Field.Guid,
					State = DataState.Added
				} );
		}

		/// <summary>
		///     Loads the application metadata.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public Metadata GetMetadata( IProcessingContext context )
		{
			Entity solutionEntity = GetActiveSolutionEntity( );

			string name = null;
			string description = null;
			string publisher = null;
			string publisherUrl = null;
			Guid appId = Guid.Empty;
			DateTime releaseDate = DateTime.UtcNow;
			string version = null;

			if ( solutionEntity != null )
			{
				/////
				// Get the member values.
				/////
				name = GetMemberValue( solutionEntity, "core", "name" );
				description = GetMemberValue( solutionEntity, "core", "description" );
				publisher = GetMemberValue( solutionEntity, "core", "solutionPublisher" );
				publisherUrl = GetMemberValue( solutionEntity, "core", "solutionPublisherUrl" );
				version = GetMemberValue( solutionEntity, "core", "solutionVersionString" );

				string releaseDateString = GetMemberValue( solutionEntity, "core", "solutionReleaseDate" );

				if ( !string.IsNullOrEmpty( releaseDateString ) )
				{
					releaseDate = DateTime.Parse( releaseDateString );
				}

				appId = solutionEntity.Guid;
			}

			/////
			// Calculate the fallback values.
			/////
			if ( string.IsNullOrEmpty( name ) )
			{
				name = SolutionDocument.ReadString( "/resource/name" );
			}

			if ( appId == Guid.Empty )
			{
				appId = SolutionDocument.ReadGuid( "/resource/solutionId" );
			}

			if ( string.IsNullOrEmpty( version ) )
			{
				version = SolutionDocument.ReadString( "/resource/version", "1.0" );
			}

			Guid dependentApplicationGuid = new Guid( "{8f2ed791-a613-4e9c-a6d4-694a8c56dba1}" );
			Guid dependencyApplicationGuid = new Guid( "{d7fe48a6-3ad9-494d-8740-086f432aaaa4}" );
			Guid nameGuid = new Guid( "{f8def406-90a1-4580-94f4-1b08beac87af}" );
			Guid minimumVersionGuid = new Guid( "{f2c90c9e-92a0-4461-a49c-ee601776f468}" );
			Guid maximumVersionnameGuid = new Guid( "{f00f048e-6aab-479e-9981-19a7506c20db}" );
			Guid isRequiredGuid = new Guid( "{1f7dd780-a123-4ec6-9722-5189d107eb8f}" );

			List<SolutionDependency> dependencies = new List<SolutionDependency>( );

			List <Relationship> dependentRelationships = Relationships.Where( r => r.SolutionEntity.Guid == appId && r.Type.Guid == dependentApplicationGuid && r.To.Guid == appId ).ToList( );

			foreach ( Relationship relationship in dependentRelationships )
			{
				List<Relationship> dependencyRelationships = Relationships.Where( r => r.SolutionEntity.Guid == appId && r.Type.Guid == dependencyApplicationGuid && r.From.Guid == relationship.From.Guid ).ToList( );

				foreach ( Relationship dependencyRelationship in dependencyRelationships )
				{
					Entity details = Entities.FirstOrDefault( e => e.Guid == dependencyRelationship.From.Guid );

					Member nameMember = details.Members.FirstOrDefault( m => m.MemberDefinition.Entity.Guid == nameGuid );
					Member minimumVersionMember = details.Members.FirstOrDefault( m => m.MemberDefinition.Entity.Guid == minimumVersionGuid );
					Member maximumVersionMember = details.Members.FirstOrDefault( m => m.MemberDefinition.Entity.Guid == maximumVersionnameGuid );
					Member isRequiredMember = details.Members.FirstOrDefault( m => m.MemberDefinition.Entity.Guid == isRequiredGuid );

					Entity dependencyApplication = Entities.FirstOrDefault( e => e.Guid == dependencyRelationship.To.Guid );

					Member dependencyNameMember = null;

					if ( dependencyApplication != null )
					{
						dependencyNameMember = dependencyApplication.Members.FirstOrDefault( m => m.MemberDefinition.Entity.Guid == nameGuid );
					}

					Version minVersion;

					if ( string.IsNullOrEmpty( minimumVersionMember?.Value ) || minimumVersionMember.Value.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !Version.TryParse( minimumVersionMember.Value, out minVersion ) )
					{
						minVersion = null;
					}

					Version maxVersion;

					if ( string.IsNullOrEmpty( maximumVersionMember?.Value ) || maximumVersionMember.Value.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !Version.TryParse( maximumVersionMember.Value, out maxVersion ) )
					{
						maxVersion = null;
					}

					bool required;

					if ( string.IsNullOrEmpty( isRequiredMember?.Value ) || !bool.TryParse( isRequiredMember.Value, out required ) )
					{
						required = true;
					}

					SolutionDependency dependency = new SolutionDependency( dependencyRelationship.To.Guid, nameMember?.Value, dependencyNameMember?.Value, minVersion, maxVersion, required );

					dependencies.Add( dependency );
				}
			}

			/////
			// Return the metadata.
			/////
			return new Metadata
			{
				AppId = appId,
				AppName = name,
				AppVerId = GenerateAppVerId( name, version ),
				Description = description,
				Dependencies = dependencies,
				Name = name,
				Publisher = publisher,
				PublisherUrl = publisherUrl,
				ReleaseDate = releaseDate,
				Version = version,
				Type = SourceType.AppPackage,
				PlatformVersion = null
			};
		}


		/// <summary>
		///     Load relationships.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public IEnumerable<RelationshipEntry> GetRelationships( IProcessingContext context )
		{
			if ( string.IsNullOrEmpty( ActiveSolution ) )
			{
				throw new InvalidOperationException( "No 'ActiveSolution' has been specified." );
			}

			IEnumerable<Relationship> relationships = Relationships;

			IEnumerable<Relationship> activeSolutionRelationships = relationships.Where( r => r.SolutionEntity.Alias.Namespace == "core" && r.SolutionEntity.Alias.Value == ActiveSolution ).DropSystemTenantOnlyRelationships( );

			return activeSolutionRelationships.Select( r => new RelationshipEntry( r.Type.Guid, r.From.Guid, r.To.Guid ) );
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			_document = null;
		}

		/// <summary>
		///     Sets up this instance.
		/// </summary>
		public void Setup( IProcessingContext context )
		{
		}

		/// <summary>
		///     Tears down this instance.
		/// </summary>
		public void TearDown( IProcessingContext context )
		{
		}

		/// <summary>
		///     Gets the missing field data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<DataEntry> GetMissingFieldData( IProcessingContext context )
		{
			return Enumerable.Empty<DataEntry>( );
		}

		/// <summary>
		///     Gets the missing relationships.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<RelationshipEntry> GetMissingRelationships( IProcessingContext context )
		{
			return Enumerable.Empty<RelationshipEntry>( );
		}

        /// <summary>
        ///     Return empty set of SecureData
        /// </summary>
        public IEnumerable<SecureDataEntry> GetSecureData(IProcessingContext context)
        {
            return Enumerable.Empty<SecureDataEntry>();
        }

        /// <summary>
        /// Gets the entities that should not be removed as part of an upgrade operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public IEnumerable<Guid> GetDoNotRemove( IProcessingContext context )
        {
            return Enumerable.Empty<Guid>( );
        }

        /// <summary>
        ///     Apply upgrade ids to each entity.
        /// </summary>
        /// <param name="entityList">The entity list.</param>
        private void ApplyUpgradeIds( IEnumerable<Entity> entityList )
		{
			if ( entityList == null )
			{
				return;
			}

			/////
			// Determine if the upgrade map element exists.
			/////
			XmlNode upgradeMapNode = XmlHelper.SelectSingleNode( SolutionNode, "configuration/metadata/upgradeMap" );

			if ( upgradeMapNode != null )
			{
				/////
				// Get the upgrade maps path.
				/////
				string upgradeMapPath = XmlHelper.ReadAttributeString( upgradeMapNode, "@path", null );

				if ( !string.IsNullOrEmpty( upgradeMapPath ) )
				{
					string fullUpgradeMapPath = Path.Combine( ConfigFolder, upgradeMapPath );

					/////
					// Ensure that the upgrade map file exists.
					/////
					if ( !File.Exists( fullUpgradeMapPath ) )
					{
						return;
					}

					var upgradeMapDoc = new XmlDocument( );
					upgradeMapDoc.Load( fullUpgradeMapPath );

					var map = new Dictionary<string, Guid>( );

					/////
					// Obtain all the entities from the upgrade map.
					/////
					XmlNodeList upgradeEntityNodes = upgradeMapDoc.SelectNodes( "upgradeMap/entity" );

					/////
					// Process each entity node into an anonymous type that holds the Alias and UpgradeId.
					/////
					var upgradeEntities =
						from XmlNode node in upgradeEntityNodes
						let alias = XmlHelper.ReadAttributeString( node, "@alias" )
						let guid = XmlHelper.ReadAttributeGuid( node, "@upgradeId" )
						select new
						{
							Alias = alias,
							UpgradeId = guid
						};

					/////
					// Store each object into a dictionary for fast lookup.
					/////
					foreach ( var upgradeEntity in upgradeEntities )
					{
						map[ upgradeEntity.Alias ] = upgradeEntity.UpgradeId;
					}

					/////
					// Loop through the entities looking for their upgrade id and apply it to the entity if found.
					/////
					foreach ( Entity entity in entityList )
					{
						if ( entity.Alias != null && entity.Alias.Value != null )
						{
							Guid upgradeId;

							if ( map.TryGetValue( entity.Alias.Namespace == null ? entity.Alias.Value : $"{entity.Alias.Namespace}:{entity.Alias.Value}", out upgradeId ) )
							{
								/////
								// Found the upgrade id so apply it to the entity.
								/////
								entity.Guid = upgradeId;
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Converts the db field table to field data table.
		/// </summary>
		/// <param name="dbFieldTable">The db field table.</param>
		/// <returns></returns>
		private string ConvertDbFieldTableToFieldDataTable( string dbFieldTable )
		{
			return dbFieldTable.Replace( "Data_", string.Empty );
		}

		/// <summary>
		///     Expands the binary source.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="entity">The entity.</param>
		private List<KeyValuePair<string, object>> ExpandBinarySource( XElement element, Entity entity )
		{
			if ( element == null || entity == null )
			{
				return null;
			}

			XAttribute sourceAttribute = element.Attribute( "source" );

			if ( sourceAttribute != null )
			{
				try
				{
					// Find the entity source formatter for this alias                    
					string typeName = string.Format( CultureInfo.InvariantCulture, "EDC.SoftwarePlatform.Migration.SourceFormatters.{0}SourceFormatter", entity.Type.Alias.Value.ToPascalCase( ) );
					Type type = Type.GetType( typeName, false );

					if ( type != null )
					{
						object instance = Activator.CreateInstance( type );

						var results = new List<KeyValuePair<string, object>>( );

						var typeFormatter = instance as IEntityContainerSourceFormatter;

						if ( typeFormatter != null )
						{
							string referencePath = Path.GetDirectoryName( element.BaseUri );

							if ( referencePath != null )
							{
								var uri = new Uri( Path.Combine( referencePath, sourceAttribute.Value ) );

								results.AddRange( typeFormatter.Format( Path.GetFullPath( uri.AbsolutePath ) ) );
							}
						}

						return results;
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteError( "Failed to expand binary source. " + exc.Message );
				}
			}

			return null;
		}

		/// <summary>
		///     Formats the data.
		/// </summary>
		/// <param name="fieldData">The field data.</param>
		/// <returns></returns>
		private static object FormatData( FieldData fieldData )
		{
			if ( fieldData?.Value == null )
			{
				return null;
			}

			if ( fieldData.Field?.Type != null && fieldData.Field.Type.Alias != null )
			{
				Alias typeAlias = fieldData.Field.Type.Alias;

				if ( typeAlias.Namespace == "core" )
				{
					if ( typeAlias.Value == "timeField" )
					{
						/////
						// Time fields must be set the 1753-01-01T
						/////
						return SqlDateTime.MinValue.Value.AddTicks( TimeSpan.Parse( fieldData.Value ).Ticks );
					}
					if ( typeAlias.Value == "dateTimeField" )
					{
						/////
						// Ensure that DateTime fields are exported in universal time.
						/////
						return DateTime.Parse( fieldData.Value ).ToUniversalTime( );
					}
				}
			}

			return fieldData.Value;
		}

		/// <summary>
		///     Generates the applications version id. This is composed of the hash from the xml file along with the applications
		///     name
		/// </summary>
		/// <returns></returns>
		private Guid GenerateAppVerId( string name, string version )
		{
			if ( name == null )
			{
				name = string.Empty;
			}

			if ( version == null )
			{
				version = string.Empty;
			}

			/////
			// Get the hash from the xml file.
			/////
			string xmlHash = SolutionDocument.ReadString( "/resource/@hash", null );

			/////
			// If there is no hash, simply return a brand new application version id.
			/////
			if ( xmlHash == null )
			{
				return Guid.NewGuid( );
			}

			/////
			// Specify the string as a concatenation of the xml hash-code, the solution name and the solution version.
			/////
			string hashSource = $"{xmlHash}_{name}_{version}";

			MD5 md5 = MD5.Create( );

			/////
			// Use the hash string as the source for the GUID.
			/////
			return new Guid( md5.ComputeHash( Encoding.Default.GetBytes( hashSource ) ) );
		}

		/// <summary>
		///     Gets the active solution entity.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">No 'ActiveSolution' has been specified.</exception>
		private Entity GetActiveSolutionEntity( )
		{
			if ( string.IsNullOrEmpty( ActiveSolution ) )
			{
				throw new InvalidOperationException( "No 'ActiveSolution' has been specified." );
			}

			return Entities.FirstOrDefault( e => e.Alias.Namespace == "core" && e.Alias.Value == ActiveSolution );
		}

		/// <summary>
		///     Gets the field type parser.
		/// </summary>
		/// <param name="fieldType">Type of the field.</param>
		/// <returns></returns>
		private static Func<string, object> GetFieldTypeParser( Entity fieldType )
		{
			// Note: can't use entity model during bootstrap

			switch ( fieldType.Alias.Value )
			{
				case "aliasField":
				case "stringField":
				case "emailField":
				case "uriField":
				case "phoneField":
				case "xmlField":
					return s => ( object ) s;
				case "dateField":
					// No UTC adjustment
					return s => ( object ) DateTime.Parse( s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal );
				case "timeField":
					// No UTC adjustment
					return s => ( object ) DateTime.Parse( s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal );
				case "dateTimeField":
					// yyyy-mm-ddTHH:MM:SS to specify input as local time, or use yyyy-mm-ddTHH:MM:SSZ to specify input at UTC.
					return s => ( object ) DateTime.Parse( s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal );
				case "intField":
					return s => ( object ) int.Parse( s );
				case "boolField":
					return s => ( object ) bool.Parse( s );
				case "guidField":
					return s => ( object ) Guid.Parse( s );
				case "decimalField":
				case "percentField":
				case "currencyField":
					return s => ( object ) decimal.Parse( s );
				default:
					string fieldName = fieldType.Alias.Value;
					return s =>
					{
						throw new Exception( "No bootstrap field parser defined for field type: " + fieldName );
					};
			}
		}

		/// <summary>
		///     Gets the fields with defaults.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		private IEnumerable<FieldData> GetFieldsWithDefaults( Entity type )
		{
			List<FieldData> result;

			if ( !_defaultsOnType.TryGetValue( type, out result ) )
			{
				Alias defaultValueAlias = Aliases.CoreAlias( "defaultValue" );

				result =
					( from ancType in SchemaResolver.GetAncestors( type )
						from field in SchemaResolver.GetDeclaredFields( ancType )
						let defaultValue = SchemaResolver.GetStringFieldValue( field, defaultValueAlias )
						where defaultValue != null
						select new FieldData
						{
							Field = field,
							Value = defaultValue
						} ).ToList( );

				_defaultsOnType[ type ] = result;
			}
			return result;
		}

		/// <summary>
		///     Gets the member value.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="nameSpace">The name space.</param>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		private string GetMemberValue( Entity entity, string nameSpace, string alias )
		{
			/////
			// Return the default value if any of the input arguments are incorrect.
			/////
			if ( entity != null && !string.IsNullOrEmpty( nameSpace ) && !string.IsNullOrEmpty( alias ) )
			{
				Member member = entity.Members.FirstOrDefault( m => m.MemberDefinition.Alias.Namespace == nameSpace && m.MemberDefinition.Alias.Value == alias );

				if ( member != null )
				{
					return member.Value;
				}
			}

			return null;
		}

		/// <summary>
		///     Sets the solution version.
		/// </summary>
		/// <param name="version">The version.</param>
		public void SetSolutionVersions( string version )
		{
			if ( string.IsNullOrEmpty( version ) || AvailableSolutions == null || !AvailableSolutions.Any( ) )
			{
				return;
			}

			foreach ( string solutionAlias in AvailableSolutions )
			{
				Entity solution = Entities.FirstOrDefault( e => e.Alias.Value == solutionAlias && e.Type.Alias.Namespace == "core" && e.Type.Alias.Value == "solution" );

				Member solutionVersionString = solution?.Members.FirstOrDefault( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "solutionVersionString" );

				if ( solutionVersionString != null )
				{
					solutionVersionString.Value = version;
				}
			}
		}
	}
}