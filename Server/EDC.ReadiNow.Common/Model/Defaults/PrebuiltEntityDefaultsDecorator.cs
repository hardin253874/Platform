// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Common;
using System.Collections.Generic;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Decorator that ensures that all defaults values are set on entities.
    /// </summary>
    /// <remarks>
    /// Use a IEntityDefaultsDecoratorProvider to get an instance that is pre-loaded for processing a particular type.
    /// </remarks>
    internal class PrebuiltEntityDefaultsDecorator : IEntityDefaultsDecorator
    {
        // The actions to perform on each entity.
        private readonly IReadOnlyCollection<Action<IEnumerable<IEntity>>> _actions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="actions">List of actions required to apply the default values.</param>
        internal PrebuiltEntityDefaultsDecorator( [NotNull] IReadOnlyCollection<Action<IEnumerable<IEntity>>> actions )
        {
            if ( actions == null )
                throw new ArgumentNullException( "actions" );
            _actions = actions;
        }

        /// <summary>
        /// Set default values on fields and relationships that do not have a value set.
        /// </summary>
        /// <param name="entities">The entites to update.</param>
        public void SetDefaultValues( IEnumerable<IEntity> entities )
        {
            if ( entities == null )
                throw new ArgumentNullException( "entities" );

            foreach ( Action<IEnumerable<IEntity>> action in _actions )
            {
                if ( action == null )
                    throw new Exception( "Internal error. Actions must not be null." );
                action( entities.WhereNotNull( ) );
            }
        }

    }
}
