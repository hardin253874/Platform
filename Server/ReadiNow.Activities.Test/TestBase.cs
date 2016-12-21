// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using Autofac;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities.Test
{
	public class TestBase
	{
		#region Initialize / Cleanup

		/// <summary>
		///     Performs any special cleanup after running each test.
		/// </summary>
		[TestFixtureTearDown]
		public void TestFinalize( )
		{
			using ( new TenantAdministratorContext( "EDC" ) )
			{
                DateTime start = DateTime.Now;

                var deleteList = ToDelete.Distinct().ToList();

				if ( deleteList.Count > 0 )
				{
				    try
				    {
                        Entity.Delete(deleteList);
				    }
                    // ReSharper disable once EmptyGeneralCatchClause
				    catch
				    {
                        // This can sometimes fail due to test errors or dirty data
				    }
				}

                Console.WriteLine("Deleted took: " + (DateTime.Now - start) + "  Items deleted: " + deleteList.Count());
			}
		}

		#endregion

		protected readonly List<long> ToDelete = new List<long>( );

        public static WorkflowRun RunWorkflow(Workflow workflowToRun, Dictionary<string, object> args = null, WorkflowRun parentRun = null, bool trace = false)
        {
            if (args == null)
                args = new Dictionary<string, object>();

            WorkflowRun run;

            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                run = WorkflowRunner.Instance.RunWorkflow(new WorkflowStartEvent(workflowToRun) { Arguments = args, ParentRun = parentRun, Trace = trace });
            }

            return Entity.Get<WorkflowRun>(run);
        }

        public static bool WaitForWorkflowToStop(WorkflowRun run, int timeoutMs = 2 * 1000, int pauseMs = 200, int leadTimeoutMs = 2 * 1000, int leadPauseMs = 200)
        {
            if (run == null)
                throw new ArgumentNullException("run");

            if (run.WorkflowBeingRun == null)
                throw new ArgumentException("No valid workflow being run.");

            var taskManager = Factory.WorkflowRunTaskManager;

            string taskId = run.TaskId;
            
            var started = SpinWait.SpinUntil(() =>
            {
                Thread.Sleep(leadPauseMs);
                return taskManager.HasStarted(taskId);
            }, leadTimeoutMs);

            if (!started)
                throw new Exception(string.Format("No task found started for workflow '{1}' with this id. [{0}]", taskId, run.WorkflowBeingRun == null ? run.Id : run.WorkflowBeingRun.Id));

            var result = SpinWait.SpinUntil(() =>
            {
                Thread.Sleep(pauseMs);
                return taskManager.HasCompleted(taskId);
            }, timeoutMs);

            taskManager.Clear(taskId);

            return result;
        }


		private static void AddFieldToType( ActivityType activity )
		{
			var inputArgs = new List<ActivityArgument>
				{
					( new IntegerArgument
						{
							Name = "InIntArg1"
						} ).Cast<ActivityArgument>( ),
					( new StringArgument
						{
							Name = "InStringArg1"
						} ).Cast<ActivityArgument>( )
				};

			activity.InputArguments.AddRange( inputArgs );

			var outputArgs = new List<ActivityArgument>
				{
					( new IntegerArgument
						{
							Name = "OutIntArg1"
						} ).Cast<ActivityArgument>( ),
					( new StringArgument
						{
							Name = "OutStringArg1"
						} ).Cast<ActivityArgument>( )
				};


			activity.OutputArguments.AddRange( outputArgs );
		}

		/// <summary>
		///     Create a class backed type ready for use
		/// </summary>
		/// <returns></returns>
		protected ActivityType CreateClassBackedType( Type backingClass )
		{
			var testType = new ActivityType( );

			testType.Inherits.Add( WfActivity.WfActivity_Type.As<EntityType>( ) );
            var exitPoint = Entity.Create<ExitPoint>();
            exitPoint.IsDefaultExitPoint = true;
			testType.ExitPoints.Add(exitPoint);
			testType.Save( );

			if ( backingClass.AssemblyQualifiedName != null )
			{
				testType.ActivityExecutionClass = new Class
					{
						TypeName = backingClass.FullName,
						AssemblyName = backingClass.AssemblyQualifiedName.Split( ',' )[ 1 ]
					};
			}

			AddFieldToType( testType );

			testType.Save( ); // Types need to be saved before they can be instanciated from

			ToDelete.Add( testType.Id );

			return testType; // This shouldn't be necessary, but there seem to be bugs if I don't do this.
		}


	    protected Workflow CreateLoggingWorkflow()
	    {
	        return CreateLoggingWorkflow("Ignore");
	    }

	    protected Workflow CreateLoggingWorkflow( string message )
		{
            var workflow = new Workflow
            {
                Name = $"Test workflow:{message ?? ""}:{DateTime.Now}"
			};

			workflow.AddDefaultExitPoint().AddLog("log", message);

			return workflow;
		}



        protected Workflow CreateEditNameWorkflow()
        {
            var workflow = new Workflow
            {
                Name = "Test Edit Name Workflow " + DateTime.Now
            };

            var input = new ResourceArgument
				{
					Name = "in",
                    ConformsToType = Resource.Resource_Type
				};
            var inputAs = input.As<ActivityArgument>();


			workflow.InputArguments.Add( inputAs );
            workflow.InputArgumentForAction = inputAs;
            workflow
                .AddDefaultExitPoint()
                .AddUpdate("update", "in", new string[] { "in.Name", "in.Name + '1'" })
            ;

            return workflow;
        }

// ReSharper disable UnusedParameter.Global
		protected static IDictionary<string, object> RunActivity( ActivityImplementationBase activity, IDictionary<string, object> inputs, bool expectedToComplete = true )
// ReSharper restore UnusedParameter.Global
		{
            Assert.IsFalse(activity is WorkflowImplementation, "Dont use RunActivity to run a workflow, use WorkflowRunner.Instance.");



            var invoker = new WorkflowInvoker();

           // invoker.RunStateFactory = new TestRunStateFactory();
 
            var workflowRun = ActivityTestHelper.CreateWfRunForActivity(activity);

            workflowRun.Name = "Dummy Run" + DateTime.Now;


            var convertedInputs = new ActivityInputs(activity.ActivityInstance.GetInputArguments(), inputs);

            var factory = new TestRunStateFactory();


            // var runState = Factory.Current.Resolve<IRunStateFactory>().CreateRunState(new WorkflowMetadata(), workflowRun);     
             var runState = factory.CreateRunState(new WorkflowMetadata(), workflowRun);
            runState.CurrentActivity = activity.ActivityInstance;

            bool hasCompleted = activity.Execute(runState, convertedInputs);

            if (expectedToComplete && !hasCompleted)
            {
                Assert.Fail("Unfinished activity");
            }

		    return runState.GetResult(activity.ActivityInstance.GetOutputArguments());
		}

        public static Random Rand = new Random();

     
	}
}