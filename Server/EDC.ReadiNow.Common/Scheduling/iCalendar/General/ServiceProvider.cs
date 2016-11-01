// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ServiceProvider class.
	/// </summary>
	public class ServiceProvider : IServiceProvider
	{
		/// <summary>
		///     Named services.
		/// </summary>
		private readonly IDictionary<string, object> _namedServices = new Dictionary<string, object>( );

		/// <summary>
		///     Typed services.
		/// </summary>
		private readonly IDictionary<Type, object> _typedServices = new Dictionary<Type, object>( );

		#region IServiceProvider Members

		/// <summary>
		///     Gets the service object of the specified type.
		/// </summary>
		/// <param name="serviceType">An object that specifies the type of service object to get.</param>
		/// <returns>
		///     A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type
		///     <paramref
		///         name="serviceType" />
		///     .
		/// </returns>
		public virtual object GetService( Type serviceType )
		{
			if ( _typedServices.ContainsKey( serviceType ) )
			{
				return _typedServices[ serviceType ];
			}
			return null;
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public virtual object GetService( string name )
		{
			if ( _namedServices.ContainsKey( name ) )
			{
				return _namedServices[ name ];
			}
			return null;
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual T GetService<T>( )
		{
			object service = GetService( typeof ( T ) );
			if ( service is T )
			{
				return ( T ) service;
			}
			return default( T );
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public virtual T GetService<T>( string name )
		{
			object service = GetService( name );
			if ( service is T )
			{
				return ( T ) service;
			}
			return default( T );
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="obj">The obj.</param>
		public virtual void SetService( string name, object obj )
		{
			if ( !string.IsNullOrEmpty( name ) && obj != null )
			{
				_namedServices[ name ] = obj;
			}
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void SetService( object obj )
		{
			if ( obj != null )
			{
				Type type = obj.GetType( );
				_typedServices[ type ] = obj;

				// Get interfaces for the given type
				foreach ( Type iface in type.GetInterfaces( ) )
				{
					_typedServices[ iface ] = obj;
				}
			}
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="type">The type.</param>
		public virtual void RemoveService( Type type )
		{
			if ( type != null )
			{
				if ( _typedServices.ContainsKey( type ) )
				{
					_typedServices.Remove( type );
				}

				// Get interfaces for the given type
				foreach ( Type iface in type.GetInterfaces( ) )
				{
					if ( _typedServices.ContainsKey( iface ) )
					{
						_typedServices.Remove( iface );
					}
				}
			}
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="name">The name.</param>
		public virtual void RemoveService( string name )
		{
			if ( _namedServices.ContainsKey( name ) )
			{
				_namedServices.Remove( name );
			}
		}

		#endregion
	}
}