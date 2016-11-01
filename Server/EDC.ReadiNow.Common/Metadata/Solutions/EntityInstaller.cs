// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EDC.Common;
using EDC.ReadiNow.Common.ConfigParser;
using EDC.ReadiNow.Common.ConfigParser.Containers;
using EDC.ReadiNow.Database;
using System.Globalization;
using EDC.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using EDC.ReadiNow.Messaging;
using Entity = EDC.ReadiNow.Common.ConfigParser.Containers.Entity;
using EntityRef = EDC.ReadiNow.Common.ConfigParser.Containers.EntityRef;
using Relationship = EDC.ReadiNow.Common.ConfigParser.Containers.Relationship;

namespace EDC.ReadiNow.Metadata.Solutions
{
	/// <summary>
	/// Installs entity configuration xml into the database.
	/// </summary>
	internal class EntityInstaller
	{
		private IAliasResolver _aliasResolver;
		private DatabaseContext _dbContext;
		private Dictionary<Entity, List<FieldData>> _defaultsOnType = new Dictionary<Entity, List<FieldData>>( );
		private IEnumerable<Entity> _entities;
		private Dictionary<Entity, long> _entityToId;
		private Dictionary<Entity, FieldType> _fieldTypeInfo;
		private ISchemaResolver _schema;
		private long _tenantId = 0;
		private Alias AliasFieldAlias = Aliases.CoreAlias( "aliasField" );
		private Alias StringFieldAlias = Aliases.CoreAlias( "stringField" );

		private EntityInstaller( )
		{
		}

		/// <summary>
		/// Imports a collection of entity configuration files.
		/// </summary>
		/// <param name="paths">Full paths to the configuration files.</param>
		public static void ImportBootstrapEntityConfig( IEnumerable<string> paths )
		{
			// Log entry
			EventLog.Application.WriteInformation( "Importing entity configurations:\r\n" + string.Join( "\r\n", paths ) );

			// Start parsing the entities
			IEnumerable<Entity> entities = XmlParser.ReadEntities( paths ).ToList();
            Decorator.DecorateEntities(entities, s => EventLog.Application.WriteTrace(s));

			// And insert into database
			var entityInstaller = new EntityInstaller( );
			entityInstaller.ImportBootstrapEntityConfig( entities );

			EventLog.Application.WriteTrace( "Completed imports entity configurations:\r\n" + string.Join( "\r\n", paths ) );
		}

		/// <summary>
		/// Upgrades the bootstrap entity configuration.
		/// </summary>
		/// <param name="paths">The paths.</param>
		/// <param name="upgradeMap">The upgrade map.</param>
		public static void UpgradeBootstrapEntityConfig( IEnumerable<string> paths, IDictionary<string, Guid> upgradeMap )
		{
			// Log entry
			var pathList = paths as IList<string> ?? paths.ToList( );

			EventLog.Application.WriteInformation( "Upgrading entity configurations:\r\n" + string.Join( "\r\n", pathList ) );

			// Start parsing the entities
			IEnumerable<Entity> entities = XmlParser.ReadEntities( pathList ).ToList( );
			Decorator.DecorateEntities( entities, s => EventLog.Application.WriteTrace( s ) );

			// And insert into database
			var entityInstaller = new EntityInstaller( );
			entityInstaller.UpgradeBootstrapEntityConfig( entities, upgradeMap );

			EventLog.Application.WriteTrace( "Completed upgrade of entity configurations:\r\n" + string.Join( "\r\n", pathList ) );
		}

		/// <summary>
		/// Imports a stream of configuration XML entities into the database.
		/// </summary>
		/// <param name="entities">The entities.</param>
		public void ImportBootstrapEntityConfig( IEnumerable<Entity> entities )
		{
			_entities = entities;
			_aliasResolver = new EntityStreamAliasResolver( entities );
			_schema = new SchemaResolver( entities, _aliasResolver );
			_schema.Initialize( );
			_entityToId = new Dictionary<Entity, long>( );

			using ( _dbContext = DatabaseContext.GetContext( ) )
			{
				SaveEntities( );
				SaveRelationships( );
				FindFieldTypes( );
				SaveFieldData( );
				Cleanup( );
			}            

			// From this point on we're ready to use the model
            GenerateResourceDataKeyHashes(0);
		}

