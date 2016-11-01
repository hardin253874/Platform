// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Web.Http;
using EDC.Exceptions;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEngine
{
    /// <summary>
    ///     Entity Calculation controller.
    /// </summary>
    [RoutePrefix("data/v1/calcEngine")]
    public class CalcEngineController : ApiController
    {
        private readonly CalcEngineRequestHandler _requestHandler = new CalcEngineRequestHandler();

        /// <summary>
        ///     Runs the specified expressions.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [Route("evalExpressions")]
        [HttpPost]
        public IHttpActionResult EvaluateExpressions([FromBody] CalcEngineEvalRequest request)
        {
            if (request == null)
                throw new WebArgumentNullException(nameof(request), "Request is null");

            if (request.ContextEntity == null)
                throw new WebArgumentNullException(nameof(request), "ContextEntity is not specified.");

            if (request.Expressions == null)
                throw new WebArgumentNullException(nameof(request), "Expressions are not specified");

            if (request.Expressions.Count == 0)
                throw new WebArgumentException("Expressions are not specified", nameof(request));

            using (Profiler.Measure("CalcEngineController.EvaluateExpressions"))
            {
                var contextEntity = EntityNugget.DecodeEntity(request.ContextEntity);
                if (contextEntity == null)
                    throw new ArgumentNullException(nameof(request), @"ContextEntity does not exist.");

                var response = _requestHandler.EvaluateExpressions(contextEntity, request.Expressions);

                return Ok(response);
            }
        }
    }
}