// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Service for persisting entities.
    /// </summary>
    class EntitySaver : IEntitySaver
    {
        /// <summary>
        /// Save the entities.
        /// </summary>
        /// <param name="entities"></param>
        public void SaveEntities( IEnumerable<IEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            Entity.Save(entities);
        }
    }
}
