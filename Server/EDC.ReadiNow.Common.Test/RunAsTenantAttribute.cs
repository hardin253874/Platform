// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Globalization;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test
{
	/// <summary>
	///     Runs the applied test case method under the specified tenant
	/// </summary>
	public class RunAsTenantAttribute : ReadiNowTestAttribute, IDisposable
	{
		/// <summary>
		///     Request context cache.
		/// </summary>
		private static readonly IDictionary<string, RequestContextData> RequestContextCache = new Dictionary<string, RequestContextData>( );

		/// <summary>
		///     Initializes a new instance of the <see cref="RunAsTenantAttribute" /> class.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		public RunAsTenantAttribute( string tenantName )
		{
			TenantName = tenantName;		   
		}

		/// <summary>
		///     Gets or sets the name of the tenant.
		/// </summary>
		/// <value>
		///     The name of the tenant.
		/// </value>
		private string TenantName
		{
			get;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			RequestContext.FreeContext( );            
        }

		/// <summary>
		///     Executed before each test is run
		/// </summary>
		/// <param name="testDetails">Provides details about the test that is going to be run.</param>
		public override void BeforeTest( TestDetails testDetails )
		{            
            SetRequestContext( );
		}

		/// <summary>
		///     Executed after each test is run
		/// </summary>
		/// <param name="testDetails">Provides details about the test that has just been run.</param>
		public override void AfterTest( TestDetails testDetails )
		{            
            Dispose( );            
        }

		/// <summary>
		///     Sets the request context.
		/// </summary>
		private void SetRequestContext( )
		{
			RequestContext context = RequestContext.GetContext( );

			if ( context != null && context.IsValid && context.Tenant?.Name != null )
			{
				if ( context.Tenant.Name.Equals( TenantName, StringComparison.OrdinalIgnoreCase ) )
				{
					/////
					// Context already set.
					/////
					return;
				}
			}

			RequestContextData contextData;

			/////
			// See if the request context has been cached.
			/////
			if ( ! RequestContextCache.TryGetValue( TenantName, out contextData ) )
			{
				if ( TenantName.Equals( SpecialStrings.GlobalTenant ) )
				{
					var tenantInfo = new TenantInfo(0);
					var identityInfo = new IdentityInfo( 0, SpecialStrings.SystemAdministratorUser );

					contextData = new RequestContextData( identityInfo, tenantInfo, CultureHelper.GetUiThreadCulture( CultureType.Neutral ) );
				}
				else
				{
					/////
					// Set system administrators context to retrieve the tenant.
					/////
					RequestContext.SetSystemAdministratorContext( );

					/////
					// Retrieve the requested tenant.
					/////
					Tenant tenant = TenantHelper.Find( TenantName );
					RequestContext.SetTenantAdministratorContext( tenant.Id );
                    UserAccount userAccount = Entity.GetByField<UserAccount>(SpecialStrings.SystemAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault() ?? Entity.GetByField<UserAccount>(SpecialStrings.TenantAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault();

					if ( userAccount == null )
					{
						throw new EntityNotFoundException( "The 'Administrator' account for tenant '" + TenantName + "' could not be found." );
					}

					/////
					// Set the context data
					/////
					var identityInfo = new IdentityInfo( userAccount.Id, userAccount.Name );
					var tenantInfo = new TenantInfo( tenant.Id );
					contextData = new RequestContextData( identityInfo, tenantInfo, CultureHelper.GetUiThreadCulture( CultureType.Neutral ) );
				}

				RequestContextCache[ TenantName ] = contextData;
			}

			RequestContext.SetContext( contextData );
		}
	}
}