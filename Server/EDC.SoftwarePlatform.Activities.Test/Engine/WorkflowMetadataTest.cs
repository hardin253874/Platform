// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.Engine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class WorkflowMetadataTest
    {
        [Test]
        public void DefaultConstructor()
        {
            var metadata = new WorkflowMetadata();
        }

        [Test]
        [RunWithTransaction]
        public void WfConstructor()
        {
            var workflow = new Workflow();
            workflow.Save();
            var metadata = new WorkflowMetadata(workflow);
        }

        [Test]
        [RunWithTransaction]

        public void Factory()
        {
            var workflow = new Workflow();
            workflow.Save();

            var fac = new WorkflowMetadataFactory();
            fac.Create(workflow);
        }

        [Test]
        [RunWithTransaction]

        public void CachingFactory()
        {
            var workflow = new Workflow();
            workflow.Save();

            var fac = new CachingWorkflowMetadataFactory();
            var md = fac.Create(workflow);
            Assert.That(md, Is.Not.Null);
        }

        [Test]
        [RunWithTransaction]

        public void CachingFactoryReturnsSameMetadata()
        {
            var workflow = new Workflow();
            workflow.WorkflowUpdateHash = "1";
            workflow.Save();

            var fac = new CachingWorkflowMetadataFactory();
            var md = fac.Create(workflow);
            Assert.That(md, Is.Not.Null);

            var workflow2 = Entity.Get<Workflow>(workflow.Id);
            var md2 = fac.Create(workflow2);

            Assert.That(workflow.WorkflowUpdateHash, Is.EqualTo(workflow2.WorkflowUpdateHash));
            Assert.That(md, Is.SameAs(md2));
        }

        [Test]
        [RunWithTransaction]

        public void CachingFactoryHandlesUpdatedWorkflow()
        {
            var workflow = new Workflow();
            workflow.WorkflowUpdateHash = "1";
            workflow.Save();

            var fac = new CachingWorkflowMetadataFactory();
            var md = fac.Create(workflow);
            Assert.That(md, Is.Not.Null);

            var writeableWf = workflow.AsWritable<Workflow>();
            writeableWf.WorkflowUpdateHash = "2";
            writeableWf.Save();

            var md2 = fac.Create(writeableWf);

            Assert.That(md, Is.Not.SameAs(md2));

        }

        public void CachingFactoryHandlesNullUnchangedWorkflow()
        {
            var workflow = new Workflow();
            workflow.WorkflowUpdateHash = null;
            workflow.Save();

            var fac = new CachingWorkflowMetadataFactory();
            var md = fac.Create(workflow);
            Assert.That(md, Is.Not.Null);

            var md2 = fac.Create(Entity.Get<Workflow>(workflow.Id));

            Assert.That(md, Is.SameAs(md2));

        }
        public void CachingFactoryHandlesNullUpdatedWorkflow()
        {
            var workflow = new Workflow();
            workflow.WorkflowUpdateHash = null;
            workflow.Save();

            var fac = new CachingWorkflowMetadataFactory();
            var md = fac.Create(workflow);
            Assert.That(md, Is.Not.Null);

            var writeableWf = workflow.AsWritable<Workflow>();
            writeableWf.WorkflowUpdateHash = "2";
            writeableWf.Save();

            var md2 = fac.Create(writeableWf);

            Assert.That(md, Is.Not.SameAs(md2));

        }
    }
}
