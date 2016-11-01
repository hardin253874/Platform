// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command runs vsperfmon.exe.
    /// </summary>
    internal class VsPerfMonCommand : StartProcessCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VsPerfMonCommand"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public VsPerfMonCommand(string arguments, bool mandatory)
            : base(Settings.Default.VsPerfMonExePath, arguments, false, true, 1, mandatory)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                throw new ArgumentNullException("arguments");
            }
        }


        /// <summary>
        /// Execute this command.
        /// </summary>
        protected override void OnExecute()
        {
            try
            {
                VsPerfCmdCommand vsPerfCmdCommand = new VsPerfCmdCommand("/shutdown", false, true);
                vsPerfCmdCommand.Execute();
            }
            catch
            {
            }

            base.OnExecute();

            // Give perfmon a chance to start
            Thread.Sleep(1000);
        }
    }
}
