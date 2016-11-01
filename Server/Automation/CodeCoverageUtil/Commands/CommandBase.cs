// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// Base class for commands
    /// </summary>
    internal abstract class CommandBase : ICommand
    {
        #region Properties
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        public string Name
        {
            get
            {
                return name;
            }
        }        
        protected string name;


        /// <summary>
        /// True if the command succeeded, false otherwise.
        /// </summary>
        public bool Succeeded
        {
            get
            {
                return succeeded;
            }
        }
        protected bool succeeded = true;


        /// <summary>
        /// The error message or the exception message
        /// if the command failed.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
        }
        protected string errorMessage;


        /// <summary>
        /// True to always run the command. False will only run the command
        /// if there were no errors from previous commands.
        /// </summary>
        public bool Mandatory
        {
            get
            {
                return mandatory;
            }
        }
        protected bool mandatory;


        /// <summary>
        /// This is called to report output data from the command.
        /// </summary>
        /// <param name="data">The data.</param>
        protected virtual void OnOutputDataReceived(string data)
        {
            EventHandler<OutputDataEventArgs> outputDataReceived = OutputDataReceived;
            if (outputDataReceived != null)
            {
                outputDataReceived(this, new OutputDataEventArgs(data));
            }
        }


        /// <summary>
        /// Event used to report output data from the command.
        /// </summary>
        public event EventHandler<OutputDataEventArgs> OutputDataReceived;
        #endregion                    


        #region Methods
        /// <summary>
        /// Execute the command.
        /// </summary>
        public void Execute()
        {
            try
            {
                OnExecute();
            }
            catch(Exception ex)
            {
                errorMessage = ex.ToString();
                succeeded = false;
            }
        }


        /// <summary>
        /// Execute the command.
        /// </summary>
        protected virtual void OnExecute()
        {
        }
        #endregion        
    }
}