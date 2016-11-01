// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Net;
using EDC.Database;
using NUnit.Framework;

namespace EDC.Test.Database
{
	/// <summary>
	///     This is a test class for the SqlDatabaseInfo type
	/// </summary>
	[TestFixture]
	public class SqlDatabaseInfoTests
	{
		/// <summary>
		///     Tests that ConnectionString returns a valid connection string when using a basic database authentication template.
		/// </summary>
		[Test]
		public void GetConnectionString_BasicDatabaseAuthenticationTemplate_ReturnsCorrectConnectionString( )
		{
			var databaseInfo = new SqlDatabaseInfo( "LocalServer", "Northwind", DatabaseAuthentication.Database, new NetworkCredential( "username", "password", "domain" ), 60, 30, 60 );

			string connectionString = databaseInfo.ConnectionString;

			Assert.IsTrue( connectionString.Contains( "Data Source=LocalServer" ) );
			Assert.IsTrue( connectionString.Contains( "Initial Catalog=Northwind" ) );
			Assert.IsTrue( connectionString.Contains( "User ID=domain\\username" ) );
			Assert.IsTrue( connectionString.Contains( "Password=password" ) );
			Assert.IsTrue( connectionString.Contains( "Connect Timeout=60" ) );
            Assert.IsTrue( connectionString.Contains( "Max Pool Size=200" ));
        }

		/// <summary>
		///     Tests that ConnectionString returns a valid connection string when using a basic integrated authentication template.
		/// </summary>
		[Test]
		public void GetConnectionString_BasicIntegratedAuthenticationTemplate_ReturnsCorrectConnectionString( )
		{
			var databaseInfo = new SqlDatabaseInfo( "LocalServer", "Northwind", DatabaseAuthentication.Integrated, null, 60, 30, 60, 50 );

			string connectionString = databaseInfo.ConnectionString;

			Assert.IsTrue( connectionString.Contains( "Data Source=LocalServer" ) );
			Assert.IsTrue( connectionString.Contains( "Initial Catalog=Northwind" ) );
			Assert.IsTrue( connectionString.Contains( "Integrated Security=True" ) );
			Assert.IsTrue( connectionString.Contains( "Connect Timeout=60" ) );
            Assert.IsTrue( connectionString.Contains( "Max Pool Size=50" ) );
        }

		/// <summary>
		///     Tests that ConnectionString returns the correct value when the authentication mode is modified.
		/// </summary>
		[Test]
		public void GetConnectionString_ModifyAuthentication_ReturnsCorrectConnectionString( )
		{
			// Set the base properties
			var databaseInfo = new SqlDatabaseInfo( "LocalServer", "Northwind", DatabaseAuthentication.Integrated, null, 60, 30, 60 );

			string connectionString = databaseInfo.ConnectionString;
			Assert.IsTrue( connectionString.Contains( "Integrated Security=True" ) );

			// Update the base properties
			databaseInfo = new SqlDatabaseInfo( databaseInfo.Server, databaseInfo.Database, DatabaseAuthentication.Database, new NetworkCredential( "username", "password", "domain" ), databaseInfo.ConnectionTimeout, databaseInfo.CommandTimeout, databaseInfo.TransactionTimeout );

			connectionString = databaseInfo.ConnectionString;
			Assert.IsTrue( connectionString.Contains( "Initial Catalog=Northwind" ) );
			Assert.IsTrue( connectionString.Contains( "User ID=domain\\username" ) );
		}

		/// <summary>
		///     Tests that ConnectionString returns the correct value when the database is modified.
		/// </summary>
		[Test]
		public void GetConnectionString_ModifyDatabase_ReturnsCorrectConnectionString( )
		{
			// Set the base properties
			var databaseInfo = new SqlDatabaseInfo( "LocalServer", "Database1", DatabaseAuthentication.Integrated, null, 60, 30, 60 );

			string connectionString = databaseInfo.ConnectionString;
			Assert.IsTrue( connectionString.Contains( "Initial Catalog=Database1" ) );

			// Update the base properties
			databaseInfo = new SqlDatabaseInfo( databaseInfo.Server, "Database2", databaseInfo.Authentication, databaseInfo.Credentials, databaseInfo.ConnectionTimeout, databaseInfo.CommandTimeout, databaseInfo.TransactionTimeout );

			connectionString = databaseInfo.ConnectionString;
			Assert.IsTrue( connectionString.Contains( "Initial Catalog=Database2" ) );
		}

		/// <summary>
		///     Tests that ConnectionString returns the correct value when the server is modified.
		/// </summary>
		[Test]
		public void GetConnectionString_ModifyServer_ReturnsCorrectConnectionString( )
		{
			var databaseInfo = new SqlDatabaseInfo( "Server1", "Northwind", DatabaseAuthentication.Integrated, null, 60, 30, 60 );

			string connectionString = databaseInfo.ConnectionString;
			Assert.IsTrue( connectionString.Contains( "Data Source=Server1" ) );

			// Update the base properties
			databaseInfo = new SqlDatabaseInfo( "Server2", databaseInfo.Database, databaseInfo.Authentication, databaseInfo.Credentials, databaseInfo.ConnectionTimeout, databaseInfo.CommandTimeout, databaseInfo.TransactionTimeout );

			connectionString = databaseInfo.ConnectionString;
			Assert.IsTrue( connectionString.Contains( "Data Source=Server2" ) );
		}

		/// <summary>
		///     Tests that ConnectionString returns the correct value when the database is modified.
		/// </summary>
		[Test]
		public void GetConnectionString_ModifyTimeout_ReturnsCorrectConnectionString( )
		{
			// Set the base properties
			var databaseInfo = new SqlDatabaseInfo( "LocalServer", "Database1", DatabaseAuthentication.Integrated, null, 60, 30, 60 );

			string connectionString = databaseInfo.ConnectionString;
			Assert.IsTrue( connectionString.Contains( "Connect Timeout=60" ) );

			// Update the base properties
			databaseInfo = new SqlDatabaseInfo( databaseInfo.Server, databaseInfo.Database, databaseInfo.Authentication, databaseInfo.Credentials, 30, databaseInfo.CommandTimeout, databaseInfo.TransactionTimeout );

			connectionString = databaseInfo.ConnectionString;
			Assert.IsTrue( connectionString.Contains( "Connect Timeout=30" ) );
		}
	}
}