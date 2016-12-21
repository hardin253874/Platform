// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Interface for an activity that can pause and resume.
    /// </summary>
    public interface IResumableActivity
    {
        /// <summary>
        /// Start the activity running
        /// </summary>
        /// <returns>True if the activity has completed, false if it is paused. Along with a sequence number of if it is paused</returns>
        bool OnStart(IRunState context, ActivityInputs inputs);

        /// <summary>
        /// Continue a paused activity
        /// </summary>
        /// <returns>True if the activity has completed, false if it is paused.</returns>
        bool OnResume(IRunState context, IWorkflowEvent resumeEvent);
    }
}