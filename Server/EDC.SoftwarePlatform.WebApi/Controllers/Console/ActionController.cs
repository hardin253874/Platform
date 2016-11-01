// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Linq;
using System.Web.Http;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Services.Console;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Exceptions;
using EDC.ReadiNow.Utc;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
	/// <summary>
	///     Controller for building dynamic context or action menus.
	/// </summary>
	[RoutePrefix( "data/v1/actions" )]
	public class ActionController : ApiController
	{
		#region Constructor

		/// <summary>
		///     Basic constructor (server-side).
		/// </summary>
		public ActionController( )
		{
			ActionServiceImpl = new ActionService( );
		}

		#endregion

		#region Internal Properties

		/// <summary>
		///     Holds a handle to an instance of the action service.
		/// </summary>
		internal ActionService ActionServiceImpl
		{
			get;
			set;
		}

		#endregion

		#region Service Methods

		/// <summary>
		///     Gets the console actions relevant to a selection context.
		/// </summary>
		/// <param name="request">A console action request.</param>
		/// <returns>A console action response containing a list of actions.</returns>
		[Route("")]
        [HttpPost]
		public HttpResponseMessage<ActionResponse> Post( [FromBody] ActionRequest request )
		{
            if (request == null)
                throw new WebArgumentNullException("request");

            using (Profiler.Measure("ActionController.Post"))
            {
                var padded = new ActionRequestExtended(request);
                
                var tz = TimeZoneHelper.SydneyTimeZoneName;
                if (Request.Headers.Contains("tz"))
                {
                    tz = Request.Headers.GetValues("tz").First();
                }
                padded.TimeZone = tz;

                ActionResponse response = ActionServiceImpl.GetActions(padded);
                return new HttpResponseMessage<ActionResponse>(response);
            }
		}

		/// <summary>
		///     Gets the configurable actions and the state of any menu already applied to the context.
		/// </summary>
		/// <param name="request">The request giving context.</param>
		/// <returns>The state of any configured menu.</returns>
		[Route( "menu" )]
        [HttpPost]
		public HttpResponseMessage<ActionMenuState> Menu( [FromBody] ActionRequest request )
		{
            if (request == null)
                throw new WebArgumentNullException("request");

            using (Profiler.Measure("ActionController.Menu"))
            {
                var padded = new ActionRequestExtended(request);
                ActionMenuState response = ActionServiceImpl.GetActionsMenuState(padded);
                return new HttpResponseMessage<ActionMenuState>(response);
            }
		}

        /// <summary>
		///     Gets the configurable actions and the state of any menu already applied to the context.
		/// </summary>
		/// <param name="request">The request giving context.</param>
		/// <returns>The state of any configured menu.</returns>
		[Route("formActions")]
        [HttpPost]
        public HttpResponseMessage<ActionMenuState> Form([FromBody] ActionRequest request)
        {
            if (request == null)
                throw new WebArgumentNullException("request");

            using (Profiler.Measure("ActionController.Form"))
            {
                var padded = new ActionRequestExtended(request);
                ActionMenuState response = ActionServiceImpl.GetFormActionsMenuState(padded);
                return new HttpResponseMessage<ActionMenuState>(response);
            }
        }

        #endregion
    }
}