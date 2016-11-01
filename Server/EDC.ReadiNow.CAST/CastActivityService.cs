// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using Autofac;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST
{
    /// <summary>
    /// Defines what action a platform installation (rig) may take in response to a request from the CAST Server.
    /// </summary>
    public class CastActivityService : ICastActivityService
    {
        #region Private Properties

        private ICastService CastService { get; set; }

        private ITenantService TenantService { get; set; }

        private IUserService UserService { get; set; }

        private IApplicationService ApplicationService { get; set; }

        #endregion
        
        /// <summary>
        /// Basic constructor.
        /// </summary>
        public CastActivityService()
        {
            CastService = Factory.Current.Resolve<ICastService>();
            TenantService = Factory.Current.Resolve<ITenantService>();
            UserService = Factory.Current.Resolve<IUserService>();
            ApplicationService = Factory.Current.Resolve<IApplicationService>();
        }

        /// <summary>
        /// Performs a simple write to the event log.
        /// </summary>
        /// <param name="logRequest">The log request.</param>
        /// <returns>The response to the request.</returns>
        public LogResponse Log(LogRequest logRequest)
        {
            using (Profiler.Measure("CastActivityService.Log"))
            {
                var response = new LogResponse();

                try
                {
                    if (logRequest == null)
                    {
                        throw new ArgumentNullException("logRequest");
                    }

                    if (!CastService.GetIsCastConfigured())
                    {
                        throw new InvalidOperationException();
                    }

                    EventLog.Application.WriteInformation(logRequest.Message);
                    response.Time = DateTime.UtcNow;
                }
                catch (Exception e)
                {
                    response.IsError = true;
                    response.Error = e.Message;
                }

                return response;
            }
        }

        /// <summary>
        /// Carries out an operation on a <see cref="Tenant"/> on this plaform identified by the request.
        /// </summary>
        /// <param name="tenantRequest">The request.</param>
        /// <returns>The response.</returns>
        public TenantInfoResponse TenantOperation(TenantOperationRequest tenantRequest)
        {
            using (Profiler.Measure("CastActivityService.TenantOperation"))
            {
                var response = new TenantInfoResponse
                {
                    Tenants = new TenantList()
                };

                try
                {
                    if (tenantRequest == null)
                    {
                        throw new ArgumentNullException("tenantRequest");
                    }

                    if (string.IsNullOrEmpty(tenantRequest.Name))
                    {
                        throw new ArgumentException("Tenant name may not be empty.");
                    }

                    if (!CastService.GetIsCastConfigured())
                    {
                        throw new InvalidOperationException();
                    }

                    var name = tenantRequest.Name;

                    switch (tenantRequest.Operation)
                    {
                        case Operation.Create:
                            TenantService.Create(name);
                            break;

                        case Operation.Delete:
                            TenantService.Delete(name);
                            break;

                        case Operation.Enable:
                            TenantService.Enable(name);
                            break;

                        case Operation.Disable:
                            TenantService.Disable(name);
                            break;

                        case Operation.Rename:
                            TenantService.Rename(tenantRequest.Id, tenantRequest.Name);
                            break;
                            
                        default:
                            throw new NotSupportedException(tenantRequest.Operation.ToString());
                    }

                    if (tenantRequest.Operation != Operation.Delete)
                    {
                        var ti = TenantService.GetTenant(name);
                        if (ti != null)
                        {
                            response.Tenants.Add(ti);
                        }
                    }
                }
                catch (Exception e)
                {
                    response.IsError = true;
                    response.Error = e.Message;
                }

                return response;
            }
        }

        /// <summary>
        /// Carries out an operation on a <see cref="UserAccount"/> within a tenant identified by the request.
        /// </summary>
        /// <param name="userRequest">The request.</param>
        /// <returns>The response.</returns>
        public UserInfoResponse UserOperation(UserOperationRequest userRequest)
        {
            using (Profiler.Measure("CastActivityService.UserOperation"))
            {
                var response = new UserInfoResponse
                {
                    Users = new UserList()
                };

                try
                {
                    if (userRequest == null)
                    {
                        throw new ArgumentNullException("userRequest");
                    }

                    if (!CastService.GetIsCastConfigured())
                    {
                        throw new InvalidOperationException();
                    }
                    
                    var name = userRequest.User;
                    var tenant = userRequest.Tenant;

                    switch (userRequest.Operation)
                    {
                        case Operation.Create:
                            UserService.Create(name, userRequest.Password, tenant, userRequest.Roles);
                            break;

                        case Operation.Delete:
                            UserService.Delete(name, tenant);
                            break;

                        default:
                            throw new NotSupportedException(userRequest.Operation.ToString());
                    }

                    if (userRequest.Operation != Operation.Delete)
                    {
                        var u = UserService.GetUser(name, tenant);
                        if (u != null)
                        {
                            response.Users.Add(u);
                        }
                    }
                }
                catch (Exception e)
                {
                    response.IsError = true;
                    response.Error = e.Message;
                }

                return response;
            }
        }

        /// <summary>
        /// Carries out an operation on an <see cref="App"/> with respect to the tenant identified by the request.
        /// </summary>
        /// <param name="appRequest">The request.</param>
        /// <returns>The response.</returns>
        public ApplicationInfoResponse ApplicationOperation(ApplicationOperationRequest appRequest)
        {
            using (Profiler.Measure("CastActivityService.ApplicationOperation"))
            {
                var response = new ApplicationInfoResponse();

                try
                {
                    if (appRequest == null)
                    {
                        throw new ArgumentNullException("appRequest");
                    }

                    if (!CastService.GetIsCastConfigured())
                    {
                        throw new InvalidOperationException();
                    }

                    var tenant = appRequest.Tenant;

                    switch (appRequest.Operation)
                    {
                        case Contracts.ApplicationOperation.Install:
                            ApplicationService.Install(tenant, appRequest.Id, appRequest.Version);
                            break;
                            
                        case Contracts.ApplicationOperation.Uninstall:
                            ApplicationService.Uninstall(tenant, appRequest.Id);
                            break;

                        default:
                            throw new NotSupportedException(appRequest.Operation.ToString());
                    }

                    var apps = ApplicationService.GetInstalledApps(tenant);
                    if (apps != null)
                    {
                        response.Installed = apps.ToList();
                    }
                }
                catch (Exception e)
                {
                    response.IsError = true;
                    response.Error = e.Message;
                }

                return response;
            }
        }
    }
}
