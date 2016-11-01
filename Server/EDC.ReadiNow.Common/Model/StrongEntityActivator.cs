// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Activates strong entities.
    /// </summary>
    public class StrongEntityActivator : IStrongEntityActivator
    {
        public static StrongEntityActivator Instance = new StrongEntityActivator( );

        /// <summary>
        ///     Creates the instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="activationData">The activation data.</param>
        /// <returns>A new instance of the specified type.</returns>
        public IEntity CreateInstance( Type type, IActivationData activationData )
        {
            return ( IEntity ) Activator.CreateInstance( type, BindingFlags.Instance | BindingFlags.NonPublic, null, new object [ ]
				{
					activationData
				}, null );
        }

        /// <summary>
        ///     Creates the instance.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        /// <returns>
        ///     A new instance of the specified type.
        /// </returns>
        public T CreateInstance<T>( IActivationData activationData )
            where T : class, IEntity
        {
            return ( T ) CreateInstance( typeof( T ), activationData );
        }
    }
}
