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

    public class DecisionImplementation : ActivityImplementationBase, IRunNowActivity
    {
        /// <summary>
        /// Use to pass the result of the expression evaluation
        /// </summary>

        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var decisionArgumentKey = GetArgumentKey("decisionActivityDecisionArgument");

            var result = (bool) inputs[decisionArgumentKey];

            EntityRef exitPoint;
            
            if (result)
                exitPoint = new EntityRef("core", "decisionActivityYesExitPoint");
            else
                exitPoint = new EntityRef("core", "decisionActivityNoExitPoint");

            context.ExitPointId = exitPoint;

        }
    }
}
