// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Web.Http;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Infrastructure;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
	[RoutePrefix( "data/v1/type" )]
	public class TypeController : ApiController
	{
		/// <summary>
		///     Generate a form for the given type
		///     Examples of URLs (note to escape such as [] and :, but not doing here for clarity)
		///     .../type/112233/generateform
		/// </summary>
		[HttpGet]
		[Route( "{id}/generateform" )]
		[Route( "{ns}/{alias}/generateform" )]
		public HttpResponseMessage<JsonQueryResult> GetGenerateForm(
			string id = null,
			string ns = null,
			string alias = null,
			[FromUri( Name = "designmode" )] bool isInDesignMode = false
			)
		{
            using (Profiler.Measure("TypeController.GetGenerateForm"))
            {
                // Get the entityRef
                EntityRef entityRef = WebApiHelpers.MakeEntityRef(id, ns, alias);

                return Helper.GetGeneratedFormForType(entityRef, isInDesignMode);
            }
		}


		/// <summary>
		///     Examples of URLs (note to escape such as [] and :, but not doing here for clarity)
		///     .../instance/112233    Get the data using the default or generated form
		///     .../instance/core/emailsettings  Use the alias
		/// </summary>
		[HttpGet]
		[Route( "{id}/defaultForm" )]
		[Route( "{ns}/{alias}/defaultForm" )]
		public HttpResponseMessage<JsonQueryResult> GetDefaultForm(
			string id = null,
			string ns = null,
			string alias = null,
			[FromUri( Name = "designmode" )] bool isInDesignMode = false,
			[FromUri( Name = "forcegenerate" )] bool forceGenerate = false
			)
		{
			// Get the entityRef
			EntityRef typeRef = WebApiHelpers.MakeEntityRef( id, ns, alias );

            var entityType = ReadiNow.Model.Entity.Get<EntityType>(typeRef);

            return Helper.GetDefaultFormForType(entityType, isInDesignMode, forceGenerate);
		}
	}
}