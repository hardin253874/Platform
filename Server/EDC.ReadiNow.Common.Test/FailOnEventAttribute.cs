// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.Diagnostics;
using NUnit.Framework;

namespace EDC.ReadiNow.Test
{
    /// <summary>
    /// When applied to an NUnit [Test] or [TestFixture], this will fail the test
    /// if an error is written to the Platform Event Log.
    /// </summary>
    public class FailOnEventAttribute : ReadiNowTestAttribute, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Create a new <see cref="FailOnEventAttribute"/>.
        /// </summary>
        /// <param name="failingLevels">
        /// Fail the test if an entry is written to the platform event log with one of these levels. 
        /// If omitted, fail the test if an error is written to the platform event log.
        /// </param>
        public FailOnEventAttribute(EventLogLevel[] failingLevels = null)
            : this(ReadiNow.Diagnostics.EventLog.Application, failingLevels)
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="FailOnEventAttribute"/>.
        /// </summary>
        /// <param name="failingLevels">
        /// Fail the test if an entry is written to the platform event log with one of these levels. 
        /// If omitted, fail the test if an error is written to the platform event log.
        /// </param>
        /// <param name="eventLog">
        /// The <see cref="IEventLog"/> to test. Defaults to <see cref="EDC.ReadiNow.Diagnostics.EventLog.Application"/> 
        /// if omitted.
        /// </param>
        public FailOnEventAttribute(IEventLog eventLog, EventLogLevel[] failingLevels = null)
        {
			Targets = ActionTargets.Test;
            _disposed = false;

            EventLog = eventLog ?? ReadiNow.Diagnostics.EventLog.Application;

            FailingLevels = new HashSet<EventLogLevel>();
            if (failingLevels != null)
            {
                FailingLevels.UnionWith(failingLevels);
            }
            else
            {
                FailingLevels.Add(EventLogLevel.Error);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The event log to monitor.
        /// </summary>
        public IEventLog EventLog { get; }

        /// <summary>
        /// The <see cref="EventLogLevel"/>s that, if logged, cause the test to fail.
        /// </summary>
        public ISet<EventLogLevel> FailingLevels { get; }

        /// <summary>
        /// Called before the test runs.
        /// </summary>
        /// <param name="testDetails">
        /// Test details.
        /// </param>
        public override void BeforeTest(TestDetails testDetails)
        {
            EventLog.WriteEvent += EventLogOnWriteEvent;
        }

        /// <summary>
        /// Called after the test runs.
        /// </summary>
        /// <param name="testDetails">
        /// Test details.
        /// </param>
        public override void AfterTest(TestDetails testDetails)
        {
            EventLog.WriteEvent -= EventLogOnWriteEvent;
        }

        /// <summary>
        /// What the attribute applies to.
        /// </summary>
        public override ActionTargets Targets { get; }

        /// <summary>
        /// Release unmanaged managed resources.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only
        /// unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                EventLog.WriteEvent -= EventLogOnWriteEvent;
            }

            _disposed = true;
        }

		/// <summary>
		/// Called when an event is written to <see cref="EventLog"/>.
		/// </summary>
		/// <param name="sender">
		/// The object that raised the event (ignored).
		/// </param>
		/// <param name="args">
		/// Event details, including the entry written.
		/// </param>
		internal void EventLogOnWriteEvent( object sender, EventLogWriteEventArgs args )
		{
			if ( FailingLevels.Contains( args.Entry.Level ) && args.Entry.Message != null && !args.Entry.Message.StartsWith( "Slow SQL took" ) )
			{
				Assert.Fail( "Event log: {0}", args.Entry );
			}
		}
    }
}
