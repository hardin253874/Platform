// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    public class AssignToVariableImplementation : ActivityImplementationBase, IRunNowActivity
    {
        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var activityAs = ActivityInstance.Cast<AssignToVariable>();
            var valueArgument = Entity.Get<ActivityArgument>("assignValueArgument", Resource.Name_Field);
            var targetVar = activityAs.TargetVariable;

            context.SetArgValue(targetVar, inputs[valueArgument]);
        }

        

        public override void Validate(WorkflowMetadata metadata)
        {
            base.Validate(metadata);

            var activityAs = ActivityInstance.Cast<AssignToVariable>();

            if (activityAs.TargetVariable == null)
            {
                metadata.AddValidationError(string.Format("Variable has not been assigned. Activity: '{0}'", activityAs.Name));
            }
        }
    }
}
