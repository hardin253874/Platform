// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Linq;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	[TestFixture]
	[RunWithTransaction]
	public class TemporaryIdTests
	{
		[Test]
		[RunAsDefaultTenant]
		public void TestNewTemporaryEntityMasterIndex( )
		{
			using ( DatabaseContext.GetContext( true ) )
			{
				var u1 = new UserAccount( );
				var u2 = new UserAccount( );

				long u1Id = u1.Id;
				long u2Id = u2.Id;

				u1.SecurityOwnerOf.Add( u2.As<Resource>( ) );
				u2.SecurityOwnerOf.Add( u1.As<Resource>( ) );

				u1.Save( );
				u2.Save( );

				Assert.AreNotEqual( u1.Id, u1Id );
				Assert.AreNotEqual( u2.Id, u2Id );
				Assert.IsTrue( u2.SecurityOwnerOf.Any( r => r.Id == u1.Id ) );
				Assert.IsTrue( u1.SecurityOwnerOf.Any( r => r.Id == u2.Id ) );
			}
		}
	}
}