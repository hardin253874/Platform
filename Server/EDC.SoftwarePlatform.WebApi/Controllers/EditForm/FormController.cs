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
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
	[RoutePrefix( "data/v1/form" )]
	public class FormController : ApiController
	{
        ICache<long, JsonQueryResult> _formsCache;


        public FormController()
        {
            _formsCache = Factory.Current.ResolveNamed<ICache<long, JsonQueryResult>>("FormController Secured Result");
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

                JsonQueryResult result;

                _formsCache.TryGetOrAdd(formId, out result, (key) =>
                {
                    EntityData formEntityData = EditFormHelper.GetFormAsEntityData(key, isInDesignMode);
                    if (formEntityData == null)
                        throw new Exception("formEntityData was null");

                    var context = new EntityPackage();

                    context.AddEntityData(formEntityData, "formEntity");

                    return context.GetQueryResult();
                });

                return new HttpResponseMessage<JsonQueryResult>( result );
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
    }
}