// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Net;
using System.Net.Http;
using System.Web.Http;
using EDC.ReadiNow.Diagnostics;
using EDC.Exceptions;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EventHandler
{
	/// <summary>
	///     EventLog Controller
	/// </summary>
	[RoutePrefix( "data/v1/eventhandler" )]
	public class EventhandlerController : ApiController
	{
		/// <summary>
		///     Posts the event.
		/// </summary>
		/// <param name="spevent">The event.</param>
		/// <returns></returns>
		[Route( "postevent" )]
        [HttpPost]
		public HttpResponseMessage PostEvent( [FromBody] EventData spevent )
		{
            if (spevent == null)
                throw new WebArgumentNullException("spevent");

            if (spevent.Message != null)
                EventLog.Application.WriteError(spevent.Message);

            var response = new HttpResponseMessage(HttpStatusCode.Created);

            return response;
		}
	}
}