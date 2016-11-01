// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Utc;
using EDC.SoftwarePlatform.Services.ReportTemplate;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Exceptions;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ReportTemplate
{
	/// <summary>
	///     Controller for working with the report templates/
	/// </summary>
	[RoutePrefix( "data/v1/reportTemplate" )]
	public class ReportTemplateController : ApiController
	{
		/// <summary>
		///     Generates a report based on parameters given on the query string.
		/// </summary>
		/// <returns>A token that can be used to retrieve or check the state of the document.</returns>
		[HttpGet]
        [Route("")]
		public HttpResponseMessage<string> Get( )
		{
            using ( Profiler.Measure( "ReportTemplateController.Get" ) )
			{
                ReportTemplateSettings settings;
                try
                {
                    settings = SettingsFromQuery(Request.RequestUri.ParseQueryString());
                }
                catch (Exception ex)
                {
                    throw new WebArgumentException("Request parameters", ex);
                }
                    
                return GenerateReport(settings);
			}
		}

		#region Private Methods

		private static HttpResponseMessage<string> GenerateReport( ReportTemplateSettings settings )
		{
			try
			{
				var reportTemplateInterface = new ReportTemplateInterface( );
				string result = reportTemplateInterface.GenerateReportFromTemplate( settings );
				return string.IsNullOrEmpty( result ) ? new HttpResponseMessage<string>( HttpStatusCode.NotFound ) : new HttpResponseMessage<string>( result );
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteError( ex.Message );
				return new HttpResponseMessage<string>( HttpStatusCode.BadRequest );
			}
		}

        /// <summary>
        /// Parse parameters from query string and headers.
        /// </summary>
		private static ReportTemplateSettings SettingsFromQuery( NameValueCollection queryString )
		{
			long templateValue = -1;
			var resources = new List<EntityRef>( );

            // Read parameters
			foreach ( string key in queryString.Keys )
			{
                string keyTrimmed = key.ToLower( ).Trim( );
				switch ( keyTrimmed )
				{
					case "template":
						string templateValueString = queryString.Get( key ).Trim( ).ToLower( );
						if ( string.IsNullOrEmpty( templateValueString ) || !long.TryParse( templateValueString, out templateValue ) )
						{
                            throw new WebArgumentException(string.Format("Invalid value {0} Parameter Type {1}", templateValueString, keyTrimmed));
						}
						break;

					case "resource":
						string resourceValueString = queryString.Get( key );
						if ( string.IsNullOrEmpty( resourceValueString ) )
						{
                            throw new WebArgumentException(string.Format("Invalid value {0} Parameter Type {1}", resourceValueString, keyTrimmed));
						}

						string[ ] resourceElements = resourceValueString.Split( ',' );
						foreach ( string resourceElement in resourceElements )
						{
							long resourceId;
							if ( !long.TryParse( resourceElement, out resourceId ) )
							{
                                throw new WebArgumentException(string.Format("Invalid value {0} Parameter Type {1}", resourceElement, keyTrimmed));
							}
							resources.Add( resourceId );
						}
						break;

					default:
						throw new FormatException( string.Format( "Invalid Parameter Type {0}", key.ToLower( ).Trim( ) ) );
				}
			}

            // Validate inputs
			if ( templateValue == -1 || resources.Count <= 0 )
			{
				throw new WebArgumentException( "Invalid parameters specified for document generation." );
			}

            // Read time zone
			string timezone = HttpContext.Current.Request.Headers.Get( "Tz" );
			if ( string.IsNullOrEmpty( timezone ) )
			{
				throw new WebArgumentException( "No time zone specified in the request header." );
			}

            // Create settings object
			return new ReportTemplateSettings
			{
				ReportTemplate = templateValue,
                SelectedResources = resources,
                TimeZone = TimeZoneHelper.GetTimeZoneInfo( timezone ).DisplayName
			};
		}

		#endregion
	}
}