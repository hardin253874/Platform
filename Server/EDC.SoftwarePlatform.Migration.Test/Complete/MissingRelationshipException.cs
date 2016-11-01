// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Test.Complete
{
	/// <summary>
	///     Missing Relationship exception.
	/// </summary>
	public class MissingRelationshipException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MissingRelationshipException" /> class.
		/// </summary>
		public MissingRelationshipException( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MissingRelationshipException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public MissingRelationshipException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MissingRelationshipException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="exception">The exception.</param>
		public MissingRelationshipException( string message, Exception exception )
			: base( message, exception )
		{
		}
	}
}