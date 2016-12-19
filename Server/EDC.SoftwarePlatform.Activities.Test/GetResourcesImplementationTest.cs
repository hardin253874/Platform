// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Expressions;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
    //[Category("ExtendedTests")]
    [Category("WorkflowTests")]
	public class GetResourcesImplementationTest : TestBase
	{
		[Test]
        [RunAsDefaultTenant]
		public void GetResourceByReport( )
		{
            var personReport = CodeNameResolver.GetInstance("AA_Person", "Report");

			var getResources = new GetResourcesActivity( );
			getResources.Save( );
			//_toDelete.Add(getResources.Id);

			var getResourcesAs = getResources.As<WfActivity>( );

			ActivityImplementationBase nextActivity = getResourcesAs.CreateWindowsActivity( );

			var args = new Dictionary<string, object>
				{
					{
						"Report", personReport
					},
				};

			IDictionary<string, object> result = RunActivity( nextActivity, args );

			var list = result[ "List" ] as IEnumerable<IEntity>;
			var first = result[ "First" ];

		    var personType = CodeNameResolver.GetTypeByName("AA_Person").As<EntityType>();

            long countPeople = personType.GetDescendantsAndSelf().SelectMany(t => t.InstancesOfType).Select(i => i.Id).Distinct().Count();
			Assert.IsNotNull( list );
			Assert.AreEqual( list.Count( ), countPeople, "The count of people is correct" );
		}

		[Test]
        [RunAsDefaultTenant]
		public void GetResourceByType( )
		{
			var getResources = new GetResourcesActivity( );
			getResources.Save( );

			var getResourcesAs = getResources.As<WfActivity>( );

			ActivityImplementationBase nextActivity = getResourcesAs.CreateWindowsActivity( );

			var args = new Dictionary<string, object>
				{
					{
						"Object",  TriggeredOnEnum.TriggeredOnEnum_Type
					},
				};

			IDictionary<string, object> result = RunActivity( nextActivity, args );

			var list = result[ "List" ] as IEnumerable<IEntity>;
			var first = result[ "First" ];

            int countFields = TriggeredOnEnum.TriggeredOnEnum_Type.GetDescendantsAndSelf().Select(t => t.InstancesOfType.Count()).Sum();
			Assert.IsNotNull( list );
            Assert.AreEqual(list.Count(), countFields, "The count of fields is correct");
		}
	}
}