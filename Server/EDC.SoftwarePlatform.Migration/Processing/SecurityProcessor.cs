// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Migration.Processing
{
    /// <summary>
    ///     Performs security checks for a data source against the current tenant.
    /// </summary>
    internal interface ISecurityProcessor
    {
        void CheckEntityPermissions( IDataSource source, IList<EntityRef> permissions, IProcessingContext context );

        void CheckTypeCreatePermissions( IDataSource source, IProcessingContext context );

        void CheckUserInRole( IEntityRef role );
    }


    /// <summary>
    ///     Performs security checks for a data source against the current tenant.
    /// </summary>
    class SecurityProcessor : ISecurityProcessor
    {
        private IEntityRepository EntityRepository { get; }

        private IEntityAccessControlService EntityAccessControlService { get; }

        private IUserRoleRepository UserRoleRepository { get; }

        private IUpgradeIdProvider UpgradeIdProvider { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userRoleRepository"></param>
        public SecurityProcessor( IEntityRepository entityRepository, IEntityAccessControlService entityAccessControlService, IUserRoleRepository userRoleRepository, IUpgradeIdProvider upgradeIdProvider )
        {
            if ( entityRepository == null )
                throw new ArgumentNullException( nameof( entityRepository ) );
            if ( entityAccessControlService == null )
                throw new ArgumentNullException( nameof( entityAccessControlService ) );
            if ( userRoleRepository == null )
                throw new ArgumentNullException( nameof( userRoleRepository ) );
            if ( upgradeIdProvider == null )
                throw new ArgumentNullException( nameof( upgradeIdProvider ) );

            EntityRepository = entityRepository;
            EntityAccessControlService = entityAccessControlService;
            UserRoleRepository = userRoleRepository;
            UpgradeIdProvider = upgradeIdProvider;
        }

        /// <summary>
        ///     Verify that the current user has permission to perform an import.
        /// </summary>
        /// <remarks>
        ///     Loads all entities that are specified in the data source, then looks up the current user/tenant to see
        ///     if those entities are present. Performs a security demand on those that are present.
        /// </remarks>
        /// <param name="source">Data source to read entities from.</param>
        /// <param name="permissions">Permission(s) to demand.</param>
        /// <param name="context">Processing context for reading the source.</param>
        /// <exception cref="PlatformSecurityException">Thrown if permission is denied.</exception>
        public void CheckEntityPermissions( IDataSource source, IList<EntityRef> permissions, IProcessingContext context )
        {
            // Load entities that would be imported
            IEnumerable<Guid> upgradeIds =
                source.GetEntities( context )
                    .Select( e => e.EntityId ).Distinct( );

            // Note: UpgradeIdProvider will drop any upgradeIds that don't exist in the tenant.
            IList<EntityRef> entityIds =
                UpgradeIdProvider.GetIdsFromUpgradeIds( upgradeIds )
                    .Select( pair => new EntityRef( pair.Value ) ).ToList( );

            // Demand 'modify' for instances
            EntityAccessControlService.Demand( entityIds, permissions );
        }

        /// <summary>
        ///     Verify that the current user has permission to create instances of types mentioned in a data source.
        /// </summary>
        /// <remarks>
        ///     Loads all relationship instances and searches for 'isOfType' relationships.
        ///     Then performs a 'can create' check on those types, if they are present in the current tenant.
        /// </remarks>
        /// <param name="source">Data source to read relationships from.</param>
        /// <param name="context">Processing context for reading the source.</param>
        /// <exception cref="PlatformSecurityException">Thrown if permission is denied.</exception>
        public void CheckTypeCreatePermissions( IDataSource source, IProcessingContext context )
        {
            // Load types that would be imported
            IEnumerable<Guid> typeUpgradeIds =
                source.GetRelationships( context )
                    .Where( rel => rel.TypeId == Helpers.IsOfTypeRelationshipUpgradeId )
                    .Select( rel => rel.ToId ).Distinct( );

            // Note: UpgradeIdProvider will drop any upgradeIds that don't exist in the tenant.
            IEnumerable<long> typeIds =
                UpgradeIdProvider.GetIdsFromUpgradeIds( typeUpgradeIds )
                    .Select( pair => pair.Value );
            IDictionary<long, EntityType> types = EntityRepository.Get<EntityType>( typeIds ).ToDictionary( e => e.Id );

            // Demand 'create' for types
            IDictionary<long, bool> canCreate = EntityAccessControlService.CanCreate( types.Values.ToList( ) );
            IList<long> denied = canCreate.Where( pair => !pair.Value ).Select( pair => pair.Key ).ToList( );
            if ( denied.Count > 0 )
            {
                string message = "Permission denied to create records of type: " + string.Join( ", ", denied.Select( id => types[ id ].Name ) );
                throw new PlatformSecurityException( message );
            }
        }

        /// <summary>
        ///     Verify that the current user is in the specified role.
        /// </summary>
        /// <param name="role">The security role.</param>
        /// <exception cref="PlatformSecurityException">Thrown if permission is denied.</exception>
        public void CheckUserInRole( IEntityRef role )
        {
            // Check user is in import/export role
            var userRoles = UserRoleRepository.GetUserRoles( RequestContext.UserId );


            if ( !userRoles.Contains( role.Id ) )
            {
                string roleName;
                using ( new SecurityBypassContext( ) )
                {
                    roleName = EntityRepository.Get<Role>( role )?.Name ?? "specified";
                }
                throw new PlatformSecurityException( $"Must be a member of the '{roleName}' role." );
            }
                    
        }
    }
}
