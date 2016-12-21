// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Activities.Test
{
    public class TestRunState : RunStateBase
    {
        private Dictionary<Tuple<long, long, string>, object> _storage = new Dictionary<Tuple<long, long, string>, object>();

        public TestRunState(WorkflowMetadata metaData, WorkflowRun workflowRun)
            : base(metaData, workflowRun, new RequestContextData(RequestContext.GetContext()))
        {
        }

        public static TestRunState CreateDummyState(Workflow workflow)
        {
            var workflowRun = new WorkflowRun() { WorkflowBeingRun = workflow };

            return new TestRunState(new WorkflowMetadata(workflow), workflowRun);
        }



        private Tuple<long, long, string> GetKey(WfActivity activity, ActivityArgument targetArg)
        {
            return new Tuple<long, long, string>(activity != null ? activity.Id : 0, targetArg.Id, targetArg.Name);
        }

        public override T GetArgValue<T>(WfActivity activity, ActivityArgument targetArg)
        {
            return (T) _storage[GetKey(activity, targetArg)];
        }

        public override T GetArgValue<T>(ActivityArgument targetArg)
        {
            return (T)_storage[GetKey(null, targetArg)];
        }

        public override void SetArgValue(ActivityArgument targetArg, object value)
        {
            _storage[GetKey(null, targetArg)] = value;
        }

        public override void SetArgValue(WfActivity activity, ActivityArgument targetArg, object value)
        {
            _storage[GetKey(activity, targetArg)] = value;
        }


        public override IEnumerable<Tuple<WfActivity, ActivityArgument, object>> GetArgValues()
        {
            throw new NotImplementedException();
        }

        public override void FlushInternalArgs()
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetResult(IEnumerable<ActivityArgument> args = null)
        {
            var result = _storage.ToDictionary(kvp => kvp.Key.Item3, kvp=>kvp.Value);
            result.Add(WorkflowActivityHelper.ExitPointIdKeyName, ExitPointId);
            return result;
        }

        public override object ResolveParameterValue(string name,  IDictionary<string, Resource> knownEntities)
        {
            return knownEntities[name];
        }

        public override void RemoveReference(EntityRef removedEntity)
        {
        }
    }

    public class TestRunStateFactory: IRunStateFactory 
    {
        public IRunState CreateRunState(WorkflowMetadata metaData, WorkflowRun workflowRun)
        {
            return new TestRunState(metaData, workflowRun);
        }
    }
}
