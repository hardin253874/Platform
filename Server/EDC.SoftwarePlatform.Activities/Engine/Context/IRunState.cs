// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// A workflow context is the dynamic part of a workflow that conatins it's state. It is also responsible to persisting at completion and during pausing.
    /// WorkflowRun values are loaded to the runstate and synced back at the end to reduce issues with context and readonly workflow runs.
    /// </summary>
    public interface IRunState
    {
        RequestContextData EffectiveSecurityContext { get; }
        long WorkflowRunId { get; }
        WorkflowRun WorkflowRun { get; }
        int StepsTakenInSession { get; set; }                    // The current run step. 
        int TimeTakenInSession { get; set; }                    // The current run step. 
        EntityRef ExitPointId { get; set; }
        WorkflowRunState_Enumeration? RunStatus { get; set; }
        DateTime? CompletedAt { get; set; }
        bool HasTimeout { get; set; }
        WfActivity PendingActivity { get; set; }
        WfActivity CurrentActivity { get; set; }
        string GetSafeWorkflowDescription();

        IDictionary<string, object> GetResult(IEnumerable<ActivityArgument> args = null);
        WorkflowMetadata Metadata { get;  }
        WorkflowInvoker WorkflowInvoker { get; }
        object EvaluateExpression(WfExpression expression);

        void SetArgValue(WfActivity activity, ActivityArgument targetArg, object value);
        void SetArgValue(ActivityArgument targetArg, object value);
        T GetArgValue<T>(WfActivity activity, ActivityArgument targetArg);
        T GetArgValue<T>(ActivityArgument targetArg);
        IEnumerable<Tuple<WfActivity, ActivityArgument, object>> GetArgValues();

        void FlushInternalArgs();

        void RemoveReference(EntityRef entity);    
        void PrettyPrint(StringBuilder sb);
        object ResolveParameterValue(string name, IDictionary<string, Resource> knownEntities);

        void Log(IEntity log);
        void LogError(IEntity log);
        void SetUserTask(BaseUserTask userTask);
        void SetPostRunAction(Action postRunAction);
        void RecordTrace(string message);
        void SyncToRun(WorkflowRun writeableRun);
        void SyncFromRun();
    }
}