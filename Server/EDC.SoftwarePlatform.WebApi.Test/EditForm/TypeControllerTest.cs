// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Linq;
using System.Net;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.EditForm
{
	/// <summary>
	/// </summary>
	[TestFixture]
	[RunAsDefaultTenant]
	public class TypeControllerTests
	{
		private long ReturnedFormId( JsonQueryResult result )
		{
			return result.Results.First( ).Ids.First( );
		}

		[Test]
		public void TestGetInstance_DefaultForm_ByAlias( )
		{
			var expectedForm = Entity.Get<Definition>( new EntityRef( "test", "employee" ) ).DefaultEditForm;
			using ( var request = new PlatformHttpRequest( @"data/v1/type/test/employee/defaultForm" ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );

				var result = request.DeserialiseResponseBody<JsonQueryResult>( );

				Assert.AreEqual( expectedForm.Id, ReturnedFormId( result ), "The form is the expected one" );
			}
		}

		[Test]
		public void TestGetInstance_DefaultFromForTypeWithNoForm( )
		{
			using ( var request = new PlatformHttpRequest( @"data/v1/type/core/logActivityLogEntry/defaultForm" ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );

				var result = request.DeserialiseResponseBody<JsonQueryResult>( );

				Assert.IsTrue( 9999 < ReturnedFormId( result ), "The form is a generated one" );
			}
		}

		[Test]
		public void TestGetInstance_ForceGenerateForm( )
		{
			var expectedForm = Entity.Get<Definition>( new EntityRef( "test", "employee" ) ).DefaultEditForm;
			using ( var request = new PlatformHttpRequest( @"data/v1/type/test/employee/defaultForm" ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.AreEqual( HttpStatusCode.OK, response.StatusCode );

				var result = request.DeserialiseResponseBody<JsonQueryResult>( );

				Assert.AreEqual( expectedForm.Id, ReturnedFormId( result ), "The form is a generated one" );
			}
		}

		[Test]
		public void TestGetInstance_GenerateForm_ByAlias( )
		{
			using (
				var request = new PlatformHttpRequest( @"data/v1/type/test/employee/generateform" ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );
			}
		}


		[Test]
		public void TestGetType_DefaultForm_ById( )
		{
			var aaemployee = Entity.Get( new EntityRef( "test", "employee" ) );

			using (
				var request = new PlatformHttpRequest( string.Format( @"data/v1/type/{0}/defaultForm", aaemployee.Id ) ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );
			}
		}

		[Test]
		public void TestGetType_GenerateForm_ById( )
		{
			var aaemployee = Entity.Get( new EntityRef( "test", "employee" ) );

			using (
				var request = new PlatformHttpRequest( string.Format( @"data/v1/type/{0}/generateform", aaemployee.Id ) ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );
			}
		}
	}
}