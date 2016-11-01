// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command stops the specified service
    /// </summary>
    internal class StopServiceCommand : CommandBase
    {
        #region Fields
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public string ServiceName
        {
            get
            {
                return serviceName;
            }
        }
        protected string serviceName;
        #endregion


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StopServiceCommand"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public StopServiceCommand(string serviceName, bool mandatory)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException("serviceName");
            }

            this.serviceName = serviceName;

            name = string.Format(CultureInfo.CurrentCulture, "Stop service {0}", serviceName);

            this.mandatory = mandatory;
        }
        #endregion


        #region Methods
        /// <summary>
        /// Execute this command.
        /// </summary>
        protected override void OnExecute()
        {
            StopService();
        }


        /// <summary>
        /// Stops the service.
        /// </summary>
        private void StopService()
        {
            using (ServiceController sc = new ServiceController(serviceName))
            {
                StopService(sc);
            }
        }


        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <param name="sc">The sc.</param>
        private void StopService(ServiceController sc)
        {
            if (sc == null)
            {
                return;
            }

            try
            {
                OnOutputDataReceived("  * Stopping service " + sc.ServiceName);

                if (sc.DependentServices != null)
                {
                    foreach (ServiceController dc in sc.DependentServices)
                    {
                        StopService(dc);
                    }
                }

                sc.Refresh();

                if (sc.Status != ServiceControllerStatus.Stopped)
                {
                    int processId = -1;

                    try
                    {
                        Service service = new Service(sc.ServiceName);
                        processId = (int)service.ProcessId;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("{0}.{1} failed. Exception {2}. Process for service may not be killed.",
                                MethodInfo.GetCurrentMethod().ReflectedType.FullName,
                                MethodInfo.GetCurrentMethod().Name, ex.ToString());
                    }

                    try
                    {
                        if (sc.Status != ServiceControllerStatus.StopPending)
                        {
                            sc.Stop();                            
                        }
                        try
                        {
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        }
                        catch (System.ServiceProcess.TimeoutException)
                        {
                            KillProcess(processId);
                        }
                        finally
                        {
                            if (processId > 0)
                            {
                                Process p = Process.GetProcessById(processId);
                                if (!p.WaitForExit(30000))
                                {
                                    KillProcess(processId);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        KillProcess(processId);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0}.{1} failed. Exception {2}.",
                        MethodInfo.GetCurrentMethod().ReflectedType.FullName,
                        MethodInfo.GetCurrentMethod().Name, ex.ToString());
            }
        }


        /// <summary>
        /// Kills the process.
        /// </summary>
        /// <param name="processId">The process id.</param>
        private static void KillProcess(int processId)
        {
            Process p = null;
            try
            {
                if (processId > 0)
                {
                    p = Process.GetProcessById(processId);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0}.{1} failed. Exception {2}",
                    MethodInfo.GetCurrentMethod().ReflectedType.FullName,
                    MethodInfo.GetCurrentMethod().Name, ex.ToString());
            }
            if (p != null)
            {
                try
                {
                    Process.EnterDebugMode();
                    p.Kill();
                }                    
                finally
                {
                    Process.LeaveDebugMode();
                }
            }
        }
        #endregion        
    }
}
