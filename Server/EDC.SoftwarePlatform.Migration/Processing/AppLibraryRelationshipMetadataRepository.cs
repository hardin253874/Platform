// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.ReadiNow.Database;
using ReadiNow.Database;

namespace EDC.SoftwarePlatform.Migration.Processing
{
    /// <summary>
    ///     Helper class that can fabricate relationship metadata from the App Library, when
    ///     it is unavailable from anywhere else.
    /// </summary>
    /// <remarks>
    ///     This takes a list of relationship types, and for each one determines the app-libary package
    ///     with the highest version number, and retrieves its metadata from that app-library package.
    /// </remarks>
    class AppLibraryRelationshipMetadataRepository
    {
        
        /// <summary>
        ///     Return a callback that can resolve RelationshipTypeEntrys for types of the specified relationships.
        /// </summary>
        /// <param name="relationships">Relationship entries we'd like to resolve types for.</param>
        /// <returns>A callback that returns either a valid entry or null if info could not be found.</returns>
        public Func<Guid, RelationshipTypeEntry> CreateMetadataCallback( IEnumerable<RelationshipEntry> relationships )
        {
            if ( relationships == null )
                throw new ArgumentNullException( nameof( relationships ) );

            // Get unique relationship types
            IEnumerable<Guid> uniqueRelTypes =
                relationships.Select( rel => rel.TypeId ).Distinct( );

            // Load from library
            IDictionary<Guid, RelationshipTypeEntry> results =
                GetEntriesFromAppLibrary( uniqueRelTypes );

            // Create a callback
            Func<Guid, RelationshipTypeEntry> callback = ( Guid relType ) =>
            {
                RelationshipTypeEntry result;

                if ( results.TryGetValue( relType, out result ) )
                    return result;
                return null;
            };
            return callback;
        }

        /// <summary>
        ///     Call database proc to gather relationship metadata from whichever package has the highest version number.
        /// </summary>
        /// <param name="uniqueTypes"></param>
        /// <returns></returns>
        private IDictionary<Guid, RelationshipTypeEntry> GetEntriesFromAppLibrary( IEnumerable<Guid> uniqueRelTypes )
        {
            Dictionary<Guid, RelationshipTypeEntry> results = new Dictionary<Guid, RelationshipTypeEntry>( );

            using ( IDatabaseContext ctx = DatabaseContext.GetContext( ) )
            using ( IDbCommand command = ctx.CreateCommand( ) )
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "spGetRelationshipMetadataFromAppLibraryLatest";

                command.AddListParameter( "@relTypeIds", TableValuedParameterType.Guid, uniqueRelTypes.Select( g => (object)g ) );

                using ( IDataReader reader = command.ExecuteReader( ) )
                {
                    int relTypeIdCol = reader.GetOrdinal( "RelTypeId" );
                    int aliasCol = reader.GetOrdinal( "alias" );
                    int reverseAliasCol = reader.GetOrdinal( "reverseAlias" );
                    int cardinalityCol = reader.GetOrdinal( "cardinality" );
                    int cloneActionCol = reader.GetOrdinal( "cloneAction" );
                    int reverseCloneActionCol = reader.GetOrdinal( "reverseCloneAction" );

                    while ( reader.Read( ) )
                    {
                        Guid relTypeId = reader.GetGuid( relTypeIdCol );

                        var relTypeEntry = new RelationshipTypeEntry
                        {
                            TypeId = relTypeId,
                            Alias = reader.IsDBNull(aliasCol) ? null : reader.GetString( aliasCol ),
                            ReverseAlias = reader.IsDBNull( reverseAliasCol ) ? null : reader.GetString( reverseAliasCol ),
                            CloneAction = DecodeCloneAction( reader, cloneActionCol ),
                            ReverseCloneAction = DecodeCloneAction( reader, reverseCloneActionCol ),
                        };

                        results.Add( relTypeId, relTypeEntry );
                    }
                }
            }
            return results;
        }

        /// <summary>
        ///     Map cloneAction enum value GUIDs back to enum values.
        /// </summary>
        /// <param name="reader">The data reader.</param>
        /// <param name="column">The column to read from.</param>
        private CloneActionEnum_Enumeration? DecodeCloneAction( IDataReader reader, int column )
        {
            if ( reader.IsDBNull( column ) )
                return null;

            Guid cloneAction = reader.GetGuid( column );

            if ( cloneAction == Guids.CloneReferences )
                return CloneActionEnum_Enumeration.CloneReferences;
            if ( cloneAction == Guids.CloneEntities )
                return CloneActionEnum_Enumeration.CloneEntities;
            if ( cloneAction == Guids.Drop )
                return CloneActionEnum_Enumeration.Drop;

            return null;// assert false
        }
    }
}
