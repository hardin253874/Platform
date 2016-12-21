// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EDC.ReadiNow.Diagnostics;

/////
// ReSharper disable CheckNamespace
/////

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Target class.
	/// </summary>
	public static class TargetHelper
	{
		/// <summary>
		///     The FQDN cache
		/// </summary>
		private static readonly ConcurrentDictionary<string, object> FqdnCache = new ConcurrentDictionary<string, object>( );

		/// <summary>
		///     Gets the name of the fully qualified type.
		/// </summary>
		/// <value>
		///     The name of the fully qualified type.
		/// </value>
		private static string FullyQualifiedTypeName(Target target)
		{
            string fqtn = string.Empty;

            if (!string.IsNullOrEmpty(target.TypeName))
            {
                fqtn += target.TypeName;
            }

		    string asmName = ClassHelper.CheckAssemblyName( target.AssemblyName );

            if (!string.IsNullOrEmpty( asmName ) )
            {
                fqtn += string.Format(", {0}", asmName );
            }

            return fqtn;
		}

		/// <summary>
		///     Invokes the specified function.
		/// </summary>
		/// <typeparam name="T">Type of entity that the target is being invoked on.</typeparam>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="target">The target.</param>
		/// <param name="action">The action.</param>
		/// <param name="entities">The entities.</param>
		/// <returns>
		///     True to cancel the operation; false otherwise.
		/// </returns>
		public static bool Invoke<T, TInterface>( this Target target, Func<TInterface, IEnumerable<T>, bool> action, IEnumerable<T> entities )
			where TInterface : class, IEntityEvent
		{
			object instance;

			string fqdn = FullyQualifiedTypeName( target );

			if ( !FqdnCache.TryGetValue( fqdn, out instance ) )
			{
				try
				{
					Type type = Type.GetType( fqdn, true );

					instance = Activator.CreateInstance( type );

					FqdnCache.AddOrUpdate( fqdn, instance, ( key, value ) => instance );
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteWarning( "Failed to instantiate the specified target type '{0}'. Exception: {1}", fqdn, exc.Message );
				}
			}

			var interfaceInstance = instance as TInterface;

			if ( interfaceInstance != null )
			{
				return action( interfaceInstance, entities );
			}

			return false;
		}

		/// <summary>
		///     Invokes the specified action.
		/// </summary>
        /// <param name="target">The target.</param>
        /// <typeparam name="T">Type of entity that the target is being invoked on.</typeparam>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <param name="action">The action.</param>
		/// <param name="entities">The entities.</param>
		public static void Invoke<T, TInterface>( this Target target, Action<TInterface, IEnumerable<T>> action, IEnumerable<T> entities )
			where TInterface : class, IEntityEvent
		{
			Func<TInterface, IEnumerable<T>, bool> f = ( interfaceInstances, entityInstances ) =>
				{
					action( interfaceInstances, entityInstances );
					return false;
				};

			target.Invoke( f, entities );
		}
	}
}

/////
// ReSharper restore CheckNamespace
/////