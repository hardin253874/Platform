// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.Diagnostics;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Diagnostics.ActivityLog
{
    [TestFixture]
    [FailOnEvent]
    public class ActivityLogPurgerTests
    {
        [Test]
        public void DefaultMaxEventLogEntries()
        {
            Assert.That(ActivityLogPurger.DefaultMaxEventLogEntries, Is.EqualTo(10000));
        }

        [Test]
        public void EventLogEntriesLimit()
        {
            Assert.That(ActivityLogPurger.EventLogEntriesLimit, Is.EqualTo(10000));
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void GetEventLogEntitiesToDelete_Argument(int maxEventLogEntries)
        {
            Assert.That(
                () => new ActivityLogPurger().GetEventLogEntitiesToDelete(maxEventLogEntries),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("maxEventLogEntities"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(3, 1)]
        [TestCase(3, 2)]
        [TestCase(3, 3)]
        public void GetEventLogEntitiesToDelete(int numEventLogEntries, int maxEventLogEntries)
        {
            IList<WorkflowRunLogEntry> entries;
            ICollection<long> entitiesToDelete;

            // Remove any existing log entries
            Entity.Delete(Entity.GetInstancesOfType<WorkflowRunLogEntry>().Select(e=> e.Id));

            // Create the test entries
            entries = Enumerable.Range(0, numEventLogEntries)
                                .Select(i =>
                                {
                                    WorkflowRunLogEntry entry;

                                    entry = new WorkflowRunLogEntry
                                    {
                                        Name = string.Format("Test Entry {0}", i + 1),
                                        LogEntrySeverity_Enum = LogSeverityEnum_Enumeration.InformationSeverity
                                    };
                                    entry.Save();

                                    // Wait to ensure creation times are different. The resolution of
                                    // CreatedDate is a second, unfortunately.
                                    Thread.Sleep(1000);

                                    return entry;
                                })
                                .ToList();

            // Reload the entities to get the correct created date and other fields
            entries = Entity.Get<WorkflowRunLogEntry>(entries.Select(e => e.Id), new IEntityRef[]{ Resource.CreatedDate_Field })
                            .OrderBy(e => e.CreatedDate)
                            .ToList();

            // Find the entities to delete
            entitiesToDelete = new ActivityLogPurger().GetEventLogEntitiesToDelete(maxEventLogEntries);

            Assert.That(
                entitiesToDelete, 
                Is.EquivalentTo(
                    entries.Take(Math.Max(numEventLogEntries - maxEventLogEntries, 0))
                           .Select(e => e.Id)));
        }


        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(3, 1)]
        [TestCase(3, 2)]
        [TestCase(3, 3)]
        public void Purge(int numEventLogEntries, int maxEventLogEntries)
        {
            IList<WorkflowRunLogEntry> entries;
            EventLogSettings eventLogSettings;
            int numberEntitiesDeleted;

			var workflowRunLogEntries = Entity.GetInstancesOfType<WorkflowRunLogEntry>( ).ToList( );

			var longs = workflowRunLogEntries.Select( e => e.Id ).ToList( );

			// Remove any existing log entries
			Entity.Delete( longs );

            // Create the test entries
            entries = Enumerable.Range(0, numEventLogEntries)
                                .Select(i =>
                                {
                                    WorkflowRunLogEntry entry;

                                    entry = new WorkflowRunLogEntry
                                    {
                                        Name = string.Format("Test Entry {0}", i + 1),
                                        LogEntrySeverity_Enum = LogSeverityEnum_Enumeration.InformationSeverity
                                    };
                                    entry.Save();

                                    // Wait to ensure creation times are different. The resolution of
                                    // CreatedDate is a second, unfortunately.
                                    Thread.Sleep(1000);

                                    return entry;
                                })
                                .ToList();

            // Reload the entities to get the correct created date and other fields
            entries = Entity.Get<WorkflowRunLogEntry>(entries.Select(e => e.Id), new IEntityRef[] { Resource.CreatedDate_Field })
                            .OrderBy(e => e.CreatedDate)
                            .ToList();

            // Set the maximum number of event log entries
            eventLogSettings = Entity.Get<EventLogSettings>(ActivityLogPurger.EventLogSettingsAlias, true);
            eventLogSettings.MaxEventLogEntries = maxEventLogEntries;
            eventLogSettings.Save();

            using (EventLogMonitor eventLogMonitor = new EventLogMonitor())
            {
                new ActivityLogPurger().Purge();

                numberEntitiesDeleted = Math.Max(numEventLogEntries - maxEventLogEntries, 0);
                if (numberEntitiesDeleted > 0)
                {
                    Assert.That(
                        eventLogMonitor.Entries,
                        Has.Exactly(1)
                           .Property("Message").Matches("Deleting \\d+ excess event log entry entities.")
                           .And.Property("Level").EqualTo(EventLogLevel.Information));
                }

				workflowRunLogEntries = Entity.GetInstancesOfType<WorkflowRunLogEntry>( ).ToList( );

				longs = workflowRunLogEntries.Select( e => e.Id ).ToList( );

				Assert.That( longs,
                    Is.EquivalentTo(
                        entries.Reverse()
                               .Take(Math.Min(maxEventLogEntries, numEventLogEntries))
                               .Select(e => e.Id)));
            }
        }
    }
}
