// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test
{
    /// <summary>
    /// This context will complete all background and defered operations on dispose
    /// </summary>
    public class TestWfRunContext: WorkflowRunContext, IDisposable
    {
        Queue<Action> _queue = new Queue<Action>();

        public TestWfRunContext() : base(true)
        {
        }

        public override void QueueAction(Action action)
        {
            var contextData = new RequestContextData(RequestContext.GetContext());

            _queue.Enqueue(() =>
            {
                using (CustomContext.SetContext(contextData))
                {
                    action();
                }

            });
        }

        override public void Dispose()
        {
            base.Dispose();

            foreach (var act in _queue)
            {
                act();
            }
        }
    }
}
