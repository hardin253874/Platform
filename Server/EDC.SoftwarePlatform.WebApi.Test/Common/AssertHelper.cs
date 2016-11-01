// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Common
{
	/// <summary>
	///     Assert helper methods.
	/// </summary>
	public static class AssertHelper
	{
		/// <summary>
		///     Verifies that the specified code throws an argument exception of the specified type with the specified parameter
		///     name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="testCode">The code to test.</param>
		/// <param name="paramName">Name of the parameter.</param>
		public static void ThrowsArgumentException<T>( Action testCode, string paramName ) where T : ArgumentException, new( )
		{
			T caughtException = null;

			try
			{
				testCode( );
			}
			catch ( Exception ex )
			{
				caughtException = ex as T;
			}

			Assert.IsNotNull( caughtException, "The caught exception should not be null." );
			if ( !string.IsNullOrEmpty( paramName ) )
			{
				Assert.AreEqual( paramName, caughtException.ParamName, "Unexpected param name." );
			}
		}

		/// <summary>
		///     Verifies that the specified code throws the exception of the specified type.
		/// </summary>
		/// <typeparam name="T">The expected exception type.</typeparam>
		/// <param name="testCode">The code to test.</param>
		public static void ThrowsException<T>( Action testCode ) where T : Exception, new( )
		{
			T caughtException = null;

			try
			{
				testCode( );
			}
			catch ( Exception ex )
			{
				caughtException = ex as T;
			}

			Assert.IsNotNull( caughtException, "The caught exception should not be null." );
		}


		/// <summary>
		///     Verifies that the specified code throws the exception of the specified type.
		/// </summary>
		/// <typeparam name="T">The expected exception type.</typeparam>
		/// <param name="testCode">The code to test.</param>
		/// <param name="message">The message.</param>
		public static void ThrowsException<T>( Action testCode, string message ) where T : Exception, new( )
		{
			T caughtException = null;

			try
			{
				testCode( );
			}
			catch ( Exception ex )
			{
				caughtException = ex as T;
			}

			Assert.IsNotNull( caughtException, "The caught exception should not be null." );
			Assert.AreEqual( caughtException.Message, message, "The exception message is invalid." );
		}
	}
}