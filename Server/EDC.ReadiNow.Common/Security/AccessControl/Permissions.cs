// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

/////
// ReSharper disable CheckNamespace
/////

namespace EDC.ReadiNow.Security.AccessControl
{
	/// <summary>
	///     Various static properties and helpers for referring to specific types of permissions.
	/// </summary>
	public static class Permissions
	{
		/// <summary>
		///     Modification map.
		/// </summary>
        internal static readonly ConcurrentDictionary<long, EntityRef> ModifyMap = new ConcurrentDictionary<long, EntityRef>();

		/// <summary>
		///     Read map.
		/// </summary>
        internal static readonly ConcurrentDictionary<long, EntityRef> ReadMap = new ConcurrentDictionary<long, EntityRef>();

        /// <summary>
        ///     Create map.
        /// </summary>
        internal static readonly ConcurrentDictionary<long, EntityRef> CreateMap = new ConcurrentDictionary<long, EntityRef>();

        /// <summary>
        ///     Delete map.
        /// </summary>
        internal static readonly ConcurrentDictionary<long, EntityRef> DeleteMap = new ConcurrentDictionary<long, EntityRef>();

		/// <summary>
		///     Modify permission.
		/// </summary>
		public static EntityRef Modify
		{
			get
			{
                return TryGetCreatePermission(ModifyMap, () => new EntityRef("core", "modify"));
            }
		}

		/// <summary>
		///     Read permission.
		/// </summary>
		public static EntityRef Read
		{
			get
			{
                return TryGetCreatePermission(ReadMap, () => new EntityRef("core", "read"));
			}
		}

        /// <summary>
        ///     Create permission.
        /// </summary>
        public static EntityRef Create
        {
            get
            {
                return TryGetCreatePermission(CreateMap, () => new EntityRef("core", "create"));
            }
        }

        /// <summary>
        ///     Delete permission.
        /// </summary>
        public static EntityRef Delete
        {
            get
            {
                return TryGetCreatePermission(DeleteMap, () => new EntityRef("core", "delete"));
            }
        }

        /// <summary>
        /// Lookup the tenant in <paramref name="tenantPermissionMap"/>. If not found, create it, 
        /// add it to the map and return it.
        /// </summary>
        /// <param name="tenantPermissionMap">
        /// The map of tenant ID to permission. This cannot be null.
        /// </param>
        /// <param name="createFunc">
        /// Create the new <see cref="EntityRef"/>. This cannot be null.
        /// </param>
        /// <returns>
        /// An <see cref="EntityRef"/> containing the permission for the current tenant.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> is not set.
        /// </exception>
        internal static EntityRef TryGetCreatePermission(ConcurrentDictionary<long, EntityRef> tenantPermissionMap, Func<EntityRef> createFunc)
        {
	        if (tenantPermissionMap == null)
	        {
                throw new ArgumentNullException("tenantPermissionMap");
	        }
            if (createFunc == null)
            {
                throw new ArgumentNullException("createFunc");
            }
            if (!RequestContext.IsSet)
            {
                throw new InvalidOperationException("Request context not set.");
            }

            return tenantPermissionMap.GetOrAdd(RequestContext.TenantId, id => createFunc());
        }

        /// <summary>
        /// Lookup the alias for <paramref name="permissionEntityRef"/>, given the 
        /// alias may not be supplied.
        /// </summary>
        /// <param name="permissionEntityRef"></param>
        /// <returns></returns>
        internal static string GetPermissionByAlias(EntityRef permissionEntityRef)
        {
            if (permissionEntityRef == null)
            {
                throw new ArgumentNullException("permissionEntityRef");
            }

            string alias;

            alias = permissionEntityRef.Id.ToString();
            if (!string.IsNullOrWhiteSpace(permissionEntityRef.Alias))
            {
                alias = permissionEntityRef.Alias;
            }
            else if (permissionEntityRef.Id == Read.Id)
            {
                alias = Read.Alias;
            }
            else if (permissionEntityRef.Id == Modify.Id)
            {
                alias = Modify.Alias;
            }
            else if (permissionEntityRef.Id == Create.Id)
            {
                alias = Create.Alias;
            }
            else if (permissionEntityRef.Id == Delete.Id)
            {
                alias = Delete.Alias;
            }

            return alias;
        }

        /// <summary>
        /// Determine the most specific permission requested.
        /// </summary>
        /// <remarks>
        /// For each rule, the system currently allows grants that are either:
        /// a. Read
        /// b. Read/Modify
        /// c. Read/Modify/Delete
        /// d. Read/Modify/Delete/Create
        /// Allowing arbitrary combinations of permissionIds will cause problems for rule resolution because, for example,
        /// the user's role may allow one permission, but the user themselves may allow for another permission. So in order
        /// to process them together, we would need to keep track of which permissions are satisfied along which paths.
        /// This isn't going to work, so for now we take advantage of the incremental levels of permissions, and only perform
        /// the security check on the most specific - because a grant on that permission will imply a grant on the others,
        /// and a deny on the most specific is sufficient to cause an overall deny.
        /// </remarks>
        /// <param name="permissionIds"></param>
        /// <returns></returns>
        public static long MostSpecificPermission( IEnumerable<long> permissionIds )
        {
            if ( permissionIds == null )
                throw new ArgumentNullException( "permissionIds" );

            // TODO: Make nicer .. but keep fast.

            bool canRead = false;
            bool canModify = false;
            bool canDelete = false;
            bool canCreate = false;
            long single = 0;        // allow any single entry through .. 0 = unset; -1 = multiple entries; +ve = single entry value

            foreach ( long permissionId in permissionIds )
            {
                if ( single == 0 )
                    single = permissionId;
                else
                    single = -1;

                if ( permissionId == Permissions.Read.Id )
                    canRead = true;
                else if ( permissionId == Permissions.Modify.Id )
                    canModify = true;
                else if ( permissionId == Permissions.Delete.Id )
                    canDelete = true;
                else if ( permissionId == Permissions.Create.Id )
                    canCreate = true;
                else
                    throw new InvalidOperationException("Unknown permission");
            }

            if ( single > 0 )
                return single;
            if ( canCreate )
                return Permissions.Create.Id;
            if ( canDelete )
                return Permissions.Delete.Id;
            if ( canModify )
                return Permissions.Modify.Id;
            if ( canRead )
                return Permissions.Read.Id;

            throw new InvalidOperationException( "Unknown permission" );
        }
	}
}

/////
// ReSharper restore CheckNamespace
/////