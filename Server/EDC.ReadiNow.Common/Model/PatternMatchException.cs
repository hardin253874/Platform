// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Exception that is raised when a pattern match fails.
	/// </summary>
	/// <remarks>
	///     The pattern that caused the match failure can be found via the 'Pattern' property.
	/// </remarks>
	[Serializable]
	public class PatternMatchException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="PatternMatchException" /> class.
		/// </summary>
		public PatternMatchException( )
			: base( "The specified value does not conform to the required pattern." )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PatternMatchException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="pattern">The pattern.</param>
		public PatternMatchException( string message, string pattern )
			: base( message )
		{
			Pattern = pattern;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PatternMatchException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public PatternMatchException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PatternMatchException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public PatternMatchException( string message, Exception inner )
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
		protected PatternMatchException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}

		/// <summary>
		///     Gets or sets the pattern that caused the failure.
		/// </summary>
		/// <value>
		///     The pattern.
		/// </value>
		public string Pattern
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

			info.AddValue( "Pattern", Pattern );
		}
	}
}