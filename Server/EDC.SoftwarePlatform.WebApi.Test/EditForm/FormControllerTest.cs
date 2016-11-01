// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Net;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using System.IO;
using System.Linq;
using EDC.SoftwarePlatform.WebApi.Controllers.EditForm;

namespace EDC.SoftwarePlatform.WebApi.Test.EditForm
{
	/// <summary>
	/// </summary>
	[TestFixture]
	public class FormControllerTests
	{
		/// <summary>
		///     Tests the get image.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void GetForm_Returns404ForUnknownAlias( )
		{
			using (
				var request = new PlatformHttpRequest( @"data/v1/form/blah/blah" ) )
			{
				HttpWebResponse response = request.GetResponse( );

				Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.NotFound ) );
			}
		}

		/// <summary>
		///     Tests the get image.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void GetForm_Returns404ForUnknownId( )
		{
			using (
				var request = new PlatformHttpRequest( @"data/v1/form/12345678" ) )
			{
				HttpWebResponse response = request.GetResponse( );

				Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.NotFound ) );
			}
		}

		/// <summary>
		///     Tests the get image.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestGetForm_ByAlias( )
		{
			using (
				var request = new PlatformHttpRequest( @"data/v1/form/test/allFieldsForm" ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );
			}
		}

		/// <summary>
		///     Tests the get image.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestGetForm_ById( )
		{
			var employeeType = CodeNameResolver.GetTypeByName( "AA_Employee" ).As<EntityType>( );
			var employeeForm = employeeType.DefaultEditForm;

			using (
				var request = new PlatformHttpRequest( string.Format( @"data/v1/form/{0}", employeeForm.Id ) ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );
			}
        }

        /// <summary>
        ///     Tests the get image.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetForm_DoesNotReturnCalculations()
        {
            var calcTestType = CodeNameResolver.GetTypeByName("AA_Calculations").As<EntityType>();
            var calcTestForm = calcTestType.DefaultEditForm;

            using (
                var request = new PlatformHttpRequest(string.Format(@"data/v1/form/{0}", calcTestForm.Id)))
            {
                HttpWebResponse response = request.GetResponse();

                // check that it worked (200)
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                using (Stream stream = response.GetResponseStream())
                using (TextReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    Assert.That(json, Contains.Substring("isCalculatedField"));
                    Assert.That(json, Is.Not.ContainsSubstring("fieldCalculation"));

                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestGetFormData()
        {
            var employeeType = CodeNameResolver.GetTypeByName("AA_Employee").As<EntityType>();
            var employeeForm = employeeType.DefaultEditForm;
            var employee = Entity.GetInstancesOfType(employeeType).FirstOrDefault();

            using (var request = new PlatformHttpRequest(@"data/v1/form/data", PlatformHttpMethod.Post))
            {
                FormDataRequest formDataRequest = new FormDataRequest
                {
                    FormId = employeeForm.Id.ToString(),
                    EntityId = employee.Id.ToString(),
                    Query = "name, description"
                };

                request.PopulateBody(formDataRequest);

                HttpWebResponse response = request.GetResponse();

                var formDataResponse = request.DeserialiseResponseBody<FormDataResponse>();
                
                Assert.IsNotNull(formDataResponse);

                // check that it worked (200)
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));                
            }
        }
    }
}