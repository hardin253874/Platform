// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Survey
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class UserSurveyTaskEventTargetTest
    {
        [Test]
        //[RunWithTransaction]
        public void CompleteCampaign()
        {
            var campaign = CreateSurveyPersonCampaign();

            Assert.That(campaign.SurveyResponses, Is.Not.Empty);

            Assert.That(campaign.SurveyResponses.First().UserSurveyTaskForResults, Is.Not.Null);

            var task = campaign.SurveyResponses.First().UserSurveyTaskForResults.First().AsWritable<UserSurveyTask>();

            task.UserTaskIsComplete = true;
            task.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
            var taskId = task.Id;
            task.Save();

            Assert.That(Entity.Get(taskId), Is.Null, "Task has been cleaned up");

            var results = Entity.Get<SurveyResponse>(campaign.SurveyResponses.First().Id);
            Assert.That(results.SurveyCompletedOn, Is.Not.Null, "The results have been completed");
            Assert.That(results.SurveyAnswers.First().SurveyAnswerCalculatedValue, Is.EqualTo(0.0M));
            Assert.That(results.SurveyResponseOutcome, Is.Not.Null);
            Assert.That(results.SurveyResponseOutcome.Name, Is.EqualTo("Default"));

            Assert.That(campaign.CampaignIsComplete, Is.True);

        }


        [Test]
        public void CampaignWithCampaignCompleteTrigger()
        {
            var workflow = Entity.Create<Workflow>();
            SurveyCampaign campaign = CreateSurveyPersonCampaign().AsWritable<SurveyCampaign>();

            var survey = campaign.SurveyForCampaign.AsWritable<UserSurvey>();

            survey.SurveyTriggerOnCampaignComplete = workflow;

            survey.Save();


            var dummyRunner = new DummyWorkflowRunner();


            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => dummyRunner).As<IWorkflowRunner>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                int runCount = 0;
                dummyRunner.StartWorkflowAsyncFn = (dummyWf, args) =>
                {
                    Assert.That(dummyWf.Id, Is.EqualTo(workflow.Id));
                    Assert.That(args.Keys, Contains.Item("Input"));

                    var inputEntity = args["Input"] as IEntity;
                    Assert.That(inputEntity.Id, Is.EqualTo(campaign.Id));

                    runCount++;
                    return "111";
                };

                var task = campaign.SurveyResponses.First().UserSurveyTaskForResults.First().AsWritable<UserSurveyTask>();

                task.UserTaskIsComplete = true;
                task.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
                var taskId = task.Id;
                task.Save();

                //Thread.Sleep(1000);

                Assert.That(runCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void CampaignWithSurveyCompleteTrigger()
        {
            var workflow = Entity.Create<Workflow>();
            SurveyCampaign campaign = CreateSurveyPersonCampaign().AsWritable<SurveyCampaign>();

            var survey = campaign.SurveyForCampaign.AsWritable<UserSurvey>();

            survey.SurveyTriggerOnSurveyComplete = workflow;

            survey.Save();


            var dummyRunner = new DummyWorkflowRunner();


            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => dummyRunner).As<IWorkflowRunner>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                int runCount = 0;
                dummyRunner.StartWorkflowAsyncFn = (dummyWf, args) =>
                {
                    Assert.That(dummyWf.Id, Is.EqualTo(workflow.Id));
                    Assert.That(args.Keys, Contains.Item("Input"));

                    var inputEntity = args["Input"] as IEntity;
                    Assert.That(inputEntity.Id, Is.EqualTo(campaign.SurveyResponses.First().Id));

                    runCount++;
                    return "111";
                };

                var task = campaign.SurveyResponses.First().UserSurveyTaskForResults.First().AsWritable<UserSurveyTask>();

                task.UserTaskIsComplete = true;
                task.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
                var taskId = task.Id;
                task.Save();

                //Thread.Sleep(1000);

                Assert.That(runCount, Is.EqualTo(1));
            }
        }


        private static SurveyPersonCampaign CreateSurveyPersonCampaign()
        {
            var person = Entity.Create<Person>();
            var question = Entity.Create<TextQuestion>().As<SurveyQuestion>();
            var outcome = Entity.Create<SurveyOutcome>();
            outcome.Name = "Default";

            var survey = Entity.Create<UserSurvey>();
            var section = Entity.Create<SurveySection>();
            survey.SurveySections.Add(section);
            section.SurveyQuestions.Add(question);
            survey.SurveyOutcomes.Add(outcome);
            survey.Save();

            var campaign = Entity.Create<SurveyPersonCampaign>();
            campaign.SurveyForCampaign = survey;
            campaign.CampaignPersonRecipients.Add(person);
            campaign.Save();
            return campaign;
        }
    }
}
