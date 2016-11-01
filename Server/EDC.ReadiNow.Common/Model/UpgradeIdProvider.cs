// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using ReadiNow.Database;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    ///     Service that can return upgrade IDs for entities.
    /// </summary>
    class UpgradeIdProvider : IUpgradeIdProvider
    {
        private IDatabaseProvider DatabaseProvider { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="databaseProvider">Service for accessing the database.</param>
        public UpgradeIdProvider(IDatabaseProvider databaseProvider)
        {
            if (databaseProvider == null)
                throw new ArgumentNullException(nameof(databaseProvider));
            DatabaseProvider = databaseProvider;
        }


        /// <summary>
        ///     Gets the upgrade ID of an entity.
        /// </summary>
        /// <param name="entityId">The Int64 ID of an entity.</param>
        public Guid GetUpgradeId(long entityId)
        {
            // TODO: This is a low use method, however consider offering some form of cache.

            if (entityId <= 0)
            {
                return Guid.Empty;
            }

            const string sql = @"select UpgradeId from Entity where Id = @id and TenantId = @tenantId";

            using (IDatabaseContext ctx = DatabaseProvider.GetContext())
            {
                using (IDbCommand command = ctx.CreateCommand(sql))
                {
                    command.AddParameter("@tenantId", DbType.Int64, RequestContext.TenantId);
                    command.AddParameter("@id", DbType.Int64, entityId);

                    object oRes = command.ExecuteScalar();
                    if (oRes == null)
                    {
                        return Guid.Empty;
                    }
                    return (Guid)oRes;
                }
            }
        }


        /// <summary>
        ///     Gets the upgrade IDs of a multiple entities.
        /// </summary>
        /// <param name="entityIds">The Int64 ID of an entity.</param>
        public IDictionary<long, Guid> GetUpgradeIds(IEnumerable<long> entityIds)
        {
            // TODO: This is a low use method, however consider offering some form of cache.

            if (entityIds == null)
                throw new ArgumentNullException(nameof(entityIds));

            const string sql = @"select e.Id, e.UpgradeId from Entity e join @ids i on e.Id = i.Id and e.TenantId = @tenantId";

            var result = new Dictionary<long, Guid>();

            using (IDatabaseContext ctx = DatabaseProvider.GetContext())
            using (IDbCommand command = ctx.CreateCommand(sql))
            {
                command.AddParameter("@tenantId", DbType.Int64, RequestContext.TenantId);
                command.AddIdListParameter("@ids", entityIds);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        Guid upgradeId = reader.GetGuid(1);
                        result.Add(id, upgradeId);
                    }
                }
            }

            return result;
        }


        /// <summary>
		///     Gets the ID of an entity by upgrade ID.
		/// </summary>
		/// <param name="upgradeId">The fixed upgradeID guid of an entity.</param>
		/// <returns>The ID of the entity within the context of the current tenant, or -1 if not found.</returns>
		public long GetIdFromUpgradeId( Guid upgradeId )
        {
            // TODO: This is a low use method, however consider offering some form of cache.

            const string sql = @"select Id from Entity where UpgradeId = @upgradeId and TenantId = @tenantId";

            using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
            {
                using ( IDbCommand command = ctx.CreateCommand( sql ) )
                {
                    ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
                    ctx.AddParameter( command, "@upgradeId", DbType.Guid, upgradeId );

                    object oRes = command.ExecuteScalar( );
                    if ( oRes == null )
                    {
                        return -1;
                    }
                    return (long)oRes;
                }
            }
        }

        /// <summary>
        ///     Gets the upgrade IDs of a multiple entities.
        /// </summary>
        /// <param name="upgradeIds">The Int64 ID of an entity.</param>
        public IDictionary<Guid, long> GetIdsFromUpgradeIds( IEnumerable<Guid> upgradeIds )
        {
            // TODO: This is a low use method, however consider offering some form of cache.

            if ( upgradeIds == null )
                throw new ArgumentNullException( nameof( upgradeIds ) );

            const string sql = @"select e.UpgradeId, e.Id from Entity e join @ids i on e.UpgradeId = i.Id and e.TenantId = @tenantId";

            var result = new Dictionary<Guid, long>( );

            using ( IDatabaseContext ctx = DatabaseProvider.GetContext( ) )
            using ( IDbCommand command = ctx.CreateCommand( sql ) )
            {
                command.AddParameter( "@tenantId", DbType.Int64, RequestContext.TenantId );
                command.AddListParameter( "@ids", TableValuedParameterType.Guid, upgradeIds.Select( g => (object)g ) );

                using ( IDataReader reader = command.ExecuteReader( ) )
                {
                    while ( reader.Read( ) )
                    {
                        Guid upgradeId = reader.GetGuid( 0 );
                        long id = reader.GetInt64( 1 );
                        result.Add( upgradeId, id );
                    }
                }
            }

            return result;
        }
    }
}
