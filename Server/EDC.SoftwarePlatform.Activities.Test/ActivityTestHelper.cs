// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Activities.Test
{
    public static class ActivityTestHelper
    {
        public static WorkflowRun CreateWfRunForActivity(ActivityImplementationBase activity)
        {
            //TODO: change to using CreateWorkflowRun in WorkflowRunner.Instance

            var run = new WorkflowRun();

            var workflow = activity.ActivityInstance.As<Workflow>();

            if (workflow == null)
            {
                workflow = new Workflow { };

                workflow.Save();
            }

            run.WorkflowBeingRun = workflow;
            run.TriggeringUser = Entity.Get<UserAccount>( RequestContext.GetContext().Identity.Id );

            return run;
        }

        /// <summary>
        /// Add a a log activity
        /// </summary>
        public static Workflow AddLog(this Workflow wf, string name, string template, string fromNamed = null, string fromExit = null)
        {
            return AddLog_imp(wf, name, template, fromNamed, fromExit, true);
        }

        /// <summary>
        /// Add a a log activity
        /// </summary>
        public static Workflow AddLogWithExpression(this Workflow wf, string name, string template, string fromNamed = null, string fromExit = null)
        {
            return AddLog_imp(wf, name, template, fromNamed, fromExit, false);
        }

        static Workflow AddLog_imp(this Workflow wf, string name, string template, string fromNamed , string fromExit, bool isTemplate)
        {
            var log = new LogActivity
            {
                Name = name
            };
            var logAs = log.As<WfActivity>();

            AddExpressionToActivityArgument(wf, logAs, "Message", template, isTemplate);


            wf.AddActivity(logAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }

        /// <summary>
        /// Add a a log activity
        /// </summary>
        public static Workflow AddUpdateField(this Workflow wf, string name, Resource fieldOrRel = null, string resourceExpression = null, string valueExpression = null, string fromNamed = null, string fromExit = null)
        {
            var uf = new UpdateFieldActivity { Name = name };
            var ufAs = uf.As<WfActivity>();

            if (resourceExpression != null)
                AddExpressionToActivityArgument(wf, ufAs, "Record", resourceExpression);

            if (fieldOrRel != null)
            {
                uf.InputArguments.Add(new ResourceArgument { Name = "1" }.Cast<ActivityArgument>());
                SetActivityArgumentToResource(wf, ufAs, "1", fieldOrRel);
            }


            if (valueExpression != null)
            {
                uf.InputArguments.Add(new ObjectArgument { Name = "1_value" }.Cast<ActivityArgument>());
                AddExpressionToActivityArgument(wf, ufAs, "1_value", valueExpression);
            }

            wf.AddActivity(ufAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }

        /// <summary>
        /// Add Create activity
        /// </summary>
        public static Workflow AddCreate(this Workflow wf, string name, string resourceTypeExpression = null, string fromNamed = null, string fromExit = null)
        {
            var ca = new CreateActivity { Name = name };
            var caAs = ca.As<WfActivity>();

            if (resourceTypeExpression != null)
                AddExpressionToActivityArgument(wf, caAs, "Object", resourceTypeExpression);

            wf.AddActivity(caAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }


        /// <summary>
        /// Add Clone activity
        /// </summary>
        public static Workflow AddClone(this Workflow wf, string name, string resourceExpression = null, string fromNamed = null, string fromExit = null)
        {
            var ca = new CloneActivity { Name = name };
            var caAs = ca.As<WfActivity>();

            if (resourceExpression != null)
                AddExpressionToActivityArgument(wf, caAs, "Record", resourceExpression);

            wf.AddActivity(caAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }


        /// <summary>
        /// Add a delete activity
        /// </summary>
        public static Workflow AddDelete(this Workflow wf, string name, string resourceExpression = null, string fromNamed = null, string fromExit = null)
        {
            var uf = new DeleteActivity()
            {
                Name = name
            };
            var ufAs = uf.As<WfActivity>();


            if (resourceExpression != null)
                ActivityTestHelper.AddExpressionToActivityArgument(wf, ufAs, "Record", resourceExpression, false);

            wf.AddActivity(ufAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }

        /// <summary>
        /// Add a get records activity
        /// </summary>
        public static Workflow AddGetRecords(this Workflow wf, string name, string definitionExpression, string reportExpression = null, string fromNamed = null, string fromExit = null)
        {
            var act = new GetResourcesActivity()
            {
                Name = name
            };
            var actAs = act.As<WfActivity>();

            ActivityTestHelper.AddExpressionToActivityArgument(wf, actAs, "Object", definitionExpression, false);


            if (reportExpression != null)
                ActivityTestHelper.AddExpressionToActivityArgument(wf, actAs, "Report", reportExpression, false);

            wf.AddActivity(actAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }
        /// <summary>
        /// Add a foreach activity
        /// </summary>
        public static Workflow AddForEach(this Workflow wf, string name, string listExpression, EntityType listType, string fromNamed = null, string fromExit = null)
        {
            var forEach = new ForEachResource()
            {
                Name = name
            };
            var forEachAs = forEach.As<WfActivity>();

            AddExpressionToActivityArgument(wf, forEachAs, "List", listExpression);

            wf.AddActivity(forEachAs, fromNamed, fromExit);

            AddMissingExpressionParametersToWorkflow(wf);

            var resourceArgInst = forEachAs.GetOutputArgumentInstances().First(ai => ai.Name == name + "_Record");
            resourceArgInst.InstanceConformsToType = listType;

            return wf;
        }


        /// <summary>
        /// Add a foreach activity
        /// </summary>
        public static Workflow AddForEach(this Workflow wf, string name, string listExpression, string listType,
                                          string fromNamed = null, string fromExit = null)
        {
            return AddForEach(wf, name, listExpression, Entity.Get<EntityType>(listType), fromNamed, fromExit);
        }


        /// <summary>
        /// Add a switch activity
        /// </summary>
        public static Workflow AddSwitch(this Workflow wf, string name, string switchExpression, string[] exits, string fromNamed = null, string fromExit = null)
        {
            var act = new SwitchActivity()
            {
                Name = name
            };
            var actAs = act.As<WfActivity>();

            AddExpressionToActivityArgument(wf, actAs, "Value to Switch On", switchExpression);

            AddExits(act.ExitPoints, exits);

            wf.AddActivity(actAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }


        private static void AddExits(IEntityCollection<ExitPoint> exitPoints, string[] exits)
        {
            bool isFirstExit = true;
            foreach (var exit in exits)
            {
                var ep = Entity.Create<ExitPoint>();
                ep.Name = exit;
                ep.IsDefaultExitPoint = isFirstExit;
                exitPoints.Add(ep);
                isFirstExit = false;
            }
        }

        /// <summary>
        /// Add display form with an exit point called "First Exit" that is the default;
        /// </summary>
        public static Workflow AddDisplayForm(this Workflow wf, string name, string[] exits, string formExpression = null, string recordExpression = null, string personExpression = null, string fromNamed = null, string fromExit = null)
        {
            var act = new DisplayFormActivity()
            {
                Name = name
            };
            var actAs = act.As<WfActivity>();

            if (formExpression != null)
                AddExpressionToActivityArgument(wf, actAs, "Form", formExpression);

            if (recordExpression != null)
                AddExpressionToActivityArgument(wf, actAs, "Record", recordExpression);

            if (personExpression != null)
                AddExpressionToActivityArgument(wf, actAs, "For Person", personExpression);
            else
                AddExpressionToActivityArgument(wf, actAs, "For Person", "[Triggering Person]");


            AddExits(act.ExitPoints, exits);

            wf.AddActivity(actAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }

        public static Workflow AddPromptUser(this Workflow wf, string name, string personExpression = null, string fromNamed = null, string fromExit = null, string[] promptFor = null)
        {
            var act = new PromptUserActivity
            {
                Name = name
            };

            var actAs = act.As<WfActivity>();

            AddExpressionToActivityArgument(wf, actAs, "For Person", personExpression ?? "[Triggering Person]");
            
            foreach (var argument in wf.InputArguments.Union(wf.Variables, new EntityIdComparer()))
            {
                if (promptFor == null || promptFor.Contains(argument.Name))
                    act.PromptForArguments.Add(new ActivityPrompt { Name = argument.Name, ActivityPromptArgument = argument });
            }    

            wf.AddActivity(actAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }
        
        /// <summary>
        /// Add a assign to var activity
        /// </summary>
        public static Workflow AddAssignToVar(this Workflow wf, string name, string expression, string targetVarName, string fromNamed = null, string fromExit = null)
        {
            var targetVar = wf.Variables.Union(wf.OutputArguments).First(v => v.Name == targetVarName);

            var assign = new AssignToVariable()
            {
                Name = name,
                TargetVariable = targetVar
            };
            var assignAs = assign.As<WfActivity>();

            AddExpressionToActivityArgument(wf, assignAs, "Value", expression);

            wf.AddActivity(assignAs, fromNamed, fromExit);

            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldAliasExpressionPairs">An array of field-alias, expression pairs</param>
        public static Workflow AddUpdate(this Workflow wf, string name, string resourceExpression, string[] fieldAliasExpressionPairs,
                                          string fromNamed = null, string fromExit = null)
        {
            var update = new UpdateFieldActivity()
            {
                Name = name
            };
            var updateAs = update.As<WfActivity>();

            ActivityTestHelper.AddExpressionToActivityArgument(wf, updateAs, "Record", resourceExpression, false);

            wf.AddActivity(updateAs, fromNamed, fromExit);

            var count = 0;

            for (int i = 0; i < fieldAliasExpressionPairs.Count(); i += 2)
            {
                var argName = "input" + count++;
                var argValueName = argName + "_value";

                var result1 = (new StringArgument() { Name = argName }).As<ActivityArgument>(); ;
                update.InputArguments.Add(result1);
                AddExpressionToActivityArgument(wf, updateAs, argName, fieldAliasExpressionPairs[i]);

                var result2 = (new ObjectArgument { Name = argValueName }).As<ActivityArgument>();
                update.InputArguments.Add(result2);
                AddExpressionToActivityArgument(wf, updateAs, argValueName, fieldAliasExpressionPairs[i + 1]);
            }

            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }

        public static Workflow AddWorkflowProxy(this Workflow wf, string name, Workflow wfToProxy, string fromNamed = null, string fromExit = null)
        {
            var proxy = ActivityTestHelper.CreateWorkflowProxy(wfToProxy);
            proxy.Name = name;

            var proxyAs = proxy.As<WfActivity>();

            wf.AddActivity(proxyAs, fromNamed, fromExit);

            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }

        /// <summary>
        /// Add display form with an exit point called "First Exit" that is the default;
        /// </summary>
        public static Workflow AddStartSurvey(
            this Workflow wf, 
            string name,
            string campaignExpression,
            string fromNamed = null,
            string fromExit = null)
        {
            var act = new StartSurveyActivity
            {
                Name = name
            };
            var actAs = act.As<WfActivity>();

            if (campaignExpression != null)
                AddExpressionToActivityArgument(wf, actAs, "Campaign", campaignExpression);

            wf.AddActivity(actAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }


        /// <summary>
        /// Add a notify activity
        /// </summary>
        public static Workflow AddNotify(
            this Workflow wf,
            string name,
            string peopleExpression,
            string messageExpression,
            bool waitForReplies,
            Dictionary<string, Workflow> replyMap = null,
            string linkToRecordExpression = null,
            string fromNamed = null, string fromExit = null)
        {
            var act = new NotifyActivity()
            {
                Name = name
            };
            var actAs = act.As<WfActivity>();

            if (peopleExpression != null)
                AddExpressionToActivityArgument(wf, actAs, "People", peopleExpression);

            if (messageExpression != null)
                AddExpressionToActivityArgument(wf, actAs, "Message", messageExpression);

            if (linkToRecordExpression != null)
                AddExpressionToActivityArgument(wf, actAs, "Link to record", linkToRecordExpression);
            
            AddExpressionToActivityArgument(wf, actAs, "Wait for replies", waitForReplies ? "true" : "false");
            AddExpressionToActivityArgument(wf, actAs, "Wait for", "1");

            if (replyMap != null)
            {
                foreach (var entry in replyMap)
                {
                    act.NReplyMap.Add(new ReplyMapEntry { Name = entry.Key, RmeWorkflow = entry.Value });
                }
            }

            wf.AddActivity(actAs, fromNamed, fromExit);
            AddMissingExpressionParametersToWorkflow(wf);

            return wf;
        }

        public static Workflow AddTransition(this Workflow wf, string from, string to, string exitPoint = null)
        {
            var fromA = wf.ContainedActivities.First(a => a.Name == @from);
            var toA = wf.ContainedActivities.First(a => a.Name == to);
            ExitPoint exit;
            if (exitPoint == null)
                exit = fromA.GetDefaultExitPoint();
            else
                exit = fromA.GetNamedExitPoint(exitPoint);

            AddTransition(wf, fromA, toA, exit);

            return wf;
        }


        public static Workflow AddExpressionToArgument(this Workflow wf, string activityName, string argumentName, string expressionString, bool isTemplate = false)
        {
            var wfAs = wf.As<WfActivity>();

            var activity = wf.ContainedActivities.First(a => a.Name == activityName);
            var argument = activity.GetInputArgument(argumentName);

            if (argument == null)
                throw new ArgumentException(String.Format("No matching argument name for workflow. Activity='{0}' Argument='{1}'", wf.Name ?? "<unnamed>", argumentName ?? "<unnamed>"));

            return AddExpressionToArgument(wf, activity, argument, expressionString, isTemplate);
        }

        public static Workflow AddExpressionToArgument(this Workflow wf, WfActivity activity, ActivityArgument destination, string expressionString, bool isTemplate)
        {
            if (isTemplate)
            {
                expressionString = ExpressionHelper.ConvertTemplateToExpressionString(expressionString);
            }

            if (expressionString == null)
            {
                throw new ArgumentNullException("expressionString");
            }

            var exp = new WfExpression()
                {
                    ExpressionString = expressionString,
                    ArgumentToPopulate = destination,
                    ExpressionInActivity = activity,
                    IsTemplateString = false
                };

            activity.ExpressionMap.Add(exp.As<WfExpression>());
            return wf;
        }



        public static Workflow AddEntityExpressionToInputArgument(this Workflow wf, WfActivity activity, string argumentName, EntityRef entityRef)
        {
            var destination = activity.GetInputArgument(argumentName);
            if (destination == null)
                throw new ArgumentException();

            return AddEntityExpression(wf, activity, destination, entityRef);
        }

        public static Workflow AddEntityExpression(this Workflow wf, WfActivity activity, ActivityArgument arg, EntityRef entityRef)
        {
            var r = Entity.Get(entityRef).As<Resource>();
            var exp = new WfExpression { ArgumentToPopulate = arg, ExpressionInActivity = activity, ExpressionString = string.Format("[{0}]", r.Id.ToString()) };
            exp.WfExpressionKnownEntities.Add(new NamedReference { Name = r.Id.ToString(), ReferencedEntity = r });
            activity.ExpressionMap.Add(exp);

            return wf;
        }

        /// <summary>
        /// Add any expression parameter to the workflow that have not been defined yet.
        /// </summary>
        /// <param name="wf"></param>
        public static void AddMissingExpressionParametersToWorkflow(Workflow wf)
        {
            // workflow inputs and variables
            var wfAs = wf.Cast<WfActivity>();

            foreach (var argument in wfAs.GetInputArguments().Union(wf.Variables, new EntityIdComparer()))
            {
                if (FindExpressionParameter(wf, wfAs, argument) == null)
                    CreateArgumentInstance(wf, wfAs, argument);
            }           
            
            // activity outputs
            foreach (var activity in wf.ContainedActivities)
            {
                foreach (var argument in activity.GetOutputArguments().Union(activity.GetInputArguments()))
                {
                    if (FindExpressionParameter(wf, activity, argument) == null)
                        CreateArgumentInstance(wf, activity, argument);
                }
            }

            // workflow outputs
            foreach (var argument in wf.OutputArguments)
            {
                if (FindExpressionParameter(wf, wfAs, argument) == null)
                    CreateArgumentInstance(wf, wfAs, argument);
            }
            



        }

        private static WfArgumentInstance FindExpressionParameter(Workflow wf, WfActivity activity, ActivityArgument argument)
        {
            return wf.ExpressionParameters.FirstOrDefault( ep => ep.ArgumentInstanceActivity.Id == activity.Id && ep.ArgumentInstanceArgument.Id == argument.Id);
        }

        /// <summary>
        /// Create a transition between the given activity instances for the named exit point
        /// </summary>
        public static void AddTermination(Workflow wf, WfActivity from, string fromExitPointName = null, string wfExitPointName = null)
        {
            ExitPoint fromExitPoint = fromExitPointName == null ? @from.GetDefaultExitPoint() : GetNamedExitPoint(@from, fromExitPointName);

            ExitPoint wfExitPoint = null;

            if (wfExitPointName == null )
            {
                wfExitPoint = wf.GetDefaultExitPoint();

                if (wfExitPoint == null)
                {
                    wf.AddDefaultExitPoint();
                    wfExitPoint = wf.GetDefaultExitPoint();
                }
            }
            else 
            {
                wfExitPoint = GetNamedExitPoint(wf.As<WfActivity>(), wfExitPointName);
            }

            if (fromExitPoint == null)
                throw new ArgumentException("From activity missing named exit point.");

            if (wfExitPoint == null)
                throw new ArgumentException("Workflow missing named exit point.");

            AddTermination(wf, @from, fromExitPoint, wfExitPoint);
        }

        /// <summary>
        /// Create a transition between the given activity instances for the provided exit point
        /// </summary>
        public static void AddTermination(Workflow wf, WfActivity from, ExitPoint fromExitPoint, ExitPoint wfExitPoint)
        {
            // remove exiting conflicting transitions
            var toRemove = @from.ForwardTransitions.Where(t => t.FromExitPoint.Id == fromExitPoint.Id).ToList();
            @from.ForwardTransitions.RemoveRange(toRemove);
            Entity.Delete(toRemove.Select(t => t.Id));

            // remove existing conflicting terminations
            var toRemoveTerm = wf.Terminations.Where(t => t.FromActivity.Id == from.Id && t.FromExitPoint.Id == fromExitPoint.Id).ToList();
            wf.Terminations.RemoveRange(toRemoveTerm);
            Entity.Delete(toRemoveTerm.Select(t => t.Id));


            var term = new Termination() { Name = fromExitPoint.Name,  FromActivity = @from, FromExitPoint = fromExitPoint, WorkflowExitPoint = wfExitPoint };
            @from.ForwardTransitions.Add(term.As<TransitionStart>());
            wf.Terminations.Add(term);
        }

        /// <summary>
        /// Add an expression parameter, return the parameters substitution name
        /// </summary>
        /// <param name="wf"></param>
        /// <param name="from"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string CreateArgumentInstance(Workflow wf, WfActivity from, ActivityArgument argument)
        {
            var uniqueName = GeneratetUniqueNameForExpressionParameter(wf, from, argument);
            var sourceInstance = new WfArgumentInstance { Name = uniqueName,  ArgumentInstanceActivity = @from, ArgumentInstanceArgument = argument };

            if (@from.IsReadOnly)
            {
                @from = @from.AsWritable<WfActivity>();
            }
            @from.ArgumentInstanceFromActivity.Add(sourceInstance);
            wf.ExpressionParameters.Add(sourceInstance);

            return uniqueName;
        }

        private static void CreateResourceListArgumentInstance(Workflow wf, WfActivity from, ActivityArgument argument, string uniqueName)
        {
            
        }

        /// <summary>
        /// Generate a parameter name that is unique to the workflow. Use the argument name if possible, otherwise append a number. 
        /// </summary>
        private static string GeneratetUniqueNameForExpressionParameter(Workflow wf, WfActivity activity, ActivityArgument argument)
        {

            if (argument.Name == null)
                throw new ArgumentException("Argument missing name");

            var composedName = activity.Is<Workflow>()
                                   ? argument.Name
                                   : string.Format("{0}_{1}", (activity.Name ?? string.Empty), argument.Name);


            if (!wf.ExpressionParameters.Any( expPar => expPar.Name == composedName))
                return composedName;

            var count = 0;
            string proposedName;
            do
            {
                count++;
                proposedName = String.Format("{0}{1}", composedName, count);
            }
            while (wf.ExpressionParameters.Any( expPar => expPar.Name == proposedName));

            return proposedName;
        }

        /// <summary>
        /// Create a transition between the given activity instances for the provided exit point
        /// </summary>
        public static void AddTransition(Workflow wf, WfActivity from, WfActivity to)
        {
            AddTransition(wf, @from, to, @from.GetDefaultExitPoint());
        }

        /// <summary>
        /// Create a transition between the given activity instances for the provided exit point
        /// </summary>
        public static void AddTransition(Workflow wf, WfActivity from, WfActivity to, string exitPointName)
        {
            var exitPoint = GetNamedExitPoint(@from, exitPointName);
            if (exitPoint == null) 
                throw new ArgumentException(String.Format("Attempted to create a transition from a non-existent exit point. '{0}'", exitPointName));

            var trans = new Transition() { Name = exitPointName, FromActivity = @from, ToActivity = to, FromExitPoint = exitPoint };
            wf.Transitions.Add(trans);
        }

        /// <summary>
        /// Create a transition between the given activity instances for the provided exit point
        /// </summary>
        public static void AddTransition(Workflow wf, WfActivity from, WfActivity to, ExitPoint exitPoint)
        {
            // remove any existing
            var toRemove = from.ForwardTransitions.FirstOrDefault(t => t.FromExitPoint.Id == exitPoint.Id);

            if (toRemove != null)
            {
                from.ForwardTransitions.Remove(toRemove);
                
                var asTerm = toRemove.As<Termination>();
                if (asTerm!= null)
                    wf.Terminations.Remove(asTerm);

                var asTrans = toRemove.As<Transition>();
                if (asTrans != null)
                    wf.Transitions.Remove(asTrans);

                toRemove.Delete();
            }

            var trans = new Transition() { Name = exitPoint.Name, FromActivity = @from, ToActivity = to, FromExitPoint = exitPoint };

            from.ForwardTransitions.Add(trans.As<TransitionStart>());
            wf.Transitions.Add(trans);
        }

        /// <summary>
        /// Create a transition between the given activity instances for the provided exit point and field mappings
        /// </summary>
        public static void AddTransitionWithMapping(Workflow wf, WfActivity from, WfActivity to, ExitPoint exitPoint, IDictionary<string, string> argumentMappings)
        {
            var trans = new Transition() { FromActivity = @from, ToActivity = to, FromExitPoint = exitPoint };
            wf.Transitions.Add(trans);
            
            if (argumentMappings != null)
            {
                foreach (var entry in argumentMappings)
                {
                    //AddExpressionToActivityArgument(wf, to, entry.Value, entry.Key);
                    AddExpressionToMapArguments(wf, @from, to, entry.Key, entry.Value);
             
                }
            }
        }

        public static void AddFirstActivityWithMapping(Workflow workflow, WfActivity firstActivity, IDictionary<string, string> newArgumentMappings)
        {
            workflow.FirstActivity = firstActivity;
            workflow.ContainedActivities.Add(firstActivity);
            
            if (newArgumentMappings != null)
            {
                foreach (var entry in newArgumentMappings)
                {


                    var source = workflow.As<WfActivity>().GetInputArgument(entry.Key);

                    var subName = CreateArgumentInstance(workflow, firstActivity, source);

                    AddExpressionToActivityArgument(workflow, firstActivity, entry.Value, String.Format("[{0}]", subName), false);
                }
            }
        }

        public static void AddExpressionToMapArguments(Workflow wf, WfActivity fromActivity, WfActivity toActivity, string fromArgumentName, string toArgumentName)
        {
            ActivityArgument fromArgument;

            if (fromActivity.Is<Workflow>())
                fromArgument = fromActivity.GetInputArgument(fromArgumentName);
            else
                fromArgument = fromActivity.GetOutputArgument(fromArgumentName);

            if (fromArgument == null)
                throw new ArgumentException(String.Format("Unable to find argument on the from Activity: [{0}].[{1}]", fromActivity.Name, fromArgumentName));

            var subString = CreateArgumentInstance(wf, fromActivity, fromArgument);
            AddExpressionToActivityArgument(wf, toActivity, toArgumentName, String.Format("[{0}]", subString), false);
        }


        public static Workflow AddExpressionToActivityArgument(this Workflow wf, WfActivity activity, string argumentName, string expressionString, bool isTemplate = false)
        {
            var destination = activity.GetInputArgument( argumentName );

            if (destination == null)
                throw new ArgumentException(String.Format("No matching argument name for activity. Activity='{0}' Argument='{1}'", activity.Name ?? "<unnamed>", argumentName ?? "<unnamed>"));

            return AddExpressionToArgument(wf, activity, destination, expressionString, isTemplate);
        }

        public static Workflow AddExpressionToActivityArgument(this Workflow wf, WfActivity activity, string argumentName, IEnumerable<Resource> resources)
        {
            SetActivityArgumentToResourceList(wf, activity, argumentName, resources);

            return wf;
        }

        public static void SetActivityArgumentToResource(Workflow wf, WfActivity activity, string argumentName, Resource resource)
        {
            var arg = activity.GetInputArgument(argumentName);
            if (arg == null)
                throw new ArgumentException(String.Format("No matching argument name for activity. Activity='{0}' Argument='{1}'", activity.Name ?? "<unnamed>", argumentName ?? "<unnamed>"));

            var exp = new WfExpression { ArgumentToPopulate = arg, ExpressionInActivity = activity, ExpressionString = null };
            exp.WfExpressionKnownEntities.Add(new NamedReference() { Name = "E1", ReferencedEntity = resource });
            activity.ExpressionMap.Add(exp);  // BUG: needed to work around an issue saving relationships
        }

        public static void SetActivityArgumentToResourceList(Workflow wf, WfActivity activity, string argumentName, IEnumerable<Resource> resources)
        {
            var arg = activity.GetInputArgument(argumentName);
            if (arg == null)
                throw new ArgumentException(String.Format("No matching argument name for activity. Activity='{0}' Argument='{1}'", activity.Name ?? "<unnamed>", argumentName ?? "<unnamed>"));

            var exp = new WfExpression { ArgumentToPopulate = arg, ExpressionInActivity = activity, ExpressionString = null };
            exp.WfExpressionKnownEntities.AddRange(resources.Select(r => new NamedReference() { Name = Guid.NewGuid().ToString(), ReferencedEntity = r }));

            activity.ExpressionMap.Add(exp);  // BUG: needed to work around an issue saving relationships
        }

        public static string AddVariableToWorkflow(Workflow wf, ActivityArgument variable)
        {
            wf.Variables.Add(variable);

            return CreateArgumentInstance(wf, wf.Cast<WfActivity>(), variable);
        }

        public static string AddInputToWorkflow(Workflow wf, ActivityArgument variable)
        {
            wf.InputArguments.Add(variable);

            return CreateArgumentInstance(wf, wf.Cast<WfActivity>(), variable);
        }

        public static string AddOutputToWorkflow(Workflow wf, ActivityArgument variable)
        {
            wf.OutputArguments.Add(variable);

            return CreateArgumentInstance(wf, wf.Cast<WfActivity>(), variable);
        }


        private static ActivityArgument CloneArgument(ActivityArgument arg)
        {
            var newArg = Entity.Create(arg.TypeIds).As<ActivityArgument>();
            newArg.Name = arg.Name;
            newArg.Description = arg.Description;

            // deal with type specific info.
            var resArg = arg.As<ResourceArgument>();
            if (resArg != null)
                newArg.As<ResourceArgument>().ConformsToType = resArg.ConformsToType;
            return newArg;
        }

        public static Workflow AddDefaultExitPoint(this Workflow wf)
        {
            var ep = Entity.Create<ExitPoint>();
            ep.Name = "Generated Default";
            ep.IsDefaultExitPoint = true;
            wf.ExitPoints.Add(ep);

            return wf;
        }

        public static ExitPoint GetNamedExitPoint(this WfActivity activity, string name)
        {
            ExitPoint result = null;

            var actAsDyn = activity.As<EntityWithArgsAndExits>();

            if (actAsDyn != null)
                result = actAsDyn.ExitPoints.FirstOrDefault(ep => ep.Name == name);

            if (result != null)
                return result;
            else
                return activity.IsOfType.Select(t => t.As<EntityWithArgsAndExits>()).First(t=>t != null).ExitPoints.FirstOrDefault(ep => ep.Name == name);
        }

        /// <summary>
        /// Create a proxy for a workflow for embedding within another workflow
        /// </summary>
        /// <param name="workflow"></param>
        public static WorkflowProxy CreateWorkflowProxy(Workflow workflow)
        {
            var proxy = new WorkflowProxy() {Name = workflow.Name};

            proxy.WorkflowToProxy = workflow;

            foreach (var arg in workflow.InputArguments)
                proxy.InputArguments.Add(CloneArgument(arg));

            foreach (var arg in workflow.OutputArguments)
                proxy.OutputArguments.Add(CloneArgument(arg));

            foreach (var exitPoint in workflow.ExitPoints)
            {
                var newEp = Entity.Create<ExitPoint>();
                newEp.Name = exitPoint.Name;
                newEp.Description = exitPoint.Description;
                newEp.IsDefaultExitPoint = exitPoint.IsDefaultExitPoint;
                proxy.ExitPoints.Add(newEp);

            }

            return proxy;
        }
    }
}
