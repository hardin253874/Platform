// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiNow.Annotations;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// An interface for creating a service that finds entities by field value.
    /// </summary>
    public interface IEntityResolverProvider
    {
        /// <summary>
        /// Create a resolver to find entities with a field of a particular value.
        /// </summary>
        /// <param name="typeId">The type of resource to search.</param>
        /// <param name="fieldId">The field to search on.</param>
        /// <param name="secured">True if the generated resolver should be secured to the current user.</param>
        /// <returns>An IEntityResolver that can efficiently locate instances based on the field provided.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// typeId
        /// or
        /// fieldId
        /// </exception>
        [NotNull]
        IEntityResolver GetResolverForField( long typeId, long fieldId, bool secured );
    }
}
