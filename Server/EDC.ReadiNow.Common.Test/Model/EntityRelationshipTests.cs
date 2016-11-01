// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	[TestFixture]
	[RunWithTransaction]
	public class EntityRelationshipTests
	{
		/// <summary>
		///     Tests the equality.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestEquality( )
		{
			var r = new EntityRelationship<IEntity>( null );
			var entity = new Entity( new ActivationData( ) );

			Assert.AreEqual( r, new EntityRelationship<IEntity>( null ) );
			Assert.AreNotEqual( r, new EntityRelationship<IEntity>( entity ) );

			r = new EntityRelationship<IEntity>( entity );
			Assert.AreNotEqual( r, new EntityRelationship<IEntity>(  null ) );
			Assert.AreEqual( r, new EntityRelationship<IEntity>( entity ) );
		}

		/// <summary>
		///     Tests the hash code.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestHashCode( )
		{
			var entity = new Entity( new ActivationData( ) );

			Assert.IsTrue( new EntityRelationship<IEntity>( null ).GetHashCode( ) == 0 );
			Assert.IsTrue( new EntityRelationship<IEntity>( entity ).GetHashCode( ) != 0 );
		}
	}
}