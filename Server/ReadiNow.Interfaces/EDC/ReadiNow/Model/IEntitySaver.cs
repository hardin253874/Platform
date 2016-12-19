// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Interface for a service that can persist entities.
    /// </summary>
    public interface IEntitySaver
    {
        /// <summary>
        /// Save the entities.
        /// </summary>
        /// <param name="entities"></param>
        void SaveEntities( [NotNull] IEnumerable<IEntity> entities);
    }
}
