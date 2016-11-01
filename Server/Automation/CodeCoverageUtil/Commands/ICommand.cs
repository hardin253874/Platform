// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// 
    /// </summary>
    interface ICommand
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        string Name { get; }


        /// <summary>
        /// True if the command succeeded, false otherwise.
        /// </summary>
        /// <value>
        ///   <c>true</c> if succeeded; otherwise, <c>false</c>.
        /// </value>
        bool Succeeded { get; }


        /// <summary>
        /// The error message if the command failed.
        /// </summary>
        string ErrorMessage { get; }


        /// <summary>
        /// True if the command is mandatory, false otherwise.
        /// Non-mandatory commands are only run if the previous
        /// commands succeed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if mandatory; otherwise, <c>false</c>.
        /// </value>
        bool Mandatory { get; }


        /// <summary>
        /// Event to return output data.
        /// </summary>
        event EventHandler<OutputDataEventArgs> OutputDataReceived;


        /// <summary>
        /// Execute this command.
        /// </summary>
        void Execute();                        
    }
}
