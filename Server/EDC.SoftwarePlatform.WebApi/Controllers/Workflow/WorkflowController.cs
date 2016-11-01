// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using EDC.Database;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.Activities;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using MDL = EDC.ReadiNow.Model;
using EC = EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.Exceptions;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Core;
using System.Linq;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Workflow
{
	/// <summary>
	///     Workflow Controller class.
	/// </summary>
	[RoutePrefix( "data/v1/workflow" )]
	public class WorkflowController : ApiController
	{
		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <param name="idOrAlias">The identifier or alias.</param>
		/// <returns></returns>
		private static MDL.EntityRef GetId( string idOrAlias )
		{
            using (Profiler.Measure("WorkflowController.GetId"))
		    {
		        long id;
		        if (long.TryParse(idOrAlias, out id))
		            return new MDL.EntityRef(id);

		        string[] parts = idOrAlias.Split(':');
		        return parts.Length > 1 ? new MDL.EntityRef(parts[0], parts[1]) : new MDL.EntityRef(idOrAlias);
		    }
		}

		/// <summary>
		///     Runs the specified identifier or alias.
		/// </summary>
		/// <param name="idOrAlias">The identifier or alias.</param>
		/// <param name="values">The values.</param>
		/// <param name="trace">if set to <c>true</c> [trace].</param>
		/// <returns></returns>
		/// <exception cref="System.ApplicationException">
		///     Resource list arguments not implemented.
		///     or
		///     Object arguments not implemented.
		/// </exception>
		[Route( "run/{idOrAlias}" )]
        [HttpPost]
		public HttpResponseMessage<string> Run( string idOrAlias, [FromBody] List<ParameterValue> values, bool trace = false )
		{
			using ( Profiler.Measure( "WorkflowController.Run" ) )
			{
				MDL.EntityRef eid = GetId( idOrAlias );

				var workflow = MDL.Entity.Get<MDL.Workflow>( eid );

				var parameterValues = new Dictionary<string, object>( ); // { { "ResourceId", new EntityRef(resourceId) } };

                using (new SecurityBypassContext())
                {
                    if (values != null)
                    {
                        foreach (ParameterValue kv in values)
                        {
                            EventLog.Application.WriteTrace("Running workflow {0} with {1}={2} ({3})", eid.Id, kv.Name,
                                kv.Value, kv.TypeName);

                            var argType = MDL.Entity.Get<MDL.ArgumentType>(kv.TypeName, MDL.ArgumentType.InternalDisplayName_Field);

                            switch (argType.Alias)
                            {
                                case "core:resourceListArgument":
                                    throw new ApplicationException("Resource list arguments not implemented.");

                                case "core:objectArgument":
                                    throw new ApplicationException("Object arguments not implemented.");

                                case "core:resourceArgument":
                                    parameterValues.Add(kv.Name, GetId(kv.Value).Entity);
                                    break;

                                default:
                                    DatabaseType dbType = DatabaseType.ConvertFromDisplayName(argType.InternalDisplayName);
                                    parameterValues.Add(kv.Name, dbType.ConvertFromString(kv.Value));
                                    break;
                            }
                        }
                    }
                }
                
                var taskId = WorkflowRunner.Instance.RunWorkflowAsync( new WorkflowStartEvent(workflow) { Arguments = parameterValues, Trace = trace } );

                return new HttpResponseMessage<string>(taskId);
			}
		}

		/// <summary>
		///     Gets the validate.
		/// </summary>
		/// <param name="idOrAlias">The identifier or alias.</param>
		/// <returns></returns>
		[Route( "validate/{idOrAlias}" )]
        [HttpGet]
		public HttpResponseMessage<IEnumerable<string>> GetValidate( string idOrAlias )
		{
            using (Profiler.Measure("WorkflowController.GetValidate"))
            {
                MDL.EntityRef eid = GetId(idOrAlias);

                var workflow = MDL.Entity.Get<MDL.Workflow>(eid);

                IEnumerable<string> messages = workflow.Validate().Distinct();

                return new HttpResponseMessage<IEnumerable<string>>(messages);
            }
		}

        /// <summary>
        ///     Runs the report entity for report.
        /// </summary>
        /// <param name="entityData">The entity data.</param>
        /// <returns>true if the workflow was clones</returns>
        [Route("update/{id}")]
        [HttpPost]
        public HttpResponseMessage<bool> UpdateWorkflow(long id, [FromBody] EC.EntityNugget entityNugget)
        {
            using (Profiler.Measure("WorkflowController.UpdateWorkflow"))
            {
                HttpResponseMessage<bool> result = null;

                DatabaseContext.RunWithRetry(() =>
                {
                    Action updateAction = () =>
                    {
                        MDL.IEntity updatedEntity = EC.EntityNugget.DecodeEntity(entityNugget, true);

                        var wf = updatedEntity.As<MDL.Workflow>();

                        if (updatedEntity == null)
                            throw new WebArgumentException("Workflow update does not contain data");

                        if (updatedEntity.Id != id)
                            throw new WebArgumentException("Request and nugget ids do not match");


                        updatedEntity.Save();
                    };

                    bool isCloned = WorkflowUpdateHelper.Update(id, updateAction);

                    result = new HttpResponseMessage<bool>(isCloned);
                });

                return result;
            }
        }

        /// <summary>
        /// Checks if the current workflow run has stopped. Returns the resulting run ID if it has. Zero if it has not.
        /// </summary>
        /// <param name="id">The id to lookup the progress of the run.</param>
        /// <returns>The id of the saved workflow run.</returns>
        /// <exception cref="System.ApplicationException">
        ///     Resource list arguments not implemented.
        ///     or
        ///     Object arguments not implemented.
        /// </exception>
        [Route("hasrunstopped/{id}")]
        [HttpGet]
        public HttpResponseMessage<long> HasRunStopped(string id)
        {
            using (Profiler.Measure(string.Format("WorkflowController.HasRunStopped [{0}]", id)))
            {
                var tskMgr = Factory.WorkflowRunTaskManager;
                var runid = 0L;

                // This is a little ugly. It's a patch to deal with old behaviour of workflow taks where a guid was used to identify running workflows before they were
                // initially save and then the Id was used. We now only used the task Id. I'm relying of the Entity field cache to make it fast. Ideally we should 
                // create two seperate services and get client to call the right one.
                Guid taskIdGuid;
                if (!Guid.TryParse(id, out taskIdGuid))
                {
                    long runId;
                    if (!long.TryParse(id, out runId))
                    {
                        throw new WebArgumentException("Provided Workflow Id must be a taskId string  or a run Id");
                    }

                    id = MDL.Entity.Get<MDL.WorkflowRun>(runId).TaskId;
                }

                var completed = tskMgr.HasCompleted(id);
                if (completed)
                {
                    var result = tskMgr.GetResult(id);

                    long.TryParse(result, out runid);
                }
                
                return new HttpResponseMessage<long>(runid);
            }
        }
	}
}