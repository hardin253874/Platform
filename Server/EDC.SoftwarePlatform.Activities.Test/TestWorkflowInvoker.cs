// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test
{
    public class TestWorkflowInvoker : WorkflowInvoker
    {
        Action<Boolean> _onCompletion;

        public TestWorkflowInvoker(Action<Boolean> onCompletion)
        {
            Assert.IsNotNull(onCompletion);
            _onCompletion = onCompletion;
        }

        public override bool RunTillCompletion(IRunState runState)
        {
            var result = base.RunTillCompletion(runState);
            _onCompletion(result);

            return result;
        }
    }
}
