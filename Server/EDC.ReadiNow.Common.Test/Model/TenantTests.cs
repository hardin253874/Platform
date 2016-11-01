// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	[TestFixture]
	[RunWithTransaction]
	public class TenantTests
	{
		/// <summary>
		///     Creates the tenant.
		/// </summary>
		/// <exception cref="System.Exception">
		///     Failed to get the tenant by name. The full-text catalog may not have updated yet.
		///     or
		///     Able to retrieve the tenant by name. The full-text catalog may not have updated yet.
		/// </exception>
		[Test]
		[RunAsGlobalTenant]
		public void CreateTenant( )
		{
			Tenant tenant = null;
			string name = "TestTenant " + DateTime.Now.Ticks.ToString( CultureInfo.InvariantCulture );
			try
			{
				// Create tenant
				tenant = new Tenant
				{
					Name = name
				};
				tenant.Save( );

				// Get tenant Id
				Tenant tenantGet = null;

				long tenantId = 0;

				if ( !Delegates.Retry( 10, 1000, ( ) =>
				{
					tenantGet = Entity.GetByName<Tenant>( name, false, CaseComparisonOption.Sensitive, StringComparisonOption.Exact ).FirstOrDefault( );

					return tenantGet != null;
				} ) )
				{
					throw new Exception( "Failed to get the tenant by name. The full-text catalog may not have updated yet." );
				}

				if ( tenantGet != null )
				{
					tenantId = tenantGet.Id;
				}

				Assert.AreEqual( tenant.Id, tenantId );
			}
			finally
			{
				if ( tenant != null )
				{
					tenant.Delete( );
				}

				if ( ! Delegates.Retry( 10, 1000, ( ) =>
				{
					Tenant tenantGet = Entity.GetByName<Tenant>( name, false, CaseComparisonOption.Sensitive, StringComparisonOption.Exact ).FirstOrDefault( );

					return tenantGet == null;
				} ) )
				{
					throw new Exception( "Able to retrieve the tenant by name. The full-text catalog may not have updated yet." );
				}
			}
		}

		/// <summary>
		///     Invalidates the default tenant.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void Invalidate( )
		{
			TenantHelper.Invalidate( );
		}
	}
}