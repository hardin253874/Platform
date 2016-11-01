// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC;
using EDC.Globalization;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using System;
using System.Linq;

namespace ReadiNow.TenantHealth.Test
{
    /// <summary>
    /// A wrapper class to act as an abstraction between use of the tenant for tests, and the actual tenant data.
    /// </summary>
    public class TenantInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TenantInfo( long tenantId, string name )
        {
            TenantId = tenantId;
            TenantName = name ?? "Null";
        }

        /// <summary>
        /// Tenant Name
        /// </summary>
        public string TenantName { get; }

        /// <summary>
        /// Tenant ID
        /// </summary>
        public long TenantId { get; }

        /// <summary>
        /// Impersonate an admin of this tenant and return the context object, which must be disposed.
        /// </summary>
        public IDisposable GetSystemAdminContext()
        {
            return new TenantAdministratorContext( TenantId );
        }

        /// <summary>
        /// Impersonate an admin of this tenant and return the context object, which must be disposed.
        /// </summary>
        public IDisposable GetTenantAdminContext( )
        {
            if ( TenantAdminIdentityInfo == null )
            {
                using ( GetSystemAdminContext( ) )
                {
                    Role role = Entity.Get<Role>( "core:administratorRole" );
                    UserAccount userAccount = role.RoleMembers.FirstOrDefault( );
                    if ( userAccount == null )
                        throw new Exception( $"Tenant '{TenantName}' has no administrator." );

                    TenantAdminIdentityInfo = new IdentityInfo( userAccount.Id, userAccount.Name );
                }
            }

            // Set context
            var tenantInfo = new EDC.ReadiNow.Metadata.Tenants.TenantInfo( TenantId );
            RequestContextData contextData = new RequestContextData( TenantAdminIdentityInfo, tenantInfo, CultureHelper.GetUiThreadCulture( CultureType.Neutral ) );
            RequestContext.SetContext( contextData );

            // Return delegate to revoke context
            return ContextHelper.Create( RequestContext.FreeContext );
        }

        private IdentityInfo TenantAdminIdentityInfo { get; set; }

        /// <summary>
        /// Overload of ToString - to render into testcase name.
        /// </summary>
        public override string ToString( ) => TenantName;
    }
}
