// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace EDC.SoftwarePlatform.WebApi.Test.EditForm
{
    /// <summary>
    /// 
    /// </summary>
    public static class FormControllerTestHelper
    {
        /// <summary>
        /// Run a request to get the layout of a form.
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="expectedResponse"></param>
        public static void GetFormLayout( long formId, HttpStatusCode expectedResponse = HttpStatusCode.OK )
        {
            string uri = string.Format( @"data/v1/form/{0}", formId );

            using (
                var request = new PlatformHttpRequest( uri ) )
            {
                HttpWebResponse response = request.GetResponse( );

                Assert.That( response.StatusCode, Is.EqualTo( expectedResponse ) );
            }
        }
    }
}
