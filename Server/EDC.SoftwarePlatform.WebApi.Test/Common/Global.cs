// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.Globalization;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.WebApi.Test.Common
{
	/// <summary>
	///     Hosts any per assembly test initialization.
	/// </summary>
	public static class Global
	{
		/// <summary>
		///     Thread synchronization
		/// </summary>
		private static readonly object SyncRoot = new object( );

		/// <summary>
		///     Default context data.
		/// </summary>
		private static RequestContextData _defaultContextData;

		/// <summary>
		///     EDC tenant
		/// </summary>
		private static TenantInfo _edcTenant;

		/// <summary>
		///     Gets the default context data.
		/// </summary>
		/// <value>
		///     The default context data.
		/// </value>
		public static RequestContextData DefaultContextData
		{
			get
			{
				if ( _defaultContextData == null )
				{
					lock ( SyncRoot )
					{
						if ( _defaultContextData == null )
						{
							_defaultContextData = new RequestContextData( new IdentityInfo( 0, SpecialStrings.SystemAdministratorUser ), EdcTenant, CultureHelper.GetUiThreadCulture( CultureType.Neutral ) );
						}
					}
				}

				return _defaultContextData;
			}
		}

		/// <summary>
		///     Gets the EDC Tenant info object.
		/// </summary>
		public static TenantInfo EdcTenant
		{
			get
			{
				if ( _edcTenant == null )
				{
					lock ( SyncRoot )
					{
						if ( _edcTenant == null )
						{
							/////
							// Create the identity objects used for the request context.
							/////
							var tenantInfo = new TenantInfo( 0 );
							var identityInfo = new IdentityInfo( 0, SpecialStrings.SystemAdministratorUser );
							string culture = CultureHelper.GetUiThreadCulture( CultureType.Neutral );
							tenantInfo.Id = 0;

							using ( new CustomContext( identityInfo, tenantInfo, culture ) )
							{
								/////
								// Lookup the EDC tenant.
								/////
								Tenant tenant = TenantHelper.Find( "EDC" );
								if ( tenant == null )
								{
									throw new InvalidOperationException( "Failed to locate the EDC Tenant." );
								}

								_edcTenant = new TenantInfo( tenant.Id );
							}
						}
					}
				}

				return _edcTenant;
			}
		}
	}
}