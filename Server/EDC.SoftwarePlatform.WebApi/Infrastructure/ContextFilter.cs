// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using EDC.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Context Filter class.
	/// </summary>
	public class ContextFilter : IActionFilter
	{
		#region Implementation of IFilter

		/// <summary>
		///     Executes the filter action asynchronously.
		/// </summary>
		/// <param name="actionContext">The action context.</param>
		/// <param name="cancellationToken">The cancellation token assigned for this task.</param>
		/// <param name="continuation">The delegate function to continue after the action method is invoked.</param>
		/// <returns>
		///     The newly created task for this operation.
		/// </returns>
		public Task<HttpResponseMessage> ExecuteActionFilterAsync( HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation )
		{
            // Get web context
            string entryPoint = "Failed";
            try
            {
                entryPoint = actionContext.ControllerContext.Controller.GetType( ).Name + "." + actionContext.ActionDescriptor.ActionName;
            }
            catch { }

			using ( DatabaseContextInfo.SetContextInfo( $"WebApi - {actionContext.ControllerContext.ControllerDescriptor.ControllerName}.{actionContext.ActionDescriptor.ActionName}" ) )
			using ( EntryPointContext.SetEntryPoint( entryPoint ) )
			{
				ProcessMonitorWriter.Instance.Write( entryPoint );

				RequestContext requestContext = RequestContext.GetContext( );
				if ( requestContext != null && requestContext.IsValid )
				{
					// Attach timezone
					string tz = HttpContext.Current.Request.Headers.Get( "Tz" );
					if ( !string.IsNullOrEmpty( tz ) )
					{
						// Set the timezone in the logical context
						var contextData = new RequestContextData( requestContext.Identity, requestContext.Tenant, requestContext.Culture, tz );
						RequestContext.SetContext( contextData );
					}
				}

				// Do the actual API call work
				return continuation( );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether more than one instance of the indicated attribute can be specified for a
		///     single program element.
		/// </summary>
		/// <returns>
		///     true if more than one instance is allowed to be specified; otherwise, false. The default is false.
		/// </returns>
		public bool AllowMultiple
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}