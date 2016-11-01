// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Add extension method for the SurveyAnswer model class
    /// </summary>
    public static class SurveyAnswerHelper
    {
        /// <summary>
        /// Calculate an adjusted value from the question weight, type and answer.
        /// </summary>
        /// <returns></returns>
        public static decimal CalcAdjustedValue(this SurveyAnswer answer)
        {
            var question = answer.QuestionBeingAnswered;
            var weight = question.QuestionWeight ?? 0.0M;

            if (question == null || weight == 0.0M)
                return 0.0M;

            return weight * GetAnswerValue(answer);
        }

        /// <summary>
        /// Get the calculated value of the answer. If no answer has been given it is worth 0.
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        public static decimal GetAnswerValue(this SurveyAnswer answer )
        {
            var typeId = answer.QuestionBeingAnswered.TypeIds.First();

            if (typeId == ChoiceQuestion.ChoiceQuestion_Type.Id)
                return GetChoiceAnswerValue(answer);
            else if (typeId == TextQuestion.TextQuestion_Type.Id)
                return GetTextAnswerValue(answer);
            else if (typeId == NumericQuestion.NumericQuestion_Type.Id)
                return GetNumericAnswerValue(answer);
            else
                throw new ArgumentException();
        }

        static decimal GetChoiceAnswerValue(SurveyAnswer answer)
        {
            if (!IsMultiChoice(answer))
            {
                if (answer.SurveyAnswerSingleChoice == null)
                    return 0.0M;
                else
                    return answer.SurveyAnswerSingleChoice.ChoiceOptionValue ?? 0.0M;
            }
            else
                return answer.SurveyAnswerMultiChoice.Sum(c => c.ChoiceOptionValue ?? 0.0M);
        }

        static bool IsMultiChoice(SurveyAnswer answer)
        {
            if (answer.QuestionBeingAnswered == null)
                throw new ArgumentException("No question");

            var choiceQuestion = answer.QuestionBeingAnswered.As<ChoiceQuestion>();

            if (choiceQuestion == null)
                throw new ArgumentException("question is not a choice");

            return choiceQuestion.ChoiceQuestionIsMulti ?? false;
        }


        static decimal GetTextAnswerValue(SurveyAnswer answer)
        {
            return String.IsNullOrWhiteSpace(answer.SurveyAnswerString) ? 0.0M : 1.0M;      // You have either answered it or not.
        }

        static decimal GetNumericAnswerValue(SurveyAnswer answer)
        {
            return answer.SurveyAnswerNumber ?? 0.0M;
        }

        /// <summary>
        /// Generate an string representation of the answer. Not that null indicates that there has not been an answer or that the answer is whitespace.
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        public static string GenerateTextSummary(this SurveyAnswer answer)
        {
            var typeId = answer.QuestionBeingAnswered.TypeIds.First();

            if (typeId == ChoiceQuestion.ChoiceQuestion_Type.Id)
                return GenerateChoiceTextSummary(answer);

            else if (typeId == TextQuestion.TextQuestion_Type.Id)
                return !String.IsNullOrWhiteSpace(answer.SurveyAnswerString) ? answer.SurveyAnswerString : null;

            else if (typeId == NumericQuestion.NumericQuestion_Type.Id)
                return answer.SurveyAnswerNumber != null ? answer.SurveyAnswerNumber.ToString(): null;

            else
                throw new ArgumentException();
        }

        /// <summary>
        /// If something has been selected return the name of the choice or the string concatinated names
        /// otherwise return null.
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        static string GenerateChoiceTextSummary(SurveyAnswer answer)
        {
            if (!IsMultiChoice(answer))
                return answer.SurveyAnswerSingleChoice != null ? answer.SurveyAnswerSingleChoice.Name : null;
            else
            {
                if (!answer.SurveyAnswerMultiChoice.Any())
                    return null;

                return String.Join(", ", answer.SurveyAnswerMultiChoice.Select(a => a.Name ?? "[Unnamed]"));
            }
        }
    }
}
