// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Monitoring;
using EDC.ReadiNow.Monitoring.AccessControl;
using NUnit.Framework;
using EDC.ReadiNow.Monitoring.Workflow;
using EDC.ReadiNow.Monitoring.Reports;
using EDC.ReadiNow.Monitoring.Model;

namespace EDC.ReadiNow.Test.Monitoring
{
    /// <summary>
    /// Register performance counters (useful while the install does not).
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    [Explicit]
    public class PerformanceCounterHelper
    {
        [Test]
        public void RegisterAccessControlPerformanceCounters()
        {
            new AccessControlPerformanceCounters().CreateCategory();
            new AccessControlPermissionPerformanceCounters().CreateCategory();
            new AccessControlCacheInvalidationPerformanceCounters().CreateCategory();
        }

        [Test]
        public void RegisterReportsPerformanceCounters()
        {
            new ReportsPerformanceCounters().CreateCategory();
        }

        [Test]
        public void RegisterWorkflowPerformanceCounters()
        {
            new WorkflowPerformanceCounters().CreateCategory();
        }

        [Test]
        public void RegisterPlatformTracePerformanceCounters()
        {
            new PlatformTracePerformanceCounters().CreateCategory();
        }

        [Test]
        public void RegisterEntityInfoServicePerformanceCounters()
        {
            new EntityInfoServicePerformanceCounters().CreateCategory();
        }
    }
}