		/// <summary>
		/// Upgrades the bootstrap entity configuration.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="upgradeMap">The upgrade map.</param>
		public void UpgradeBootstrapEntityConfig( IEnumerable<Entity> entities, IDictionary<string, Guid> upgradeMap )
		{
			_entities = entities;
			_aliasResolver = new EntityStreamAliasResolver( entities );
			_schema = new SchemaResolver( entities, _aliasResolver );
			_schema.Initialize( );
			_entityToId = new Dictionary<Entity, long>( );

			using ( _dbContext = DatabaseContext.GetContext( ) )
			{
				UpgradeEntities( upgradeMap );
				UpgradeRelationships( );
				FindFieldTypes( );
				UpgradeFieldData( );
				Cleanup( );
			}
		}

		/// <summary>
		/// Creates a parser delegate that can process an XML string value and return the appropriate object.
		/// </summary>
		/// <param name="fieldType">Entity that represents the field type.</param>
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
					return ( s ) => ( object ) s;
				case "dateField":
                    // No UTC adjustment
                    return (s) => (object)DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                case "timeField":
                    // No UTC adjustment
                    return (s) =>
                        {
                            DateTime parsed = DateTime.Parse(s, CultureInfo.InvariantCulture,
                                               DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                            DateTime result = System.Data.SqlTypes.SqlDateTime.MinValue.Value + parsed.TimeOfDay;
                            return (object)result;
                        };
                case "dateTimeField":
                    // yyyy-mm-ddTHH:MM:SS to specify input as localtime, or use yyyy-mm-ddTHH:MM:SSZ to specify input at UTC.
                    return (s) => (object)DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal);
                case "intField":
					return ( s ) => ( object ) int.Parse( s );
				case "boolField":
					return ( s ) => ( object ) bool.Parse( s );
				case "guidField":
					return ( s ) => ( object ) Guid.Parse( s );
                case "decimalField":
                case "percentField":
                case "currencyField":
                    return ( s ) => ( object ) decimal.Parse(s);
				default:
					string fieldName = fieldType.Alias.Value;
					return ( s ) =>
					{
						throw new Exception( "No bootstrap field parser defined for field type: " + fieldName );
					};
			}
		}

		private string DebugInfo( Entity entity )
		{
			if ( entity == null )
				return "<null>";
			if ( entity.Alias != null )
				return entity.Alias.Value;
			return "<anon>";
		}

		/// <summary>
		/// Find all field types and create database tables and objects
		/// </summary>
		private void FindFieldTypes( )
		{
			// Create dictionary to hold per-field-type info
			_fieldTypeInfo = new Dictionary<Entity, FieldType>( );

			// Some aliases we'll need
			Alias dbFieldTableField = Aliases.CoreAlias( "dbFieldTable" );
			Alias dbTypeField = Aliases.CoreAlias( "dbType" );
			//Alias dbTypeFullField = Aliases.CoreAlias("dbTypeFull");
			Entity rootFieldType = A( Aliases.Field );

			// Get the entities for all field types
			var fieldTypes =
				_schema.GetInstancesOfType( A( Aliases.FieldType ) );

			// Process each field type
			foreach ( Entity fieldType in fieldTypes )
			{
				// Except the abstract root type
				if ( fieldType == rootFieldType )
					continue;

				// Get the db type details
				string dbFieldTable = _schema.GetStringFieldValue( fieldType, dbFieldTableField );
				string dbType = _schema.GetStringFieldValue( fieldType, dbTypeField );

				// Create type-specific XML parser
				Func<string, object> parser = GetFieldTypeParser( fieldType );


				Func<Assembly, string, bool, Type> typeResolver = ( assembly, name, ignoreCase ) =>
				{
					if ( string.IsNullOrEmpty( name ) )
					{
						return null;
					}

					Type type = Type.GetType( name, false, ignoreCase );

					if ( type != null )
					{
						return type;
					}

					/////
					// Check the system namespace.
					/////
					name = string.Format( "System.{0}", name );

					return Type.GetType( name, false, ignoreCase );
				};

				var table = new DataTable( );

				if ( fieldType.Alias == StringFieldAlias )
				{
					table.Columns.Add( new DataColumn( "Id", typeof( long ) ) );
				}

				table.Columns.Add( new DataColumn( "EntityId", typeof( long ) ) );
				table.Columns.Add( new DataColumn( "TenantId", typeof( long ) ) );
				table.Columns.Add( new DataColumn( "FieldId", typeof( long ) ) );

				Type dataType = Type.GetType( dbType, null, typeResolver, true, true );

				if ( dataType == null )
				{
					throw new TypeLoadException( string.Format( "Unable to resolve type '{0}'.", dbType ) );
				}

				table.Columns.Add( new DataColumn( "Data", dataType ) );

				if ( fieldType.Alias == AliasFieldAlias )
				{
					table.Columns.Add( new DataColumn( "Namespace", typeof( string ) ) );
					table.Columns.Add( new DataColumn( "AliasMarkerId", typeof( Int32 ) ) );
				}

				_fieldTypeInfo[ fieldType ] = new FieldType( )
					{
						Parser = parser,
						TableName = dbFieldTable,
						Table = table
					};

			}
		}

		/// <summary>
		/// Looks up all fields that apply to this type that have default values.
		/// </summary>
		private List<FieldData> GetFieldsWithDefaults( Entity type )
		{
			List<FieldData> result;

			if ( !_defaultsOnType.TryGetValue( type, out result ) )
			{
				Alias defaultValueAlias = Aliases.CoreAlias( "defaultValue" );

				result =
					( from ancType in _schema.GetAncestors( type )
					  from field in _schema.GetDeclaredFields( ancType )
					  let defaultValue = _schema.GetStringFieldValue( field, defaultValueAlias )
					  where defaultValue != null
					  select new FieldData( )
					 {
						 Field = field,
						 Value = defaultValue
					 } ).ToList( );

				_defaultsOnType [ type ] = result;
			}
			return result;
		}

		/// <summary>
		/// Writes entities to the Entity table.
		/// </summary>
		private void SaveEntities( )
		{
			EventLog.Application.WriteTrace( "Writing to Entities table." );

			// Get defined entities, as well as implied entities.
			// I.e. those that back relationships without being explicitly declared in the configuration xml.
			var allEntities = _entities.Concat( _schema.GetImpliedRelationshipEntites( ) );

			var impliedEntities = new HashSet<Entity>( _schema.GetImpliedRelationshipEntites( ) );

			var entities = new DataTable( );

			entities.Columns.Add( new DataColumn( "Id", typeof ( long ) ) );
			entities.Columns.Add( new DataColumn( "TenantId", typeof( long ) ) );
			entities.Columns.Add( new DataColumn( "UpgradeId", typeof( Guid ) ) );

			bool missingAlias = false;

			if ( _dbContext != null )
			{
				long identity = 0;

				using ( IDbCommand command = _dbContext.CreateCommand( "SELECT CAST( IDENT_CURRENT( 'Entity' ) AS BIGINT )" ) )
				{
					object identityObject = command.ExecuteScalar( );

					if ( identityObject != null && identityObject != DBNull.Value )
					{
						identity = ( long ) identityObject;
					}
				}

				foreach ( Entity entity in allEntities )
				{
					if ( ( entity.Alias == null || string.IsNullOrEmpty( entity.Alias.Value ) ) && ( !impliedEntities.Contains( entity ) ) )
					{
						EventLog.Application.WriteError( "Entity was found in configuration XML without an alias.\r\n{0}", entity.LocationInfo );
						missingAlias = true;
					}

					DataRow row = entities.NewRow( );
					row [ 1 ] = _tenantId;
					entities.Rows.Add( row );
					_entityToId[ entity ] = identity++;
				}

				var connection = _dbContext.GetUnderlyingConnection( ) as SqlConnection;

				if ( connection != null )
				{
					using ( var bulk = new SqlBulkCopy( connection, SqlBulkCopyOptions.FireTriggers, null ) )
					{
						bulk.DestinationTableName = "Entity";
						bulk.BatchSize = 10000;
						bulk.ColumnMappings.Clear( );
						bulk.ColumnMappings.Add( "TenantId", "TenantId" );
						bulk.WriteToServer( entities );
					}
				}
			}

			if (missingAlias)
            {
                throw new Exception("One or more entities did not specify an alias.");
            }
		}

		/// <summary>
		/// Writes missing entities to the Entity table.
		/// </summary>
		/// <param name="upgradeMap">The upgrade map.</param>
		private void UpgradeEntities( IDictionary<string, Guid> upgradeMap )
		{
			EventLog.Application.WriteTrace( "Writing to Entities table." );

			try
			{
				string createTempTable = @"CREATE TABLE #EntityUpgrade
(
	TenantId BIGINT,
	UpgradeId UNIQUEIDENTIFIER,
	CONSTRAINT [PK_EntityUpgrade] PRIMARY KEY CLUSTERED ([TenantId] ASC, [UpgradeId] ASC)
)";

				using ( IDbCommand command = _dbContext.CreateCommand( createTempTable ) )
				{
					command.ExecuteNonQuery( );
				}

				// Get defined entities, as well as implied entities.
				// I.e. those that back relationships without being explicitly declared in the configuration xml.
				var allEntities = _entities.Concat( _schema.GetImpliedRelationshipEntites( ) );

				Dictionary<Guid, Entity> guidMap = new Dictionary<Guid, Entity>( );

				var entities = new DataTable( );

				entities.Columns.Add( new DataColumn( "TenantId", typeof( long ) ) );
				entities.Columns.Add( new DataColumn( "UpgradeId", typeof( Guid ) ) );

				if ( _dbContext != null )
				{
					foreach ( Entity entity in allEntities )
					{
						Guid upgradeId;

						if ( !upgradeMap.TryGetValue( entity.Alias.NsAlias, out upgradeId ) )
						{
							continue;
						}

						DataRow row = entities.NewRow( );
						row [ 0 ] = _tenantId;
						row [ 1 ] = upgradeId;
						entities.Rows.Add( row );
						guidMap [ upgradeId ] = entity;
					}

					var connection = _dbContext.GetUnderlyingConnection( ) as SqlConnection;

					if ( connection != null )
					{
						long userId;
						RequestContext.TryGetUserId( out userId );

						using ( var bulk = new SqlBulkCopy( connection ) )
						{
							bulk.DestinationTableName = "#EntityUpgrade";
							bulk.BatchSize = 10000;
							bulk.ColumnMappings.Clear( );
							bulk.ColumnMappings.Add( "TenantId", "TenantId" );
							bulk.ColumnMappings.Add( "UpgradeId", "UpgradeId" );
							bulk.WriteToServer( entities );
						}

						string upgradeTable = @"
IF ( @context IS NOT NULL )
BEGIN
	DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
	SET CONTEXT_INFO @contextInfo
END

INSERT INTO
	Entity WITH (PAGLOCK) (TenantId, UpgradeId)
SELECT DISTINCT
	e.TenantId, e.UpgradeId
FROM
	#EntityUpgrade e
LEFT JOIN
	Entity e2 ON e.UpgradeId = e2.UpgradeId AND e.TenantId = e2.TenantId
WHERE
	e2.Id IS NULL

SELECT
	e.Id,
	e.TenantId,
	e.UpgradeId
FROM
	Entity e
JOIN
	#EntityUpgrade e2 ON
		e.TenantId = e2.TenantId AND
		e.UpgradeId = e2.UpgradeId";

						using ( DatabaseContextInfo.SetContextInfo( "Entity installer upgrade entities" ) )
						using ( IDbCommand command = _dbContext.CreateCommand( upgradeTable ) )
						{
							command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

							using ( IDataReader reader = command.ExecuteReader( ) )
							{
								while ( reader.Read( ) )
								{
									_entityToId [ guidMap [ reader.GetGuid( 2 ) ] ] = reader.GetInt64( 0 );
								}
							}
						}
					}
				}
			}
			finally
			{
				using ( IDbCommand command = _dbContext.CreateCommand( "DROP TABLE #EntityUpgrade" ) )
				{
					command.ExecuteNonQuery( );
				}
			}
		}

		/// <summary>
		/// Find and save all field values to database
		/// </summary>
		private void SaveFieldData( )
		{
			// TODO: Casting?
			// TODO: Bulk inserts?

			// Comparer for equating two field data objects by their field.
			var comparer = new CastingComparer<FieldData, Entity>( fd => fd.Field );

			// Alias field type
			FieldType aliasFieldType = _fieldTypeInfo [ _aliasResolver [ AliasFieldAlias ] ];
			Alias aliasMarkerIdField = Aliases.CoreAlias( "aliasMarkerId" );

			FieldType stringFieldType = _fieldTypeInfo [ _aliasResolver [ StringFieldAlias ] ];

			// Visit each entity
			foreach ( Entity entity in _entities )
			{
				// Combine declared values with defaults
				var definedFields = _schema.GetAllEntityFields( entity );
				var defaultFields = GetFieldsWithDefaults( _aliasResolver [ entity.Type ] );
				var fieldsToSave = definedFields.Concat( defaultFields.Except( definedFields, comparer ) );

				// Process each field
				foreach ( FieldData fieldData in fieldsToSave )
				{
					FieldType fieldType = _fieldTypeInfo [ _aliasResolver [ fieldData.Field.Type ] ];

					object data = fieldType.Parser( fieldData.Value );

					DataRow row = fieldType.Table.NewRow( );

					int index = 0;
					if ( fieldType == stringFieldType )
					{
						index++;
					}

					row [ index++ ] = GetId( entity );
					row [ index++ ] = _tenantId;
					row [ index++ ] = GetId( fieldData.Field );
					row [ index ] = data;

					if ( fieldType == aliasFieldType )
					{
						// Special case processing for alias fields
						Alias alias = _schema.GetAliasFieldValue( entity, fieldData.Field.Alias ); //hmm
						int markerId = _schema.GetIntFieldValue( fieldData.Field, aliasMarkerIdField ).Value;
						row [ index++ ] = alias.Value;
						row [ index++ ] = alias.Namespace;
						row [ index ] = markerId;
					}

					fieldType.Table.Rows.Add( row );
				}
			}

			var connection = _dbContext.GetUnderlyingConnection( ) as SqlConnection;

			if ( connection != null )
			{
				foreach ( var fieldType in _fieldTypeInfo.Values )
				{
					if ( fieldType.Table.Rows.Count > 0 )
					{
						using ( var bulk = new SqlBulkCopy( connection, SqlBulkCopyOptions.FireTriggers, null ) )
						{
							bulk.DestinationTableName = fieldType.TableName;
							bulk.BatchSize = 10000;
							bulk.WriteToServer( fieldType.Table );
						}
					}
				}
			}
		}

		/// <summary>
		/// Upgrades the field data.
		/// </summary>
		private void UpgradeFieldData( )
		{
			// Comparer for equating two field data objects by their field.
			var comparer = new CastingComparer<FieldData, Entity>( fd => fd.Field );

			// Alias field type
			FieldType aliasFieldType = _fieldTypeInfo [ _aliasResolver [ AliasFieldAlias ] ];
			Alias aliasMarkerIdField = Aliases.CoreAlias( "aliasMarkerId" );

			FieldType stringFieldType = _fieldTypeInfo [ _aliasResolver [ StringFieldAlias ] ];

			// Visit each entity
			foreach ( Entity entity in _entities )
			{
				// Combine declared values with defaults
				var definedFields = _schema.GetAllEntityFields( entity );
				var defaultFields = GetFieldsWithDefaults( _aliasResolver [ entity.Type ] );
				var fieldsToSave = definedFields.Concat( defaultFields.Except( definedFields, comparer ) );

				// Process each field
				foreach ( FieldData fieldData in fieldsToSave )
				{
					FieldType fieldType = _fieldTypeInfo [ _aliasResolver [ fieldData.Field.Type ] ];

					object data = fieldType.Parser( fieldData.Value );

					DataRow row = fieldType.Table.NewRow( );

					int index = 0;
					if ( fieldType == stringFieldType )
					{
						index++;
					}

					row [ index++ ] = GetId( entity );
					row [ index++ ] = _tenantId;
					row [ index++ ] = GetId( fieldData.Field );
					row [ index ] = data;

					if ( fieldType == aliasFieldType )
					{
						// Special case processing for alias fields
						Alias alias = _schema.GetAliasFieldValue( entity, fieldData.Field.Alias ); //hmm
						int markerId = _schema.GetIntFieldValue( fieldData.Field, aliasMarkerIdField ).Value;
						row [ index++ ] = alias.Value;
						row [ index++ ] = alias.Namespace;
						row [ index ] = markerId;
					}

					fieldType.Table.Rows.Add( row );
				}
			}

			var connection = _dbContext.GetUnderlyingConnection( ) as SqlConnection;

			if ( connection != null )
			{
				long userId;
				RequestContext.TryGetUserId( out userId );

				foreach ( var fieldType in _fieldTypeInfo.Values )
				{
					if ( fieldType.Table.Rows.Count > 0 )
					{
						string dataType;

						/////
						// Determine the tables 'Data' field type.
						/////
						switch ( fieldType.TableName )
						{
							case "Data_Alias":
								dataType = "NVARCHAR(100)";
								break;
							case "Data_Bit":
								dataType = "BIT";
								break;
							case "Data_DateTime":
								dataType = "DATETIME";
								break;
							case "Data_Decimal":
								dataType = "DECIMAL(38,10)";
								break;
							case "Data_Guid":
								dataType = "UNIQUEIDENTIFIER";
								break;
							case "Data_INT":
								dataType = "INT";
								break;
							default:
								dataType = "NVARCHAR(MAX)";
								break;
						}

						try
						{
							string additionalAliasFields = @"
	Namespace NVARCHAR(100),
	AliasMarkerId INT,
";

							string createTempTable = $@"
CREATE TABLE #FieldUpgrade
(
	EntityId BIGINT,
	TenantId BIGINT,
	FieldId BIGINT,
	Data {dataType},
	{(fieldType == aliasFieldType ? additionalAliasFields : string.Empty)}
	CONSTRAINT [PK_DataUpgrade] PRIMARY KEY CLUSTERED ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC)
)";

							using ( IDbCommand command = _dbContext.CreateCommand( createTempTable ) )
							{
								/////
								// Create the temp table.
								/////
								command.ExecuteNonQuery( );
							}

							using ( var bulk = new SqlBulkCopy( connection ) )
							{
								bulk.DestinationTableName = "#FieldUpgrade";
								bulk.BatchSize = 10000;
								bulk.ColumnMappings.Clear( );
								bulk.ColumnMappings.Add( "EntityId", "EntityId" );
								bulk.ColumnMappings.Add( "TenantId", "TenantId" );
								bulk.ColumnMappings.Add( "FieldId", "FieldId" );
								bulk.ColumnMappings.Add( "Data", "Data" );

								if ( fieldType.TableName == "Data_Alias" )
								{
									bulk.ColumnMappings.Add( "Namespace", "Namespace" );
									bulk.ColumnMappings.Add( "AliasMarkerId", "AliasMarkerId" );
								}

								/////
								// Populate the temp table.
								/////
								bulk.WriteToServer( fieldType.Table );
							}

							string upgradeFields = $@"
IF ( @context IS NOT NULL )
BEGIN
	DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
	SET CONTEXT_INFO @contextInfo
END

INSERT INTO
	{fieldType.TableName} WITH (PAGLOCK) (EntityId, TenantId, FieldId, Data{( fieldType.TableName == "Data_Alias" ? ", Namespace, AliasMarkerId" : string.Empty )})
SELECT DISTINCT
	f.EntityId, f.TenantId, f.FieldId, f.Data{(fieldType.TableName == "Data_Alias" ? ", f.Namespace, f.AliasMarkerId" : string.Empty)}
FROM
	#FieldUpgrade f
LEFT JOIN
	{fieldType.TableName} f2 ON f.EntityId = f2.EntityId AND f.TenantId = f2.TenantId AND f.FieldId = f2.FieldId
WHERE
	f2.EntityId IS NULL";

							using ( DatabaseContextInfo.SetContextInfo( $"Entity installer upgrade field data '{fieldType.TableName}'" ) )
							using ( IDbCommand command = _dbContext.CreateCommand( upgradeFields ) )
							{
								command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

								/////
								// Upgrade the field data.
								/////
								command.ExecuteNonQuery( );
							}
						}
						finally
						{
							using ( IDbCommand command = _dbContext.CreateCommand( "DROP TABLE #FieldUpgrade" ) )
							{
								/////
								// Drop the temp table
								/////
								command.ExecuteNonQuery( );
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Save relationships to database
		/// </summary>
		private void SaveRelationships( )
		{
			// TODO: Bulk insert
			IEnumerable<Relationship> allRelationships = _schema.GetAllRelationships( );

			var relationships = new DataTable( );

			relationships.Columns.Add( new DataColumn( "Id", typeof( long ) ) );
			relationships.Columns.Add( new DataColumn( "TenantId", typeof( long ) ) );
			relationships.Columns.Add( new DataColumn( "TypeId", typeof( long ) ) );
			relationships.Columns.Add( new DataColumn( "FromId", typeof( long ) ) );
			relationships.Columns.Add( new DataColumn( "ToId", typeof( long ) ) );

			foreach ( Relationship relationship in allRelationships )
			{
				DataRow row = relationships.NewRow( );

				row [ 1 ] = _tenantId;
				row [ 2 ] = GetId( relationship.Type );
				row [ 3 ] = GetId( relationship.From );
				row [ 4 ] = GetId( relationship.To );

				relationships.Rows.Add( row );
			}

			var connection = _dbContext.GetUnderlyingConnection( ) as SqlConnection;

			try
			{
				if ( connection != null )
				{
					using ( var bulk = new SqlBulkCopy( connection, SqlBulkCopyOptions.FireTriggers, null ) )
					{
						bulk.DestinationTableName = "Relationship";
						bulk.BatchSize = 10000;
						bulk.ColumnMappings.Clear( );
						bulk.ColumnMappings.Add( "TenantId", "TenantId" );
						bulk.ColumnMappings.Add( "TypeId", "TypeId" );
						bulk.ColumnMappings.Add( "FromId", "FromId" );
						bulk.ColumnMappings.Add( "ToId", "ToId" );
						bulk.WriteToServer( relationships );
					}
				}
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteError( "Failed to write relationships. {0}", ex.ToString( ) );
			}
		}

		/// <summary>
		/// Upgrades the relationships.
		/// </summary>
		private void UpgradeRelationships( )
		{
			try
			{
				string createTempTable = @"
CREATE TABLE #RelationshipUpgrade
(
	TenantId BIGINT NOT NULL,
	TypeId BIGINT NOT NULL,
	FromId BIGINT NOT NULL,
	ToId BIGINT NOT NULL,
	CONSTRAINT [PK_RelationshipUpgrade] PRIMARY KEY NONCLUSTERED ([TenantId] ASC, [TypeId] ASC, [FromId] ASC, [ToId] ASC)
)";

				using ( IDbCommand command = _dbContext.CreateCommand( createTempTable ) )
				{
					/////
					// Create the temp table.
					/////
					command.ExecuteNonQuery( );
				}

				IEnumerable<Relationship> allRelationships = _schema.GetAllRelationships( );

				var relationships = new DataTable( );

				relationships.Columns.Add( new DataColumn( "TenantId", typeof( long ) ) );
				relationships.Columns.Add( new DataColumn( "TypeId", typeof( long ) ) );
				relationships.Columns.Add( new DataColumn( "FromId", typeof( long ) ) );
				relationships.Columns.Add( new DataColumn( "ToId", typeof( long ) ) );

				foreach ( Relationship relationship in allRelationships )
				{
					DataRow row = relationships.NewRow( );

					row [ 0 ] = _tenantId;
					row [ 1 ] = GetId( relationship.Type );
					row [ 2 ] = GetId( relationship.From );
					row [ 3 ] = GetId( relationship.To );

					relationships.Rows.Add( row );
				}

				var connection = _dbContext.GetUnderlyingConnection( ) as SqlConnection;

				try
				{
					if ( connection != null )
					{
						long userId;
						RequestContext.TryGetUserId( out userId );

						using ( var bulk = new SqlBulkCopy( connection ) )
						{
							bulk.DestinationTableName = "#RelationshipUpgrade";
							bulk.BatchSize = 10000;
							bulk.ColumnMappings.Clear( );
							bulk.ColumnMappings.Add( "TenantId", "TenantId" );
							bulk.ColumnMappings.Add( "TypeId", "TypeId" );
							bulk.ColumnMappings.Add( "FromId", "FromId" );
							bulk.ColumnMappings.Add( "ToId", "ToId" );
							bulk.WriteToServer( relationships );
						}

						string upgradeRelationships = @"
IF ( @context IS NOT NULL )
BEGIN
	DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
	SET CONTEXT_INFO @contextInfo
END

INSERT INTO
	Relationship WITH (PAGLOCK) (TenantId, TypeId, FromId, ToId)
SELECT DISTINCT
	r.TenantId, r.TypeId, r.FromId, r.ToId
FROM
	#RelationshipUpgrade r
LEFT JOIN
	Relationship r2 ON r.TenantId = r2.TenantId AND r.TypeId = r2.TypeId AND r.FromId = r2.FromId AND r.ToId = r2.ToId
WHERE
	r2.Id IS NULL";
						using ( DatabaseContextInfo.SetContextInfo( "Entity installer upgrade relationships" ) )
						using ( IDbCommand command = _dbContext.CreateCommand( upgradeRelationships ) )
						{
							command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

							/////
							// Upgrade the relationship data.
							/////
							command.ExecuteNonQuery( );
						}
					}
				}
				catch ( Exception ex )
				{
					EventLog.Application.WriteError( "Failed to write relationships. {0}", ex.ToString( ) );
				}
			}
			finally
			{
				using ( IDbCommand command = _dbContext.CreateCommand( "DROP TABLE #RelationshipUpgrade" ) )
				{
					/////
					// Drop the temp table.
					/////
					command.ExecuteNonQuery( );
				}
			}
		}


		/// <summary>
		/// Generates the resource data key hashes.
		/// </summary>
		/// <param name="solution">The solution.</param>
		public static void GenerateResourceDataKeyHashes(long solution)
        {            
            EventLog.Application.WriteInformation("Start generating resource data key hashes (Start Time: {0}. Solution {1})", DateTime.Now, solution);

            using (new DeferredChannelMessageContext())            
            using (DatabaseContext.GetContext())
            {
                IEnumerable<ResourceKey> resourceKeys = Model.Entity.GetInstancesOfType<ResourceKey>();

	            var enumerable = resourceKeys as IList<ResourceKey> ?? resourceKeys.ToList( );

	            if (enumerable.Count > 0)
                {
                    IEnumerable<ResourceKey> writableResourceKeys = enumerable.Select(r => r.AsWritable<ResourceKey>());

                    using (new ResourceKeyHelper.InstallKeysContext())
                    {
                        // Generate the hashes
                        Model.Entity.Save(writableResourceKeys);
                    }
                }
            }            

            EventLog.Application.WriteInformation("Completed generating resource data key hashes (End Time: {0}. Solution {1})", DateTime.Now, solution);
        }        

		// A field type
		private class FieldType
		{
			public DataTable Table;
			public Func<string, object> Parser;
			public string TableName;
		}
		#region Helpers

		/// <summary>
		/// Helper to locate the entity for an alias in as few keystrokes as possible.
		/// </summary>
		public Entity A( Alias alias )
		{
			return _aliasResolver [ alias ];
		}

		/// <summary>
		/// Dispose of commands.
		/// </summary>
		private void Cleanup( )
		{
			foreach ( FieldType fieldType in _fieldTypeInfo.Values )
			{
				fieldType.Table.Dispose( );
			}
		}

		/// <summary>
		/// Helper to get the Id that was assigned to an entity.
		/// </summary>
		private long GetId( Entity entity )
		{
			return _entityToId [ entity ];
		}

		/// <summary>
		/// Helper to get the Id that was assigned to a referenced entity.
		/// </summary>
		private long GetId( EntityRef entity )
		{
			return _entityToId [ _aliasResolver [ entity ] ];
		}
		#endregion Helpers
	}
}