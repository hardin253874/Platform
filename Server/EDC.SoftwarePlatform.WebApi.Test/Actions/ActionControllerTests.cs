// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Services.Console;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using System.Net;

namespace EDC.SoftwarePlatform.WebApi.Test.Actions
{
    [TestFixture]
    class ActionControllerTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test001Get()
        {
            var someId = Entity.GetId("test", "peterAylett");

            var actionRequest = new ActionRequest {SelectedResourceIds = new[] {someId}};
            using (var request = new PlatformHttpRequest("data/v1/actions", PlatformHttpMethod.Post))
            {
                request.PopulateBody(actionRequest);
                HttpWebResponse response = request.GetResponse();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
        }


        [Test]
        [RunAsDefaultTenant]
        public void TestActionsMenuNoCreateAccessMultipleTypes()
        {            
            var actionRequest = new ActionRequest
            {                
                ReportId = Entity.GetByName<ReadiNow.Model.Report>("Staff Report").FirstOrDefault().Id,
                ActionDisplayContext = ActionContext.ActionsMenu
            };

            var userAccount = Entity.GetByName<UserAccount>("Nelle.Odom").FirstOrDefault();
            using (var request = new PlatformHttpRequest("data/v1/actions", PlatformHttpMethod.Post, userAccount))
            {
                request.PopulateBody(actionRequest);
                HttpWebResponse response = request.GetResponse();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                var actionResponse = request.DeserialiseResponseBody<ActionResponse>();
                Assert.IsTrue(actionResponse.Actions.All(a => a.HtmlActionState != "newHolder"));
                Assert.IsTrue(actionResponse.Actions.All(a => a.HtmlActionState != "createForm"));                
            }
        }
    }
}
