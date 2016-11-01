// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;

namespace CodeCoverageUtil.Commands
{    
    /// <summary>
    /// The command starts the specified process.
    /// </summary>
    internal class StartProcessCommand : CommandBase
    {
        #region Fields
        /// <summary>
        /// Gets the name of the process file.
        /// </summary>
        /// <value>
        /// The name of the process file.
        /// </value>
        public string ProcessFileName
        {
            get
            {
                return processFileName;
            }
        }
        protected string processFileName;


        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public string Arguments
        {
            get
            {
                return arguments;
            }
        }
        protected string arguments;


        /// <summary>
        /// True to wait for the process to exit, false otherwise.
        /// </summary>
        /// <value>
        ///   <c>true</c> to wait for the process to exit; otherwise, <c>false</c>.
        /// </value>
        public bool WaitForExit
        {
            get
            {
                return waitForExit;
            }
        }
        protected bool waitForExit;


        /// <summary>
        /// True to redirect the process standard output, false otherwise.
        /// </summary>
        /// <value>
        ///   <c>true</c> to redirect output; otherwise, <c>false</c>.
        /// </value>
        public bool RedirectOutput
        {
            get
            {
                return redirectOutput;
            }
        }
        protected bool redirectOutput;


        /// <summary>
        /// The success exit code for the process.
        /// </summary>
        public int? SuccessExitCode
        {
            get
            {
                return successExitCode;
            }
        }
        protected int? successExitCode;
        #endregion        


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StartProcessCommand"/> class.
        /// </summary>
        /// <param name="processFileName">Name of the process file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="waitForExit">if set to <c>true</c> wait for the process to exit.</param>
        /// <param name="redirectOutput">if set to <c>true</c> redirect output.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public StartProcessCommand(string processFileName, string arguments, bool waitForExit, bool redirectOutput, bool mandatory):
            this(processFileName, arguments, waitForExit, redirectOutput, null, mandatory)
        {            
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StartProcessCommand"/> class.
        /// </summary>
        /// <param name="processFileName">Name of the process file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="waitForExit">if set to <c>true</c> wait for the process to exit.</param>
        /// <param name="redirectOutput">if set to <c>true</c> redirect output.</param>
        /// <param name="successExitCode">The success exit code.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public StartProcessCommand(string processFileName, string arguments, bool waitForExit, bool redirectOutput, int successExitCode, bool mandatory):
            this(processFileName, arguments, waitForExit, redirectOutput, (int?)successExitCode, mandatory)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StartProcessCommand"/> class.
        /// </summary>
        /// <param name="processFileName">Name of the process file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="waitForExit">if set to <c>true</c> wait for the process to exit.</param>
        /// <param name="redirectOutput">if set to <c>true</c> redirect output.</param>
        /// <param name="successExitCode">The success exit code.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        private StartProcessCommand(string processFileName, string arguments, bool waitForExit, bool redirectOutput, int? successExitCode, bool mandatory)
        {
            if (string.IsNullOrEmpty(processFileName))
            {
                throw new ArgumentNullException("processFileName");
            }

            this.processFileName = processFileName;
            this.arguments = arguments;
            this.waitForExit = waitForExit;
            this.redirectOutput = redirectOutput;
            this.successExitCode = successExitCode;
            this.mandatory = mandatory;

            name = string.Format(CultureInfo.CurrentCulture, "Start process {0}", processFileName);            
        }
        #endregion


        #region Methods
        /// <summary>
        /// Execute this command.
        /// </summary>
        protected override void OnExecute()
        {
            StartProcess();
        }


        /// <summary>
        /// Starts the process.
        /// </summary>
        private void StartProcess()
        {
            Process process = new Process();            
            process.StartInfo.FileName = processFileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (redirectOutput)
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            }
            OnOutputDataReceived(string.Format("  * Starting process {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments));
            OnOutputDataReceived("");

            process.Start();

            if (redirectOutput)
            {
                process.BeginOutputReadLine();
            }

            if (waitForExit)
            {
                process.WaitForExit();
                if (successExitCode.HasValue &&
                    process.ExitCode != successExitCode.Value)                
                {
                    succeeded = false;
                }                
            }            
        }


        /// <summary>
        /// Handles the OutputDataReceived event of the process control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Diagnostics.DataReceivedEventArgs"/> instance containing the event data.</param>
        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {            
            OnOutputDataReceived(e.Data);
        }        
        #endregion        
    }
}