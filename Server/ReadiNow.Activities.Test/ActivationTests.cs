// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model.Interfaces;
using EDC.SoftwarePlatform.Activities.Engine;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    class ActivationTests
    {
        [Test]
        public void IWorkflowRunner_Instance()
        {
            IWorkflowRunner instance = Factory.Current.Resolve<IWorkflowRunner>();
            Assert.That(instance, Is.TypeOf<WorkflowRunner>());

            WorkflowRunner runner = (WorkflowRunner)instance;
            Assert.That(runner.MetadataFactory, Is.Not.Null);
        }

        [Test]
        public void IWorkflowMetadataFactory_Instance()
        {
            IWorkflowMetadataFactory instance = Factory.Current.Resolve<IWorkflowMetadataFactory>();
            Assert.That(instance, Is.TypeOf<CachingWorkflowMetadataFactory>());

            CachingWorkflowMetadataFactory factory = (CachingWorkflowMetadataFactory)instance;
            Assert.That(factory.InnerFactory, Is.TypeOf<WorkflowMetadataFactory>());
        }
    }
}
