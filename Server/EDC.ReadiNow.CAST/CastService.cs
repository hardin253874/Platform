// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Autofac;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.Remote;

namespace EDC.ReadiNow.CAST
{
    /// <summary>
    /// Provides CAST specific operations.
    /// </summary>
    public class CastService : ICastService
    {
        private static string _dbid;

        #region Private Properties

        private IRemoteSender HeartbeatSender { get; set; }

        private IRemoteSender HeartbeatRequester { get; set; }

        private IEntityRepository EntityRepository { get; set; }

        private IPlatformService PlatformService { get; set; }

        private ITenantService TenantService { get; set; }

        private IApplicationService ApplicationService { get; set; }

        #endregion

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public CastService()
        {
            HeartbeatSender = Factory.Current.Resolve<IRemoteSender>();
            HeartbeatRequester = Factory.Current.Resolve<IRemoteSender>();
            EntityRepository = Factory.Current.Resolve<IEntityRepository>();
            PlatformService = Factory.Current.Resolve<IPlatformService>();
            TenantService = Factory.Current.Resolve<ITenantService>();
            ApplicationService = Factory.Current.Resolve<IApplicationService>();
        }

        /// <summary>
        /// Checks if CAST has been enabled and remote communication has been configured for this platform.
        /// </summary>
        /// <returns>True if CAST communication should take place.</returns>
        public bool GetIsCastConfigured()
        {
            using (Profiler.Measure("CastService.GetIsCastConfigured"))
            {
                var isCastEnabled = false;
                var isRabbitMqConfigured = false;

                // is CAST enabled?
                var castConfiguration = ConfigurationSettings.GetCastConfigurationSection();
                if (castConfiguration != null)
                {
                    var castSettings = castConfiguration.Cast;
                    if (castSettings != null)
                    {
                        isCastEnabled = castSettings.Enabled;
                    }
                }

                // is RabbitMQ configured for communication?
                var mqConfiguration = ConfigurationSettings.GetRabbitMqConfigurationSection();
                if (mqConfiguration != null)
                {
                    var mqSettings = mqConfiguration.RabbitMq;
                    if (mqSettings != null)
                    {
                        isRabbitMqConfigured = !string.IsNullOrEmpty(mqSettings.HostName);
                    }
                }

                return isCastEnabled && isRabbitMqConfigured;
            }
        }

        /// <summary>
        /// Checks if this platform has the CAST application installed in any tenancy and thus should operate as a CAST Server.
        /// </summary>
        /// <returns>True if this rig is a CAST Server.</returns>
        public bool GetIsCastServer()
        {
            using (Profiler.Measure("CastService.GetIsCastServer"))
            {
                return GetCastTenantId() >= 0;
            }
        }

        /// <summary>
        /// Returns the key that uniquely defines this instance of the ReadiNow Platform for communication purposes.
        /// </summary>
        /// <returns>A key that can identify this platform for CAST/Client communication.</returns>
        public string GetClientCommunicationKey()
        {
            using (Profiler.Measure("CastService.GetClientCommunicationKey"))
            {
                return SpecialStrings.CastClientKeyPrefix + GetDatabaseId();
            }
        }

        /// <summary>
        /// Constructs a context for specific internal CAST based operations.
        /// </summary>
        /// <returns>The disposable CAST context object.</returns>
        public ContextBlock GetCastContext()
        {
            using (Profiler.Measure("CastService.GetCastContext"))
            {
                return new TenantAdministratorContext(GetCastTenantId());
            }
        }

        /// <summary>
        /// Sets the user to be the well-known CAST user account for internal CAST based operations.
        /// </summary>
        /// <returns>The disposable user context object.</returns>
        public SetUser GetCastUser()
        {
            using (Profiler.Measure("CastService.GetCastUser"))
            {
                var castUser = EntityRepository.Get<UserAccount>(new EntityRef(SpecialStrings.CastUserAlias));
                if (castUser == null)
                {
                    throw new Exception("CAST user not found.");
                }
                return new SetUser(castUser);
            }
        }

        /// <summary>
        /// Sends a heartbeat payload with detailed platform information for processing by a listening CAST Server.
        /// </summary>
        public void SendHeartbeat()
        {
            using (Profiler.Measure("CastService.SendHeartbeat"))
            {
                if (!GetIsCastConfigured())
                    return;

                using (new SecurityBypassContext())
                {
                    var dbSettings = ConfigurationSettings.GetDatabaseConfigurationSection().ConnectionSettings;
                    var dbInfo = DatabaseConfigurationHelper.Convert(dbSettings);
                    var tenants = new TenantList(TenantService.GetTenants());
                    var apps = ApplicationService.GetApps().ToList();
                    var hostname = Dns.GetHostName();
                    var domainname = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                    var pi = new RemotePlatformInfo
                    {
                        Id = GetDatabaseId(),
                        Tenants = tenants,
                        Apps = apps,
                        FrontEndHost = hostname,
                        FrontEndDomain = domainname,
                        DatabaseServer = dbInfo.Server,
                        Database = dbInfo.Database
                    };

                    HeartbeatSender.Send(SpecialStrings.CastHeartbeatKey, pi, false);
                }
            }
        }

        /// <summary>
        /// Publishes a request, as a CAST Server, for any listening platforms to send their heartbeat info asap.
        /// </summary>
        public void RequestHeartbeat()
        {
            using (Profiler.Measure("CastService.RequestHeartbeat"))
            {
                if (!GetIsCastConfigured() || !GetIsCastServer())
                    return;

                HeartbeatRequester.Publish(SpecialStrings.CastHeartbeatDemandKey, GetDatabaseId());
            }
        }

        #region Private Methods

        /// <summary>
        /// Returns the database id.
        /// </summary>
        /// <returns>The database id as a string.</returns>
        private string GetDatabaseId()
        {
            using (Profiler.Measure("CastService.GetDatabaseId"))
            {
                // this is not likely to ever change.
                if (string.IsNullOrEmpty(_dbid))
                {
                    _dbid = PlatformService.GetDatabaseId();
                }

                return _dbid;
            }
        }

        /// <summary>
        /// Retrieves the id of the tenant that the CAST application is installed on on this platform.
        /// </summary>
        /// <returns>Returns the id of the CAST tenant or -1 if it wasn't found.</returns>
        private long GetCastTenantId()
        {
            using (Profiler.Measure("CastService.GetCastTenantId"))
            {
                using (var ctx = DatabaseContext.GetContext())
                {
                    const string query = @"SELECT TOP 1 [TenantId] FROM [Data_Alias] WITH (NOLOCK) WHERE [Data] = 'castApp' AND [Namespace] = 'cast' AND [TenantId] > 0";

                    using (var cmd = ctx.CreateCommand(query))
                    {
                        var result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            long t;
                            if (long.TryParse(result.ToString(), out t))
                            {
                                return t;
                            }
                        }
                    }
                }

                return -1L;
            }
        }

        #endregion
    }
}
