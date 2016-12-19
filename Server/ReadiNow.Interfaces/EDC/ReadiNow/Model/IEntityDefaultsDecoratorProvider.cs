// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiNow.Annotations;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Creates a IEntityDefaultsDecorator for a particular type.
    /// </summary>
    /// <remarks>
    /// This provider layer exists so that any up-front costs for a type only need to be done once.
    /// </remarks>
    public interface IEntityDefaultsDecoratorProvider
    {
        /// <summary>
        /// Creates a IEntityDefaultsDecorator for a particular type.
        /// </summary>
        /// <param name="typeId">The type.</param>
        /// <returns>A default-value decorator.</returns>
        [NotNull]
        IEntityDefaultsDecorator GetDefaultsDecorator( long typeId );
    }
}
