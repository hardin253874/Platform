// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Add extension method for the SurveyAnswer model class
    /// </summary>
    public static class SurveyResponseHelper
    {
        /// <summary>
        /// Update the results weighted values and the outcomes. Input must be writable.
        /// </summary>
        /// <param name="result"></param>
        public static void UpdateWeightedValueAndOutcomes(this SurveyResponse result)
        {
            result.SurveyResponseCalculatedValue = result.CalcWeightedValue();
            result.SurveyResponseOutcome = result.CalcOutcome();
        }

        /// <summary>
        /// Calculate an adjusted value from the question weight, type and answer.
        /// </summary>
        /// <returns></returns>
        public static SurveyOutcome CalcOutcome(this SurveyResponse result)
        {
            if (result.CampaignForResults == null)
                throw new ArgumentException("result missing campaign");

            if (result.CampaignForResults.SurveyForCampaign == null)
                throw new ArgumentException("result missing survey");


            if (result.SurveyResponseCalculatedValue == null)
                throw new ArgumentException("result missing SurveyResponseCalculatedValue");

            var outcomes = result.CampaignForResults.SurveyForCampaign.SurveyOutcomes;

            return result.CalcOutcome(outcomes, result.SurveyResponseCalculatedValue);
        }

		/// <summary>
		/// Calculate the outcome of the survey. Which survey outcome does the result match.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="outcomes">The outcomes.</param>
		/// <param name="calculatedResult">The calculated result.</param>
		/// <returns></returns>
        public static SurveyOutcome CalcOutcome(this SurveyResponse result, IEnumerable<SurveyOutcome> outcomes, decimal? calculatedResult)
        {
            if (outcomes.Count() == 0)
                return null;

            var matching = outcomes.OrderByDescending(o => o.SurveyOutcomeThreshold).FirstOrDefault(o => calculatedResult >= (o.SurveyOutcomeThreshold ?? 0.0M));

            return matching;
        }

		/// <summary>
		/// Calculate the outcome of the survey. Which survey outcome does the result match.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <returns></returns>
        public static decimal CalcWeightedValue(this SurveyResponse result)
        {
          
            return result.SurveyAnswers.Sum(answer => answer.SurveyAnswerCalculatedValue ?? answer.CalcAdjustedValue());
           
        }
    }
}
