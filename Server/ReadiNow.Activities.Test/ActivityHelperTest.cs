// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class ActivityHelperTest : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
		public void Test_AddExpressionParameter_unique_names_are_generated( )
		{
			var wf = Entity.Create<Workflow>( );
			var log = Entity.Create<WfActivity>( );

			wf.ContainedActivities.Add( log );

			var messageArg = Entity.Get<ActivityArgument>( "inLogActivityMessage" );

			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );
			ActivityTestHelper.CreateArgumentInstance( wf, log, messageArg );

			Assert.AreEqual( 12, wf.ExpressionParameters.Count( ), "Ensure all the parameters were added." );
			Assert.AreEqual(
				wf.ExpressionParameters.Count( ),
				wf.ExpressionParameters.Select( p => p.Name ).Distinct( ).Count( ),
				"Ensure every parameter has a unique name" );
		}
	}
}