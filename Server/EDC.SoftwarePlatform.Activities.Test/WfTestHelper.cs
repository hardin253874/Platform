// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities.Test
{
    public static class WfTestHelper
    {
        public static Workflow AddActivity(this Workflow wf, WfActivity activity, string fromNamed = null, string fromExit = null, bool dontTerminate = false)
        {
            if (fromNamed != null)
            {
                var from = wf.ContainedActivities.First(a => a.Name == fromNamed);

                ActivityTestHelper.AddTransition(wf, from, activity, fromExit);
            }
            else
            {
				if ( wf.ContainedActivities.Count > 0 )
                {
                    var oldLast = wf.ContainedActivities.First(a=>a.ForwardTransitions.Any(t => t.Is<Termination>())).AsWritable<WfActivity>();
                    var term = oldLast.ForwardTransitions.First(t => t.Is<Termination>()).AsWritable<TransitionStart>();

                    var oldExitPoint = term.FromExitPoint;

                    oldLast.ForwardTransitions.Remove(term);
                    wf.Terminations.Remove(term.As<Termination>());
                    term.Delete();

                    ActivityTestHelper.AddTransition(wf, oldLast, activity, oldExitPoint);
                }
                else
                {
                    wf.FirstActivity = activity;
                }
            }

            wf.ContainedActivities.Add(activity);

            if (!dontTerminate)
                ActivityTestHelper.AddTermination(wf, activity);

            return wf;
        }

        public static Workflow AddTermination(this Workflow wf, string activityName, string fromExit = null, string toExit = null)
        {
            var activity = wf.ContainedActivities.First(a => a.Name == activityName);
            return AddTermination(wf, activity, fromExit, toExit);
        }

        public static Workflow  AddTermination(this Workflow wf, WfActivity activity, string fromExit = null, string toExit = null)
        {
            ActivityTestHelper.AddTermination(wf, activity, fromExit, toExit);

            return wf;
        }
        
        public static Workflow AddWfExitPoint(this Workflow wf, string name, bool isDefault = false)
        {
            wf.ExitPoints.Add(new ExitPoint
            {
                Name = name,
                IsDefaultExitPoint = isDefault
            });

            return wf;
        }
        
        public static Workflow AddVariable<T>(this Workflow wf, string name, string expression = null, EntityType conformsToType = null) where T : IEntity, new()
        {
            var myVarAs = CreateActivityArg<T>(wf, name, expression, conformsToType);
            ActivityTestHelper.AddVariableToWorkflow(wf, myVarAs);

            return wf;
        }

        public static Workflow AddInput<T>(this Workflow wf, string name, EntityType conformsToType = null) where T : IEntity, new()
        {
            var myVarAs = CreateActivityArg<T>(wf, name, null, conformsToType);
            ActivityTestHelper.AddInputToWorkflow(wf, myVarAs);
            return wf;
        }

        public static Workflow AddOutput<T>(this Workflow wf, string name, EntityType conformsToType = null) where T : IEntity, new()
        {
            var myVarAs = CreateActivityArg<T>(wf, name, null, conformsToType);
            ActivityTestHelper.AddOutputToWorkflow(wf, myVarAs);
            return wf;
        }

        public static Workflow AddEntityExpressionToVariable(this Workflow wf, string argumentName, EntityRef entityRef)
        {
            var destination = wf.Variables.First(v => v.Name == argumentName);
            ActivityTestHelper.AddEntityExpression(wf, wf.As<WfActivity>(), destination, entityRef);

            return wf;
        }

        public static Workflow AddExpressionToWorkflowVariable(this Workflow wf, string argumentName, string expressionString, bool isTemplate = false)
        {
            var destination = wf.Variables.FirstOrDefault(v => v.Name == argumentName);

            if (destination == null)
                throw new ArgumentException(String.Format("No matching variable name for workflow. Workflow='{0}' Argument='{1}'", wf.Name ?? "<unnamed>", argumentName ?? "<unnamed>"));

            ActivityTestHelper.AddExpressionToArgument(wf, wf.Cast<WfActivity>(), destination, expressionString, isTemplate);

            return wf;
        }

        private static ActivityArgument CreateActivityArg<T>(Workflow wf, string name, string expression, EntityType conformsToType) where T : IEntity, new()
        {
            var myVar = new T();
            myVar.SetField("name", name);

            if (conformsToType != null)
            {
                var typedVar = myVar.As<TypedArgument>();
                typedVar.ConformsToType = conformsToType;
            }
            var myVarAs = myVar.As<ActivityArgument>();


            if (expression != null)
                ActivityTestHelper.AddExpressionToArgument(wf, wf.Cast<WfActivity>(), myVarAs, expression, false);

            return myVarAs;
        }


    }
}

