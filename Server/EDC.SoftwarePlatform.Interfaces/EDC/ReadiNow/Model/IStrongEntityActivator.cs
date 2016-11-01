// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Activates strong entities.
    /// </summary>
    public interface IStrongEntityActivator
    {
        /// <summary>
        ///     Creates the instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="activationData">The activation data.</param>
        /// <returns>A new instance of the specified type.</returns>
        IEntity CreateInstance( Type type, IActivationData activationData );

        /// <summary>
        ///     Creates the instance.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        /// <returns>
        ///     A new instance of the specified type.
        /// </returns>
        T CreateInstance<T>( IActivationData activationData ) where T : class, IEntity;
    }
}
