// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration;
using EDC.SoftwarePlatform.Migration.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.IO;
using Tenant = EDC.SoftwarePlatform.Install.Common.Tenant;
using TenantModel = EDC.ReadiNow.Model.Tenant;

namespace EDC.ReadiNow.CAST.Services
{
    /// <summary>
    /// Implements the tenant based operations available to CAST Management.
    /// </summary>
    public class TenantService : ITenantService
    {
        #region Private Properties
        
        private ICastEntityHelper CastEntityHelper { get; set; }

        private IEntityRepository EntityRepository { get; set; }
        
        private IApplicationService ApplicationService { get; set; }

        private IUserService UserService { get; set; }

        #endregion

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public TenantService()
        {
            CastEntityHelper = Factory.Current.Resolve<ICastEntityHelper>();
            EntityRepository = Factory.Current.Resolve<IEntityRepository>();
            ApplicationService = Factory.Current.Resolve<IApplicationService>();
            UserService = Factory.Current.Resolve<IUserService>();
        }

        /// <summary>
        /// Gets information about a tenant with the given name.
        /// </summary>
        /// <param name="name">The tenant name.</param>
        /// <returns>A tenant info object.</returns>
        public RemoteTenantInfo GetTenant(string name)
        {
            using (Profiler.Measure("TenantService.GetTenant"))
            {
                using (new GlobalAdministratorContext())
                {
                    var tenant = CastEntityHelper.GetEntityByField<TenantModel>(new EntityRef(TenantModel.Name_Field), name);

                    if (tenant == null)
                    {
                        return null;
                    }

                    return GetTenantInfo(tenant);
                }
            }
        }

        /// <summary>
        /// Gets information about the tenants that exists.
        /// </summary>
        /// <returns>List of remote tenant info objects.</returns>
        public IList<RemoteTenantInfo> GetTenants()
        {
            using (Profiler.Measure("TenantService.GetTenants"))
            {
                using (new GlobalAdministratorContext())
                {
                    var tenants = TenantHelper.GetAll();

                    if (tenants == null)
                    {
                        return new List<RemoteTenantInfo>();
                    }

                    return tenants.Select(GetTenantInfo).ToList();
                }
            }
        }

        /// <summary>
        /// Creates a new tenant given only a name.
        /// </summary>
        /// <param name="name">The name to use for the new tenant.</param>
        public void Create(string name)
        {
            using (Profiler.Measure("TenantService.Create"))
            {
                try
                {
                    Tenant.CreateTenant(name);
                    AppManager.DeployApp(name, Applications.CoreApplicationId.ToString("B"));
                    AppManager.DeployApp(name, Applications.ConsoleApplicationId.ToString("B"));
                    AppManager.DeployApp(name, Applications.CoreDataApplicationId.ToString("B"));

                    if (!Tenant.GetTenants().Contains(name))
                    {
                        throw new Exception("The expected tenant does not exist.");
                    }
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("Failed to create tenant {0}", name);
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes an existing tenant of the given name.
        /// </summary>
        /// <param name="name">The name of the tenant.</param>
        public void Delete(string name)
        {
            using (Profiler.Measure("TenantService.Delete"))
            {
                try
                {
                    Tenant.DeleteTenant(name);

                    if (Tenant.GetTenants().Contains(name))
                    {
                        throw new Exception("The tenant still exists.");
                    }
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("Failed to delete tenant {0}", name);
                    throw;
                }
            }
        }

        /// <summary>
        /// Disables a tenant.
        /// </summary>
        /// <param name="name">The name of the tenant to disable.</param>
        public void Disable(string name)
        {
            using (Profiler.Measure("TenantService.Disable"))
            {
                try
                {
                    Tenant.DisableTenant(name);
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("Failed to disable tenant {0}", name);
                    throw;
                }
            }
        }

        /// <summary>
        /// Enables a tenant.
        /// </summary>
        /// <param name="name">The name of the tenant to enable.</param>
        public void Enable(string name)
        {
            using (Profiler.Measure("TenantService.Enable"))
            {
                try
                {
                    Tenant.EnableTenant(name);
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("Failed to enable tenant {0}", name);
                    throw;
                }
            }
        }

        /// <summary>
        /// Renames the tenant.
        /// </summary>
        /// <param name="id">The id of the tenant to rename.</param>
        /// <param name="name">The new name to give the tenant.</param>
        public void Rename(long id, string name)
        {
            using (Profiler.Measure("TenantService.Rename"))
            {
                try
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new ArgumentException(@"New tenant name may not be null or empty.", "name");
                    }

                    string old;

                    using (new GlobalAdministratorContext())
                    {
                        var tenant = EntityRepository.Get<TenantModel>(id);
                        if (tenant == null)
                            throw new Exception("Tenant not found.");

                        old = tenant.Name;
                    }

                    if (old == null)
                        throw new Exception("Tenant has no name.");

                    Tenant.RenameTenant(old, name);
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("Failed to rename tenant {0}", id);
                    throw;
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Builds a tenant info object from the properties of the loaded tenant entity.
        /// </summary>
        /// <param name="tenant">The tenant entity.</param>
        /// <returns>The tenant info object.</returns>
        private RemoteTenantInfo GetTenantInfo(TenantModel tenant)
        {
            using (Profiler.Measure("TenantService.GetTenantInfo"))
            {
                return new RemoteTenantInfo
                {
                    RemoteId = tenant.Id,
                    Name = tenant.Name,
                    Disabled = tenant.IsTenantDisabled == true,
                    Apps = ApplicationService.GetInstalledApps(tenant.Id).ToList(),
                    Roles = new RoleList(UserService.GetRoles(tenant.Id)),
                    Users = new UserList(UserService.GetUsers(tenant.Id))
                };
            }
        }

        #endregion
    }
}
