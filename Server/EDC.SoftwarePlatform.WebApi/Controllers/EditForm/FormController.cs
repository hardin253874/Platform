// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Autofac;

using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.EditForm;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Cache;
using EDC.Exceptions;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Security;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
	[RoutePrefix( "data/v1/form" )]
	public class FormController : ApiController
	{
	    private readonly FormControllerRequestHandler _formControllerRequestHandler;
        private readonly ICache<long, EntityData> _formsCache;


        public FormController()
        {
            _formsCache = Factory.Current.ResolveNamed<ICache<long, EntityData>>("FormController Secured Result");
            _formControllerRequestHandler = new FormControllerRequestHandler(_formsCache);
        }
        
		/// <summary>
		///     Examples of URLs (note to escape such as [] and :, but not doing here for clarity)
		///     .../form/747483/designmode=true    Get the specified edit form. DesignMode is optional.
		///     .../type/21314/form                Get the default edit form for the given type
		///     .../instance/112233                Get the default edit form and data for the given instance
		///     .../instance/112233?dataonly=true  Get the data for the default form for the instance
		///     .../instance/112233?formid=747483  Get the data for the given form and instance
		///     .../instance/112233?dataonly=true  Get only the data for the given form and instance
		/// </summary>
		[Route( "{id}" )]
		[Route( "{ns}/{alias}" )]
        [HttpGet]
		public HttpResponseMessage<JsonQueryResult> Get(
			[FromUri( Name = "designmode" )] bool isInDesignMode = false,
			string id = null,
			string ns = null,
			string alias = null
			)
		{
			using ( Profiler.Measure( "FormController.Get" ) )
			{
				// Get the entityRef
				EntityRef formRef = WebApiHelpers.MakeEntityRef( id, ns, alias );

                long formId;

                try
                {
                    formId = formRef.Id;
                } catch(ArgumentException)
                {
                    throw new FormNotFoundException();
                }

			    EntityData formEntityData = _formControllerRequestHandler.GetFormAsEntityData(formId, isInDesignMode);                

                var context = new EntityPackage();
                context.AddEntityData(formEntityData, "formEntity");
                return new HttpResponseMessage<JsonQueryResult>(context.GetQueryResult());
			}
		}
        
        /// <summary>
        ///     Removes given form Ids from the edit form cache.
        /// </summary>
        [HttpPost]
        [Route("forcerefresh")]
        public HttpResponseMessage<JsonQueryResult> ClearFormsCache([FromBody] string[] formIds)
        {
           
            if (formIds != null)
            {
                foreach (var formRef in formIds.Select(id => WebApiHelpers.MakeEntityRef(id, "", "")))
                {
                    long formId;
                    try
                    {
                        formId = formRef.Id;
                    }
                    catch (ArgumentException)
                    {
                        throw new FormNotFoundException();
                    }

                    _formsCache.Remove(formId);
                }
            }
            return new HttpResponseMessage<JsonQueryResult>(HttpStatusCode.OK);
        }

        
        /// <summary>		
		/// </summary>
		[Route("data")]
        [HttpPost]
        public IHttpActionResult GetFormData([FromBody] FormDataRequest request)
        {
            if (request == null)
            {
                throw new WebArgumentNullException(nameof(request));
            }            

            using (Profiler.Measure("FormController.GetData"))
            {
                string errorMessage;
                if (!_formControllerRequestHandler.ValidateRequest(request, out errorMessage))
                {
                    EventLog.Application.WriteError(errorMessage);
                    return BadRequest(errorMessage);
                }
               
                try
                {
                    var response = _formControllerRequestHandler.GetFormData(request);
                    if (response == null)
                    {
                        return NotFound();
                    }
                    
                    return Ok(response);
                }
                catch (PlatformSecurityException ex)
                {
                    EventLog.Application.WriteWarning(ex.ToString());
                    return StatusCode(HttpStatusCode.Forbidden);
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError(ex.ToString());
                    return InternalServerError();
                }
            }
        }

        /// <summary>		
		/// </summary>		
        [Route("visCalcDependencies/{id}")]
        [Route("visCalcDependencies/{ns}/{alias}")]        
        [HttpGet]
        public IHttpActionResult GetFormCalculationDependencies(string id = null, string ns = null, string alias = null)
        {            
            using (Profiler.Measure("FormController.GetFormCalculationDependencies"))
            {                
                try
                {
                    // Get the entityRef
                    EntityRef formRef = WebApiHelpers.MakeEntityRef(id, ns, alias);                    

                    var response = _formControllerRequestHandler.GetFormCalculationDependencies(formRef);                    

                    return Ok(response);
                }                
                catch (Exception ex)
                {
                    EventLog.Application.WriteError(ex.ToString());
                    return InternalServerError();
                }
            }
        }
    }
}