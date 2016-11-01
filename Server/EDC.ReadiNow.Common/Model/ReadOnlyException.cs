// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Exception that is raised when a read-only entity attempts to modify it's data.
	/// </summary>
	/// <remarks>
	///     To convert a read-only entity to a writable version, call its 'AsWritable' method.
	///     The entity that threw the ReadOnlyException can be found via the 'Entity' property.
	/// </remarks>
	[Serializable]
	public class ReadOnlyException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ReadOnlyException" /> class.
		/// </summary>
		public ReadOnlyException( )
			: base( "The specified entity is read-only." )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ReadOnlyException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ReadOnlyException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ReadOnlyException" /> class.
		/// </summary>
		/// <param name="entity">The entity.</param>
		public ReadOnlyException( IEntity entity )
			: this( )
		{
			Entity = entity;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ReadOnlyException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public ReadOnlyException( string message, Exception inner )
			: base( message, inner )
		{
		}

		/// <summary>
		///     A constructor is needed for serialization when an exception propagates from a remoting server to the client.
		/// </summary>
		/// <param name="info">
		///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.
		/// </param>
		/// <exception cref="T:System.ArgumentNullException">
		///     The <paramref name="info" /> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		///     The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0).
		/// </exception>
		protected ReadOnlyException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}

		/// <summary>
		///     Gets or sets the entity.
		/// </summary>
		/// <value>
		///     The entity.
		/// </value>
		public IEntity Entity
		{
			get;
			set;
		}

		/// <summary>
		///     When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
		/// </summary>
		/// <param name="info">
		///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.
		/// </param>
		/// <exception cref="T:System.ArgumentNullException">
		///     The <paramref name="info" /> parameter is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <PermissionSet>
		///     <IPermission
		///         class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
		///         version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
		///     <IPermission
		///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
		///         version="1" Flags="SerializationFormatter" />
		/// </PermissionSet>
		public override void GetObjectData( SerializationInfo info, StreamingContext context )
		{
			base.GetObjectData( info, context );

			if ( info == null )
			{
				throw new ArgumentNullException( "info" );
			}

			info.AddValue( "Entity", Entity );
		}
	}
}