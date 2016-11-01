// Copyright 2011-2016 Global Software Innovation Pty Ltd
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using EDC.ReadiNow.Model;
//using EDC.ReadiNow.Test;
//using EDC.SoftwarePlatform.Activities.EmailListener;
//using NUnit.Framework;

//namespace EDC.SoftwarePlatform.Activities.Test.EmailListener
//{
//    [TestFixture]
//    public class CompleteUserTaskEmailActionTest: TestBase
//    {

//        [TestAttribute]
//        [RunAsDefaultTenant]
//        public void Test_ActionMessages()
//        {
//            var approvalWf = CreateApprovalTestWf();

//            // start the workflow
//            var invoker = new WorkflowInvoker();


//            WorkflowInvoker.DefaultInvokerForThread = invoker;
//            WorkflowRun run = WorkflowRunner.Instance.RunWorkflow(approvalWf, new Dictionary<string, object>());

//            var approvalActivity = run.PendingActivity.Cast<ApprovalActivity>();

//            var subject = "Re: This is a test approval message " + run.TaskWithinWorkflowRun.First().As<UserTask>().SequenceId;

//            var emailMessage = new ReceivedEmailMessage() { EmSubject = subject, EmFrom = "bob@gmail.com", EmBody = "\nApproved       \n-----------\nBlah blah blah" };

//            var emailAction = new CompleteUserTaskEmailAction();

//            Action postSaveAction;
//            emailAction.BeforeSave(emailMessage, out postSaveAction);
//            postSaveAction();

//            run = WorkflowRunner.Instance.WaitForWorkflowToStop(run, WorkflowRunState_Enumeration.WorkflowRunCompleted, 60 * 1000, 500);

//            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum);

//            var exitPoint = run.GetExitPoint();

//            Assert.AreEqual("Approve", Entity.Get<Resource>(exitPoint).Name, "Ensure response was followed");

//            Assert.AreEqual("Approve", run.TaskWithinWorkflowRun.First().As<UserTask>().UserResponse.Name, "The use task has been recorded correctly.");
//        }
//    }
//}
