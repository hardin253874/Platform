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
    public class SurveyResponseTest
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true, 0, 0, "Default")]
        [TestCase(true, 1, 0, "Default")]
        [TestCase(true, 2, 3, "Five")]
        [TestCase(true, 6, 0, "Five")]
        [TestCase(true, 5, 5, "Ten")]
        [TestCase(true, 0, 11, "Ten")]
        [TestCase(false, 0, 0, null)]
        [TestCase(false, 1, 0, null)]
        [TestCase(false, 0, 5, "Five")]
        [TestCase(false, 11, 0, "Ten")]
        public void CalcOutcome(bool includeDefault, decimal calcResult1, decimal calcResult2, string expected)
        {
            var outcomes = CreateOutcomes(includeDefault);

            var result = Entity.Create<SurveyResponse>();
            result.SurveyAnswers.Add(CreateAnswer(calcResult1));
            result.SurveyAnswers.Add(CreateAnswer(calcResult2));

            var calcResult = result.CalcWeightedValue();
            var outcome = result.CalcOutcome(outcomes, calcResult);

            if (expected == null)
                Assert.That(outcome, Is.Null);
            else
            {
                Assert.That(outcome, Is.Not.Null);
                Assert.That(outcome.Name, Is.EqualTo(expected));
            }
        }

        IEnumerable<SurveyOutcome> CreateOutcomes(bool includeDefault)
        {
            var result = new List<SurveyOutcome>();
            result.Add(CreateOutcome("Five", 5.0M));
            result.Add(CreateOutcome("Ten", 10.0M));
            if (includeDefault)
                result.Add(CreateOutcome("Default", null));
            return result;
        }
        
        SurveyOutcome CreateOutcome(string name, decimal? threshold)
        {
            var result = Entity.Create<SurveyOutcome>();
            result.Name = name;
            result.SurveyOutcomeThreshold = threshold;
            return result;
        }

        SurveyAnswer CreateAnswer(decimal calcValue)
        {
            var answer = Entity.Create<SurveyAnswer>();
            answer.SurveyAnswerCalculatedValue = calcValue;
            return answer;
        }


    }
}
