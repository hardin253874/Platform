// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class ArgValueStoreTest : TestBase
	{
        //[Test]
        //[RunAsDefaultTenant]
        //public void SettingActivityArgument( )
        //{
        //    var run = new WorkflowRun();
        //    var context = new WorkflowContext( new WorkflowMetadata( ), new WorkflowInvoker( ), run.Id );

        //    var activity = new LogActivity( ).As<WfActivity>( );
        //    var arg = new IntegerArgument( ).As<ActivityArgument>( );
        //    var argInst = new WfArgumentInstance() {ArgumentInstanceActivity = activity, ArgumentInstanceArgument = arg};

        //    activity.Save( );
        //    ToDelete.Add( activity.Id );
        //    arg.Save( );
        //    ToDelete.Add( arg.Id );

        //    ArgumentValueStore.SetValue( context, argInst, 5 );

        //    var activity2 = Entity.Get<WfActivity>( activity.Id );
        //    var arg2 = Entity.Get<ActivityArgument>( arg.Id );

        //    object o;
        //    Assert.IsTrue( ArgumentValueStore.TryGetValue( context, activity2, arg2, out o ), "We have a value in the store" );
        //    Assert.AreEqual( 5, o );
        //}

        //[Test]
        //[RunAsDefaultTenant]
        //public void SettingWorkflowArgument( )
        //{
        //    var run = new WorkflowRun();

        //    var context = new WorkflowContext(new WorkflowMetadata(), new WorkflowInvoker(), run.Id);
            
        //    var arg = new IntegerArgument( ).As<ActivityArgument>( );
        //    arg.Save( );
        //    ToDelete.Add( arg.Id );
        //    ArgumentValueStore.SetRuntimeProperty( context, arg, 5 );

        //    var arg2 = new IntegerArgument( ).As<ActivityArgument>( );
        //    arg2.Save( );
        //    ToDelete.Add( arg2.Id );
        //    ArgumentValueStore.SetRuntimeProperty( context, arg2, 7 );

        //    object o;
        //    Assert.IsTrue( ArgumentValueStore.TryGetRuntimeProperties( context, arg, out o ), "We have a value in the store" );
        //    Assert.AreEqual( 5, o );
        //}
	}
}