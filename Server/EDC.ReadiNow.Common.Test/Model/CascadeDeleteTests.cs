// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Linq;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     Cascade Delete Tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class CascadeDeleteTests
	{
		[Test]
		[RunAsDefaultTenant]
		public void TestCascadeDeleteSimple( )
		{
			var f1 = Entity.Create<NavContainer>( );
			f1.Save( );
			var f2 = Entity.Create<NavContainer>( );
			f2.ResourceInFolder.Add(f1);
			f2.Save( );

			Assert.IsTrue( Entity.Exists( f1 ), "f1=true" );
			Assert.IsTrue( Entity.Exists( f2 ), "f2=true" );

			f1.Delete( );

			Assert.IsFalse( Entity.Exists( f1 ), "f1=false" );
			Assert.IsFalse( Entity.Exists( f2 ), "f2=false" );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestCascadeDeleteWithRelationships( )
		{
			var f1 = Entity.Create<NavContainer>( );
			f1.Name = "f1";
			f1.Save( );
			var f2 = Entity.Create<NavContainer>( );
			f2.Name = "f2";
            f2.ResourceInFolder.Add(f1);
			var f3 = Entity.Create<NavContainer>( );
			f3.Name = "f3";
            f3.ResourceInFolder.Add(f2);
			f3.ShortcutInFolder.Add( f1 );
			f3.Save( );

			Assert.IsTrue( Entity.Exists( f1 ), "a f1" );
			Assert.IsTrue( Entity.Exists( f2 ), "a f2" );
			Assert.IsTrue( Entity.Exists( f3 ), "a f3" );

			f2.Delete( );

			Assert.IsTrue( Entity.Exists( f1 ), "b f1" );
			Assert.IsFalse( Entity.Exists( f2 ), "b f2" );
			Assert.IsFalse( Entity.Exists( f3 ), "b f3" );

			f1.Delete( );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestCascadeDeleteWithRelationships2( )
		{
			var f1 = Entity.Create<NavContainer>( );
			f1.Name = "f1";
			f1.Save( );
			var f2 = Entity.Create<NavContainer>( );
			f2.Name = "f2";
            f2.ResourceInFolder.Add(f1);
			var f3 = Entity.Create<NavContainer>( );
			f3.Name = "f3";
            f3.ResourceInFolder.Add(f2);
			f3.ShortcutInFolder.Add( f1 );
			f3.Save( );

			long id1 = f1.Id;

			Assert.IsTrue( Entity.Exists( f1 ), "a f1" );
			Assert.IsTrue( Entity.Exists( f2 ), "a f2" );
			Assert.IsTrue( Entity.Exists( f3 ), "a f3" );

			f2.Delete( );

			Assert.AreEqual( 0, f3.Shortcuts.Count );

			IEntity e = Entity.Get( id1 );
			var relDefn = new EntityRef( "console", "shortcutInFolder" );
			IEntityRelationshipCollection<IEntity> oRelated = e.GetRelationships( relDefn, Direction.Reverse );

			Assert.IsFalse( oRelated.Any( ri => ri.Entity == null ) );
			Assert.AreEqual( 0, oRelated.Count );

			f1.Delete( );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestResourceInFolderIsCascade( )
		{
			var rel = Entity.Get<Relationship>( new EntityRef( "console", "resourceInFolder" ) );
			Assert.IsTrue( rel.CascadeDelete == true );
		}
	}
}