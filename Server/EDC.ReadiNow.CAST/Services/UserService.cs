// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using Tenant = EDC.SoftwarePlatform.Install.Common.Tenant;

namespace EDC.ReadiNow.CAST.Services
{
    /// <summary>
    /// Provides a means for interacting with user accounts and roles on any tenant.
    /// </summary>
    public class UserService : IUserService
    {
        private ICastEntityHelper CastEntityHelper { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public UserService()
        {
            CastEntityHelper = Factory.Current.Resolve<ICastEntityHelper>();
        }

        /// <summary>
        /// Gets a list of the names of the present roles that exist in the tenant.
        /// </summary>
        /// <param name="tenant">The tenant to check.</param>
        /// <returns>List of role names.</returns>
        public IList<string> GetRoles(string tenant)
        {
            using (Profiler.Measure("UserService.GetRoles"))
            {
                using (new TenantAdministratorContext(tenant))
                {
                    return GetRolesImpl();
                }
            }
        }

        /// <summary>
        /// Gets a list of the names of the present roles that exist in the tenant.
        /// </summary>
        /// <param name="id">The tenant id to check.</param>
        /// <returns>List of role names.</returns>
        public IList<string> GetRoles(long id)
        {
            using (Profiler.Measure("UserService.GetRoles"))
            {
                using (new TenantAdministratorContext(id))
                {
                    return GetRolesImpl();
                }
            }
        }

        /// <summary>
        /// Gets information about the user with the given name.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <param name="tenant">The tenant to check.</param>
        /// <returns>The user info.</returns>
        public RemoteUserInfo GetUser(string name, string tenant)
        {
            using (Profiler.Measure("UserService.GetUser"))
            {
                using (new TenantAdministratorContext(tenant))
                {
                    var user = GetUserByName(name);
                    if (user == null)
                    {
                        return null;
                    }

                    return GetUserInfo(user);
                }
            }
        }

        /// <summary>
        /// Gets a list of information about the users that already exist in a tenant.
        /// </summary>
        /// <param name="tenant">The tenant to check.</param>
        /// <returns>List of user info.</returns>
        public IList<RemoteUserInfo> GetUsers(string tenant)
        {
            using (Profiler.Measure("UserService.GetUsers"))
            {
                using (new TenantAdministratorContext(tenant))
                {
                    return GetUsersImpl();
                }
            }
        }

        /// <summary>
        /// Gets a list of information about the users that already exist in a tenant.
        /// </summary>
        /// <param name="id">The tenant id to check.</param>
        /// <returns>List of user info.</returns>
        public IList<RemoteUserInfo> GetUsers(long id)
        {
            using (Profiler.Measure("UserService.GetUsers"))
            {
                using (new TenantAdministratorContext(id))
                {
                    return GetUsersImpl();
                }
            }
        }

        /// <summary>
        /// Creates a new user in the given tenant. 
        /// </summary>
        /// <param name="user">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="tenant">The tenant to create the user in.</param>
        /// <param name="roles">The names of the roles to add the user to.</param>
        public void Create(string user, string password, string tenant, List<string> roles)
        {
            using (Profiler.Measure("UserService.Create"))
            {
                try
                {
                    var role = "";
                    if (roles != null && roles.Count > 0)
                    {
                        role = roles[0];
                    }

                    Tenant.CreateUser(user, password, tenant, role);

                    using (new TenantAdministratorContext(tenant))
                    {
                        var userAccount = GetUserByName(user);
                        if (userAccount == null)
                        {
                            throw new Exception("The expected user does not exist.");
                        }

                        userAccount = userAccount.AsWritable<UserAccount>();

                        if (roles != null)
                        {
                            // first role already handled. process any others.
                            for (var r = 1; r < roles.Count; r++) 
                            {
                                var roleName = roles[r];

                                var userRole = CastEntityHelper.GetEntityByField<Role>(new EntityRef(Role.Name_Field), roleName);

                                if (userRole != null)
                                {
                                    userAccount.UserHasRole.Add(userRole);
                                }
                            }
                        }

                        userAccount.Save();
                    }
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("Failed to create the user {0}", user);
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes a user from the given tenant.
        /// </summary>
        /// <param name="user">The user name.</param>
        /// <param name="tenant">The tenant containing the user.</param>
        public void Delete(string user, string tenant)
        {
            using (Profiler.Measure("UserService.DeleteUser"))
            {
                if (user == null)
                {
                    throw new ArgumentNullException("user");
                }

                try
                {
                    long id = 0;
                    using (new TenantAdministratorContext(tenant))
                    {
                        var userAccount = GetUserByName(user);
                        if (userAccount != null)
                        {
                            id = userAccount.Id;
                        }
                    }

                    if (id > 0)
                    {
                        Tenant.DeleteUser(user, tenant);

                        // Can't do this. It's always cached!
                        //if (Entity.Exists(new EntityRef(id)))
                        //    throw new Exception("The user still exists.");
                    }
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("Failed to delete the user {0}", user);
                    throw;
                }
            }
        }

        #region Private Methods

        private UserStatus GetStatus(UserAccountStatusEnum_Enumeration? status)
        {
            switch (status)
            {
                case UserAccountStatusEnum_Enumeration.Active:
                    return UserStatus.Active;
                case UserAccountStatusEnum_Enumeration.Disabled:
                    return UserStatus.Disabled;
                case UserAccountStatusEnum_Enumeration.Expired:
                    return UserStatus.Expired;
                case UserAccountStatusEnum_Enumeration.Locked:
                    return UserStatus.Locked;
                default:
                    return UserStatus.Unknown;
            }
        }

        private UserAccount GetUserByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            return CastEntityHelper.GetEntityByField<UserAccount>(new EntityRef(UserAccount.Name_Field), name);
        }

        private RemoteUserInfo GetUserInfo(UserAccount user)
        {
            return new RemoteUserInfo
            {
                RemoteId = user.Id,
                Name = user.Name,
                Status = GetStatus(user.AccountStatus_Enum),
                Roles = new RoleList(user.UserHasRole.Select(r => r.Name))
            };
        }

        private IList<string> GetRolesImpl()
        {
            if (!Entity.Exists(new EntityRef("core:role")))
                return new List<string>();

            var roles = CastEntityHelper.GetEntitiesByType<Role>();

            return roles.Select(r => r.Name).ToList();
        }

        private IList<RemoteUserInfo> GetUsersImpl()
        {
            if (!Entity.Exists(new EntityRef("core:userAccount")))
                return new List<RemoteUserInfo>();

            var users = CastEntityHelper.GetEntitiesByType<UserAccount>();

            return users.Select(GetUserInfo).ToList();
        }

        #endregion
    }
}
