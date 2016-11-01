// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Web.Http;
using EDC.Common;
using EDC.Exceptions;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using System.Collections.Generic;
using System.Net.Http;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using ReadiNow.Connector;
using ReadiNow.Integration.Sms;
using System.Net.Http.Headers;
using EDC.SoftwarePlatform.Activities.Approval;
using EDC.SoftwarePlatform.Activities;
using System.Linq;
using System.Text;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Integration
{
    /// <summary>
    ///     Controller for APIs used by twilio integrations.
    /// </summary>
    [RoutePrefix( "approval" )]
    public class ApproveController : ApiController
    {
        HtmlGenerator _htmlGenerator;

        /// <summary>
        /// Constructor
        /// </summary>
        public ApproveController( )
        {
            _htmlGenerator = new HtmlGenerator();
        }


        [Route("{tenant}/approve")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(
            [FromUri] string tenant,
            [FromUri] string token,
            [FromUri] string select = null
            )
        {
            if (!Factory.FeatureSwitch.Get("enableWfUserActionNotify"))
            {
               return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }

            using (Profiler.Measure("ApproveController.Approve"))
            {
                using (WebApiHelpers.GetTenantContext(tenant))
                {
                    string responseHtml = null;

                    var task = UserTaskHelper.GetTaskFromLinkToken(token);

                    if (task == null)
                    {
                        responseHtml = _htmlGenerator.GenerateRejectionPage();
                    }
                    else
                    {
                        if (select == null)
                        {
                            responseHtml = _htmlGenerator.GenerateSelectionPage(task);
                        }
                        else
                        {
                            // TODO: make the selection]
                            UserTaskHelper.ProcessApproval(task.Id, select);
                            responseHtml = _htmlGenerator.GenerateCompletedPage(task);
                        }
                    }

                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
                    response.Content = new StringContent(responseHtml, Encoding.UTF8,  "text/html");

                    return response;
                }
            }
        }
    }
}