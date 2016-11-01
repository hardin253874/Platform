// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    ///     Service that can return upgrade IDs for entities.
    /// </summary>
    public interface IUpgradeIdProvider
    {
        /// <summary>
		///     Gets the upgrade ID of an entity.
		/// </summary>
		/// <param name="entityId">The Int64 ID of an entity.</param>
		Guid GetUpgradeId(long entityId);

        /// <summary>
        ///     Gets the ID of an entity by upgrade ID.
        /// </summary>
        /// <param name="upgradeId">The fixed upgradeID guid of an entity.</param>
        /// <returns>The ID of the entity within the context of the current tenant, or -1 if not found.</returns>
        long GetIdFromUpgradeId( Guid upgradeId );

        /// <summary>
        ///     Gets the upgrade IDs of a multiple entities.
        /// </summary>
        /// <param name="entityId">The Int64 ID of an entity.</param>
        IDictionary<long, Guid> GetUpgradeIds(IEnumerable<long> entityIds);

        /// <summary>
        ///     Gets the upgrade IDs of a multiple entities.
        /// </summary>
        /// <param name="upgradeIds">The Int64 ID of an entity.</param>
        IDictionary<Guid, long> GetIdsFromUpgradeIds( IEnumerable<Guid> upgradeIds );
    }
}
