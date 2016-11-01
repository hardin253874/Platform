// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test
{
    [TestFixture]
    public class TestEntityNuggets
    {
        //[Test]
        //[RunAsDefaultTenant]
        //public void TestModifyingField()
        //{
        //    // Get the report as JSON contract data
        //    var reportId = new[] { new EntityRef("test:rpt_StandardSimpleTypes") };
        //    var requests = new[] { "reportColumns.columnDisplayOrder" };

        //    JsonEntityQueryResult data = EntityController.GetJsonEntityQueryResult(reportId, requests);

        //    // Mark all as 'create'
        //    data.entities.ForEach(e =>
        //        {
        //            e.dataState = ReadiNow.Model.Client.DataState.Create;
        //            e.relationships.ForEach(r => r.instances.ForEach(ri => ri.dataState = ReadiNow.Model.Client.DataState.Create));
        //            e.fields.ForEach(f =>
        //                {
        //                    if (f.value is int)
        //                    {
        //                        // Mark test field as 100
        //                        f.value = 100;
        //                    }
        //                });
        //        });

        //    // Create nugget
        //    var nugget = new EntityNugget
        //    {
        //        DataV1 = data
        //    };


        //    // Decode

        //    IEntity entity = EntityNugget.DecodeEntity(nugget);
        //    var reportEntity = Entity.As<ReadiNow.Model.Report>(entity);

        //    int columnOrder = reportEntity.ReportColumns.Last().ColumnDisplayOrder ?? -1;

        //    Assert.AreEqual(100, columnOrder);
        //}


        //[Test]
        //[RunAsDefaultTenant]
        //public void TestAddingRelationship()
        //{
        //    // Get the report as JSON contract data
        //    var reportId = new[] { new EntityRef("test:rpt_StandardSimpleTypes") };
        //    var requests = new[] { "rootNode.relatedReportNodes.id" };

        //    JsonEntityQueryResult data = EntityController.GetJsonEntityQueryResult(reportId, requests);
        //    var relationshipReportNode = new EntityRef("relationshipReportNode");
        //    var newNodeId = new EntityRef(9007199254740992);
            
        //    data.entityRefs.Add(new JsonEntityRef(relationshipReportNode));
        //    data.entityRefs.Add(new JsonEntityRef(newNodeId));

        //    JsonEntity newNode = new JsonEntity
        //        {
        //            id = 9007199254740992,
        //            dataState = ReadiNow.Model.Client.DataState.Create,
        //            typeIds = new List<long> { relationshipReportNode.Id }
        //        };
        //    data.entities.Add(newNode);


        //    // Mark all as 'create'
        //    data.entities.ForEach(e =>
        //        {
        //            e.dataState = ReadiNow.Model.Client.DataState.Create;
        //            e.relationships.ForEach(r =>
        //                {
        //                    r.instances.ForEach(
        //                        ri => ri.dataState = ReadiNow.Model.Client.DataState.Create);
        //                    if (r.relTypeId.alias == "relatedReportNodes")
        //                    {
        //                        JsonRelationshipInstanceData inst = new JsonRelationshipInstanceData
        //                            {         
        //                                dataState = ReadiNow.Model.Client.DataState.Create,
        //                                entity = newNode.id
        //                            };
        //                        r.instances.Add(inst);
        //                    }
        //                });                    
        //        });

        //    // Create nugget
        //    var nugget = new EntityNugget
        //    {
        //        DataV1 = data
        //    };


        //    // Decode

        //    IEntity entity = EntityNugget.DecodeEntity(nugget);
        //    var reportEntity = Entity.As<ReadiNow.Model.Report>(entity);

        //    int count = reportEntity.RootNode.RelatedReportNodes.Count();

        //    // This should work!!
        //    Assert.AreEqual(1, count);
        //}
    }
}