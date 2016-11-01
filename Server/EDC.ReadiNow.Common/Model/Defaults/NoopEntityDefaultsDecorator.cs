// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Implementation of IEntityDefaultsDecorator that does nothing. For efficiency.
    /// </summary>
    internal class NoopEntityDefaultsDecorator : IEntityDefaultsDecorator
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static readonly NoopEntityDefaultsDecorator Instance = new NoopEntityDefaultsDecorator( );

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="entities">The entites to update.</param>
        public void SetDefaultValues( IEnumerable<IEntity> entities )
        {
            if ( entities == null )
                throw new ArgumentNullException( "entities" );
        }
    }
}
