// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using ReadiNow.Annotations;
using ReadiNow.Connector.Interfaces;

namespace ReadiNow.Connector.Processing
{
    /// <summary>
    /// Monitors an ImportRun for cancellations.
    /// </summary>
    class ImportRunCancellationWatcher : ICancellationWatcher
    {
        private readonly ImportRun _importRun;
        private bool _cancelled;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importRun">A the import run entity that will be monitored, which must be provided by an automatically updating value.</param>
        public ImportRunCancellationWatcher( [NotNull] ImportRun importRun )
        {
            if ( importRun == null )
                throw new ArgumentNullException( nameof( importRun ) );

            _importRun = importRun;
        }

        /// <summary>
        /// Returns true if a cancellation has been requested.
        /// </summary>
        public bool IsCancellationRequested
        {
            get
            {
                if ( !_cancelled )
                {
                    _cancelled = SecurityBypassContext.Elevate(
                        ( ) => _importRun.ImportRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCancelled );
                }
                return _cancelled;
            }
        }
    }
}
