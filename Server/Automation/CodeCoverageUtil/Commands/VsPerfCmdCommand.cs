// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command runs vsperfcmd.exe.
    /// </summary>
    internal class VsPerfCmdCommand : StartProcessCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VsPerfCmdCommand"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public VsPerfCmdCommand(string arguments, bool mandatory)
            : base(Settings.Default.VsPerfCmdExePath, arguments, true, true, mandatory)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                throw new ArgumentNullException("arguments");
            }

            this.mandatory = true;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="VsPerfCmdCommand"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="redirectOutput">if set to <c>true</c> [redirect output].</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public VsPerfCmdCommand(string arguments, bool redirectOutput, bool mandatory)
            : base(Settings.Default.VsPerfCmdExePath, arguments, true, redirectOutput, mandatory)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                throw new ArgumentNullException("arguments");
            }

            this.mandatory = true;
        }
    }
}
