// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using EDC.Diagnostics;
using EDC.Globalization;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Security;
using System.Collections.Generic;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Provides helper methods for interacting with the application request data stored within the logical thread data.
	/// </summary>
	[Serializable]
	[DebuggerStepThrough]
	public class RequestContext
	{
		private const string ContextKey = "ReadiNow Request Context";        
		private readonly RequestContextData _contextData;

		/// <summary>
		///     Initializes a new instance of the RequestContext class.
		/// </summary>
		public RequestContext( RequestContextData contextData )
		{
			_contextData = contextData;
		}

		/// <summary>
		///     Gets the culture info associated with the request.
		/// </summary>
		public string Culture
		{
			get
			{
				return _contextData.Culture;
			}
		}

		/// <summary>
		///     Gets the identity associated with the request.
		/// </summary>
		public IdentityInfo Identity
		{
			get
			{
				return _contextData.Identity;
			}
		}

        /// <summary>
        ///  Any secondary identities associated with the request - usually the original triggering user in workflow run.
        /// </summary>
        public IdentityInfo SecondaryIdentity
        {
            get
            {
                return _contextData.SecondaryIdentity;
            }
        }

        /// <summary>
        ///     Gets whether the Request Context is currently set or not.
        /// </summary>
        public static bool IsSet
		{
			get
			{
                return CallContext.GetData(ContextKey) is RequestContextData;
			}
		}

		/// <summary>
		///     Gets a flag indicating whether the context data is valid.
		/// </summary>
		public bool IsValid
		{
			get
			{
			    if (_contextData == null) return false;

			    return
                    _contextData.Identity != null &&
                    _contextData.Tenant != null &&
                    _contextData.Culture != null;
			}
		}

		/// <summary>
		///     Gets the optional parameters associated with the request.
		/// </summary>
// ReSharper disable UnusedMember.Global
		public StringDictionary Parameters
// ReSharper restore UnusedMember.Global
		{
			get
			{
				return _contextData.Parameters;
			}
		}

		/// <summary>
		///     Gets the tenant associated with the request.
		/// </summary>
		public TenantInfo Tenant
		{
			get
			{
				return _contextData.Tenant;
			}
		}

		/// <summary>
		///     Gets the tenant entity ID for the current request.
		/// </summary>
		public static long TenantId
		{
			get
			{
                var contextData = ( RequestContextData ) CallContext.GetData( ContextKey );
                if (contextData == null)
                    throw new InvalidOperationException("Request Context has not been set.");
                if (contextData.Tenant == null)
                    throw new InvalidOperationException("Request Context has no tenant info.");
                return contextData.Tenant.Id;
			}
        }

		/// <summary>
		/// Gets the user identifier.
		/// </summary>
		/// <value>
		/// The user identifier.
		/// </value>
		/// <exception cref="System.InvalidOperationException">
		/// Request Context has not been set.
		/// or
		/// Request Context has no identity info.
		/// </exception>
		public static long UserId
		{
			get
			{
				var contextData = ( RequestContextData ) CallContext.GetData( ContextKey );
				if ( contextData == null )
					throw new InvalidOperationException( "Request Context has not been set." );
				if ( contextData.Identity == null )
					throw new InvalidOperationException( "Request Context has no identity info." );
				return contextData.Identity.Id;
			}
		}

        /// <summary>
        ///     Gets the tenant entity ID for the current request.
        /// </summary>
        /// <remarks>False if the tenant was not set, otherwise true.</remarks>
        public static bool TryGetTenantId(out long tenantId)
        {
            tenantId = 0;
            var contextData = (RequestContextData)CallContext.GetData(ContextKey);
	        if (contextData?.Tenant == null)
                return false;
            tenantId = contextData.Tenant.Id;
            return true;
        }

		/// <summary>
		/// Tries the get user identifier.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <returns></returns>
		public static bool TryGetUserId( out long userId )
		{
			userId = 0;
			var contextData = ( RequestContextData ) CallContext.GetData( ContextKey );
			if ( contextData?.Identity == null )
				return false;
			userId = contextData.Identity.Id;
			return true;
		}

        /// <summary>
        ///     Gets the time zone.
        /// </summary>
        /// <value>
        ///     The time zone.
        /// </value>
        public string TimeZone
		{
			get
			{
				return _contextData == null ? null : _contextData.TimeZone;
			}
        }

        /// <summary>
        ///     Gets the time zone.
        /// </summary>
        /// <value>
        ///     The time zone.
        /// </value>
        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                return _contextData == null ? null : _contextData.TimeZoneInfo;
            }
        }


        /// <summary>
        ///     Frees the request data from the logical thread.
        /// </summary>
        public static void FreeContext( )
		{
			DiagnosticsRequestContext.FreeContext( );

			// Free the request context data from the logical context
			CallContext.FreeNamedDataSlot( ContextKey );

            ActualUserRequestContext.FreeContext();            
		}

		/// <summary>
		///     Gets the context data from the logical thread.
		/// </summary>
		/// <returns>
		///     An object that represents the current request data.
		/// </returns>
		//[DebuggerStepThrough]
		public static RequestContext GetContext( )
		{
			// Get the request context data from logical context
			var contextData = ( RequestContextData ) CallContext.GetData( ContextKey );
			var context = new RequestContext( contextData );

			return context;
		}

		/// <summary>
		///     Sets the context data within the logical thread.
		/// </summary>
		/// <param name="identity">The identity.</param>
		/// <param name="tenant">The tenant.</param>
		/// <param name="culture">The culture.</param>
		public static void SetContext( IdentityInfo identity, TenantInfo tenant, string culture )
		{
			// Set the default request context data in the logical context
			var contextData = new RequestContextData( identity, tenant, culture );
			SetContext( contextData );
		}

		/// <summary>
		///		Sets the context.
		/// </summary>
		/// <param name="identity">The identity.</param>
		/// <param name="tenant">The tenant.</param>
		/// <param name="culture">The culture.</param>
		/// <param name="timeZone">The time zone.</param>
		/// <param name="secondaryIdentity">The secondary identity.</param>
		public static void SetContext( IdentityInfo identity, TenantInfo tenant, string culture, string timeZone, IdentityInfo secondaryIdentity = null)
		{
			// Set the default request context data in the logical context
			var contextData = new RequestContextData( identity, tenant, culture, timeZone, secondaryIdentity );
			SetContext( contextData );
		}

		/// <summary>
		///     Sets the context data within the logical thread.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <remarks></remarks>
		public static void SetContext( RequestContext context )
		{
			// Set the default request context data in the logical context
			var contextData = new RequestContextData( context.Identity, context.Tenant, context.Culture, context.SecondaryIdentity );
			SetContext( contextData );
		}

		/// <summary>
		///     Sets the context data within the logical thread.
		/// </summary>
		/// <param name="contextData">The context data.</param>
		/// <remarks></remarks>
		public static void SetContext( RequestContextData contextData )
		{
			long tenantId = -1;
		    var tenantName = string.Empty;

			if ( contextData != null &&
			     contextData.Tenant != null )
			{
			    var tenant = contextData.Tenant;
			    tenantId = tenant.Id;
			    tenantName = tenant.HaveName ? tenant.Name : TenantHelper.GetTenantName(tenantId);      // don't trigger a name fetch			    
			}

			var userName = string.Empty;
			if ( contextData != null &&
			     contextData.Identity != null )
			{
				userName = contextData.Identity.Name;
			}

			// Set the diagnostics context for logging purposes
			DiagnosticsRequestContext.SetContext( tenantId, tenantName, userName);

			// Set the default request context data in the logical context
			CallContext.SetData( ContextKey, contextData );
		    
            ActualUserRequestContext.SetContext(contextData);		    
		}

		/// <summary>
		///     Sets the system administrator context data within the logical thread.
		/// </summary>
		public static void SetSystemAdministratorContext( )
		{
			SetSystemAdministratorContext( CultureHelper.GetUiThreadCulture( CultureType.Neutral ) );
		}

		/// <summary>
		///     Sets the context to the specified tenant.
		///     This context is not compatible with the legacy resource model.
		/// </summary>
		/// <param name="tenantId">The tenant id.</param>
		/// <remarks></remarks>
		public static void SetTenantAdministratorContext( long tenantId )
		{
			SetTenantAdministratorContext( tenantId, CultureHelper.GetUiThreadCulture( CultureType.Neutral ) );
		}

        /// <summary>
        ///     Sets the context to the specified tenant.
        ///     This context is not compatible with the legacy resource model.
        /// </summary>
        /// <param name="tenantName">The tenant name.</param>
        /// <remarks></remarks>
        public static void SetTenantAdministratorContext(string tenantName)
        {
            RequestContext.SetSystemAdministratorContext();
            var tenant = TenantHelper.Find(tenantName);
            RequestContext.SetTenantAdministratorContext(tenant.Id);
        }

		/// <summary>
		///     Sets the system administrator context data within the logical thread.
		/// </summary>
		/// <param name="culture">
		///     A string containing the culture.
		/// </param>
		internal static void SetSystemAdministratorContext( string culture )
		{
		    var tenantInfo = new TenantInfo(0) {Name = SpecialStrings.GlobalTenant};
			var identityInfo = new IdentityInfo( 0, SpecialStrings.SystemAdministratorUser );

            // Note that trigger depth is not propagated into the admin context. It will have it's own trigger depth.
            var contextData = new RequestContextData(identityInfo, tenantInfo, String.IsNullOrEmpty(culture) ? CultureHelper.GetUiThreadCulture(CultureType.Neutral) : culture);

			/////
			// Set the request context data
			/////
			SetContext( contextData );
		}

		/// <summary>
		///     Sets the tenant administrator context data within the logical thread.
		/// </summary>
		/// <param name="tenantId">
		///     An ID identifying the tenant.
		/// </param>
		/// <param name="culture">
		///     A string containing the culture.
		/// </param>
		private static void SetTenantAdministratorContext( long tenantId, string culture )
		{
			var tenant = new TenantInfo( tenantId );
			var identity = new IdentityInfo( 0, SpecialStrings.TenantAdministratorUser );

			/////
			// Set the default request context data in the logical context
			/////
            var contextData = new RequestContextData(identity, tenant, String.IsNullOrEmpty(culture) ? CultureHelper.GetUiThreadCulture(CultureType.Neutral) : culture);

			/////
			// Set the request context data
			/////
			SetContext( contextData );
		}
	}
}