// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;
using NUnit.Framework;
using System.Threading;

namespace EDC.ReadiNow.Test.Security.AccessControl.Diagnostics
{
    [TestFixture]
    public class ForceSecurityTraceContextTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test()
        {
            long[] outerEntities;
            long[] innerEntities;

            outerEntities = new long[] { 1 };
            innerEntities = new long[] { 2 };

            Assert.That(ForceSecurityTraceContext.IsSet(), Is.False, "Context set initially");
            Assert.That(ForceSecurityTraceContext.EntitiesToTrace(), Is.Empty, "Entities to trace initially");

            using (new ForceSecurityTraceContext(outerEntities))
            {
                Assert.That(ForceSecurityTraceContext.IsSet(), Is.True, "Context not set (before inner)");
                Assert.That(ForceSecurityTraceContext.EntitiesToTrace(), Is.EquivalentTo(outerEntities));

                using (new ForceSecurityTraceContext(innerEntities))
                {
                    Assert.That(ForceSecurityTraceContext.IsSet(), Is.True, "Context not set (inner)");
                    Assert.That(ForceSecurityTraceContext.EntitiesToTrace(), Is.EquivalentTo(innerEntities.Union(outerEntities)));
                }

                Assert.That(ForceSecurityTraceContext.IsSet(), Is.True, "Context not set (after inner)");
                Assert.That(ForceSecurityTraceContext.EntitiesToTrace(), Is.EquivalentTo(outerEntities));
            }

            Assert.That(ForceSecurityTraceContext.IsSet(), Is.False, "Context set afterwards");
            Assert.That(ForceSecurityTraceContext.EntitiesToTrace(), Is.Empty, "Entities to trace afterwards");
        }

        [Test]
        public void EventLogSettingsAlias()
        {
            Assert.That(ForceSecurityTraceContext.EventLogSettingsAlias, Is.EqualTo("core:tenantEventLogSettingsInstance"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void GetInspectingEntities()
        {
            EventLogSettings eventLogSettings;

            eventLogSettings = Entity.Get<EventLogSettings>(ForceSecurityTraceContext.EventLogSettingsAlias, true);
            Assert.That(eventLogSettings, Is.Not.Null);

            eventLogSettings.InspectSecurityChecksOnResource.Add(eventLogSettings.As<Resource>());
            eventLogSettings.Save();

            Thread.Sleep( new TimeSpan( ForceSecurityTraceContext.TicksToWaitBeforeRefreshing ) );

            Assert.That(
                ForceSecurityTraceContext.GetInspectingEntities(),
                Has.Exactly(1).EqualTo(eventLogSettings.Id));

            eventLogSettings.InspectSecurityChecksOnResource.Clear( );
            eventLogSettings.Save( );

            Thread.Sleep( new TimeSpan( ForceSecurityTraceContext.TicksToWaitBeforeRefreshing ) );
        }
    }
}
