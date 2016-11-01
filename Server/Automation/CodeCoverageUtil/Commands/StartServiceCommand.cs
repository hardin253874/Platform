// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command starts the specified service
    /// </summary>
    internal class StartServiceCommand : CommandBase
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
        /// Initializes a new instance of the <see cref="StartServiceCommand"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public StartServiceCommand(string serviceName, bool mandatory)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException("serviceName");
            }

            this.serviceName = serviceName;
            name = string.Format(CultureInfo.CurrentCulture, "Start service {0}", serviceName);
            this.mandatory = mandatory;
        }
        #endregion


        #region Methods
        /// <summary>
        /// Execute this command.
        /// </summary>
        protected override void OnExecute()
        {
            StartService();
        }


        /// <summary>
        /// Starts the service.
        /// </summary>
        private void StartService()
        {
            using (ServiceController sc = new ServiceController(serviceName))
            {
                StartService(sc);
            }
        }


        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <param name="sc">The sc.</param>
        private void StartService(ServiceController sc)
        {
            OnOutputDataReceived("  * Starting service " + sc.DisplayName);

            sc.Refresh();

            if (sc.Status != ServiceControllerStatus.Running)
            {
                if (sc.Status != ServiceControllerStatus.StartPending)
                {
                    sc.Start();
                }
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
            }

            foreach (ServiceController dc in sc.DependentServices)
            {
                StartService(dc);
            }
        }
        #endregion        
    }
}
