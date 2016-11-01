// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Common;
using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.BackgroundTasks;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EDC.ReadiNow.Model.EventClasses.UserSurveyTaskEventTarget;

namespace EDC.SoftwarePlatform.Activities.Test.BackgroundTasks
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class BackgroundTaskTest
    {
        [Test]
        public void Serialize_RunTriggersParams()
        {
            Serialize<RunTriggersParams>();
        }

        [Test]
        public void Serialize_ResumeSurveyEvent()
        {
            Serialize<ResumeSurveyEvent>();
        }

        [Test]
        public void Serialize_PromptUserTaskCompletedEvent()
        {
            Serialize<PromptUserTaskCompletedEvent>();
        }

        [Test]
        public void Serialize_TimeoutEvent()
        {
            Serialize<TimeoutEvent>();
        }

        [Test]
        public void Serialize_UserCompletesTaskEvent()
        {
            Serialize<UserCompletesTaskEvent>();
        }

        [Test]
        public void Serialize_ResumeWorkflowParams()
        {
            Serialize<ResumeWorkflowParams>();
        }


        public void Serialize<T>() where T: IWorkflowQueuedEvent, new()
        {
            var param = new T();
            BackgroundTask.Create<T>("test", param);
        }

        [Test]
        public void AllTested()
        {
            HashSet<string> types = new HashSet<string> {
                "RunTriggersParams",
                "ResumeSurveyEvent",
                "PromptUserTaskCompletedEvent",
                "TimeoutEvent",
                "UserCompletesTaskEvent",
                "ResumeWorkflowParams"
            };

            var found = AllQueueParams().Select(t => t.Name).Where(n => !n.Contains("Dummy")).ToSet();

            var untested = found.Except(types);

            Assert.That(untested.Any(), Is.False, $"Found some IWorkflowQueuedEvent types that have not been tested for serialization: {String.Join(", ", untested)} ");

        }

        public IEnumerable<Type> AllQueueParams()
        {
            var type = typeof(IWorkflowQueuedEvent);

            return AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes())
               .Where(p => type.IsAssignableFrom(p) && p.IsClass);
        }
    }
}
