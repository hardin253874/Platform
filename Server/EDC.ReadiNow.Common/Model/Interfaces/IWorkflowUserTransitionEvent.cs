// Copyright 2011-2016 Global Software Innovation Pty Ltd
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.Interfaces
{
 
    public interface IWorkflowUserTransitionEvent
    {
        long CompletionStateId { get; set; }

        //TransitionStart CompletionState { get; }

    }
}
