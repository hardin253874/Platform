// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test
{
    /// <summary>
    /// Helpers for testing entities.
    /// </summary>
	public class TestEntity
    {
        public static JsonQueryResult RunBatchTest(string query, HttpStatusCode expectedResult = HttpStatusCode.OK, int resultCount = 1)
	    {
            using (var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post))
            {
                request.PopulateBodyString(query);

                var response = request.GetResponse();

                // check that it worked (200)
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Check response body
                JsonQueryResult res = request.DeserialiseResponseBody<JsonQueryResult>();
                Assert.IsNotNull(res);

                Assert.AreEqual(resultCount, res.Results.Count, "res.Results.Count");
                Assert.AreEqual(expectedResult, res.Results[0].Code, "res.Results[0].Code");

                return res;
            }
	    }
    }
}