// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.PartialClasses;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using FluentAssertions;
using System.Threading;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    [Category("ExtendedTests")]
    [Category("WorkflowTests")]
    public class StartSurveyTest : TestBase
    {
        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestStartSurvey()
        {
            var survey = CreateSurvey("Survey " + DateTime.UtcNow);
            var campaign = CreatePersonCampaign(survey, "PC " + DateTime.UtcNow);
            var person = Entity.Create<Person>();
            campaign.CampaignPersonRecipients.Add(person);
            var userResource = Entity.Create<UserResource>();

            var tasks = campaign.Launch(null, userResource).ToList();
            tasks.Count.Should().Be(1);

            var task = tasks.FirstOrDefault();
            task.Should().NotBeNull();

            Assert.That(task.Name, Is.EqualTo(survey.Name));
            Assert.That(task.UserTaskDueOn, Is.Null);
            Assert.That(task.AssignedToUser, Is.Not.Null);
            Assert.That(task.AssignedToUser.Id, Is.EqualTo(person.Id));
            Assert.That(task.UserSurveyTaskSurveyResponse, Is.Not.Null);
            Assert.That(task.UserSurveyTaskSurveyResponse.SurveyTarget, Is.Not.Null);
            Assert.That(task.UserSurveyTaskSurveyResponse.SurveyTarget.Id, Is.EqualTo(userResource.Id));
        }
        
        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestTaskName()
        {
            var taskName = "Newname" + DateTime.UtcNow;
            var survey = CreateSurvey("Survey " + DateTime.UtcNow);
            var campaign = CreatePersonCampaign(survey, "PC " + DateTime.UtcNow);
            var person = Entity.Create<Person>();
            campaign.CampaignPersonRecipients.Add(person);

            var tasks = campaign.Launch(null, null, taskName);
            var task = tasks.FirstOrDefault();

            Assert.That(task.Name, Is.EqualTo(taskName));
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestTaskNameEmpty()
        {
            var survey = CreateSurvey("Survey " + DateTime.UtcNow);
            var campaign = CreatePersonCampaign(survey, "PC " + DateTime.UtcNow);
            var person = Entity.Create<Person>();
            campaign.CampaignPersonRecipients.Add(person);

            var tasks = campaign.Launch(null, null, "   ");
            var task = tasks.FirstOrDefault();

            Assert.That(task.Name, Is.EqualTo(survey.Name));
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestDueDate()
        {
            var survey = CreateSurvey("Survey " + DateTime.UtcNow);
            var campaign = CreatePersonCampaign(survey, "PC " + DateTime.UtcNow);
            var person = Entity.Create<Person>();
            campaign.CampaignPersonRecipients.Add(person);
            var due = DateTime.UtcNow.AddDays(1);

            var tasks = campaign.Launch(due);
            var task = tasks.FirstOrDefault();

            Assert.That(task.UserTaskDueOn, Is.Not.Null);

            var hours = ((DateTime)task.UserTaskDueOn - DateTime.UtcNow).TotalHours;

            Assert.That(hours, Is.LessThanOrEqualTo(24.0));
            Assert.That(hours, Is.GreaterThan(23.0));
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestInWf_nopause()
        {
            var wf = CreateWf("TestInWf" + DateTime.UtcNow);
            var survey = CreateSurvey("TestInWf Survey " + DateTime.UtcNow);
            var campaign = CreatePersonCampaign(survey, "PC " + DateTime.UtcNow);
            var person = Entity.Create<Person>();
            campaign.CampaignPersonRecipients.Add(person);

            var wfInput = new Dictionary<string, object> {{ "campaign", campaign }};

            var run = RunWorkflow(wf, wfInput);

            Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));
        }


        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestInWf_allParametersSet()
        {
            var wf = CreateWf("TestInWf" + DateTime.UtcNow);
            var survey = CreateSurvey("TestInWf Survey " + DateTime.UtcNow);
            var campaign = CreatePersonCampaign(survey, "PC " + DateTime.UtcNow);
            var person = Entity.Create<Person>();
            campaign.CampaignPersonRecipients.Add(person);
            var targetPerson = Entity.Create<Person>();

            var wfInput = new Dictionary<string, object> {
                { "campaign", campaign },
                { "pause", false },
                { "target", targetPerson },
                { "dueDays", 1m },
                { "taskName", "MyTaskName" }
            };

            var run = RunWorkflow(wf, wfInput);

            Assert.That(run.TaskWithinWorkflowRun.Count, Is.EqualTo(0));        // we are not pausing

            Assert.That(person.TaskForUser.Count, Is.EqualTo(1));

            var task = person.TaskForUser.First().Cast<UserSurveyTask>();

            Assert.That(task.AssignedToUser.Id, Is.EqualTo(person.Id), "recipient");

            var result = task.UserSurveyTaskSurveyResponse;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SurveyTarget, Is.Not.Null);
            Assert.That(result.SurveyTarget.Id, Is.EqualTo(targetPerson.Id), "targetPerson");

            var hours = ((DateTime)task.UserTaskDueOn - DateTime.UtcNow).TotalHours;

            Assert.That(hours, Is.LessThanOrEqualTo(24.0));
            Assert.That(hours, Is.GreaterThan(23.0));
            Assert.That(task.Name, Is.EqualTo("MyTaskName"), "taskName");
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithoutTransaction]
        public void TestInWf_pause()
        {
            Workflow wf = null;
            UserSurvey survey =  null;
            SurveyPersonCampaign campaign = null;
            Person person = null;

            try
            {
                wf = CreateWf("TestInWf" + DateTime.UtcNow);
                survey = CreateSurvey("TestInWf Survey " + DateTime.UtcNow);
                campaign = CreatePersonCampaign(survey, "PC " + DateTime.UtcNow);
                person = Entity.Create<Person>();
                campaign.CampaignPersonRecipients.Add(person);

                survey.Save();
                campaign.Save();
                person.Save();

                var wfInput = new Dictionary<string, object> {
                { "campaign", campaign },
                { "pause", true }
            };

                ToDelete.AddRange(new List<long> { person.Id, campaign.Id, survey.Id, wf.Id });

                var run = RunWorkflow(wf, wfInput);

                Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunPaused));

                campaign.SurveyResponses.Count.Should().Be(1);

                var result = campaign.SurveyResponses.First();

                result.UserSurveyTaskForResults.Count.Should().Be(1);

                var task = result.UserSurveyTaskForResults.First();
                var taskId = task.Id;

                var writableTask = task.AsWritable<UserSurveyTask>();

                using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
                {
                    writableTask.UserTaskIsComplete = true;
                    writableTask.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
                    writableTask.Save();
                }

                WaitForWorkflowToStop(run,10000);

                run = Entity.Get<WorkflowRun>(run.Id);

                Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));

                Assert.That(Entity.Get(taskId), Is.Null, "Task has been deleted");
            }
            finally
            {
                wf?.Delete();
                survey?.Delete(); ;
                campaign?.Delete(); ;
                person?.Delete(); ;
            }
        }

        public static UserSurvey CreateSurvey(string name)
        {
            var survey = Entity.Create<UserSurvey>();
            survey.Name = name;
            var section = Entity.Create<SurveySection>();
            section.Name = "First section";
            var question = Entity.Create<TextQuestion>();
            question.Name = "First question";

            survey.SurveySections.Add(section);

            section.SurveyQuestions.Add(question.As<SurveyQuestion>());

            return survey;
        }

        public static SurveyPersonCampaign CreatePersonCampaign(UserSurvey survey, string name)
        {
            var campaign = Entity.Create<SurveyPersonCampaign>();
            campaign.Name = name;
            campaign.SurveyForCampaign = survey;

            survey.SurveyCampaigns.Add(campaign.Cast<SurveyCampaign>());

            return campaign;
        }

        Workflow CreateWf(string name)
        {
            var wf = Entity.Create<Workflow>();
            wf.Name = name;
            wf
                .AddDefaultExitPoint()
                .AddInput<ResourceArgument>("campaign", SurveyCampaign.SurveyCampaign_Type)
                .AddInput<ResourceArgument>("recipient", Person.Person_Type)
                .AddInput<BoolArgument>("pause")
                .AddInput<DecimalArgument>("dueDays")
                .AddInput<StringArgument>("taskName")
                .AddInput<ResourceArgument>("target", Person.Person_Type)
                .AddStartSurvey("Start Survey", "[campaign]", "[pause]", "[target]", "[dueDays]", "[taskName]");

            wf.Save();
            return wf;
        }
        //TestCustomName
        //TestTimeout
        //TestTimeoutWithDeletedTask
    }
}