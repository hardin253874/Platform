// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     SerializerBase class.
	/// </summary>
	public abstract class SerializerBase : IStringSerializer
	{
		/// <summary>
		///     Serialization context.
		/// </summary>
		private ISerializationContext _serializationContext;

		/// <summary>
		///     Initializes a new instance of the <see cref="SerializerBase" /> class.
		/// </summary>
		protected SerializerBase( )
		{
			_serializationContext = Serialization.SerializationContext.Default;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SerializerBase" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		protected SerializerBase( ISerializationContext ctx )
		{
			_serializationContext = ctx;
		}

		/// <summary>
		///     Gets or sets the serialization context.
		/// </summary>
		/// <value>
		///     The serialization context.
		/// </value>
		public virtual ISerializationContext SerializationContext
		{
			get
			{
				return _serializationContext;
			}
			set
			{
				_serializationContext = value;
			}
		}

		/// <summary>
		///     Gets the type of the target.
		/// </summary>
		/// <value>
		///     The type of the target.
		/// </value>
		public abstract Type TargetType
		{
			get;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public abstract string SerializeToString( object obj );

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public abstract object Deserialize( TextReader tr );

		/// <summary>
		///     Deserializes the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public object Deserialize( Stream stream, Encoding encoding )
		{
			object obj;
			using ( var sr = new StreamReader( stream, encoding ) )
			{
				// Push the current encoding on the stack
				var encodingStack = GetService<IEncodingStack>( );
				encodingStack.Push( encoding );

				obj = Deserialize( sr );

				// Pop the current encoding off the stack
				encodingStack.Pop( );
			}
			return obj;
		}

		/// <summary>
		///     Serializes the specified obj.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="encoding">The encoding.</param>
		public void Serialize( object obj, Stream stream, Encoding encoding )
		{
			// NOTE: we don't use a 'using' statement here because
			// we don't want the stream to be closed by this serialization.
			// Fixes bug #3177278 - Serialize closes stream
			var sw = new StreamWriter( stream, encoding );

			// Push the current object onto the serialization stack
			SerializationContext.Push( obj );

			// Push the current encoding on the stack
			var encodingStack = GetService<IEncodingStack>( );
			encodingStack.Push( encoding );

			sw.Write( SerializeToString( obj ) );
			sw.Flush( );
			// Pop the current encoding off the serialization stack
			encodingStack.Pop( );

			// Pop the current object off the serialization stack
			SerializationContext.Pop( );
		}

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
			if ( SerializationContext != null )
			{
				return SerializationContext.GetService( serviceType );
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
			if ( SerializationContext != null )
			{
				return SerializationContext.GetService( name );
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
			if ( SerializationContext != null )
			{
				return SerializationContext.GetService<T>( );
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
			if ( SerializationContext != null )
			{
				return SerializationContext.GetService<T>( name );
			}
			return default( T );
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="obj">The obj.</param>
		public void SetService( string name, object obj )
		{
			if ( SerializationContext != null )
			{
				SerializationContext.SetService( name, obj );
			}
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public void SetService( object obj )
		{
			if ( SerializationContext != null )
			{
				SerializationContext.SetService( obj );
			}
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="type">The type.</param>
		public void RemoveService( Type type )
		{
			if ( SerializationContext != null )
			{
				SerializationContext.RemoveService( type );
			}
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="name">The name.</param>
		public void RemoveService( string name )
		{
			if ( SerializationContext != null )
			{
				SerializationContext.RemoveService( name );
			}
		}

		#endregion
	}
}