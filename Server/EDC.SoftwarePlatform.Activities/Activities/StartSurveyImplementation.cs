// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.PartialClasses;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Distributes a survey for completion.
    /// </summary>
    public class StartSurveyImplementation : ActivityImplementationBase, IRunNowActivity
    {
        /// <summary>
        /// Runs the activity.
        /// </summary>
        /// <param name="context">The run context.</param>
        /// <param name="inputs">The input parameters.</param>
        public void OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var campaign = GetArgumentEntity<SurveyCampaign>(inputs, "inStartSurveyCampaign");

            var tasks = campaign.Launch().ToList();

            Entity.Save(tasks);

            var responses = tasks.Select(t => t.UserSurveyTaskSurveyResponse);

            context.SetArgValue(ActivityInstance, GetArgumentKey("core:outStartSurveyResponses"), responses);
        }
    }
}
