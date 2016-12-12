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
        public void TestInWf_LaunchPersonCampaign()
        {
            var wf = CreatePersonWf("TestInWf" + DateTime.UtcNow);
            var survey = CreateSurvey("TestInWf Survey " + DateTime.UtcNow);
            var person = Entity.Create<Person>();
            var recipients = new EntityCollection<Person>();
            recipients.Add(person);

            var targetObject = Entity.Create(new EntityRef("test:drink")).As<UserResource>();

            var wfInput = new Dictionary<string, object> {
                { "survey", survey },
                { "recipients", recipients },
                { "target", targetObject },
                { "taskName", "MyTaskName" },
                { "dueDays", 1m },
                { "pause", false }
            };

            WorkflowRun run = RunWorkflow(wf, wfInput);

            Assert.That(run.TaskWithinWorkflowRun.Count, Is.EqualTo(0));        // we are not pausing

            person = Entity.Get<Person>(person.Id);

            Assert.That(person.TaskForUser.Count, Is.EqualTo(1));

            var task = person.TaskForUser.First().Cast<UserSurveyTask>();

            Assert.That(task.AssignedToUser.Id, Is.EqualTo(person.Id), "recipient");

            var result = task.UserSurveyTaskSurveyResponse;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SurveyTarget, Is.Not.Null);
            Assert.That(result.SurveyTarget.Id, Is.EqualTo(targetObject.Id), "targetObject");

            var hours = ((DateTime)task.UserTaskDueOn - DateTime.UtcNow).TotalHours;

            Assert.That(hours, Is.LessThanOrEqualTo(24.0));
            Assert.That(hours, Is.GreaterThan(23.0));
            Assert.That(task.Name, Is.EqualTo("MyTaskName"), "taskName");
        }
        
        [Test]
        [RunAsDefaultTenant]
        [RunWithoutTransaction]
        public void TestInWf_LaunchTargetCampaign_pause()
        {
            Workflow wf = null;
            UserSurvey survey =  null;
            Person person = null;
            Relationship surveyTaker = null;
            Definition targetObject = null;
            IEntity targetInstance = null;

            try
            {
                wf = CreateTargetWf("TestInWf" + DateTime.UtcNow);
                survey = CreateSurvey("TestInWf Survey " + DateTime.UtcNow);
                person = Entity.Create<Person>();

                surveyTaker = new Relationship
                {
                    Name = "Survey Tooker " + DateTime.UtcNow,
                    Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne,
                    ToType = Person.Person_Type
                };

                targetObject = Entity.Create<Definition>();
                targetObject.Name = "TestInWf Target " + DateTime.UtcNow;
                targetObject.Inherits.Add(UserResource.UserResource_Type);
                targetObject.Relationships.Add(surveyTaker);
                surveyTaker.FromType = targetObject.As<EntityType>();
                targetObject.Save();

                targetInstance = Entity.Create(targetObject.Id);
                targetInstance.SetRelationships(surveyTaker.Id, new EntityRelationship<Person>(person).ToEntityRelationshipCollection(), Direction.Forward);
                targetInstance.Save();

                var targets = new EntityCollection<UserResource>();
                targets.Add(targetInstance.As<UserResource>());

                var wfInput = new Dictionary<string, object>
                {
                    {"survey", survey},
                    {"targets", targets},
                    {"surveyTaker", surveyTaker},
                    {"targetObject", targetObject},
                    {"taskName", "MyTaskName"},
                    {"dueDays", 1m},
                    {"pause", true}
                };

                ToDelete.AddRange(new List<long> { targetInstance.Id, targetObject.Id, surveyTaker.Id, person.Id, survey.Id, wf.Id });

                var run = RunWorkflow(wf, wfInput);

                Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunPaused));

                survey = Entity.Get<UserSurvey>(survey.Id).AsWritable<UserSurvey>();

                survey.SurveyCampaigns.Count.Should().Be(1);
                survey.SurveyCampaigns.First().SurveyResponses.Count.Should().Be(1);

                var result = survey.SurveyCampaigns.First().SurveyResponses.First();

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
                targetInstance?.Delete();
                targetObject?.Delete();
                surveyTaker?.Delete();
                person?.Delete();
                survey?.Delete();
                wf?.Delete();
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
            wf.AddDefaultExitPoint()
                .AddInput<ResourceArgument>("campaign", SurveyCampaign.SurveyCampaign_Type)
                .AddStartSurvey("Start Survey", "[campaign]");

            wf.Save();
            return wf;
        }

        Workflow CreatePersonWf(string name)
        {
            var wf = Entity.Create<Workflow>();
            wf.Name = name;
            wf.AddDefaultExitPoint()
                .AddInput<ResourceArgument>("survey", UserSurvey.UserSurvey_Type)
                .AddInput<ResourceListArgument>("recipients", Person.Person_Type)
                .AddInput<ResourceArgument>("target", UserResource.UserResource_Type)
                .AddInput<StringArgument>("taskName")
                .AddInput<DecimalArgument>("dueDays")
                .AddInput<BoolArgument>("pause")
                .AddLaunchPersonCampaign("Launch Person Campaign",
                "[survey]", "[recipients]", "[target]", "[taskName]", "[dueDays]", "[pause]");

            wf.Save();
            return wf;
        }

        Workflow CreateTargetWf(string name)
        {
            var wf = Entity.Create<Workflow>();
            wf.Name = name;
            wf.AddDefaultExitPoint()
                .AddInput<ResourceArgument>("survey", UserSurvey.UserSurvey_Type)
                .AddInput<ResourceListArgument>("targets", UserResource.UserResource_Type)
                .AddInput<ResourceArgument>("surveyTaker", Relationship.Relationship_Type)
                .AddInput<ResourceArgument>("targetObject", Definition.Definition_Type)
                .AddInput<StringArgument>("taskName")
                .AddInput<DecimalArgument>("dueDays")
                .AddInput<BoolArgument>("pause")
                .AddLaunchTargetCampaign("Launch Target Campaign",
                "[survey]", "[targets]", "[surveyTaker]", "[targetObject]", "[taskName]", "[dueDays]", "[pause]");

            wf.Save();
            return wf;
        }
    }
}