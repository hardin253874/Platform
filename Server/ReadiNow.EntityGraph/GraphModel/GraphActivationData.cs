// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// Contains information to activate a strong entity type with a GraphEntity.
    /// </summary>
    class GraphActivationData : IActivationData
    {
        GraphEntity _entity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entity">GraphEntity to use during activation.</param>
        internal GraphActivationData( GraphEntity entity )
        {
            if ( entity == null )
                throw new ArgumentNullException( "entity" );

            _entity = entity;
        }

        /// <summary>
        /// Returns the GraphEntity.
        /// </summary>
        public IEntity CreateEntity( )
        {
            return _entity;
        }
    }
}
