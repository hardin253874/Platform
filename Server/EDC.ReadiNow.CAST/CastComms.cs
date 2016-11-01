// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.Remote;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using System;
using System.Timers;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.CAST
{
    /// <summary>
    /// Initiates and manages communication for CAST based operations.
    /// </summary>
    public sealed class CastComms : IDisposable
    {
        private static volatile CastComms _cc;
        private static readonly object Sync = new object();
        private readonly Timer _timer = new Timer();

        #region Private Properties

        private ICastService CastService { get; set; }

        private IPlatformService PlatformService { get; set; }

        private ICastActivityService CastActivityService { get; set; }

        private IRemoteListener HeartbeatListener { get; set; }

        private IRemoteListener SendHeartbeatNowListener { get; set; }

        private IRemoteListener ClientListener { get; set; }

        #endregion

        /// <summary>
        /// Private constructor for singleton instance.
        /// </summary>
        private CastComms()
        {
            CastService = Factory.Current.Resolve<ICastService>();
            CastActivityService = Factory.Current.Resolve<ICastActivityService>();
            PlatformService = Factory.Current.Resolve<IPlatformService>();
            HeartbeatListener = Factory.Current.Resolve<IRemoteListener>();
            SendHeartbeatNowListener = Factory.Current.Resolve<IRemoteListener>();

            ClientListener = Factory.Current.Resolve<IRemoteListener>();
        }

        /// <summary>
        /// Starts CAST communication channels.
        /// </summary>
        public void Start()
        {
            try
            {
                if (CastService.GetIsCastConfigured())
                {
                    // Server specific
                    if (CastService.GetIsCastServer())
                    {
                        HeartbeatListener.Receive<RemotePlatformInfo>(SpecialStrings.CastHeartbeatKey, HandleHeartbeat, false);
                    }

                    SendHeartbeatNowListener.Subscribe<string>(SpecialStrings.CastHeartbeatDemandKey, SendHeartbeatNow);
                    StartHeartbeat();

                    ClientListener.Respond<CastRequest, CastResponse>(CastService.GetClientCommunicationKey(), HandleRequest);
                }
            }
            catch (Exception err)
            {
                EventLog.Application.WriteError("Failed to start CAST communications. {0}", err);
            }
        }

        /// <summary>
        /// Stops CAST communication channels.
        /// </summary>
        public void Stop()
        {
            try
            {
                // Stop heartbeat
                if (_timer != null)
                {
                    _timer.Stop();
                }

                HeartbeatListener.Stop();
                SendHeartbeatNowListener.Stop();

                ClientListener.Stop();
            }
            catch (Exception err)
            {
                EventLog.Application.WriteError("Failed to stop CAST communications. {0}", err);
            }
        }

        /// <summary>
        /// Stop communication if the object ceases to be.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Initializes and runs a single instance of CAST communication.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                if (_cc != null)
                    return;

                lock (Sync)
                {
                    if (_cc == null)
                    {
                        _cc = new CastComms();
                        _cc.Start();
                    }
                }
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Unexpected failure initializing CAST communications. {0}", e);
            }
        }

        /// <summary>
        /// Shuts down the CAST communication instance if it was running.
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                if (_cc != null)
                {
                    _cc.Dispose();
                }
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Unexpected failure shutting down CAST communications. {0}", e);
            }
        }

        #region Private Methods

        /// <summary>
        /// Sends a heartbeat message immediately.
        /// </summary>
        /// <param name="request">The request received.</param>
        private void SendHeartbeatNow(string request)
        {
            EventLog.Application.WriteInformation("Sending heartbeat by request ({0})", request);
            CastService.SendHeartbeat();
        }

        /// <summary>
        /// Starts a timer which will send a heartbeat message every hour.
        /// </summary>
        private void StartHeartbeat()
        {
            try
            {
                var castConfiguration = ConfigurationSettings.GetCastConfigurationSection();
                if (castConfiguration == null)
                    return;

                var castSettings = castConfiguration.Cast;
                if (castSettings == null)
                    return;

                if (castSettings.Enabled != true)
                    return;

                var interval = castConfiguration.Cast.Heartbeat;
                if (interval < 0)
                    interval = 60;

                EventLog.Application.WriteInformation("Sending heartbeat every {0} minutes.", interval);
                CastService.SendHeartbeat();

                _timer.Interval = interval * 1000 * 60;
                _timer.Elapsed += (s, a) =>
                {
                    EventLog.Application.WriteInformation("Sending heartbeat.");
                    CastService.SendHeartbeat();
                };
                _timer.Start();
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Unexpected failure starting CAST heartbeat. {0}", e);
            }
        }

        /// <summary>
        /// Handles the receipt of a heartbeat message.
        /// </summary>
        /// <param name="pi">The platform information received.</param>
        private void HandleHeartbeat(RemotePlatformInfo pi)
        {
            try
            {
                if (!CastService.GetIsCastConfigured() || !CastService.GetIsCastServer())
                    return;

                using (new DeferredChannelMessageContext())
                using (CastService.GetCastContext())
                using (CastService.GetCastUser())
                {
                    PlatformService.CreateOrUpdate(pi);
                }
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Unexpected failure processing CAST heartbeat. {0}", e);
            }
        }

        /// <summary>
        /// Handles a CAST request received, passing on to the appropriate CAST activities service call.
        /// </summary>
        /// <param name="request">The request object received.</param>
        /// <returns>The response to pass back to the CAST Server.</returns>
        private CastResponse HandleRequest(CastRequest request)
        {
            try
            {
                // direct the appropriate requests to the activity service (TODO: some kind of registration would be nice)
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (CastService.GetIsCastConfigured())
                {
                    using (new DeferredChannelMessageContext())
                    {
                        // Log
                        var logRequest = request as LogRequest;
                        if (logRequest != null)
                            return CastActivityService.Log(logRequest);

                        // Tenant
                        var tenantRequest = request as TenantOperationRequest;
                        if (tenantRequest != null)
                            return CastActivityService.TenantOperation(tenantRequest);

                        // User
                        var userRequest = request as UserOperationRequest;
                        if (userRequest != null)
                            return CastActivityService.UserOperation(userRequest);

                        // Application
                        var appRequest = request as ApplicationOperationRequest;
                        if (appRequest != null)
                            return CastActivityService.ApplicationOperation(appRequest);
                    }
                }
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Unexpected failure processing CAST request. {0}", e);
            }

            return default(CastResponse);
        }

        #endregion
    }
}
