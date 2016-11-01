// Copyright 2011-2016 Global Software Innovation Pty Ltd


using System.Web.Http;

namespace EDC.SoftwarePlatform.WebApiTestControllers.Controllers
{
    [System.Web.Mvc.RoutePrefix("data/v1/test/testapicontroller")]
    public class TestApiController : ApiController
    {
        [Route("menu")]
        [HttpGet]
		public System.Web.Mvc.ActionResult Menu( )
        {
			return new System.Web.Mvc.ContentResult
			{
				Content = "Hellloooo?!"
			};
        }
    }
}
