// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Services.ApplicationManager;

namespace EDC.ReadiNow.CAST.Services
{
    /// <summary>
    /// Provides a means for interacting with apps and packages on the platform.
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private ICastEntityHelper CastEntityHelper { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public ApplicationService()
        {
            CastEntityHelper = Factory.Current.Resolve<ICastEntityHelper>();
        }

        /// <summary>
        /// Gets a full list of applications that this platform is aware of.
        /// </summary>
        /// <returns>List of available applications.</returns>
        public IEnumerable<AvailableApplication> GetApps()
        {
            using (Profiler.Measure("ApplicationService.GetApps"))
            {
                using (new TenantAdministratorContext(0))
                {
                    var list = new List<AvailableApplication>();

                    var apps = CastEntityHelper.GetEntitiesByType<App>().ToList();
                    apps.ForEach(app =>
                    {
                        var versions = app.ApplicationPackages.ToList();
                        versions.ForEach(version => list.Add(new AvailableApplication
                        {
                            Name = app.Name,
                            ApplicationEntityId = app.Id,
                            ApplicationVersionId = version.AppVerId ?? Guid.Empty,
                            PackageEntityId = version.Id,
                            PackageVersion = version.AppVersionString,
                            ApplicationId = app.ApplicationId ?? Guid.Empty,
                            Publisher = app.Publisher,
                            PublisherUrl = app.PublisherUrl,
                            ReleaseDate = app.ReleaseDate
                        }));
                    });

                    return list;
                }
            }
        }

        /// <summary>
        /// Get the list of applications presently installed on the tenant.
        /// </summary>
        /// <param name="tenant">The name of the tenant.</param>
        /// <returns>The list of installed application information.</returns>
        public IEnumerable<InstalledApplication> GetInstalledApps(string tenant)
        {
            using (Profiler.Measure("ApplicationService.GetInstalledApps"))
            {
                using (new TenantAdministratorContext(tenant))
                {
                    return GetInstalledAppsImpl();
                }
            }
        }

        /// <summary>
        /// Get the list of applications presently installed on the tenant.
        /// </summary>
        /// <param name="id">The id of the tenant.</param>
        /// <returns>The list of installed application information.</returns>
        public IEnumerable<InstalledApplication> GetInstalledApps(long id)
        {
            using (Profiler.Measure("ApplicationService.GetInstalledApps"))
            {
                using (new TenantAdministratorContext(id))
                {
                    return GetInstalledAppsImpl();
                }
            }
        }

        /// <summary>
        /// Installs a specific application version to a tenant with the given name.
        /// </summary>
        /// <param name="tenant">The name of the tenant.</param>
        /// <param name="appId">The identifier of the application.</param>
        /// <param name="appVersion">Specifies a specific version number of the application to install.</param>
        public void Install(string tenant, Guid appId, string appVersion = null)
        {
            using (Profiler.Measure("ApplicationService.Install"))
            {
                try
                {
                    AppManager.DeployApp(tenant, appId.ToString("B"), appVersion);
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("Failed to install app {0} to tenant {1}.", appId, tenant);
                    throw;
                }
                finally
                {
                    InvalidateTenant(tenant);
                }
            }
        }

        /// <summary>
        /// Uninstalls a specific application version from a tenant with the given name.
        /// </summary>
        /// <param name="tenant">The name of the tenant.</param>
        /// <param name="appId">The identifier of the application.</param>
        public void Uninstall(string tenant, Guid appId)
        {
            using (Profiler.Measure("ApplicationService.Uninstall"))
            {
                try
                {
                    AppManager.RemoveApp(tenant, appId.ToString("B"));
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("Failed to uninstall app {0} from tenant {1}", appId, tenant);
                    throw;
                }
                finally
                {
                    InvalidateTenant(tenant);
                }
            }
        }

        #region Private Methods

        private IEnumerable<InstalledApplication> GetInstalledAppsImpl()
        {
            var available = GetApps();
            var installed = new List<InstalledApplication>();

            if (!Entity.Exists(new EntityRef("core:solution")))
                return installed;

            var solutions = CastEntityHelper.GetEntitiesByType<Solution>().ToList();
            solutions.ForEach(solution =>
            {
                var app = available.FirstOrDefault(a => a.ApplicationVersionId == solution.PackageId);
                installed.Add(new InstalledApplication
                {
                    Name = solution.Name,
                    SolutionEntityId = solution.Id,
                    SolutionVersion = solution.SolutionVersionString,
                    ApplicationVersionId = solution.PackageId ?? Guid.Empty,
                    PackageEntityId = app != null ? app.PackageEntityId : -1,
                    PackageVersion = app != null ? app.PackageVersion : null,
                    ApplicationEntityId = app != null ? app.ApplicationEntityId : -1,
                    ApplicationId = app != null ? app.ApplicationId : Guid.Empty,
                    Publisher = app != null ? app.Publisher : null,
                    PublisherUrl = app != null ? app.PublisherUrl : null,
                    ReleaseDate = solution.SolutionReleaseDate
                });
            });

            return installed;
        }

        private void InvalidateTenant(string tenant)
        {
            long tid;
            using (new GlobalAdministratorContext())
            {
                tid = TenantHelper.GetTenantId(tenant);
            }

            if (tid > 0)
            {
                using (new TenantAdministratorContext(tid))
                {
                    TenantHelper.Invalidate(tid);
                }
            }
        }

        #endregion
    }
}
