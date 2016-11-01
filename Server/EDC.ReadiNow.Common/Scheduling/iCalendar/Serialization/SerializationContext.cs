// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     SerializationContext class.
	/// </summary>
	public sealed class SerializationContext : ISerializationContext
	{
		/// <summary>
		///     Default.
		/// </summary>
		private static SerializationContext _default;

		/// <summary>
		///     Stack.
		/// </summary>
		private readonly Stack<WeakReference> _stack = new Stack<WeakReference>( );

		/// <summary>
		///     Service Provider.
		/// </summary>
		private ServiceProvider _serviceProvider = new ServiceProvider( );

		/// <summary>
		///     Initializes a new instance of the <see cref="SerializationContext" /> class.
		/// </summary>
		public SerializationContext( )
		{
			// Add some services by default
			SetService( new SerializationSettings( ) );
			SetService( new SerializerFactory( ) );
			SetService( new ComponentFactory( ) );
			SetService( new DataTypeMapper( ) );
			SetService( new EncodingStack( ) );
			SetService( new EncodingProvider( this ) );
			SetService( new CompositeProcessor<IICalendar>( ) );
			SetService( new CompositeProcessor<ICalendarComponent>( ) );
			SetService( new CompositeProcessor<ICalendarProperty>( ) );
		}

		#region ISerializationContext Members

		/// <summary>
		///     Pushes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Push( object item )
		{
			if ( item != null )
			{
				_stack.Push( new WeakReference( item ) );
			}
		}

		/// <summary>
		///     Pops this instance.
		/// </summary>
		/// <returns></returns>
		public object Pop( )
		{
			if ( _stack.Count > 0 )
			{
				WeakReference r = _stack.Pop( );
				if ( r.IsAlive )
				{
					return r.Target;
				}
			}
			return null;
		}

		/// <summary>
		///     Peeks this instance.
		/// </summary>
		/// <returns></returns>
		public object Peek( )
		{
			if ( _stack.Count > 0 )
			{
				WeakReference r = _stack.Peek( );
				if ( r.IsAlive )
				{
					return r.Target;
				}
			}
			return null;
		}

		#endregion

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
		public object GetService( Type serviceType )
		{
			return _serviceProvider.GetService( serviceType );
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public object GetService( string name )
		{
			return _serviceProvider.GetService( name );
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetService<T>( )
		{
			return _serviceProvider.GetService<T>( );
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public T GetService<T>( string name )
		{
			return _serviceProvider.GetService<T>( name );
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="obj">The obj.</param>
		public void SetService( string name, object obj )
		{
			_serviceProvider.SetService( name, obj );
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public void SetService( object obj )
		{
			_serviceProvider.SetService( obj );
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="type">The type.</param>
		public void RemoveService( Type type )
		{
			_serviceProvider.RemoveService( type );
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="name">The name.</param>
		public void RemoveService( string name )
		{
			_serviceProvider.RemoveService( name );
		}

		#endregion

		/// <summary>
		///     Gets the Singleton instance of the SerializationContext class.
		/// </summary>
		public static ISerializationContext Default
		{
			get
			{
				if ( _default == null )
				{
					_default = new SerializationContext( );
				}

				// Create a new serialization context that doesn't contain any objects
				// (and is non-static).  That way, if any objects get pushed onto
				// the serialization stack when the Default serialization context is used,
				// and something goes wrong and the objects don't get popped off the stack,
				// we don't need to worry (as much) about a memory leak, because the
				// objects weren't pushed onto a stack referenced by a static variable.
				var ctx = new SerializationContext
					{
						_serviceProvider = _default._serviceProvider
					};
				return ctx;
			}
		}
	}
}