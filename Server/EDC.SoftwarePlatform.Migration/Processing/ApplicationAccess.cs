// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Database;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Processing
{
    public enum AppPermission
    {
        /// <summary>
        /// Permission to install, upgrade and repair the app.
        /// </summary>
        Install,

        /// <summary>
        /// Permission to publish and export the app.
        /// </summary>
        Publish        
    }

    public static class ApplicationAccess
    {
        /// <summary>
        /// Grant or deny a tenant permission to operate with an application.
        /// </summary>
        /// <param name="tenantName">The tenant name or guid.</param>
        /// <param name="app">The application name or guid.</param>
        /// <param name="permission">The type of permission (e.g. Install, Publish).</param>
        /// <param name="grant">True to grant permission; false to remove permission.</param>
        public static void ChangeAppAccess( string tenantName, string app, AppPermission permission, bool grant )
        {
            // Log activity
            var context = new ProcessingContext
            {
                Report =
                {
                    Action = AppLibraryAction.ChangeAccess
                }
            };

            context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant", tenantName ) );
            context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application", app ) );
            context.Report.Arguments.Add( new KeyValuePair<string, string>( "Permission", permission.ToString() ) );
            context.Report.Arguments.Add( new KeyValuePair<string, string>( "Access", grant ? "Grant" : "Deny" ) );

            // Run
            using ( new SecurityBypassContext( ) )
            {
                long tenantId;
                long appId;

                using ( new GlobalAdministratorContext( ) )
                {
                    // Process arguments
                    tenantId = TenantHelper.GetTenantId( tenantName );
                    appId = SystemHelper.GetGlobalApplicationIdByNameOrGuid( app );

                    if ( appId <= 0 || tenantId <= 0 )
                    {
                        return;
                    }

                    // Update database
                    switch ( permission )
                    {
                        case AppPermission.Install:
                            UpdateTenantPermissionRelationship( tenantId, appId, "canInstallApplication", grant );
                            break;

                        case AppPermission.Publish:
                            UpdateTenantPermissionRelationship( tenantId, appId, "canPublishApplication", grant );
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Changes the tenant application can modify field.
        /// </summary>
        /// <param name="tenantName"></param>
        /// <param name="app"></param>
        /// <param name="grant"></param>
        public static void ChangeTenantApplicationCanModify(string tenantName, string app, bool grant)
        {        
            // Log activity
            var context = new ProcessingContext
            {
                Report =
                {
                    Action = AppLibraryAction.ChangeAccess
                }
            };

            context.Report.Arguments.Add(new KeyValuePair<string, string>("Tenant", tenantName));
            context.Report.Arguments.Add(new KeyValuePair<string, string>("Application", app));
            context.Report.Arguments.Add(new KeyValuePair<string, string>("Permission", "CanModifyApplication"));
            context.Report.Arguments.Add(new KeyValuePair<string, string>("Access", grant ? "Grant" : "Deny"));

            // Run
            using (new SecurityBypassContext())
            {
                using (new GlobalAdministratorContext())
                {
                    // Process arguments
                    var tenantId = TenantHelper.GetTenantId(tenantName);
                    if (tenantId <= 0)
                    {
                        return;
                    }

                    var appId = SystemHelper.GetTenantApplicationIdByName(tenantId, app);

                    if (appId <= 0)
                    {
                        return;
                    }
                    
                    UpdateCanModifyApplicationField(tenantId, appId, grant);                    
                }
            }
        }

        /// <summary>
        /// Updates the canModifyApplication field
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="appId">The application id.</param>
        /// <param name="grant">True to grant, false otherwise.</param>
        private static void UpdateCanModifyApplicationField(long tenantId, long appId, bool grant)
        {
            using (new TenantAdministratorContext(tenantId))
            {
                var application = Entity.Get<Solution>(appId, true);
                application.CanModifyApplication = grant;
                application.Save();
            }            
        }

        /// <summary>
        /// Creates or removes a permission relationship between a tenant and an app in the global tenant.
        /// </summary>
        private static void UpdateTenantPermissionRelationship( long tenantId, long appId, string relationshipAlias, bool grant )
        {            
            const string grantSql = @"-- Grant app access
				IF ( @context IS NOT NULL )
				BEGIN
					DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
					SET CONTEXT_INFO @contextInfo
				END

                declare @relId bigint = dbo.fnAliasNsId( @relAlias, 'core', default )
                if not exists ( select 1 from Relationship where TenantId = 0 and TypeId = @relId and FromId = @tenantId and ToId = @appId )
                begin
	                insert
		                into Relationship (TenantId, TypeId, FromId, ToId)
		                values (0, @relId, @tenantId, @appId)  
                end
                ";

            const string denySql = @"-- Deny app access
				IF ( @context IS NOT NULL )
				BEGIN
					DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
					SET CONTEXT_INFO @contextInfo
				END

                declare @relId bigint = dbo.fnAliasNsId( @relAlias, 'core', default )
                delete from Relationship
                	where TenantId = 0 and TypeId = @relId and FromId = @tenantId and ToId = @appId
                ";

			long userId;
			RequestContext.TryGetUserId( out userId );

			using ( DatabaseContextInfo.SetContextInfo( "Update tenant permission relationship" ) )
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				string sql = grant ? grantSql : denySql;

				using ( IDbCommand command = ctx.CreateCommand( sql ) )
				{
					ctx.AddParameter( command, "@relAlias", DbType.String, relationshipAlias );
					ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
					ctx.AddParameter( command, "@appId", DbType.Int64, appId );
					ctx.AddParameter( command, "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

					command.ExecuteNonQuery( );
				}
			}
        }

    }
}
