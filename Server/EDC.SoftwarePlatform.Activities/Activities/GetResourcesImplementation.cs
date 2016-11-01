// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Linq;
using EDC.ReadiNow.Model;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Security;
using System.Collections.Generic;
using EDC.ReadiNow.Services.Exceptions;
using ReadiNow.Reporting;
using ReadiNow.Reporting.Request;

namespace EDC.SoftwarePlatform.Activities
{
    public sealed class GetResourcesImplementation : ActivityImplementationBase, IRunNowActivity
    {
        private const int MaxResourcesInList = 1000;


        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var typeRefKey = GetArgumentKey("getResourcesResourceType");
            var reportRefKey = GetArgumentKey("getResourcesReport");
            var listKey = GetArgumentKey("getResourcesList");
            var firstKey = GetArgumentKey("getResourcesFirst");
            var countKey = GetArgumentKey("getResourcesCount");

            object o;
            EntityType resourceType = null;
            IEntity reportRef = null;

            if (inputs.TryGetValue(typeRefKey, out o))
                if (o != null)
                    resourceType = ((IEntity)o).As<EntityType>();

            if (inputs.TryGetValue(reportRefKey, out o))
                if (o != null)
                    reportRef = (IEntity) o;

            if (resourceType == null && reportRef == null)
                throw new WorkflowRunException("Get Resources must have one of either the Type or Report parameters specified.");

                       
            IEnumerable<IEntity> list = null;

            SecurityBypassContext.RunAsUser(() =>
            {
                list = reportRef != null ? GetListFromReport(context, reportRef) : GetListFromType(resourceType);
            });

            context.SetArgValue(ActivityInstance, listKey, list);
            context.SetArgValue(ActivityInstance, firstKey, list.FirstOrDefault());
            context.SetArgValue(ActivityInstance, countKey, list.Count());
        }


        IEnumerable<IEntity> GetListFromReport(IRunState context, IEntity reportRef)
        {

            var reportSettings = new ReportSettings
            {
                InitialRow = 0,
                PageSize = MaxResourcesInList,
                SupportPaging = true,
                CpuLimitSeconds = EDC.ReadiNow.Configuration.EntityWebApiSettings.Current.ReportCpuLimitSeconds
            };

           
            var reportingInterface = new ReportingInterface();

            try
            {
                var result = reportingInterface.RunReport(reportRef.Id, reportSettings);
                return result.GridData.Where(row => row.EntityId > 0).Select(row => Entity.Get(row.EntityId)).ToList();
            }
            catch (TenantResourceLimitException ex)
            {
                throw new WorkflowRunException(ex.CustomerMessage, ex);
            }
        }

        IEnumerable<IEntity> GetListFromType(EntityType entityType)
        {
            return Entity.GetInstancesOfType(new EntityRef(entityType.Id));
        }

       
    }
}