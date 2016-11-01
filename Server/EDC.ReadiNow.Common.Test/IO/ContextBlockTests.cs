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
	///     Tests the ContextBlock class.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class ContextBlockTests
	{
		/// <summary>
		///     Tests the IDisposable method of the Block context.
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
				var tenant = new TenantInfo( 0 );

				var blockIdentity = new IdentityInfo( 0, "BlockUser" );
				var blockTenant = new TenantInfo( 9999 );

				RequestContext.SetContext( identity, tenant, "en-US" );

				/////
				// Run the block context.
				/////
				using ( new ContextBlock( ( ) => RequestContext.SetContext( blockIdentity, blockTenant, "en-US" ) ) )
				{
					/////
					// Obtain the current context.
					/////
					RequestContext currentContext = RequestContext.GetContext( );

					/////
					// Confirm the current context is the block context.
					/////
					Assert.IsNotNull( currentContext );
					Assert.IsNotNull( currentContext.Tenant );
					Assert.IsNotNull( currentContext.Identity );
					Assert.AreEqual( blockTenant.Id, currentContext.Tenant.Id );
					Assert.AreEqual( 0, currentContext.Identity.Id );
					Assert.AreEqual( blockIdentity.Name, currentContext.Identity.Name );
				}

				/////
				// The current context should have reverted to the dummy context.
				/////
				RequestContext revertedContext = RequestContext.GetContext( );

				Assert.IsNotNull( revertedContext );
				Assert.IsNotNull( revertedContext.Tenant );
				Assert.IsNotNull( revertedContext.Identity );
				Assert.AreEqual( 0, revertedContext.Tenant.Id );
				Assert.AreEqual( tenant.Name, revertedContext.Tenant.Name );
				Assert.AreEqual( string.Empty, revertedContext.Tenant.Description );
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

		/// <summary>
		///     Tests nesting of multiple ContextBlock objects and their unwinding.
		/// </summary>
		[Test]
		public void TestStackingOfContextBlocks( )
		{
			var c1 = new RequestContext( new RequestContextData( new IdentityInfo( 7, "Starting Context" ), new TenantInfo( 9999 ), "en-US" ) );
			RequestContext.SetContext( c1 );

		    try
		    {

		        RequestContext context;

		        using (
		            new ContextBlock(
		                () =>
		                RequestContext.SetContext(
		                    new RequestContext(new RequestContextData(new IdentityInfo(1, "First"), new TenantInfo(1111),
		                                                              "en-US")))))
		        {
		            using (
		                new ContextBlock(
		                    () =>
		                    RequestContext.SetContext(
		                        new RequestContext(new RequestContextData(new IdentityInfo(2, "Second"), new TenantInfo(2222),
		                                                                  "en-US")))))
		            {
		                using (
		                    new ContextBlock(
		                        () =>
		                        RequestContext.SetContext(
		                            new RequestContext(new RequestContextData(new IdentityInfo(3, "Third"),
		                                                                      new TenantInfo(3333), "en-US")))))
		                {
		                    context = RequestContext.GetContext();

		                    Assert.IsNotNull(context);
		                    Assert.IsNotNull(context.Identity);
		                    Assert.AreEqual("Third", context.Identity.Name);
		                }

		                context = RequestContext.GetContext();

		                Assert.IsNotNull(context);
		                Assert.IsNotNull(context.Identity);
		                Assert.AreEqual("Second", context.Identity.Name);
		            }

		            context = RequestContext.GetContext();

		            Assert.IsNotNull(context);
		            Assert.IsNotNull(context.Identity);
		            Assert.AreEqual("First", context.Identity.Name);
		        }

		        context = RequestContext.GetContext();

		        Assert.IsNotNull(context);
		        Assert.IsNotNull(context.Identity);
		        Assert.AreEqual("Starting Context", context.Identity.Name);
		    }
		    finally
		    {
		        RequestContext.FreeContext();
		    }
		}
	}
}