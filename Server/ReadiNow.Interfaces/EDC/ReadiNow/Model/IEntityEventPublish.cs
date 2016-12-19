// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    ///     Interface for specifying publish events.
    /// </summary>
    public interface IEntityEventPublish : IEntityEvent
    {
        /// <summary>
        ///     Called after publishing an application.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        void OnAfterPublish(IEnumerable<IEntity> entities, IDictionary<string, object> state);


        /// <summary>
        ///     Called if a failure occurs publishing an application
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        void OnPublishFailed(IEnumerable<IEntity> entities, IDictionary<string, object> state);
    }
}