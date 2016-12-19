// Copyright 2011-2016 Global Software Innovation Pty Ltd
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
    public interface IEntityDefaultsDecorator
    {
        /// <summary>
        /// Set default values on fields and relationships that do not have a value set.
        /// </summary>
        /// <param name="entities">The entites to update.</param>
        void SetDefaultValues( [NotNull] IEnumerable<IEntity> entities );
    }
}
