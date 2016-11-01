// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Linq;
using System.Web.Http;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Infrastructure;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
	[RoutePrefix( "data/v1/instance" )]
	public class InstanceController : ApiController
	{
		/// <summary>
		///     Examples of URLs (note to escape such as [] and :, but not doing here for clarity)
		///     .../instance/112233    Get the data using the default or generated form
		///     .../instance/core/emailsettings  Use the alias
		/// </summary>
		[Route( "{id}" )]
		[Route( "{ns}/{alias}" )]
        [HttpGet]
		public HttpResponseMessage<JsonQueryResult> Get(
			string id = null,
			string ns = null,
			string alias = null,
			[FromUri( Name = "designmode" )] bool isInDesignMode = false,
			[FromUri( Name = "forcegenerate" )] bool forceGenerate = false
			)
		{
            using (Profiler.Measure("InstanceController.Get"))
            {
                // Get the entityRef
                EntityRef entityRef = WebApiHelpers.MakeEntityRef(id, ns, alias);

                var entityType = ReadiNow.Model.Entity.Get(entityRef).EntityTypes.First().Cast<EntityType>();

                return Helper.GetDefaultFormForType(entityType, isInDesignMode, forceGenerate);
            }
		}
	}
}