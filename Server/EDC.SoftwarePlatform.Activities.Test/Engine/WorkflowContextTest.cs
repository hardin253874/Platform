// Copyright 2011-2016 Global Software Innovation Pty Ltd
//using System.Collections.Generic;
//using System.Linq;
//using EDC.ReadiNow.Model;
//using EDC.ReadiNow.Silverlight.Test.Services.Metadata;
//using EDC.ReadiNow.Test;
//using EDC.SoftwarePlatform.Activities.Engine.Events;
//using NUnit.Framework;

//namespace EDC.SoftwarePlatform.Activities.Test.Engine
//{
//    [TestFixture]
//    public class WorkflowContextTest : TestBase
//    {
//        private void CreateContext( out WorkflowInvoker invoker, out WorkflowMetadata metadata, out WorkflowContext context )
//        {
//            invoker = new WorkflowInvoker( );
//            metadata = new WorkflowMetadata( );

//            ActivityType dummyActivityType = CreateClassBackedType( typeof ( DummyInvokerActivityImplementation ) );

//            var dummyActivity = new Entity( dummyActivityType.Id ).Cast<WfActivity>( );


//            var dummyImp = new DummyInvokerActivityImplementation( );
//            dummyImp.Initialize( dummyActivity, CommonServiceTestsHelper.TestDefaultContextData );
//            dummyImp.AddMetadata( metadata );

//            Assert.AreEqual( 0, metadata.ValidationMessages.Count, "Ensure there are no validation errors." );

//            var run = new WorkflowRun();
//            context = new WorkflowContext( metadata, invoker, run.Id );
//        }

//        // Create a dummyActivity nested within a workflow with an argument
//        private void CreateNestedContext( out WorkflowInvoker invoker, out WorkflowMetadata metadata, out WorkflowContext context )
//        {
//            invoker = new WorkflowInvoker( );
//            metadata = new WorkflowMetadata( );

//            ActivityType dummyActivityType = CreateClassBackedType( typeof ( DummyInvokerActivityImplementation ) );
//            var dummyActivity = new Entity( dummyActivityType.Id ).Cast<WfActivity>( );

//            var wfActivity = new Workflow( );
//            var wfActicityAs = wfActivity.As<WfActivity>( );
//            var wfVar = new IntegerArgument
//                {
//                    Name = "wfVar"
//                };
//            wfActivity.Variables.Add( wfVar.Cast<ActivityArgument>( ) );
//            wfActivity.FirstActivity = dummyActivity;
//            wfActivity.ContainedActivities.Add( dummyActivity );

//            ActivityHelper.AddMissingExpressionParametersToWorkflow( wfActivity );
//            wfActivity.Save( );
//            ToDelete.Add( wfActivity.Id );

//            ActivityImplementationBase wfImp = wfActivity.Cast<WfActivity>( ).CreateWindowsActivity( );

//            wfImp.Initialize( wfActicityAs, CommonServiceTestsHelper.TestDefaultContextData );
//            wfImp.AddMetadata( metadata );

//            Assert.AreEqual( 0, metadata.ValidationMessages.Count, "Ensure there are no validation errors." );

//            var run = new WorkflowRun();

//            context = new WorkflowContext( metadata, invoker, run.Id );
//        }


//        private class DummyInvokerActivityImplementation : ActivityImplementationBase, IResumableActivity
//        {
//            private Variable<long> _dummyMetaData;

//            bool IResumableActivity.OnStart( WorkflowContext context, Dictionary<string, object> inputs )
//            {
//                _dummyMetaData.Set( context, 77L );
//                //context.Properties.Add("dummy", "dummy");

//                return false;
//            }

//            bool IResumableActivity.OnResume( WorkflowContext context, EventBase resumeEvent )
//            {
//                Assert.IsNotNull( context.WorkflowInvoker, "Ensure the workflow invoker has been restored after resume." );
//                Assert.IsNotNull( context.Metadata, "Ensure metadata is not null after restore." );

//                // var dummyValue = (string) context.FindProperty("dummy");

//                // Assert.AreEqual("dummy", dummyValue, "Ensure properties are preserved after resume");

//                var dummyMetaDataValue = ( int ) _dummyMetaData.Get( context );

//                Assert.AreEqual( 77L, _dummyMetaData.Get( context ), "Ensure variables are preserved after resume" );

//                _dummyMetaData.Set( context, dummyMetaDataValue + 1 );

//                context.Result = new Dictionary<string, object>( );

//                return true;
//            }

//            public override bool AddMetadata(WorkflowMetadata metadata)
//            {
//                if (base.AddMetadata(metadata))
//                {

//                    _dummyMetaData = new Variable<long>(999, "dummyMetaData");
//                    metadata.AddVariable(_dummyMetaData);

//                    return true;

//                }
//                else
//                {
//                    return false;
//                }
//            }
//        }

//        [Test]
//        [Description( "This test reproduces the (rather involved) process of creating a context for an activity." )]
//        [RunAsGlobalTenant]
//        public void CreateContextTest( )
//        {
//            WorkflowInvoker invoker;
//            WorkflowMetadata metadata;
//            WorkflowContext context;

//            CreateContext( out invoker, out metadata, out context );
//        }

//        //[Test]
//        //[Description( "This test ensures that persisting and resuming a context preserves both properties and variablevalues." )]
//        //[RunAsGlobalTenant]
//        //public void PersistContextTest( )
//        //{
//        //    WorkflowInvoker invoker;
//        //    WorkflowMetadata metadata;
//        //    WorkflowContext context;

