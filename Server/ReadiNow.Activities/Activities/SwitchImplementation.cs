// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Activities;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{

    public class SwitchImplementation : ActivityImplementationBase, IRunNowActivity
    {
        /// <summary>
        /// Switch on the provided value
        /// </summary>

        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var valuetoSwitchOnKey = GetArgumentKey("switchActivityDecisionArgument");

            var result = (string)inputs[valuetoSwitchOnKey];

            var activityAs = ActivityInstance.Cast<EntityWithArgsAndExits>();

            var selectedExitPoint = activityAs.ExitPoints.FirstOrDefault( ep => ep.Name == result);

            if (selectedExitPoint == null)
                selectedExitPoint = Entity.Get<ExitPoint>("switchActivityOtherwiseExitPoint");

            context.ExitPointId = selectedExitPoint;
         }
    }
}
