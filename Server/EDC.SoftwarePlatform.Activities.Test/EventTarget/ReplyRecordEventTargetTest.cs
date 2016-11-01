// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.EventTarget;
using EDC.SoftwarePlatform.Activities.Test.Mocks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.EventTarget
{
    [Category("WorkflowTests")]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    public class ReplyRecordEventTargetTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void ThatTheReplyWorflowRunsAndHasCorrectInput(bool workflowHasInput)
        {
            var dummyrunner = new DummyWorkflowRunner();


            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => dummyrunner).As<IWorkflowRunner>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                var workflow = new Workflow { WorkflowRunAsOwner = true };

                if (workflowHasInput)
                {
                    var inputArg = new ResourceArgument { Name = "myInput", ConformsToType = ReplyRecord.ReplyRecord_Type };
                    workflow.InputArguments.Add(inputArg.As<ActivityArgument>());
                }

                var notification = new Notification();
                notification.NReplyMapCopy.Add(new ReplyMapEntry { Name = "Reply", RmeWorkflow = workflow });

                var send = new SendRecord();
                notification.SendRecords.Add(send);

                notification.Save();

                int runs = 0;

                var reply = new ReplyRecord { RrToSend = send, RrReply = "Reply to anything", RrReplyDate = DateTime.UtcNow };

                dummyrunner.StartWorkflowAsyncFn = (startEvent) => {
                    runs++;
                    if (workflowHasInput)
                    {
                        Assert.That(startEvent.Arguments.Keys, Has.Member("myInput"));
                        Assert.That(startEvent.Arguments["myInput"] is IEntity, Is.True);
                        Assert.That(((IEntity)startEvent.Arguments["myInput"]).Id, Is.EqualTo(reply.Id));
                    }

                    return "1";
                };

                reply.Save();

                Assert.That(runs, Is.EqualTo(1));
            }

        }

        [TestCase("f",             -1)]
        [TestCase("  first  ",      1)]
        [TestCase("first of many",  1)]
        [TestCase("second",         2)]
        [TestCase("THIRD",          3)]
        [TestCase("fourth",         4)]
        [TestCase(" fifth ",        5)]
        [TestCase(" This is the fifth entry  ", 5)]
        public void SelectReplyMapEntry(string reply, int order)
        {
            var replyMap = new List<ReplyMapEntry>
            {
                new ReplyMapEntry { Name = "first", RmeOrder = 1},
                new ReplyMapEntry { Name = "second", RmeOrder = 2},
                new ReplyMapEntry { Name = "third", RmeOrder = 3},
                new ReplyMapEntry { Name = "FOURTH", RmeOrder = 4},
                new ReplyMapEntry { Name = "   fifth   ", RmeOrder = 5},
            };

            var entry = ReplyRecordEventTarget.SelectReplyMapEntry(replyMap, reply);

            if (order == -1)
                Assert.That(entry, Is.Null);
            else
            {
                Assert.That(entry, Is.Not.Null);
                Assert.That(entry.RmeOrder, Is.EqualTo(order));
            }
        }


    }
}