//        //    CreateContext( out invoker, out metadata, out context );

//        //    var run = new WorkflowRun( );
//        //    run.Save( );
//        //    ToDelete.Add( run.Id );


//        //    context.SetVariableValue( metadata.Variables.First( ), 10 );

//        //    Assert.AreEqual( 10, metadata.Variables.First( ).GetValue( context ), "Ensure variable saved into context" );

//        //    var intArg = new IntegerArgument( ).As<ActivityArgument>( );
//        //    intArg.Save( );
//        //    ToDelete.Add( intArg.Id );

//        //    var resArg = new ResourceArgument( ).As<ActivityArgument>( );
//        //    resArg.Save( );
//        //    ToDelete.Add( resArg.Id );

//        //    var resListArg = new ResourceListArgument( ).As<ActivityArgument>( );
//        //    resListArg.Save( );
//        //    ToDelete.Add( resListArg.Id );

//        //    ArgumentValueStore.SetRuntimeProperty( context, intArg, 7 );
//        //    ArgumentValueStore.SetRuntimeProperty( context, resArg, ( EntityRef ) resArg );
//        //    ArgumentValueStore.SetRuntimeProperty( context, resListArg, new List<EntityRef>
//        //        {
//        //            resListArg,
//        //            intArg
//        //        } );

//        //    invoker.SaveWorkflowContextToStorage( run.Id, context );

//        //    WorkflowContext retrievedContext = invoker.GetWorkflowContextFromStorage( metadata, run.Id );

//        //    Assert.AreEqual( 10, metadata.Variables.First( ).GetValue( retrievedContext ), "Ensure variable persisted into context" );

//        //    object o;
//        //    Assert.IsTrue( ArgumentValueStore.TryGetRuntimeProperties( retrievedContext, intArg, out o ), "Integer Argument value retrieved." );
//        //    Assert.AreEqual( 7, ( int ) o, "Integer Mapping store value correct" );

//        //    Assert.IsTrue( ArgumentValueStore.TryGetRuntimeProperties( retrievedContext, resArg, out o ), "Resource Argument value retrieved." );
//        //    Assert.AreEqual( ( ( EntityRef ) resArg ).Id, ( ( EntityRef ) o ).Id, "Resource Mapping store value correct" );

//        //    Assert.IsTrue( ArgumentValueStore.TryGetRuntimeProperties( retrievedContext, resListArg, out o ), "Resource List Argument value retrieved." );
//        //    Assert.AreEqual( 2, ( ( List<EntityRef> ) o ).Count( ), "Resource List Mapping store value correct" );
//        //}


//        [Test]
//        [Description( "This test ensures that persisting a nexted context preserves properties and variablevalues from both levels." )]
//        [RunAsGlobalTenant]
//        public void PersistNestedContextTest( )
//        {
//            WorkflowInvoker invoker;
//            WorkflowMetadata metadata;
//            WorkflowContext context;

//            CreateNestedContext( out invoker, out metadata, out context );

//            var run = new WorkflowRun( );
//            run.Save( );
//            ToDelete.Add( run.Id );

//            context.SetVariableValue( metadata.Variables.First( ), 10 );

//            Assert.AreEqual( 10, metadata.Variables.First( ).GetValue( context ), "Ensure variable saved into context" );

//            context.Push( );

//            Assert.AreNotEqual( 10, metadata.Variables.First( ).GetValue( context ), "Ensure variable can not be read from lower context" );


//            context.SetVariableValue( context.Metadata.Variables.ElementAt( 2 ), 110 );

//            Assert.AreEqual( 110, context.Metadata.Variables.ElementAt( 2 ).GetValue( context ), "Ensure variable saved into child context" );


//            invoker.SaveWorkflowContextToStorage( run.Id, context );

//            WorkflowContext retrievedContext = invoker.GetWorkflowContextFromStorage( metadata, run.Id );

//            Assert.AreEqual( 110, retrievedContext.Metadata.Variables.ElementAt( 2 ).GetValue( retrievedContext ), "Ensure variable persisted into child context" );

//            retrievedContext.Pop( );
//            Assert.AreEqual( 10, retrievedContext.Metadata.Variables.First( ).GetValue( retrievedContext ), "Ensure variable persisted into context" );
//        }

//        [Test]
//        [RunAsGlobalTenant]
//        public void TestContextPauseResume( )
//        {
//            ActivityType dummyAction = CreateClassBackedType( typeof ( DummyInvokerActivityImplementation ) );


//            var innerWf = new Workflow
//                {
//                    Name = "Test Wf"
//                };
//            innerWf.AddDefaultExitPoint( );

//            var l1 = new Entity( dummyAction.Id ).Cast<WfActivity>( );
//            l1.Name = "inner testType l1";

//            ActivityHelper.AddFirstActivityWithMapping(
//                innerWf,
//                l1, null );

//            ActivityHelper.AddTermination(
//                innerWf,
//                l1 );

//            innerWf.Save( );
//            ToDelete.Add( innerWf.Id );

//            WorkflowRun run = WorkflowRunHelper.RunWorkflow( innerWf, new Dictionary<string, object>( ) );

//            Assert.AreEqual( WorkflowRunState_Enumeration.WorkflowRunPaused, run.WorkflowRunStatus_Enum );

//            run = WorkflowRunHelper.ResumeWorkflow(run, new TimeoutEvent( ));

//            Assert.AreEqual( WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum );
//        }
//    }
//}