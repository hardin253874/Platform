// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using ProtoBuf;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities.Engine.Events
{

    [ProtoContract]
    public class ChildWorkflowCompletedEvent : IWorkflowEvent
    {
        [ProtoIgnore]
        public WorkflowRun CompletedRun { get; private set; }


        public ChildWorkflowCompletedEvent(WorkflowRun childRun)
            : base()
        {
            CompletedRun = childRun;
        }

        [ProtoMember(1)]
        public long ProtoState
        {
            get
            {
                return CompletedRun?.Id ?? 0;
            }

            set
            {

                if (value != 0)
                    CompletedRun = Entity.Get<WorkflowRun>(value);
            }
        }
    }
}
