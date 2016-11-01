// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Test.EntityRequests
{
    /// <summary>
    /// 
    /// </summary>
    [RunAsDefaultTenant]
    [TestFixture]
    public class VirtualAccessControlFields
    {
        [TestCase("canCreateType", "test:employee")]
        [TestCase("canModify", "test:aaPeterAylett")]
        [TestCase("canDelete", "test:aaPeterAylett")]
        public void Test_Can_Read(string field, string instance)
        {
            string request = field;

            EntityData res = BulkRequestRunner.GetEntityData(new EntityRef(instance), request);

            Assert.That(res, Is.Not.Null, "res");
            Assert.That(res.Fields, Has.Count.EqualTo(1));
            Assert.That(res.Fields[0].FieldId.Alias, Is.EqualTo(field));
            Assert.That(res.Fields[0].Value.Value, Is.EqualTo(true));
        }

    }
}
