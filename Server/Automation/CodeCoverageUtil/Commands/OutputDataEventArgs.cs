// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// Event argument for command output data.
    /// </summary>
    internal class OutputDataEventArgs : EventArgs
    {
        /// <summary>
        /// The command output data.
        /// </summary>
        public string Data { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDataEventArgs"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public OutputDataEventArgs(string data)
        {
            Data = data;
        }        
    }
}
