// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Database;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Database
{
	/// <summary>
	///     The database change tracking tests class.
	/// </summary>
	[ReadiNowTestFixture]
	public class DatabaseChangeTrackingTests
	{
		/// <summary>
		///     Tests the change tracking enable disable.
		/// </summary>
		[Test]
		public void TestChangeTrackingEnableDisable( )
		{
			bool initialState = DatabaseChangeTracking.Enabled;

			try
			{
				DatabaseChangeTracking.Enabled = false;

				bool enabled = DatabaseChangeTracking.Enabled;

				Assert.AreEqual( enabled, false );

				DatabaseChangeTracking.Enabled = true;

				enabled = DatabaseChangeTracking.Enabled;

				Assert.AreEqual( enabled, true );
			}
			finally
			{
				DatabaseChangeTracking.Enabled = initialState;
			}
		}

		/// <summary>
		///     Tests the GetLastTransaction method.
		/// </summary>
		[Test]
		public void TestGetLastTransaction( )
		{
			long transactionId = DatabaseChangeTracking.GetLastTransactionId( );

			Assert.Greater( transactionId, 0 );
		}
	}
}