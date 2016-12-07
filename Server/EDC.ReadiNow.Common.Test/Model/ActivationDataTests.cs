// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     ActivationData Tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class ActivationDataTests
	{
		/// <summary>
		///     Tests the constructors.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void TestConstructors( )
		{
			const long id = -1;

			var activationData = new ActivationData( id, RequestContext.TenantId );

			IEntity e = new Entity( activationData );

			Assert.IsNotNull( e );
			Assert.AreEqual( id, e.Id );
			Assert.AreEqual( true, e.IsReadOnly );

			activationData = new ActivationData( id, RequestContext.TenantId, false );

			e = new Entity( activationData );

			Assert.IsNotNull( e );
			Assert.AreEqual( id, e.Id );
			Assert.AreEqual( false, e.IsReadOnly );

			e = new Person( activationData );

			Assert.IsNotNull( e );
			Assert.AreEqual( id, e.Id );
			Assert.AreEqual( false, e.IsReadOnly );
		}

		/// <summary>
		///     Tests the empty.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void TestEmpty( )
		{
			const long id = 0;
			const bool readOnly = false;

			ActivationData activationData = ActivationData.Empty;

			Assert.IsNotNull( activationData );
			Assert.AreEqual( id, activationData.Id );
			Assert.AreEqual( readOnly, activationData.ReadOnly );
		}

		/// <summary>
		///     Tests the entity load with activation data.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void TestEntityLoadWithActivationData( )
		{
			var data = new ActivationData( 123456789, RequestContext.TenantId, true );

			var entity = new Entity( data );

			Assert.AreEqual( 123456789, entity.Id );
			Assert.AreEqual( true, entity.IsReadOnly );

			IEntityInternal entityInternal = entity;

			Assert.AreEqual( false, entityInternal.IsTemporaryId );
			Assert.AreEqual( 123456789, entityInternal.ModificationToken.EntityId );
			Assert.IsNull( entityInternal.CloneSource );
			Assert.AreEqual( 123456789, entityInternal.MutableId.Key );
		}

		/// <summary>
		///     Tests the equality.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void TestEquality( )
		{
			const long id = -1;

			var activationData1 = new ActivationData( id, RequestContext.TenantId );
			var activationData2 = new ActivationData( id, RequestContext.TenantId );

			Assert.IsNotNull( activationData1 );
			Assert.IsNotNull( activationData2 );
			Assert.AreEqual( activationData1, activationData2 );
			Assert.IsTrue( activationData1.Equals( activationData2 ) );
			Assert.IsTrue( activationData1 == activationData2 );
			Assert.AreEqual( activationData1.GetHashCode( ), activationData2.GetHashCode( ) );

			var activationData3 = new ActivationData( id, RequestContext.TenantId );

			Assert.IsTrue( activationData1.Equals( activationData3 ) );
			Assert.IsTrue( activationData1 == activationData3 );
			Assert.AreEqual( activationData1.GetHashCode( ), activationData3.GetHashCode( ) );
		}
	}
}