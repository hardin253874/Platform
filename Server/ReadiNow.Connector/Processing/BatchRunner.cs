// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using MoreLinq;
using ReadiNow.Connector.Interfaces;

namespace ReadiNow.Connector.Processing
{
    /// <summary>
    /// Reads records from a reader in batches. Passes them to a record processor.
    /// </summary>
    internal class BatchRunner
    {
        /// <summary>
        /// Number of records to process in each batch.
        /// </summary>
        public int BatchSize { get; set; } = 20;

        /// <summary>
        /// Source of records to process.
        /// </summary>
        public IObjectsReader ObjectsReader { get; set; }

        /// <summary>
        /// Non-batched mechanism for processing records.
        /// </summary>
        public IRecordImporter RecordImporter { get; set; }

        /// <summary>
        /// Non-batched mechanism for processing records.
        /// </summary>
        public ICancellationWatcher CancellationWatcher { get; set; }

        /// <summary>
        /// Process all records.
        /// </summary>
        public void ProcessAll( )
        {
            if ( ObjectsReader == null )
                throw new InvalidOperationException( "ObjectsReader not set" );
            if ( RecordImporter == null )
                throw new InvalidOperationException( "RecordImporter not set" );
            if ( CancellationWatcher == null )
                throw new InvalidOperationException( "CancellationWatcher not set" );
            if ( BatchSize < 1 )
                throw new InvalidOperationException( "BatchSize invalid" );

            // Open stream of objects
            IEnumerable<IObjectReader> rows = ObjectsReader.GetObjects( );

            // Group into batches
            IEnumerable<IEnumerable<IObjectReader>> batches = rows.Batch( BatchSize );

            // Run batches
            foreach ( IEnumerable<IObjectReader> batchOfInputRows in batches )
            {
                if ( CancellationWatcher.IsCancellationRequested )
                    break;

                RecordImporter.ImportRecords(batchOfInputRows);
            }
        }

    }
}
