// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.EventHandler;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace EDC.SoftwarePlatform.WebApi.Test.EventHandler
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class EventhandlerControllerTests
    {
        /// <summary>
        ///     Tests the get image.
        /// </summary>
        [Test]      
        [RunAsDefaultTenant]
        public void TestPostEventHandler()
        {          
            using (var request = new PlatformHttpRequest(@"data/v1/eventhandler/postevent", PlatformHttpMethod.Post))
            {                
                request.PopulateBody(new EventData { Message = "test message" });                
                HttpWebResponse response = request.GetResponse();

                // check that it worked (201)
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
            }

        }
    }
}
