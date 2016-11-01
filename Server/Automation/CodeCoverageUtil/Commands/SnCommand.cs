// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command runs sn.exe.
    /// </summary>
    internal class SnCommand : StartProcessCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnCommand"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public SnCommand(string arguments, bool mandatory)
            : base(Settings.Default.SnExePath, arguments, true, true, 0, mandatory)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                throw new ArgumentNullException("arguments");
            }           
        }
    }
}
