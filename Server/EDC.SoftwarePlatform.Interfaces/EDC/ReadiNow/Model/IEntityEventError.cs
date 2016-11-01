// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Interface for specifying entity error events
    /// </summary>
    public interface IEntityEventError : IEntityEvent
    {
        /// <summary>
        ///     Called if a failure occurs saving the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        /// </returns>
        void OnSaveFailed(IEnumerable<IEntity> entities, IDictionary<string, object> state);


        /// <summary>
        ///     Called if a failure occurs deleting the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        void OnDeleteFailed(IEnumerable<long> entities, IDictionary<string, object> state);
    }
}
