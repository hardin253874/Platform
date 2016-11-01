// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using System.Globalization;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command terminates the specified process.
    /// </summary>
    internal class StopProcessCommand : CommandBase
    {
        #region Fields
        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        /// <value>
        /// The name of the process.
        /// </value>
        public string ProcessName
        {
            get
            {
                return processName;
            }
        }
        protected string processName;
        #endregion        


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StopProcessCommand"/> class.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public StopProcessCommand(string processName, bool mandatory)
        {
            if (string.IsNullOrEmpty(processName))
            {
                throw new ArgumentNullException("processName");
            }

            this.processName = processName;

            name = string.Format(CultureInfo.CurrentCulture, "Stop process {0}", processName);

            this.mandatory = mandatory;
        }
        #endregion


        #region Methods
        /// <summary>
        /// Execute this command.
        /// </summary>
        protected override void OnExecute()
        {
            StopProcess();
        }


        /// <summary>
        /// Stops the process.
        /// </summary>
        private void StopProcess()
        {
            try
            {
                Process.EnterDebugMode();                

                Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
                foreach (Process process in processes)
                {
                    OnOutputDataReceived("  * Stopping process " + process.ProcessName);

                    process.Kill();
                    process.WaitForExit(60 * 1000);                    
                }
            }
            finally
            {
                Process.LeaveDebugMode();
            }
        }
        #endregion        
    }
}
