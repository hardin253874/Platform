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

    public class SurveyAnswerTest
    {
        [Test]
        [TestCase("Two", 7.0, 14.0, "Two")]
        [TestCase("Five", 2.0, 10.0, "Five")]
        [TestCase("Null", 7.0, 0.0, "Null")]
        [TestCase("Five", null, 0.0, "Five")]
        [TestCase(null, null, 0.0, null)]
        [TestCase(null, 2.0, 0.0, null)]
        [RunAsDefaultTenant]
        public void CalcAdjustedValue_TextSummary_Single(string choice, double? weight, double? result, string resultText)
        {
            Decimal? dweight = null;

            if (weight != null) 
                dweight = new Decimal((float)weight);

            Decimal? dresult = null;

            if (result != null)
                dresult = new Decimal((float)result);

            var question = CreateThreeChoices(dweight);
            var answer = Entity.Create<SurveyAnswer>();
            answer.QuestionBeingAnswered = question.As<SurveyQuestion>();
            answer.SurveyAnswerSingleChoice = question.ChoiceQuestionChoiceSet.ChoiceOptionSetChoices.FirstOrDefault(c => c.Name == choice);

            var actualCalc = answer.CalcAdjustedValue();

            Assert.That(actualCalc, Is.EqualTo(dresult), "CalcAdjustedValue");

            var actualText = answer.GenerateTextSummary();

            Assert.That(actualText, Is.EqualTo(resultText), "GenerateTextSummary");
        }

        [Test]
        [TestCase("Two", "Three", 7.0, 35.0, "Two, Three")]
        [TestCase("Two", "Three", null, 0.0, "Two, Three")]
        [TestCase("Two", "Null", 7.0, 14.0, "Two, Null")]
        [TestCase(null, null, null, 0.0, null)]
        [TestCase(null, null, 2.0, 0.0, null)]
        [RunAsDefaultTenant]
        public void CalcAdjustedValue_TextSummary_Multi(string choice1, string choice2, double? weight, double? result, string resultText)
        {
            Decimal? dweight = null;

            if (weight != null)
                dweight = new Decimal((float)weight);

            Decimal? dresult = null;

            if (result != null)
                dresult = new Decimal((float)result);

            var question = CreateThreeChoices(dweight);
            question.ChoiceQuestionIsMulti = true;

            var answer = Entity.Create<SurveyAnswer>();
            answer.QuestionBeingAnswered = question.As<SurveyQuestion>();

            if (choice1 != null)
                answer.SurveyAnswerMultiChoice.Add(question.ChoiceQuestionChoiceSet.ChoiceOptionSetChoices.FirstOrDefault(c => c.Name == choice1));

            if (choice1 != null)
                answer.SurveyAnswerMultiChoice.Add(question.ChoiceQuestionChoiceSet.ChoiceOptionSetChoices.FirstOrDefault(c => c.Name == choice2));

            var actualCalc = answer.CalcAdjustedValue();

            Assert.That(actualCalc, Is.EqualTo(dresult), "CalcAdjustedValue");

            var actualText = answer.GenerateTextSummary();

            Assert.That(actualText, Is.EqualTo(resultText), "GenerateTextSummary");
        }

        [Test]
        [TestCase("sample text", 7.0, 7.0, "sample text")]
        [TestCase("sample text", null, 0.0, "sample text")]
        [TestCase("", 7.0, 0.0, null)]
        [TestCase(null, 7.0, 0.0, null)]
        [RunAsDefaultTenant]
        public void CalcAdjustedValue_TextSummary_Text(string text, double? weight, double? result, string resultText)
        {
            Decimal? dweight = null;

            if (weight != null)
                dweight = new Decimal((float)weight);

            Decimal? dresult = null;

            if (result != null)
                dresult = new Decimal((float)result);

            var question = Entity.Create<TextQuestion>();
            question.QuestionWeight = dweight;

            var answer = Entity.Create<SurveyAnswer>();
            answer.QuestionBeingAnswered = question.As<SurveyQuestion>();
            answer.SurveyAnswerString = text;

            var actualCalc = answer.CalcAdjustedValue();

            Assert.That(actualCalc, Is.EqualTo(dresult), "CalcAdjustedValue");

            var actualText = answer.GenerateTextSummary();

            Assert.That(actualText, Is.EqualTo(resultText), "GenerateTextSummary");
        }

        ChoiceQuestion CreateThreeChoices(decimal? weight)
        {
            var question = Entity.Create<ChoiceQuestion>();

            var choiceSet = Entity.Create<ChoiceOptionSet>();
            choiceSet.ChoiceOptionSetChoices.Add(CreateChoiceOption("Two", 2.0M));
            choiceSet.ChoiceOptionSetChoices.Add(CreateChoiceOption("Three", 3.0M));
            choiceSet.ChoiceOptionSetChoices.Add(CreateChoiceOption("Five", 5.0M));
            choiceSet.ChoiceOptionSetChoices.Add(CreateChoiceOption("Null", null));

            question.ChoiceQuestionChoiceSet = choiceSet;

            question.QuestionWeight = weight;

            return question;
        }

        ChoiceOption CreateChoiceOption(string name, decimal? value)
        {
            var result = Entity.Create<ChoiceOption>();
            result.Name = name;
            result.ChoiceOptionValue = value;
            return result;

        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(null, null)]
        [TestCase(2.1, "2.1")]
        public void NumericAnswer_TextSummay(double? value, string expected)
        {
            var answer = Entity.Create<SurveyAnswer>();
            answer.QuestionBeingAnswered = Entity.Create<NumericQuestion>().As<SurveyQuestion>();

            if (value != null)
                answer.SurveyAnswerNumber = new Decimal((float) value);

            var generated = answer.GenerateTextSummary();
            Assert.That(generated, Is.EqualTo(expected));
        }
    }
}
