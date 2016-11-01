// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test
{
	/// <summary>
	///     Base class for test fixtures.
	/// </summary>
	public abstract class TestBase
	{
		/// <summary>
		/// The database context
		/// </summary>
		protected DatabaseContext Context = null;

		/// <summary>
		///     Test setup method.
		/// </summary>
		[SetUp]
		protected void Setup( )
		{
			/////
			// Ensure the timeouts are consistent with the AppManager
			// timeouts so that the same database context is used.
			/////
			Context = DatabaseContext.GetContext( true, commandTimeout: AppManager.DefaultTimeout, transactionTimeout: AppManager.DefaultTimeout );
		}

		/// <summary>
		///     Test tear down method.
		/// </summary>
		[TearDown]
		protected void TearDown( )
		{
			if ( Context != null )
			{
				Context.Dispose( );
				Context = null;
			}

			TenantHelper.Flush( );
		}
	}
}