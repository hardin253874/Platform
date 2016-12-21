// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Implementation of the log activity available for use in Workflows.  Allows a workflow to add a message to the server logs.
    /// </summary>
    public class LogActivityImplementation : ActivityImplementationBase, IRunNowActivity
    {
        /// <summary>
        /// Runs when the activity is run by the workflow.
        /// </summary>
        /// <param name="context">The run state.</param>
        /// <param name="inputs">The inputs.</param>
        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var text = GetArgumentValue<string>(inputs, "inLogActivityMessage");
            var refencedObject = GetArgumentEntity<UserResource>(inputs, "inLogActivityObject");

            IEntity logEntry;

            if (refencedObject != null)
            {
                logEntry = new LogActivityResourceLogEntry
                {
                    ObjectReferencedInLog = refencedObject,
                    ReferencedObjectName = refencedObject.Name

                };
            }
            else
            {
                logEntry = new LogActivityLogEntry();
            }

            logEntry.SetField(WorkflowRunLogEntry.Name_Field, ActivityInstance.Name);
            logEntry.SetField(WorkflowRunLogEntry.Description_Field, text);

            context.Log(logEntry);
           
            // Provide some context information to the log message
            EventLog.Application.WriteInformation(string.Format("Log | {1} | Message: {2}", DateTime.UtcNow.Ticks, context.GetSafeWorkflowDescription(), text));
        }
    }
}
