// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using EDC.ReadiNow.Utc;
namespace EDC.SoftwarePlatform.WebApiTestControllers.Controllers
{
    [RoutePrefix("data/v1/test/detecttimezonetest")]
    public class TestDetectTimeZoneController : ApiController
    {
        //*** Note: to test this from client, change the output directory to point to \bin 
        
        /// <summary>
        ///     Extracts the Olson time zone info from request query string and
        ///     gets the ms time zone name.
        /// </summary>
        [Route("mstzuri")]
        [HttpGet]
        public string GetMsTimeZoneNameUri()
        {
            NameValueCollection queryString = Request.RequestUri.ParseQueryString();

            string olsonTzName = queryString["tz"];
            string retVal = string.Empty;

            if (!string.IsNullOrEmpty(olsonTzName))
            {
                string mstzId = TimeZoneHelper.GetMsTimeZoneName(olsonTzName);
                string systemZoneName = TimeZoneInfo.FindSystemTimeZoneById(mstzId).DisplayName;

                retVal = string.Format("MS TimeZoneId : {0}; System Timezone Display name : {1}", mstzId, systemZoneName);
            }

            return retVal;
        }

        /// <summary>
        ///     Extracts the Olson time zone info from request header and
        ///     gets the ms time zone name.
        /// </summary>
        /// <returns></returns>
        [Route("mstzheader")]
        [HttpGet]
        public string GetMsTimeZoneNameHeader()
        {
            string olsonTzName = HttpContext.Current.Request.Headers.Get("Tz");
            string retVal = string.Empty;

            if (!string.IsNullOrEmpty(olsonTzName))
            {
                string mstzId = TimeZoneHelper.GetMsTimeZoneName(olsonTzName);
                string systemZoneName = TimeZoneInfo.FindSystemTimeZoneById(mstzId).DisplayName;

                retVal = string.Format("MS TimeZoneId : {0}; System Timezone Display name : {1}", mstzId, systemZoneName);
            }

            return retVal;
        }
    }
}
