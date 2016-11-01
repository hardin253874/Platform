// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.IO
{
	/// <summary>
	///     Tests the AdministratorContext class.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class AdministratorContextTests
	{
		/// <summary>
		///     Tests the IDisposable method of the Administrator context.
		/// </summary>
		[Test]
		public void TestIDisposable( )
		{
			RequestContext originalContext = RequestContext.GetContext( );

			try
			{
				/////
				// Create a dummy context.
				/////
				var identity = new IdentityInfo( 0, "TestUser" );
				var tenant = new TenantInfo( 9999 );

				RequestContext.SetContext( identity, tenant, "en-US" );

				/////
				// Run the administrator context.
				/////
				using ( new AdministratorContext( ) )
				{
					/////
					// Obtain the current context.
					/////
					RequestContext currentContext = RequestContext.GetContext( );

					/////
					// Confirm the current context is the administrator context.
					/////
					Assert.IsNotNull( currentContext );
					Assert.IsNotNull( currentContext.Tenant );
					Assert.IsNotNull( currentContext.Identity );
					Assert.AreEqual( 0, currentContext.Tenant.Id );
					Assert.AreEqual( SpecialStrings.GlobalTenant, currentContext.Tenant.Name );
					Assert.AreEqual( string.Empty, currentContext.Tenant.Description );
					Assert.AreEqual( 0, currentContext.Identity.Id );
					Assert.AreEqual( SpecialStrings.SystemAdministratorUser, currentContext.Identity.Name );
				}

				/////
				// The current context should have reverted to the dummy context.
				/////
				RequestContext revertedContext = RequestContext.GetContext( );

				Assert.IsNotNull( revertedContext );
				Assert.IsNotNull( revertedContext.Tenant );
				Assert.IsNotNull( revertedContext.Identity );
				Assert.AreEqual( tenant.Id, revertedContext.Tenant.Id );
				Assert.AreEqual( 0, revertedContext.Identity.Id );
				Assert.AreEqual( identity.Name, revertedContext.Identity.Name );
			}
			finally
			{
				/////
				// Restore any context that was active at the start of the test.
				/////
				if ( originalContext.IsValid )
				{
					RequestContext.SetContext( originalContext );
				}
			}
		}
	}
}