// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Globalization;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class CreateLinkActivityTest : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
		public void GenerateEmployeeLink( )
		{
			var employee = new Person
				{
					Name = "Bob'"
				};
			employee.Save( );
			ToDelete.Add( employee.Id );

			var createLinkActivity = new CreateLinkActivity( );
			createLinkActivity.Save( );
			ToDelete.Add( createLinkActivity.Id );

			var nextActivity = ( CreateLinkImplementation ) createLinkActivity.As<WfActivity>( ).CreateWindowsActivity( );

			var inputs = new Dictionary<string, object>
				{
					{
						"Resource", employee
					}
				};

			IDictionary<string, object> result = RunActivity( nextActivity, inputs );

			var url = ( string ) result[ "URL" ];

			Assert.IsTrue( url.StartsWith( "https://" ), "Starts with Https" );
			Assert.IsTrue( url.Contains( employee.Id.ToString( CultureInfo.InvariantCulture ) ), "Url contains the correct Id" );

			var link = ( string ) result[ "Link" ];
			Assert.IsTrue( link.StartsWith( "<a href=" ), "Starts tag exists" );
			Assert.IsTrue( link.EndsWith( "</a>" ), "End tag exists" );
			Assert.IsTrue( link.Contains( employee.Id.ToString( CultureInfo.InvariantCulture ) ), "Link contains the correct Id" );
			Assert.IsTrue( link.Contains( "Bob&apos;" ), "Link contains the name" );
		}

		[Test]
		[RunAsDefaultTenant]
		public void GenerateReportLink( )
		{
            var report = Entity.Get<Report>( "test:personReport" );

			var createLinkActivity = new CreateLinkActivity( );
			createLinkActivity.Save( );
			ToDelete.Add( createLinkActivity.Id );

			var nextActivity = ( CreateLinkImplementation ) createLinkActivity.As<WfActivity>( ).CreateWindowsActivity( );

			var inputs = new Dictionary<string, object>
				{
					{
						"Resource", report
					}
				};

			IDictionary<string, object> result = RunActivity( nextActivity, inputs );

			var url = ( string ) result[ "URL" ];

			Assert.IsTrue( url.StartsWith( "https://" ), "Starts with Https" );
			Assert.IsTrue( url.Contains( report.Id.ToString( CultureInfo.InvariantCulture ) ), "Url contains the correct Id" );
		}

        [Test]
        [RunAsDefaultTenant]
        [ExpectedException(typeof(WorkflowRunException))]
        public void HandleNull()
        {
            var createLinkActivity = new CreateLinkActivity();
            createLinkActivity.Save();
            ToDelete.Add(createLinkActivity.Id);

            var nextActivity = (CreateLinkImplementation)createLinkActivity.As<WfActivity>().CreateWindowsActivity();

            var inputs = new Dictionary<string, object>
                {
                    {
                        "Resource", null
                    }
                };

            IDictionary<string, object> result = RunActivity(nextActivity, inputs);
        }
	}
}