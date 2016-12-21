// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using EDC.ReadiNow.Model;
using System.Linq;
using EDC.ReadiNow.Model.PartialClasses;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    public class ReviewSurveyTest
    {
        [Test]
        
        public void Test()
        {
            SurveyResponse result = CreateResult();

            var status = Entity.Get<SurveyStatusEnum>(new EntityRef("sseInProgress"));
            var reviewer = Entity.Create<Person>();

            var task = ReviewSurveyImplementation.ReviewSurvey(result, reviewer, status, new decimal(5), true, "taskName");
            task.Save();

            Assert.That(result.SurveyStatus.Id, Is.EqualTo(status.Id));
            Assert.That(reviewer.TaskForUser.Count, Is.EqualTo(1));

            Assert.That(task.AssignedToUser, Is.Not.Null);
            Assert.That(task.AssignedToUser.Id, Is.EqualTo(reviewer.Id));
            Assert.That(task.UserSurveyTaskAllowComments, Is.True);

            Assert.That(task.UserTaskDueOn, Is.Not.Null);

            var dueIn = ((DateTime)task.UserTaskDueOn) - DateTime.UtcNow;

            Assert.That(dueIn.TotalDays, Is.GreaterThan(4));
            Assert.That(dueIn.TotalDays, Is.LessThanOrEqualTo(5));
        }

        [Test]
        public void NoDueDate()
        {
            SurveyResponse result = CreateResult();

            var reviewer = Entity.Create<Person>();

            var task = ReviewSurveyImplementation.ReviewSurvey(result, reviewer, null, 0, true, "taskName");
            task.Save();

            Assert.That(task.UserTaskDueOn, Is.Null);
        }

        [Test]
        public void NoComments()
        {
            SurveyResponse result = CreateResult();

            var reviewer = Entity.Create<Person>();

            var task = ReviewSurveyImplementation.ReviewSurvey(result, reviewer, null, 0, false, "taskName");
            task.Save();

            Assert.That(task.UserSurveyTaskAllowComments, Is.False);
        }

        [TestCase(null, "SurveyName")]
        [TestCase("", "SurveyName")]
        [TestCase("    ", "SurveyName")]
        [TestCase("A name", "A name")]
        public void TaskName(string taskName, string expectedTaskName)
        {
            SurveyResponse result = CreateResult();
            var survey = result.CampaignForResults.SurveyForCampaign.AsWritable<UserSurvey>();
            survey.Name = "SurveyName";
            survey.Save();

            var reviewer = Entity.Create<Person>();

            var task = ReviewSurveyImplementation.ReviewSurvey(result, reviewer, null, 0, false, taskName);
            task.Save();

            Assert.That(task.Name, Is.EqualTo(expectedTaskName));
        }


        private static SurveyResponse CreateResult()
        {
            var person = Entity.Create<Person>();

            var survey = StartSurveyTest.CreateSurvey("ReviewSurveyTest" + DateTime.UtcNow);
            survey.Save();

            var campaign = Entity.Create<SurveyPersonCampaign>();
            campaign.SurveyForCampaign = survey;
            campaign.CampaignPersonRecipients.Add(person);
            campaign.Save();

            campaign.Launch();

            var result = campaign.SurveyResponses.First();
            return result;
        }
    }
}
