// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Storage;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.SoftwarePlatform.Migration.Sources
{
    /// <summary>
    ///     Represents a reader for loading a graph of entities from within a tenant.
    ///     Namely, for exporting a single component, or group of components.
    /// </summary>
    internal class TenantGraphSource : SqlBase, IDataSource
    {
        /// <summary>
        ///     EntityAccessControlService Service
        /// </summary>
        private IEntityAccessControlService EntityAccessControlService
        {
            get;
        }

        /// <summary>
        ///     EntityRepository Service
        /// </summary>
        private IEntityRepository EntityRepository
        {
            get;
        }

        /// <summary>
        ///     UpgradeIdProvider Service
        /// </summary>
        private IUpgradeIdProvider UpgradeIdProvider
        {
            get;
        }

        /// <summary>
        ///     True to demand read permission for the current user.
        /// </summary>
        internal bool DemandReadPermission
        {
            get;
            set;
        } = true;

        /// <summary>
        ///     Constructor
        /// </summary>
        public TenantGraphSource( )
        {
            EntityRepository = Factory.EntityRepository;
            UpgradeIdProvider = Factory.UpgradeIdProvider;
            EntityAccessControlService = Factory.EntityAccessControlService;
        }

        /// <summary>
        ///     Contains all entities, relationships, fields, from the graph load.
        ///     Unsecured.
        /// </summary>
        private BulkRequestResult _bulkResult;

        /// <summary>
        ///     Map of entity IDs to upgrade IDs
        /// </summary>
        private IDictionary<long, Guid> _idToUpgradeId;

        /// <summary>
        ///     Cached entity field data.
        /// </summary>
        private Dictionary<string, List<DataEntry>> _fieldDataCache;

        /// <summary>
        ///     Cached entity data.
        /// </summary>
        private List<EntityEntry> _entityCache;

        /// <summary>
        ///     Cached relationship data.
        /// </summary>
        private List<RelationshipEntry> _relationshipCache;

        /// <summary>
        ///     Cached relationship type data.
        /// </summary>
        private IDictionary<Guid, RelationshipTypeEntry> _relationshipTypeCache;

        /// <summary>
        ///     Alias field ID
        /// </summary>
        private long _aliasFieldId;

        /// <summary>
        ///     Reverse alias field ID
        /// </summary>
        private long _reverseAliasFieldId;

        /// <summary>
        ///     Predicate that determines if the current user has read-access to the entity.
        /// </summary>
        private Predicate<long> _canRead;

        /// <summary>
        ///     Cached binary data.
        /// </summary>
        private List<BinaryDataEntry> _binaryCache;

        /// <summary>
        ///     The document cache
        /// </summary>
        private List<DocumentDataEntry> _documentCache;

        /// <summary>
        ///     The tenant to load the data from.
        /// </summary>
        public long TenantId
        {
            get;
            set;
        }

        /// <summary>
        ///     The starting entity/entities to collect from.
        /// </summary>
        public ICollection<long> RootEntities
        {
            get;
            set;
        }

        /// <summary>
        ///     Load entities.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<EntityEntry> GetEntities( IProcessingContext context )
        {
            if ( _entityCache != null )
                return _entityCache;

            // Create entity entries
            _entityCache = new List<EntityEntry>( );
            foreach ( var pair in _bulkResult.AllEntities )
            {
                // Skip entry if it is a leaf entity that we are only relating to.
                if ( IsReferenceOnly( pair.Value ) )
                    continue;

                long id = pair.Key;

                // Check security
                if ( !_canRead( id ) )
                    continue;

                // Create entry
                Guid upgradeId;
                if ( !_idToUpgradeId.TryGetValue( id, out upgradeId ) )
                    continue;

                EntityEntry entry = new EntityEntry
                {
                    EntityId = upgradeId
                };
                _entityCache.Add( entry );
            }

            return _entityCache;
        }

        /// <summary>
        ///     Load relationships.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<RelationshipEntry> GetRelationships( IProcessingContext context )
        {
            if ( _relationshipCache != null )
                return _relationshipCache;

            // Create relationship entries
            _relationshipCache = new List<RelationshipEntry>( );
            foreach ( var pair in _bulkResult.Relationships )
            {
                long originId = pair.Key.EntityId;
                long relTypeId = pair.Key.TypeId;
                Direction direction = pair.Key.Direction;
                List<long> destinations = pair.Value;

                // Security check source
                if ( !_canRead( originId ) )
                    continue;

                // Determine the clone action
                RelationshipInfo relInfo;
                if ( !_bulkResult.BulkSqlQuery.Relationships.TryGetValue( pair.Key.TypeIdAndDirection, out relInfo ) )
                    continue; // assert false
                var cloneAction = direction == Direction.Forward ? relInfo.CloneAction : relInfo.ReverseCloneAction;
                bool isDrop = cloneAction == CloneActionEnum_Enumeration.Drop;

                // Prepare to handle duplicate entries
                bool wouldDiscardIfDuplicate = WouldDiscardIfDuplicate( relInfo, direction );
                Direction oppositeDir = direction == Direction.Forward ? Direction.Reverse : Direction.Forward;

                // Resolve IDs
                Guid originUid;
                Guid relTypeUid;
                if ( !_idToUpgradeId.TryGetValue( originId, out originUid ) )
                    continue;
                if ( !_idToUpgradeId.TryGetValue( relTypeId, out relTypeUid ) )
                    continue;

                // Add each entry
                foreach ( long destId in destinations )
                {
                    // If clone action is 'drop' then only include the relationship if the target is also present in the dataset
                    if ( isDrop )
                    {
                        EntityValue entityValue;
                        if ( !_bulkResult.AllEntities.TryGetValue( destId, out entityValue ) )
                            continue; // assert false
                        if ( IsReferenceOnly( entityValue ) )
                            continue; // external node when cloneAction is drop
                    }

                    // If relationship is present in both directions, then drop the reverse
                    if ( wouldDiscardIfDuplicate )
                    {
                        RelationshipKey reverseKey = new RelationshipKey( destId, -pair.Key.TypeIdAndDirection );
                        List<long> reverseIds;
                        if ( _bulkResult.Relationships.TryGetValue( reverseKey, out reverseIds ) )
                        {
                            if ( reverseIds.Contains( originId ) )
                                continue;
                        }
                    }

                    Guid destUid;
                    if ( !_idToUpgradeId.TryGetValue( destId, out destUid ) )
                        continue;

                    RelationshipEntry entry;
                    if ( direction == Direction.Forward )
                        entry = new RelationshipEntry( relTypeUid, originUid, destUid );
                    else
                        entry = new RelationshipEntry( relTypeUid, destUid, originUid );
                    _relationshipCache.Add( entry );
                }
            }

            return _relationshipCache;
        }

        /// <summary>
        ///     If the relationship instance is present in both directions, is this the direction that would be kept.
        /// </summary>
        /// <param name="relInfo">Relationship info</param>
        /// <param name="direction">Current direction</param>
        /// <returns></returns>
        private bool WouldDiscardIfDuplicate( RelationshipInfo relInfo, Direction direction )
        {
            // Prefer the 'CloneEntities', then the 'CloneRef', finally the 'Drop'.
            // All things being equal, prefer the forward copy

            if ( relInfo.CloneAction == relInfo.ReverseCloneAction )
                return direction == Direction.Reverse;

            var curAction = direction == Direction.Forward ? relInfo.CloneAction : relInfo.ReverseCloneAction;
            var revAction = direction == Direction.Forward ? relInfo.ReverseCloneAction : relInfo.CloneAction;

            if ( curAction == CloneActionEnum_Enumeration.CloneEntities || revAction == CloneActionEnum_Enumeration.Drop )
                return false;
            if ( curAction == CloneActionEnum_Enumeration.Drop || revAction == CloneActionEnum_Enumeration.CloneEntities )
                return true;

            return direction == Direction.Forward;
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
        ///     - ensure that aliases export their namespace and direction marker.
        /// </remarks>
        public IEnumerable<DataEntry> GetFieldData( string dataTable, IProcessingContext context )
        {
            List<DataEntry> data;
            string dataTableReal = "Data_" + dataTable;

            if ( _fieldDataCache != null )
            {
                if ( _fieldDataCache.TryGetValue( dataTableReal, out data ) )
                    return data;
                return Enumerable.Empty<DataEntry>( );
            }

            // Create field data entries
            _fieldDataCache = new Dictionary<string, List<DataEntry>>( );

            foreach ( var pair in _bulkResult.FieldValues )
            {
                long entityId = pair.Key.EntityId;
                if ( !_canRead( entityId ) )
                    continue;
                long fieldId = pair.Key.FieldId;
                object fieldValue = pair.Value.RawValue;

                // Resolve table
                FieldInfo fieldInfo = _bulkResult.BulkSqlQuery.FieldTypes[ fieldId ];
                string table = fieldInfo.DatabaseTable;
                
                List<DataEntry> tableList;
                if ( !_fieldDataCache.TryGetValue( table, out tableList ) )
                {
                    tableList = new List<DataEntry>( );
                    _fieldDataCache[ table ] = tableList;
                }

                // Look up UpgradeIds
                Guid entityUid;
                Guid fieldUid;
                if ( !_idToUpgradeId.TryGetValue( entityId, out entityUid ) )
                    continue;
                if ( !_idToUpgradeId.TryGetValue( fieldId, out fieldUid ) )
                    continue;

                // Handle aliases
                int aliasMarkerId = 0;
                string ns = null;
                if ( fieldId == _aliasFieldId || fieldId == _reverseAliasFieldId)
                {
                    aliasMarkerId = fieldId == _reverseAliasFieldId ? 1 : 0;
                    string nsAlias = ( string ) fieldValue;
                    string[ ] parts = nsAlias.Split( ':' );
                    ns = parts[ 0 ];
                    fieldValue = parts[ 1 ];
                }

                // Create entry
                DataEntry fieldEntry = new DataEntry
                {
                    EntityId = entityUid,
                    FieldId = fieldUid,
                    Data = fieldValue,
                    AliasMarkerId = aliasMarkerId,
                    Namespace = ns
                };
                tableList.Add( fieldEntry );
            }

            // Fetch result
            if ( _fieldDataCache.TryGetValue( dataTableReal, out data ) )
                return data;
            return Enumerable.Empty<DataEntry>( );
        }

        /// <summary>
        ///     Sets up this instance.
        /// </summary>
        void IDataSource.Setup( IProcessingContext context )
        {
            if ( RootEntities == null )
                throw new InvalidOperationException( "RootEntities is not set." );

            // Perform demand on root entity(ies).
            if ( DemandReadPermission )
            {
                EntityAccessControlService.Demand( RootEntities.Select( id => new EntityRef( id ) ).ToList( ), new[]
                {
                    Permissions.Read
                } );
            }

            using ( new TenantAdministratorContext( TenantId ) )
            {
                _aliasFieldId = WellKnownAliases.CurrentTenant.Alias;
                _reverseAliasFieldId = WellKnownAliases.CurrentTenant.ReverseAlias;

                // Get the instance to be exported
                IEnumerable<IEntity> instances = EntityRepository.Get( RootEntities );
                ICollection<long> typeIds = instances.Select( inst => inst.TypeIds.FirstOrDefault( ) ).Distinct().ToList();
                if ( typeIds.Count == 0 )
                    typeIds = new[] { WellKnownAliases.CurrentTenant.Resource };

                // Generate a cloning request factory for loading the data
                CloneEntityMemberRequestFactory requestFactory = new CloneEntityMemberRequestFactory( EntityRepository );
                EntityMemberRequest memberRequest = requestFactory.CreateRequest( typeIds );

                EntityRequest entityRequest = new EntityRequest
                {
                    Request = memberRequest,
                    Entities = RootEntities.Select( id => new EntityRef( id ) )
                };

                // Load data, unsecured, via EntityInfoService cache
                _bulkResult = BulkResultCache.GetBulkResult( entityRequest );

                // Load all UpgradeIDs
                IEnumerable<long> allIds = _bulkResult.AllEntities.Keys
                    .Union( _bulkResult.Relationships.Keys.Select( k => k.TypeId ) )
                    .Union( _bulkResult.FieldValues.Keys.Select( k => k.FieldId ) );

                _idToUpgradeId = UpgradeIdProvider.GetUpgradeIds( allIds );

                IEnumerable<long> relTypes = _bulkResult.Relationships.Keys.Select( key => key.TypeId ).Distinct( );
                IEnumerable<Relationship> relationships =  EntityRepository.Get<Relationship>( relTypes );
                _relationshipTypeCache = relationships.ToDictionary(
                    rel => _idToUpgradeId[ rel.Id ],
                    rel => new RelationshipTypeEntry
                    {
                        CloneAction = rel.CloneAction_Enum,
                        ReverseCloneAction = rel.ReverseCloneAction_Enum,
                        Alias = rel.Alias,
                        ReverseAlias = rel.ReverseAlias
                    } );

                LoadDocumentCaches( context );
            }

            // Read permissions - for reading internal entities
            if ( DemandReadPermission )
            {
                _canRead = BulkRequestResultSecurityHelper.GetEntityReadability( Factory.EntityAccessControlService, _bulkResult );
            }
            else
            {
                _canRead = ( long id ) => true;
            }
        }

        /// <summary>
        ///     Loads the application metadata.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">@Invalid package Id</exception>
        Metadata IDataSource.GetMetadata( IProcessingContext context )
        {
            List<Guid> roots = new List<Guid>( );
            foreach ( long rootId in RootEntities )
            {
                Guid rootGuid;
                if ( _idToUpgradeId.TryGetValue( rootId, out rootGuid ) )
                    roots.Add( rootGuid );
            }

            var metadata = new Metadata
            {
                AppName = "Exported data",
                AppVerId = Guid.Empty,
                AppId = Guid.Empty,
                Description = "Exported data",
                Name = "Exported data",
                Version = "1.0",
                RootEntities = roots,
                //Dependencies = solutionDependencies,
                Type = SourceType.DataExport,
                PlatformVersion = SystemInfo.PlatformVersion,
                RelationshipTypeCallback = GetRelationshipMetadata
            };


            return metadata;
        }

        private RelationshipTypeEntry GetRelationshipMetadata( Guid typeId )
        {
            RelationshipTypeEntry result;
            _relationshipTypeCache.TryGetValue( typeId, out result );
            return result;
        }

        /// <summary>
        ///     Determine if the entity is just a reference to some entity external to the graph.
        ///     This is achieved by determining if the bulk query result came from the 'id' node, which is the only one with no fields.
        ///     (All non-leaf nodes will have at least requested the 'name' field)
        /// </summary>
        private bool IsReferenceOnly( EntityValue entityValue )
        {
            var requestNodes = entityValue.Nodes;
            bool isRefNode = requestNodes.Count( ) == 1 && requestNodes.First( ).Request.Fields.Count == 0;
            return isRefNode;
        }

        #region Non-applicable interface members

        /// <summary>
        ///     Tears down this instance.
        /// </summary>
        void IDataSource.TearDown( IProcessingContext context )
        {
        }

        /// <summary>
        ///     Gets the binary data.
        /// </summary>
        IEnumerable<BinaryDataEntry> IDataSource.GetBinaryData( IProcessingContext context )
        {
            if ( _binaryCache == null )
                LoadDocumentCaches( context );

            return _binaryCache;
        }

        /// <summary>
        ///     Gets the document data.
        /// </summary>
        IEnumerable<DocumentDataEntry> IDataSource.GetDocumentData( IProcessingContext context )
        {
            if ( _documentCache == null )
                LoadDocumentCaches( context );

            return _documentCache;
        }

        /// <summary>
        ///     Scan for document token fields.
        /// </summary>
        private void LoadDocumentCaches( IProcessingContext context )
        {
            _binaryCache = new List<BinaryDataEntry>( );
            _documentCache = new List<DocumentDataEntry>( );

            HashSet<string> tokensDone = new HashSet<string>( );

            // GUID for fileDataHash field.
            long fileDataHash = new EntityRef( "fileDataHash" ).Id;
            long isOfType = WellKnownAliases.CurrentTenant.IsOfType;

            // Determine types that are 'binary'. Others are 'document'.
            ISet<long> fileTypes = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf( new EntityRef( "fileType" ).Id );
            ISet<long> binaryTypes = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf( new EntityRef( "imageFileType" ).Id );
            ISet<long> documentTypes = new HashSet<long>( fileTypes.Except( binaryTypes ) );

            // Callbacks to load data
            Func<string, byte[]> loadDocumentData = token => FileRepositoryUtils.LoadFileData( Factory.DocumentFileRepository, token, context );
            Func<string, byte[]> loadBinaryData = token => FileRepositoryUtils.LoadFileData( Factory.BinaryFileRepository, token, context );

            foreach ( KeyValuePair<FieldKey, FieldValue> field in _bulkResult.FieldValues )
            {
                if ( field.Key.FieldId != fileDataHash )
                    continue;

                // Get token
                string token = field.Value?.RawValue as string;
                if ( string.IsNullOrEmpty( token ) )
                    continue;

                if ( tokensDone.Contains( token ) )
                    continue;
                tokensDone.Add( token );

                // Get entity type
                long entityId = field.Key.EntityId;
                RelationshipKey typeRelKey = new RelationshipKey( entityId, isOfType );
                List<long> types;
                _bulkResult.Relationships.TryGetValue( typeRelKey, out types );
                long? singleType = types.FirstOrDefault( );
                if ( singleType == null )
                    continue;
                long typeId = singleType.Value;

                // Determine type
                bool isBinary = binaryTypes.Contains( typeId );
                bool isDoc = documentTypes.Contains( typeId );

                // Create entry
                if ( isBinary )
                {
                    var binaryDataEntry = new BinaryDataEntry
                    {
                        DataHash = token,
                        LoadDataCallback = loadBinaryData
                    };

                    _binaryCache.Add( binaryDataEntry );
                }
                else if (isDoc)
                {
                    var docDataEntry = new DocumentDataEntry
                    {
                        DataHash = token,
                        LoadDataCallback = loadDocumentData
                    };

                    _documentCache.Add( docDataEntry );
                }
            }
        }

        /// <summary>
        ///     Gets the missing field data.
        /// </summary>
        IEnumerable<DataEntry> IDataSource.GetMissingFieldData( IProcessingContext context )
        {
            return Enumerable.Empty<DataEntry>( );
        }

        /// <summary>
        ///     Gets the missing relationships.
        /// </summary>
        IEnumerable<RelationshipEntry> IDataSource.GetMissingRelationships( IProcessingContext context )
        {
            return Enumerable.Empty<RelationshipEntry>( );
        }

        /// <summary>
        ///     Gets the security data relationships.
        /// </summary>
        IEnumerable<SecureDataEntry> IDataSource.GetSecureData( IProcessingContext context )
        {
            return Enumerable.Empty<SecureDataEntry>( ); // You can't get secure data from a tenant
        }

        /// <summary>
        /// Gets the entities that should not be removed as part of an upgrade operation.
        /// </summary>
        IEnumerable<Guid> IDataSource.GetDoNotRemove( IProcessingContext context )
        {
            return Enumerable.Empty<Guid>( );
        }

        #endregion

    }
}