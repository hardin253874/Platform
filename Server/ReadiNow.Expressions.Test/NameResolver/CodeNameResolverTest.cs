// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Test.NameResolver
{
    /// <summary>
    /// Note: CodeNameResolver is a legacy wrapper over IScriptNameResolver. These tests are just to test the wrapper.
    /// </summary>
    [TestFixture]
    [RunAsDefaultTenant]
    public class CodeNameResolverTest
    {
        const string WfInstanceName = "Person Name Update";

        [Test]
        public void GetInstanceTest()
        {
            IEntity entity = EDC.ReadiNow.Expressions.CodeNameResolver.GetInstance(WfInstanceName, "Workflow");
            Assert.That(entity, Is.Not.Null);
        }


        [Test]
        public void GetInstanceTest_withType()
        {
            var entities = EDC.ReadiNow.Expressions.CodeNameResolver.GetInstance(WfInstanceName, Workflow.Workflow_Type);
            Assert.That(entities, Is.Not.Empty);
        }


        [Test]
        public void GetTypeByNameTest()
        {
            IEntity entity = EDC.ReadiNow.Expressions.CodeNameResolver.GetTypeByName("Workflow");
            Assert.That(entity, Is.Not.Null);
        }
    }
}