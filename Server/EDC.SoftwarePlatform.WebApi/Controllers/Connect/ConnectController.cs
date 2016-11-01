// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Configuration;
using System.Web.Http;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Connect
{
    [RoutePrefix( "connect/v1" )]
    public class ConnectController : ApiController
    {
        [Route("ping")]
        [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public IHttpActionResult Ping()
        {
            if (!IsConnectorEnabled())
                return NotFound();

            return Ok();
        }

        private static bool IsConnectorEnabled()
        {
            return Convert.ToBoolean(ConfigurationManager.AppSettings["EnableConnector"]);
        }
    }
}