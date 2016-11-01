// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.EntityRequests
{
    /// <summary>
    /// Wrapper over the BulkRequestRunner to present the IEntityInfoRead interface.
    /// (So that we can use the same test fixture for both the EntityInfoService and the BulkRequestRunner)
    /// </summary>
    internal class BulkRequestRunnerMockService : IEntityInfoRead
    {
        public EntityData GetEntityData(EntityRef id, EntityMemberRequest request)
        {
            if (string.IsNullOrEmpty(request.RequestString))
            {
                // obsolete path
                var results = GetEntitiesData(id.ToEnumerable(), request);
                EntityData res = results.FirstOrDefault();
                return res;
            }
            else
            {
                // preferred path .. but still need to get away from passing EntityMemberRequest
                EntityData res = BulkRequestRunner.GetEntityData(id, request.RequestString);
                return res;
            }
        }

        public IEnumerable<EntityData> GetEntitiesData(IEnumerable<EntityRef> entities, EntityMemberRequest request, SecurityOption securityOption = SecurityOption.SkipDenied)
        {
            EntityRequest entityRequest = new EntityRequest
                {
                    QueryType = QueryType.Basic,
                    IgnoreResultCache = true,
                    Entities = entities,
                    Request = request
                };

            return BulkRequestRunner.GetEntitiesData(entityRequest);
        }

        IEnumerable<EntityData> IEntityInfoRead.GetEntitiesByType(EntityRef entityType, bool includeDerivedTypes, EntityMemberRequest request)
        {
            if (string.IsNullOrEmpty(request.RequestString))
            {
                Assert.Ignore("GetEntitiesByType not supported by BulkRequestRunner without request string");
            }
            else
            {
                var entityRequest = new EntityRequest
                {
                    QueryType = includeDerivedTypes ? QueryType.Instances : QueryType.ExactInstances,
                    IgnoreResultCache = true,
                    Entities = entityType.ToEnumerable(),
                    RequestString = request.RequestString
                };


                // preferred path .. but still need to get away from passing EntityMemberRequest
                IEnumerable<EntityData> res = BulkRequestRunner.GetEntitiesByType(entityRequest);
                return res;
            }

            
            return null;
        }

        IEnumerable<EntityData> IEntityInfoRead.QueryEntityData(ReadiNow.Metadata.Query.Structured.StructuredQuery query, EntityMemberRequest request)
        {
            Assert.Ignore("QueryEntityData not supported by BulkRequestRunner");
            return null;
        }
    }
}
