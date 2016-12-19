// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
//using System.Activities;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
    //[Category("ExtendedTests")]
    [Category("WorkflowTests")]
	public class ActivitiesTest
	{
		[Test]
		[RunAsDefaultTenant]
		public void EnsureAllActivityTypesHaveOneDefaultExitPoint( )
		{
            var dynamicActivites = new List<string> { "core:approvalActivity", "core:reviewSurveyActivity", "core:workflow", "core:workflowProxy" };

            IEnumerable<ActivityType> activityTypes = Entity.GetInstancesOfType<ActivityType>( );

			foreach ( ActivityType activityType in activityTypes )
			{
				if ( !( activityType.IsAbstract ?? false ) && !dynamicActivites.Contains(activityType.Alias)) // we are only interesting in installed types that are not workflow
				{
					IEnumerable<ExitPoint> defaultExitPoints = activityType.ExitPoints.Where( ep => ep.IsDefaultExitPoint ?? false );
					Assert.AreEqual( 1, defaultExitPoints.Count( ), string.Format( "ActivityType {0} does not have exactly one default exit point.", activityType.Name ) );
				}
			}
		}

        //[Test]
        //[RunAsDefaultTenant]
        //public void EvaluateExpression( )
        //{
        //    var input = new Dictionary<string, object>
        //        {
        //            {
        //                "a", 1
        //            },
        //            {
        //                "b b", 1
        //            }
        //        };

        //    Assert.AreEqual( 2, ExpressionHelper.EvaluateExpressionImpl( ExprType.Int32, "@a + @[b b]", input, new WorkflowRun() ) );

        //    input = new Dictionary<string, object>
        //        {
        //            {
        //                "one", 1.0M
        //            },
        //            {
        //                "two", 2.0M
        //            }
        //        };

        //    Assert.AreEqual( 3, ExpressionHelper.EvaluateExpressionImpl( ExprType.Decimal, "@one + @two", input, new WorkflowRun() ) );

        //    input = new Dictionary<string, object>( );

        //    Assert.AreEqual(1.0M, ExpressionHelper.EvaluateExpressionImpl(ExprType.Decimal, "1.0", input, new WorkflowRun()));
        //}
	}
}