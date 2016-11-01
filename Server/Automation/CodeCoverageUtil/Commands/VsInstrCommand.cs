// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command runs vsinstr.exe.
    /// </summary>
    internal class VsInstrCommand : StartProcessCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VsInstrCommand"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public VsInstrCommand(string arguments, bool mandatory)
            : base(Settings.Default.VsInstrExePath, arguments, true, true, mandatory)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                throw new ArgumentNullException("arguments");
            }
        }
    }
}
