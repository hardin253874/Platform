// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Net;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace EDC.Security.Test
// ReSharper restore CheckNamespace
{
	/// <summary>
	///     This is a test class for the CredentialHelper type
	/// </summary>
	[TestFixture]
	public class FileCreationComparerTests
	{
		/// <summary>
		///     Tests that GetFullyQualifiedName returns the correct fully qualified account name, when username, domain and password are specified.
		/// </summary>
		[Test]
		public void GetFullyQualifiedName_ValidUsernameDomainPassword_ReturnsCorrectName( )
		{
			var credential = new NetworkCredential( "username", "password", "domain" );
			string name = CredentialHelper.GetFullyQualifiedName( credential );

			Assert.AreEqual( name, "domain\\username" );
		}

		/// <summary>
		///     Tests that GetFullyQualifiedName returns the correct fully qualified account name, when only the username and password are specified.
		/// </summary>
		[Test]
		public void GetFullyQualifiedName_ValidUsernamePassword_ReturnsCorrectName( )
		{
			var credential = new NetworkCredential( "username", "password" );
			string name = CredentialHelper.GetFullyQualifiedName( credential );

			Assert.AreEqual( name, "username" );
		}
	}
}