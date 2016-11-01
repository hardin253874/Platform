// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Processing;

namespace ReadiNow.Connector.Test.Processing
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    class ImportRunCancellationWatcherTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_Null_ImportRun( )
        {
            Assert.Throws<ArgumentNullException>( ( ) => new ImportRunCancellationWatcher( null ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CancelWorks( )
        {
            ImportRun importRun = new ImportRun( );
            ICancellationWatcher watcher = new ImportRunCancellationWatcher( importRun );
            Assert.That( watcher.IsCancellationRequested, Is.False );
            importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunStarted;
            Assert.That( watcher.IsCancellationRequested, Is.False );
            importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunCancelled;
            Assert.That( watcher.IsCancellationRequested, Is.True );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CompletedWorks( )
        {
            ImportRun importRun = new ImportRun( );
            ICancellationWatcher watcher = new ImportRunCancellationWatcher( importRun );
            Assert.That( watcher.IsCancellationRequested, Is.False );
            importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunStarted;
            Assert.That( watcher.IsCancellationRequested, Is.False );
            importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunCompleted;
            Assert.That( watcher.IsCancellationRequested, Is.False );
        }
    }
}
