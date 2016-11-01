// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Handles WebApi methods that expect to return JsonP payloads.
	/// </summary>
	public class JsonpCallbackAttribute : ActionFilterAttribute
	{
		/// <summary>
		///     The callback query parameter
		/// </summary>
		private const string CallbackQueryParameter = "callback";

		/// <summary>
		///     Called when the action has executed.
		/// </summary>
		/// <param name="context">The context.</param>
		public override void OnActionExecuted( HttpActionExecutedContext context )
		{
			string callback;

			if ( IsJsonp( out callback ) )
			{
				var jsonBuilder = new StringBuilder( callback );

				jsonBuilder.AppendFormat( "({0})", context.Response.Content.ReadAsStringAsync( ).Result );

				context.Response.Content = new StringContent( jsonBuilder.ToString( ) );
			}

			base.OnActionExecuted( context );
		}

		/// <summary>
		///     Determines whether the specified callback is jsonp.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <returns>
		///     <c>true</c> if the specified callback is jsonp; otherwise, <c>false</c>.
		/// </returns>
		private bool IsJsonp( out string callback )
		{
			callback = HttpContext.Current.Request.QueryString[ CallbackQueryParameter ];

			return !string.IsNullOrEmpty( callback );
		}
	}
}