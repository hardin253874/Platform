// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// An exception which is thrown by a running workflow indicating there was a configuration problem with the workflow.
    /// The message will be returned to the user.
    /// </summary>
    public class WorkflowRunException : Exception
    {

        public WorkflowRunException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowRunException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
        
    }

    /// <summary>
    /// Used to hold exception deatils until theere is sufficent context down the stack to create a 
    /// workflow run exception
    /// </summary>
    public class WorkflowRunException_Internal: Exception
    {
        public WorkflowRunException_Internal(string message, Exception innerException): base(message, innerException)
        {}
    }
    
}
    
