// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Globalization;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     Handles setting request context.
	/// </summary>
	/// <remarks></remarks>
	internal static class Context
	{
		/// <summary>
		///     Frees the request context data.
		/// </summary>
		internal static void FreeRequestContext( )
		{
			// Free the request context data
			RequestContext.FreeContext( );
		}

		/// <summary>
		///     Set the system administrator context data.
		/// </summary>
		internal static void SetSystemAdministratorContext( )
		{
			/////
			// EntityId should be always set to zero for administrator context.
			/////
			var tenantInfo = new TenantInfo(0);

			var identityInfo = new IdentityInfo( 0, SpecialStrings.SystemAdministratorUser );
			string culture = CultureHelper.GetUiThreadCulture( CultureType.Neutral );

			// Set the request context data
			RequestContext.SetContext( identityInfo, tenantInfo, culture );
		}

		/// <summary>
		///     Set the tenant administrator context data.
		/// </summary>
		internal static void SetTenantAdministratorContext( long tenantId )
		{
			// Set the request context data
			RequestContext.SetTenantAdministratorContext( tenantId );
		}
	}
}