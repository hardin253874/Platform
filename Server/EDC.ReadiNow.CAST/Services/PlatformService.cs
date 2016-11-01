// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Services.ApplicationManager;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.CAST.Services
{
    /// <summary>
    /// Implements the platform related operations available to CAST Management.
    /// </summary>
    public class PlatformService : IPlatformService
    {
        private ICastEntityHelper CastEntityHelper { get; set; }

        private IEntityRepository EntityRepository { get; set; }
        
        /// <summary>
        /// Basic constructor.
        /// </summary>
        public PlatformService()
        {
            CastEntityHelper = Factory.Current.Resolve<ICastEntityHelper>();

            EntityRepository = Factory.GraphEntityRepository;
        }

        public string GetDatabaseId()
        {
            using (Profiler.Measure("PlatformService.GetDatabaseId"))
            {
                var result = string.Empty;

                using (var ctx = DatabaseContext.GetContext())
                {
                    using (var command = ctx.CreateCommand("SELECT TOP 1 [Id] FROM [RNDB]"))
                    {
                        var rndb = command.ExecuteScalar();
                        if (rndb != null && rndb != DBNull.Value)
                        {
                            result = rndb.ToString().ToLowerInvariant();
                        }
                    }
                }

                return result;
            }
        }

        public IManagedApp GetApp(Guid appId)
        {
            using (Profiler.Measure("PlatformService.GetApp"))
            {
                return CastEntityHelper.GetEntityByField<ManagedApp>(new EntityRef(ManagedAppSchema.AppIdField), appId.ToString("B"));
            }
        }

        public IManagedAppVersion GetAppVersion(Guid appVersionId)
        {
            using (Profiler.Measure("PlatformService.GetAppVersion"))
            {
                return CastEntityHelper.GetEntityByField<ManagedAppVersion>(new EntityRef(ManagedAppVersionSchema.AppVersionIdField), appVersionId.ToString("B"));
            }
        }

        public IManagedPlatform GetPlatformByDatabaseId(string id)
        {
            using (Profiler.Measure("PlatformService.GetPlatformByDatabaseId"))
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id");
                }
                
                var platform = CastEntityHelper.GetEntityByField<ManagedPlatform>(new EntityRef(ManagedPlatformSchema.DatabaseIdField), id);

                if (platform != null)
                {
                    // pre-load? may be doing this wrong. i forget how to check it.
                    EntityRepository.Get<ManagedPlatform>(platform.Id, ManagedPlatform.ManagedPlatformPreloadQuery);
                }

                return platform;

                //return CastQueryHelper.GetEntityByFieldValue<ManagedPlatform>(
                //    new EntityRef(ManagedPlatform.ManagedPlatformType),
                //    new EntityRef(ManagedPlatform.DatabaseIdField),
                //    id, preloadQuery: ManagedPlatform.ManagedPlatformPreloadQuery);
            }
        }

        public IManagedPlatform CreateOrUpdate(RemotePlatformInfo pi)
        {
            using (Profiler.Measure("PlatformService.CreateOrUpdate"))
            {
                if (pi == null || string.IsNullOrEmpty(pi.Id))
                    throw new ArgumentException("Platform information was invalid.");

                var entities = new EntityCollection<IEntity>();

                IManagedPlatform mp;

                try
                {
                    mp = GetPlatformByDatabaseId(pi.Id);

                    // load or create the platform instance
                    mp = mp != null ? mp.AsWritable<ManagedPlatform>() : Create(pi.Id);
                    if (mp == null)
                    {
                        throw new Exception(string.Format("Failed to create or update info for the platform instance '{0}'.", pi.Id));
                    }

                    mp.LastContact = DateTime.UtcNow;

                    entities.Add(mp);

                    var fe = UpdateFrontEnds(mp, pi.FrontEndHost, pi.FrontEndDomain, entities);
                    mp.FrontEndHistory.Add(fe);

                    var db = UpdateDatabases(mp, pi.Database, pi.DatabaseServer, entities);
                    mp.DatabaseHistory.Add(db);

                    var apps = GetApps().ToList();

                    var appVersions = UpdateAvailableApps(mp, pi.Apps, entities, apps);
                    mp.AvailableAppVersions.AddRange(appVersions);

                    var tenants = UpdateTenants(mp, pi.Tenants, entities, apps);
                    mp.ContainsTenants.AddRange(tenants);
                }
                finally
                {
                    CastEntityHelper.Save(entities);
                }

                return mp;
            }
        }

        public IManagedTenant CreateOrUpdateTenant(string databaseId, RemoteTenantInfo ti)
        {
            using (Profiler.Measure("PlatformService.CreateOrUpdateTenant"))
            {
                if (ti == null)
                    throw new ArgumentException("Tenant information was invalid.");

                var entities = new EntityCollection<IEntity>();

                IManagedTenant mt;

                try
                {
                    var mp = GetPlatformByDatabaseId(databaseId);
                    if (mp != null)
                    {
                        mp = mp.AsWritable<ManagedPlatform>();

                        entities.Add(mp);
                    }

                    var apps = GetApps().ToList();

                    mt = UpdateTenant(mp, ti, entities, apps);

                    entities.Add(mt);
                }
                finally
                {
                    CastEntityHelper.Save(entities);
                }

                return mt;
            }
        }

        public IManagedUser CreateOrUpdateUser(string databaseId, string tenant, RemoteUserInfo u)
        {
            using (Profiler.Measure("PlatformService.CreateOrUpdateUser"))
            {
                if (u == null)
                    throw new ArgumentException("User information was invalid.");

                var entities = new EntityCollection<IEntity>();

                var mu = default(IManagedUser);

                try
                {
                    var mp = GetPlatformByDatabaseId(databaseId);
                    if (mp != null)
                    {
                        mp = mp.AsWritable<ManagedPlatform>();

                        var mt = mp.ContainsTenants.FirstOrDefault(t => t.Name == tenant);
                        if (mt != null)
                        {
                            mt = mt.AsWritable<ManagedTenant>();

                            entities.Add(mt);

                            var roles = GetRoles().ToList();

                            mu = UpdateUser(mt, u, roles);

                            entities.Add(mu);
                        }
                    }
                }
                finally
                {
                    CastEntityHelper.Save(entities);
                }

                return mu;
            }
        }

        public IManagedTenant UpdateInstalledApplications(string databaseId, string tenant, IList<InstalledApplication> apps)
        {
            using (Profiler.Measure("PlatformService.UpdateInstalledApplications"))
            {
                var entities = new EntityCollection<IEntity>();

                var mt = default(IManagedTenant);

                try
                {
                    var mp = GetPlatformByDatabaseId(databaseId);
                    if (mp != null)
                    {
                        mp = mp.AsWritable<ManagedPlatform>();

                        mt = mp.ContainsTenants.FirstOrDefault(t => t.Name == tenant);
                        if (mt != null)
                        {
                            mt = mt.AsWritable<ManagedTenant>();

                            entities.Add(mt);

                            var existingApps = GetApps().ToList();

                            var installed = UpdateInstalledApps(mt, apps, entities, existingApps);

                            mt.HasAppsInstalled.AddRange(installed);
                        }
                    }
                }
                finally
                {
                    CastEntityHelper.Save(entities);
                }

                return mt;
            }
        }

        public void DeleteTenant(string databaseId, string tenant)
        {
            using (Profiler.Measure("PlatformService.DeleteTenant"))
            {
                var mp = GetPlatformByDatabaseId(databaseId);
                if (mp != null)
                {
                    var mt = mp.ContainsTenants.FirstOrDefault(t => t.Name == tenant);
                    if (mt != null)
                    {
                        mt.AsWritable<ManagedTenant>().Delete();
                    }
                }
            }
        }

        public void DeleteUser(string databaseId, string tenant, string user)
        {
            using (Profiler.Measure("PlatformService.DeleteUser"))
            {
                var mp = GetPlatformByDatabaseId(databaseId);
                if (mp != null)
                {
                    var mt = mp.ContainsTenants.FirstOrDefault(t => t.Name == tenant);
                    if (mt != null)
                    {
                        var mu = mt.Users.FirstOrDefault(u => u.Name == user);
                        if (mu != null)
                        {
                            mu.AsWritable<ManagedUser>().Delete();
                        }
                    }
                }
            }
        }

        #region Platform

        private ManagedPlatform Create(string dbid)
        {
            using (Profiler.Measure("PlatformService.Create"))
            {
                var managedPlatform = CastEntityHelper.CreatePlatform();

                managedPlatform.Name = dbid;
                managedPlatform.DatabaseId = dbid;
                managedPlatform.LastContact = DateTime.UtcNow;

                return (ManagedPlatform)managedPlatform;
            }
        }

        private PlatformFrontEnd UpdateFrontEnds(IManagedPlatform platform, string host, string domain, ICollection<IEntity> entities)
        {
            using (Profiler.Measure("PlatformService.UpdateFrontEnds"))
            {
                if (string.IsNullOrEmpty(host))
                {
                    throw new ArgumentNullException("host");
                }

                IPlatformFrontEnd fe = null;

                if (platform != null)
                {
                    fe = platform.FrontEndHistory.FirstOrDefault(f => 
                    string.Equals(f.Host, host, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(f.Domain, domain, StringComparison.InvariantCultureIgnoreCase));
                }

                if (fe != null)
                {
                    fe = fe.AsWritable<PlatformFrontEnd>();
                }
                else
                {
                    fe = CastEntityHelper.CreatePlatformFrontEnd();
                    fe.Host = host;
                    fe.Domain = domain;
                }

                fe.Name = host;
                if (!string.IsNullOrEmpty(domain) && !host.EndsWith(domain))
                {
                    fe.Name = host + "." + domain;
                }
                fe.LastContact = DateTime.UtcNow;

                entities.Add(fe);

                return (PlatformFrontEnd)fe;
            }
        }

        private PlatformDatabase UpdateDatabases(IManagedPlatform platform, string catalog, string server, ICollection<IEntity> entities)
        {
            using (Profiler.Measure("PlatformService.UpdateDatabases"))
            {
                if (string.IsNullOrEmpty(catalog))
                {
                    throw new ArgumentNullException("catalog");
                }

                if (string.IsNullOrEmpty(server))
                {
                    throw new ArgumentNullException("server");
                }

                IPlatformDatabase db = null;

                if (platform != null)
                {
                    db = platform.DatabaseHistory.FirstOrDefault(d =>
                    string.Equals(d.Catalog, catalog, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(d.Server, server, StringComparison.InvariantCultureIgnoreCase));
                }

                if (db != null)
                {
                    db = db.AsWritable<PlatformDatabase>();
                }
                else
                {
                    db = CastEntityHelper.CreatePlatformDatabase();
                    db.Catalog = catalog;
                    db.Server = server;
                }

                db.Name = string.Format("{0} ({1})", server, catalog);
                db.LastContact = DateTime.UtcNow;

                entities.Add(db);

                return (PlatformDatabase)db;
            }
        }
        
        #endregion

        #region Tenants

        private IEnumerable<ManagedTenant> UpdateTenants(IManagedPlatform platform, IList<RemoteTenantInfo> tenants, ICollection<IEntity> entities, ICollection<ManagedApp> existingApps)
        {
            using (Profiler.Measure("PlatformService.UpdateTenants"))
            {
                if (platform == null)
                {
                    throw new ArgumentNullException("platform");
                }

                var ids = tenants == null ? Enumerable.Empty<string>() : tenants.Select(t => t.RemoteId.ToString());

                CastEntityHelper.Delete(platform.ContainsTenants.Where(t => !ids.Contains(t.RemoteId)).Select(t => new EntityRef(t.Id)));

                if (tenants == null)
                {
                    return new List<ManagedTenant>();
                }

                var updatedEntities = tenants.Select(t => UpdateTenant(platform, t, entities, existingApps)).ToList();

                entities.AddRange(updatedEntities);

                return updatedEntities;
            }
        }

        private ManagedTenant UpdateTenant(IManagedPlatform platform, RemoteTenantInfo tenant, ICollection<IEntity> entities, ICollection<ManagedApp> existingApps)
        {
            using (Profiler.Measure("PlatformService.UpdateTenant"))
            {
                IManagedTenant mt = null;

                if (platform != null)
                {
                    mt = platform.ContainsTenants.FirstOrDefault(t => t.RemoteId == tenant.RemoteId.ToString());
                }

                if (mt != null)
                {
                    mt = mt.AsWritable<ManagedTenant>();
                }
                else
                {
                    mt = CastEntityHelper.CreateTenant();
                    mt.RemoteId = tenant.RemoteId.ToString();
                }

                mt.Name = tenant.Name;
                mt.Disabled = tenant.Disabled;

                var roles = CreateOrUpdateRoles(tenant.Roles, entities).ToList();
                roles.AddRange(GetRoles());

                var userRoles = UpdateRoles(mt, tenant.Roles, roles);
                mt.UserRoles.AddRange(userRoles);

                var users = UpdateUsers(mt, tenant.Users, roles, entities);
                mt.Users.AddRange(users);

                var apps = UpdateInstalledApps(mt, tenant.Apps, entities, existingApps);
                mt.HasAppsInstalled.AddRange(apps);

                if (platform != null)
                {
                    mt.Platform = platform;
                }

                return (ManagedTenant)mt;
            }
        }

        #endregion

        #region Apps

        private IEnumerable<ManagedApp> GetApps()
        {
            using (Profiler.Measure("PlatformService.GetApps"))
            {
                return CastEntityHelper.GetEntitiesByType<ManagedApp>();
            }
        }

        private ManagedAppVersion UpdateAppVersion(IManagedPlatform platform, IManagedTenant tenant, AvailableApplication app, ICollection<ManagedApp> existingApps)
        {
            using (Profiler.Measure("PlatformService.UpdateAppVersion"))
            {
                IManagedApp managedApp = existingApps.FirstOrDefault(e => e.ApplicationId == app.ApplicationId);
                IManagedAppVersion managedAppVersion = null;

                // does it exit on the platform?
                if (platform != null)
                {
                    managedAppVersion = platform.AvailableAppVersions.FirstOrDefault(v => v.VersionId == app.ApplicationVersionId) ??
                                        GetAppVersion(app.ApplicationVersionId);
                }

                // or the tenant?
                if (tenant != null && managedAppVersion == null)
                {
                    managedAppVersion = tenant.HasAppsInstalled.FirstOrDefault(i => i.VersionId == app.ApplicationVersionId);
                }

                // or in amongst the list of apps just being created?
                if (managedAppVersion == null)
                {
                    managedAppVersion = existingApps.SelectMany(a => a.Versions).FirstOrDefault(v => 
                    (v.VersionId == app.ApplicationVersionId) &&
                    (v.Version == app.PackageVersion) &&
                    (v.Name == app.Name));
                }

                if (managedAppVersion != null)
                {
                    managedAppVersion = managedAppVersion.AsWritable<ManagedAppVersion>();

                    // it should have an app
                    if (managedAppVersion.Application != null)
                    {
                        managedApp = managedAppVersion.Application;
                    }
                }
                else
                {
                    managedAppVersion = CastEntityHelper.CreateAppVersion();
                    managedAppVersion.VersionId = app.ApplicationVersionId;
                }

                managedAppVersion.Name = app.Name;
                managedAppVersion.Version = app.PackageVersion;
                managedAppVersion.PublishDate = app.ReleaseDate;

                // does the app exist then?
                if (managedApp == null)
                {
                    managedApp = UpdateApp(app);
                }

                if (managedApp != null)
                {
                    managedApp = managedApp.AsWritable<ManagedApp>();
                    managedApp.Versions.Add((ManagedAppVersion)managedAppVersion);

                    managedAppVersion.Application = managedApp;
                    
                    existingApps.Add((ManagedApp)managedApp);
                }

                if (platform != null || tenant != null)
                {
                    if (platform != null)
                    {
                        platform.AvailableAppVersions.Add((ManagedAppVersion)managedAppVersion);
                    }

                    if (tenant != null)
                    {
                        tenant.HasAppsInstalled.Add((ManagedAppVersion)managedAppVersion);
                    }
                }

                return (ManagedAppVersion)managedAppVersion;
            }
        }

        private IEnumerable<ManagedAppVersion> UpdateAvailableApps(IManagedPlatform platform, ICollection<AvailableApplication> apps, ICollection<IEntity> entities, ICollection<ManagedApp> existingApps)
        {
            using (Profiler.Measure("PlatformService.UpdateAvailableApps"))
            {
                if (platform == null)
                {
                    throw new ArgumentNullException("platform");
                }

                var availableApps = apps ?? new List<AvailableApplication>();

                // remove any app versions no longer on the platform
                var validVersionIds = availableApps.Where(a => a.ApplicationVersionId != Guid.Empty).Select(a => a.ApplicationVersionId).ToList();

                var removeApps = platform.AvailableAppVersions
                                         .Where(availableAppVersion => (availableAppVersion.VersionId == null) ||
                                               (availableAppVersion.VersionId.Value == Guid.Empty) ||
                                               !validVersionIds.Contains(availableAppVersion.VersionId.Value)).ToList();

                removeApps.ForEach(r => platform.AvailableAppVersions.Remove(r));

                var updatedEntities = availableApps.Where(a => a.ApplicationVersionId != Guid.Empty)
                    .Select(a => UpdateAppVersion(platform, null, a, existingApps))
                    .ToList();

                entities.AddRange(updatedEntities);

                return updatedEntities;
            }
        }
        
        private IEnumerable<ManagedAppVersion> UpdateInstalledApps(IManagedTenant tenant, ICollection<InstalledApplication> apps, ICollection<IEntity> entities, ICollection<ManagedApp> existingApps)
        {
            using (Profiler.Measure("PlatformService.UpdateInstalledApps"))
            {
                if (tenant == null)
                {
                    throw new ArgumentNullException("tenant");
                }

                var installedApps = apps ?? new List<InstalledApplication>();

                // remove any app versions no longer on the tenant
                var validVersionIds = installedApps.Where(a => a.ApplicationVersionId != Guid.Empty).Select(a => a.ApplicationVersionId).ToList();

                var removeApps = tenant.HasAppsInstalled
                                       .Where(installedAppVersion => (installedAppVersion.VersionId == null) ||
                                       (installedAppVersion.VersionId.Value == Guid.Empty) ||
                                       !validVersionIds.Contains(installedAppVersion.VersionId.Value)).ToList();

                tenant.HasAppsInstalled.RemoveRange(removeApps);

                var updatedEntities = installedApps.Where(a => a.ApplicationVersionId != Guid.Empty).Select(a =>
                {
                    var mav = UpdateAppVersion(null, tenant, a, existingApps);
                    mav.Version = a.SolutionVersion;
                    return mav;
                }).ToList();

                entities.AddRange(updatedEntities);

                return updatedEntities;
            }
        }

        private ManagedApp UpdateApp(AvailableApplication app)
        {
            using (Profiler.Measure("PlatformService.UpdateApp"))
            {
                var managedApp = default(IManagedApp);

                if (app != null && app.ApplicationId != Guid.Empty)
                {
                    managedApp = GetApp(app.ApplicationId);

                    // update or create apps
                    if (managedApp != null)
                    {
                        managedApp = managedApp.AsWritable<ManagedApp>();
                    }
                    else
                    {
                        managedApp = CastEntityHelper.CreateApp();
                        managedApp.ApplicationId = app.ApplicationId;
                    }

                    managedApp.Name = app.Name;
                    managedApp.Publisher = app.Publisher;
                    managedApp.PublisherUrl = app.PublisherUrl;
                    managedApp.ReleaseDate = app.ReleaseDate;
                }

                return (ManagedApp)managedApp;
            }
        }

        #endregion

        #region Roles

        private IEnumerable<ManagedUserRole> GetRoles()
        {
            using (Profiler.Measure("PlatformService.GetRoles"))
            {
                return CastEntityHelper.GetEntitiesByType<ManagedUserRole>();
            }
        }

        private IEnumerable<ManagedUserRole> CreateOrUpdateRoles(RoleList roles, ICollection<IEntity> entities)
        {
            using (Profiler.Measure("PlatformService.CreateRole"))
            {
                var names = new HashSet<string>();
                var existingRoles = GetRoles().ToList();

                names.AddRange(existingRoles.Select(e => e.Name));

                var missingRoles = roles != null ? roles.Where(r => !names.Contains(r)) : Enumerable.Empty<string>();

                var newRoles = missingRoles.Select(CreateRole).ToList();

                entities.AddRange(newRoles);
                
                newRoles.AddRange(existingRoles.ToList());

                return newRoles;
            }
        }

        private ManagedUserRole CreateRole(string role)
        {
            using (Profiler.Measure("PlatformService.CreateRole"))
            {
                var mur = CastEntityHelper.CreateRole();
                mur.Name = role;
                return (ManagedUserRole)mur;
            }
        }

        private IEnumerable<ManagedUserRole> UpdateRoles(IManagedTenant tenant, RoleList roles, ICollection<ManagedUserRole> existingRoles)
        {
            using (Profiler.Measure("PlatformService.UpdateRoles"))
            {
                if (tenant == null)
                {
                    throw new ArgumentNullException("tenant");
                }

                var removeRoles = roles != null ? tenant.UserRoles.Where(r => !roles.Contains(r.Name)) : Enumerable.Empty<ManagedUserRole>();
                tenant.UserRoles.RemoveRange(removeRoles);

                if (existingRoles == null)
                    return Enumerable.Empty<ManagedUserRole>();

                return roles != null ? existingRoles.Where(e => roles.Contains(e.Name)) : existingRoles;
            }
        }

        private IEnumerable<ManagedUserRole> UpdateRoles(IManagedUser user, RoleList roles, ICollection<ManagedUserRole> existingRoles)
        {
            using (Profiler.Measure("PlatformService.UpdateRoles"))
            {
                if (user == null)
                {
                    throw new ArgumentNullException("user");
                }

                var removeRoles = roles != null ? user.Roles.Where(r => !roles.Contains(r.Name)) : Enumerable.Empty<ManagedUserRole>();
                user.Roles.RemoveRange(removeRoles);

                if (existingRoles == null)
                    return Enumerable.Empty<ManagedUserRole>();

                return roles != null ? existingRoles.Where(e => roles.Contains(e.Name)) : existingRoles;
            }
        }

        #endregion

        #region Users

        private IEnumerable<ManagedUser> UpdateUsers(IManagedTenant tenant, UserList users, ICollection<ManagedUserRole> roles, ICollection<IEntity> entities)
        {
            using (Profiler.Measure("PlatformService.UpdateUsers"))
            {
                if (tenant == null)
                {
                    throw new ArgumentNullException("tenant");
                }

                var ids = users == null ? Enumerable.Empty<string>() : users.Select(u => u.RemoteId.ToString());

                CastEntityHelper.Delete(tenant.Users.Where(u => !ids.Contains(u.RemoteId)).Select(t => new EntityRef(t.Id)));

                if (users == null)
                {
                    return Enumerable.Empty<ManagedUser>();
                }
                
                var updatedEntities = users.Where(u => u.RemoteId > 0).Select(u => UpdateUser(tenant, u, roles)).ToList();

                entities.AddRange(updatedEntities);

                return updatedEntities;
            }
        }

        private ManagedUser UpdateUser(IManagedTenant tenant, RemoteUserInfo user, ICollection<ManagedUserRole> roles)
        {
            using (Profiler.Measure("PlatformService.UpdateUser"))
            {
                var mu = default(IManagedUser);

                if (user.RemoteId <= 0)
                {
                    return null;
                }

                if (tenant != null)
                {
                    mu = tenant.Users.FirstOrDefault(u => u.RemoteId == user.RemoteId.ToString());
                }

                if (mu != null)
                {
                    mu = mu.AsWritable<ManagedUser>();
                }
                else
                {
                    mu = CastEntityHelper.CreateUser();
                    mu.RemoteId = user.RemoteId.ToString();
                }

                mu.Name = user.Name;

                switch (user.Status)
                {
                    case UserStatus.Active: mu.Status_Enum = ManagedUserStatusEnumeration.Active; break;
                    case UserStatus.Disabled: mu.Status_Enum = ManagedUserStatusEnumeration.Disabled; break;
                    case UserStatus.Expired: mu.Status_Enum = ManagedUserStatusEnumeration.Expired; break;
                    case UserStatus.Locked: mu.Status_Enum = ManagedUserStatusEnumeration.Locked; break;
                    default: mu.Status_Enum = ManagedUserStatusEnumeration.Unknown; break;
                }

                var r = UpdateRoles(mu, user.Roles, roles);
                mu.Roles.AddRange(r);

                if (tenant != null)
                {
                    mu.Tenant = tenant;
                }

                return (ManagedUser)mu;
            }
        }

        #endregion
    }
}
