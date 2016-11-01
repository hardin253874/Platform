// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database;
using NUnit.Framework;
using EDC.ReadiNow.Database;
using System.Data;
using System.Collections.Generic;

namespace EDC.Test.Database
{
	/// <summary>
	///     This is a test class for the DatabaseHelper type.
	/// </summary>
	[TestFixture]
	public class DatabaseHelperTests
	{
		[Test]
		public void DatabaseExists_InvalidLocalDatabase_ReturnsFalse( )
		{
			var databaseInfo = new SqlDatabaseInfo( "localhost", "master", DatabaseAuthentication.Integrated, null );

			Assert.IsFalse( DatabaseHelper.DatabaseExists( databaseInfo, Guid.NewGuid( ).ToString( "B" ) ) );
		}

		[Test]
		public void DatabaseExists_ValidLocalDatabase_ReturnsTrue( )
		{
			var databaseInfo = new SqlDatabaseInfo( "localhost", "master", DatabaseAuthentication.Integrated );

			Assert.IsTrue( DatabaseHelper.DatabaseExists( databaseInfo, "master" ) );

			DatabaseHelper.GetDatabases( databaseInfo );

			DatabaseHelper.GetDatabaseVersion( databaseInfo );
			DatabaseHelper.GetDatabasePath( databaseInfo, "master" );
		}

        [Test]
        public void AddIdListParameter( )
        {
            using ( DatabaseContext context = DatabaseContext.GetContext( ) )
            {
                List<long> ids = new List<long>();
                ids.Add(1);
                ids.Add(2);

                IDbCommand cmd = context.CreateCommand( "select Id from @ids order by Id" );
                cmd.AddIdListParameter( "@ids", ids );

                IDataReader reader = cmd.ExecuteReader( );
                Assert.IsTrue( reader.Read( ) );
                Assert.AreEqual( 1, reader.GetInt64( 0 ) );
                Assert.IsTrue( reader.Read( ) );
                Assert.AreEqual( 2, reader.GetInt64( 0 ) );
                Assert.IsFalse( reader.Read( ) );                
            }
        }
	}
}