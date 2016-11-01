// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.XPath;
using System.Xml;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command runs MsTest.exe.
    /// </summary>
    internal class ReportGeneratorCommand : StartProcessCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportGeneratorCommand"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public ReportGeneratorCommand(string arguments, bool mandatory)
            : base(Settings.Default.ReportGeneratorExePath, arguments, true, true, mandatory)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                throw new ArgumentNullException("arguments");
            }            
        }
    }
}