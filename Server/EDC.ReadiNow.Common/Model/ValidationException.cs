// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Exception that is raised when validation fails.
	///     Note that this exception can not store the field itself as the exception
	///     is converted into a Fault on a different thread.
	/// </summary>
	[Serializable]
	public class ValidationException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ValidationException" /> class.
		/// </summary>
		public ValidationException( )
			: base( "Field validation failed." )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ValidationException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ValidationException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ValidationException" /> class.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="message">The message.</param>
		public ValidationException( Field field, string message )
			: this( message )
		{
			FieldName = field.Name;
			FieldId = field.Id;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ValidationException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public ValidationException( string message, Exception inner )
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
		protected ValidationException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}

		/// <summary>
		///     The field that failed validation
		/// </summary>
		public long FieldId
		{
			get;
			set;
		}

		/// <summary>
		///     The name of the field that failed validation
		/// </summary>
		public string FieldName
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

			info.AddValue( "FieldId", FieldId );
			info.AddValue( "FieldName", FieldName );
		}
	}
}