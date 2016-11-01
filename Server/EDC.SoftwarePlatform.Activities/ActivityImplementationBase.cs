// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using System.Diagnostics;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities
{


    /// <summary>
    /// A default implementation of an activity. Outputs are in the implicit "Result" argument.
    /// </summary>
    public abstract class ActivityImplementationBase 
    {
        public WfActivity ActivityInstance { get; set; }


        /// <summary>
        /// The ID of the workflow run this activity is part of.
        /// </summary>
        public long GetWorkflowRunId(IRunState runState)
        {
            return runState.WorkflowRunId;
        }



        public virtual void Initialize(WfActivity _instance)
        {
            //TODO: I don't like this method, should see if it can be factored out
            ActivityInstance = _instance;
        }


       
        /// <summary>
        /// Validate the activity, adding any validation errors to the metadata
        /// </summary>
        /// <param name="metadata"></param>
        public virtual void Validate(WorkflowMetadata metadata)
        {
            using (Profiler.Measure("ActivityImplementationBase.Validate"))
            {
                var manditoryArgs = ActivityInstance.GetInputArguments().Where(arg => arg.ArgumentIsMandatory ?? false);

                // we can't validate inputs unless we are part of a workflow
                if (ActivityInstance.ContainingWorkflow == null)
                    return;

                foreach (var manditoryArg in manditoryArgs)
                {
                    var expression = metadata.GetPopulatedBy(ActivityInstance, manditoryArg);

                    if (expression == null)
                    {
                        metadata.AddValidationError(
                            string.Format(
                                "Mandatory arguments must be given a value. Activity: '{0}'  Argument: '{1}'",
                                ActivityInstance.Name, manditoryArg.Name));
                    }
                }
            }
        }



        /// <summary>
        /// CALLED FROM INVOKER. Run the activity in the provided context. Used to execute an activity within a workflow. If this activity metadata has not been added to the context it 
        /// will be done now.
        /// </summary>
        /// <returns>True if completed, false if bookmarked.</returns>
        public bool RunInContext(IRunState runState, ActivityInputs inputs)
        {
            return Execute(runState, inputs);
        }

        /// <summary>
        /// Execute an activity, return true if it completed. Fase if bookmarked
        /// </summary>
        /// <returns>True if the activity completed.</returns>
        public virtual bool Execute(IRunState runState, ActivityInputs inputs)
        {
            using (Profiler.Measure("ActivityImplementationBase.Execute " + GetType().Name))
            {
                bool hasCompleted = true;

                if (ActivityInstance == null)
                    throw new Exception("IEdc activities need to be initalized before being executed.");

                var thisAsRunNow = this as IRunNowActivity;

                if (thisAsRunNow != null)
                {
                    //
                    // Run Now
                    //
                    thisAsRunNow.OnRunNow(runState, inputs);

                    hasCompleted = true;
                }
                else
                {
                    //
                    // Resumable
                    //
                    var thisAsResumable = this as IResumableActivity;

                    if (thisAsResumable == null)
                        throw new ApplicationException("We have a non resumable non num now activity. This should never occur.");

                    hasCompleted = thisAsResumable.OnStart(runState, inputs);
                }

                if (hasCompleted && runState.ExitPointId == null)
                {
                    runState.ExitPointId = ActivityInstance.GetDefaultExitPoint();
                }

                return hasCompleted;
            }
        }





        /// <summary>
        /// CALLED FROM INVOKER. Run the activity in the provided context. Used to execute an activity within a workflow. If this activity metadata has not been added to the context it 
        /// will be done now.
        /// </summary>
        /// <returns>True if completed, false if bookmarked.</returns>
        public bool ResumeInContext(IRunState runState, IWorkflowEvent resumeEvent)
        {

            bool hasCompleted = true;

            var resumable = this as IResumableActivity;

            if (resumable == null)
                throw new ApplicationException("Attempted to resume an activity that is not resumable. This should never happen.");

            hasCompleted = resumable.OnResume(runState, resumeEvent);

            if (hasCompleted && runState.ExitPointId == null)
            {
                runState.ExitPointId = ActivityInstance.GetDefaultExitPoint();
            }

            return hasCompleted;
        }


        virtual public ICollection<WfExpression> GetInputArgumentsExpressions(IRunState runState, WfActivity activity)
        {

            var inputArgs = activity.GetInputArguments();

            // Intentionally return an ICollection to ensure the expressions are loaded and evaluated in an elevated context
            var result = activity.ExpressionMap
                                 .Where(ex => inputArgs.Any(arg => ex.ArgumentToPopulate != null && arg.Id == ex.ArgumentToPopulate.Id))
                                 .ToList();
            return result;
        }


        /// <summary>
        /// Given an argument alias, get the key used in the input dictionary
        /// </summary>
        protected ActivityArgument GetArgumentKey(string paramAlias)
        {
            var arg = Entity.Get<ActivityArgument>(paramAlias, Resource.Name_Field);

            return arg;
            
        }

        /// <summary>
        /// Given an argument alias, get the key used in the input dictionary
        /// </summary>
        protected string GetArgumentKeyName(string paramAlias)
        {
            return Entity.Get<Resource>(paramAlias, Resource.Name_Field).Name;
        }

        /// <summary>
        /// Give the argument alias, return the input or the default
        /// </summary>
        protected T GetArgumentValue<T>(ActivityInputs input, string paramAlias, T defaultValue)
        {
            object o;
            if (input.TryGetValue(GetArgumentKey(paramAlias), out o))
            {
                if (o == null)
                    return defaultValue;
                return (T) o;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Give the argument alias, return the input, throw an exception if the input is undefined
        /// </summary>
        protected T GetArgumentValue<T>(ActivityInputs input, string paramAlias) where T : class
        {
            object o = GetArgumentValue_imp(input, paramAlias);

            if (o == null)
               return default(T);
            else
            {
               return (T)o;
            }
        }


        /// <summary>
        /// Give the argument alias, return the input, throw an exception if the input is undefined
        /// </summary>
        protected T GetArgumentEntity<T>(ActivityInputs input, string paramAlias) where T : class, IEntity
        {
            var o = (IEntity)GetArgumentValue_imp(input, paramAlias);

            if (o == null)
                return default(T);
            else
            {
                return o.Cast<T>();
            }
        }

        /// <summary>
        /// Get a list of entities. IF there is only one entity present, turn it into a list.
        /// If there isn't anything present, return an empty list.
        /// </summary>
        protected IEnumerable<IEntity> GetArgumentEntityList(ActivityInputs input, string paramAlias)
        {
            var o = GetArgumentValue_imp(input, paramAlias);
            
            if (o == null)
                return Enumerable.Empty<IEntity>();

            var l = o as IEnumerable<IEntity>;

            if (l != null)
            {
                return l;
            }
            else
            {
                // If it is a single entity, turn it into a list
                return GetArgumentEntity<IEntity>(input, paramAlias).ToEnumerable();
            } 
        }

        /// <summary>
        /// Give the argument alias, return the input, throw an exception if the input is undefined
        /// </summary>
        protected T GetArgumentValueStruct<T>(ActivityInputs input, string paramAlias) where T : struct
        {
            object o = GetArgumentValue_imp(input, paramAlias);

            if (o == null)
               return default(T);

            return (T)o;
        }

        /// <summary>
        /// Give the argument alias, return the input, throw an exception if the input is undefined
        /// </summary>
        private object GetArgumentValue_imp(ActivityInputs input, string paramAlias) 
        {
            object o;
            if (input.TryGetValue(GetArgumentKey(paramAlias), out o))
            {
                return o;
            }
            else
            {
                var arg = Entity.Get<ActivityArgument>(paramAlias, ActivityArgument.Name_Field, ActivityArgument.ArgumentIsMandatory_Field);

                if (arg.ArgumentIsMandatory ?? false)
                    throw new ApplicationException(string.Format("Mandatory argument '{0}' is missing. This error should have been caught earlier in the code. This should never occur.", arg.Name));

                return null;
            }
        }




        /// <summary>
        /// Thrown when an attempt is made to get the value of an argument or variable that has never been defined.
        /// </summary>
        public class WorkflowUndefinedValueException : WorkflowRunException
        {
            public WorkflowUndefinedValueException(long workflowRunId, WfActivity activity, ActivityArgument arg)
                : base("A value has attempted to be retrieved from an argument or variable where none was provided: [{0}].[{1}]", activity.Name, arg.Name)
            { }
        }      
  
        
    }
}
