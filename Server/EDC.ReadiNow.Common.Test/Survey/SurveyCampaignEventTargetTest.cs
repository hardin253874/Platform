// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Survey
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class SurveyCampaignEventTargetTest
    {
        [Test]
        [ExpectedException(ExpectedMessage = "Survey campaign must be part of an existing survey.")]
        public void CreateEmptyCampaign()
        {
            var campaign = Entity.Create<SurveyCampaign>();
            campaign.Save();
            // Do nothing - should work
        }

        [Test]
        [ExpectedException(ExpectedMessage = "A survey campaign can not be created from a survey without recipients.")]
        public void CreateEmptyCampaignWithNoQuestions()
        {
            var survey = Entity.Create<UserSurvey>();
            survey.Save();

            var campaign = Entity.Create<SurveyCampaign>();
            campaign.SurveyForCampaign = survey;
            //survey.SurveyRecipients.Add(Entity.Create<Person>());

            campaign.Save();
        }


        [Test]
        [ExpectedException(ExpectedMessage = "A survey campaign can not be created from a survey without recipients.")]
        public void CreateEmptyCampaignWithNoRecipients()
        {
            var survey = Entity.Create<UserSurvey>();
            var section = Entity.Create<SurveySection>();
            survey.SurveySections.Add(section);
            section.SurveyQuestions.Add(Entity.Create<ChoiceQuestion>().As<SurveyQuestion>());
            survey.Save();

            var campaign = Entity.Create<SurveyCampaign>();
            campaign.SurveyForCampaign = survey;

            campaign.Save();
        }

        [Test]
        public void CampaignWithUser()
        {
            var personA = CreatePerson("Person A");

            var survey = Entity.Create<UserSurvey>();
            var section = Entity.Create<SurveySection>();
            survey.SurveySections.Add(section);
            section.SurveyQuestions.Add(CreateQuestion());
            //survey.SurveyRecipients.Add(personA);
            survey.Save();

            var campaign = Entity.Create<SurveyCampaign>();
            campaign.SurveyForCampaign = survey;
            campaign.SurveyClosesOn = DateTime.Now.AddDays(1);
            
            campaign.Save();

            Assert.That(personA.TaskForUser, Is.Not.Empty);
            Assert.That(personA.TaskForUser, Has.Count.EqualTo(1));
            Assert.That(personA.TaskForUser.First().TypeIds, Has.Member(UserSurveyTask.UserSurveyTask_Type.Id));

            var task = personA.TaskForUser.First().As<UserSurveyTask>();

            Assert.That(task.Name, Is.EqualTo(survey.Name));
            Assert.That(task.UserTaskDueOn, Is.EqualTo(campaign.SurveyClosesOn));

            var result = task.UserSurveyTaskSurveyResponse;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SurveyStatus_Enum, Is.EqualTo(SurveyStatusEnum_Enumeration.SseNotStarted));
        }

        [Test]
        public void UpdatingCampaignClosesOnDate()
        {
            var survey = Entity.Create<UserSurvey>();
            var section = Entity.Create<SurveySection>();
            survey.SurveySections.Add(section);
            section.SurveyQuestions.Add(CreateQuestion());
            //survey.SurveyRecipients.Add(CreatePerson("PersonA"));
            survey.Save();

            var campaign = Entity.Create<SurveyCampaign>();
            campaign.SurveyForCampaign = survey;

            campaign.Save();

            campaign = campaign.AsWritable<SurveyCampaign>();

            var newClose = DateTime.UtcNow.AddDays(1);
            campaign.SurveyClosesOn = newClose;

            campaign.Save();

            //var task = survey.SurveyRecipients.First().TaskForUser.First().As<UserSurveyTask>();
            //Assert.That(task.UserTaskDueOn, Is.EqualTo(newClose));

        }


        [Test]
        public void UpdatingCampaignClosesOnDate_DeletedTask()
        {
            var survey = Entity.Create<UserSurvey>();
            var section = Entity.Create<SurveySection>();
            survey.SurveySections.Add(section);
            section.SurveyQuestions.Add(CreateQuestion());
            //survey.SurveyRecipients.Add(CreatePerson("PersonA"));
            survey.Save();

            var campaign = Entity.Create<SurveyCampaign>();
            campaign.SurveyForCampaign = survey;

            campaign.Save();

            // delete the task
            //var task = survey.SurveyRecipients.First().TaskForUser.First();
            //Entity.Delete(task);

            campaign = campaign.AsWritable<SurveyCampaign>();

            var newClose = DateTime.UtcNow.AddDays(1);
            campaign.SurveyClosesOn = newClose;

            campaign.Save();
        }

        Person CreatePerson(string name)
        {
            var personA = Entity.Create<Person>();
            personA.Name = name;
            return personA;
        }

        SurveyQuestion CreateQuestion()
        {
            return Entity.Create<ChoiceQuestion>().As<SurveyQuestion>();
        }

    }
}
