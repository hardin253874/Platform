// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;
using ReadiNow.Connector.Interfaces;
using EDC.ReadiNow.Common.Workflow;

namespace ReadiNow.Connector.Processing
{
    /// <summary>
    /// Service for reading multiple records from a reader and writing them into an entity adapter.
    /// </summary>
    public interface IRecordImporter
    {
        /// <summary>
        /// Adapter for converting records to entities.
        /// </summary>
        IReaderToEntityAdapter ReaderToEntityAdapter { get; set; }

        /// <summary>
        /// Imports records.
        /// </summary>
        /// <param name="records">The records to import</param>
        void ImportRecords( [NotNull] IEnumerable<IObjectReader> records);
    }


    /// <summary>
    /// Service for reading multiple records from a reader and writing them into an entity adapter.
    /// </summary>
    class RecordImporter : IRecordImporter
    {
        // http://docs.autofac.org/en/latest/advanced/delegate-factories.html
        public delegate IRecordImporter Factory( IReaderToEntityAdapter readerToEntityAdapter, IImportReporter importReporter, ApiResourceMapping resourceMapping, bool testRun );

        private readonly IEntitySaver _entitySaver;
        private readonly ApiResourceMapping _resourceMapping;
        private readonly bool _mergeRecords;
        private readonly bool _suppressWorkflows;
        private readonly bool _testRun;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="readerToEntityAdapter">Adapter for reading content and storing in entities.</param>
        /// <param name="entitySaver">Service for saving entities in bulk.</param>
        /// <param name="importReporter">Service for receiving import progress.</param>
        /// <param name="resourceMapping">The resource mapping, to provde settings such as merging and workflow suppression.</param>
        /// <param name="testRun">Indicates that the importer is performing test runs.</param>
        public RecordImporter( IReaderToEntityAdapter readerToEntityAdapter, IEntitySaver entitySaver, IImportReporter importReporter, ApiResourceMapping resourceMapping, bool testRun )
        {
            if ( readerToEntityAdapter == null )
                throw new ArgumentNullException( nameof( readerToEntityAdapter ) );
            if ( entitySaver == null )
                throw new ArgumentNullException( nameof( entitySaver ) );
            if ( importReporter == null )
                throw new ArgumentNullException( nameof( importReporter ) );
            if ( resourceMapping == null )
                throw new ArgumentNullException( nameof( resourceMapping ) );

            _entitySaver = entitySaver;
            ReaderToEntityAdapter = readerToEntityAdapter;
            Reporter = importReporter;
            _resourceMapping = resourceMapping;
            _mergeRecords = resourceMapping.ImportMergeExisting == true;
            _suppressWorkflows = resourceMapping.MappingSuppressWorkflows == true;
            _testRun = testRun;
        }

        /// <summary>
        /// Adapter for converting records to entities.
        /// </summary>
        public IReaderToEntityAdapter ReaderToEntityAdapter { get; set; }

        /// <summary>
        /// Target for progress reporting.
        /// </summary>
        public IImportReporter Reporter { get; set; }

        /// <summary>
        /// Imports records.
        /// </summary>
        /// <param name="records">The records to import</param>
        public void ImportRecords(IEnumerable<IObjectReader> records )
        {
            if (Reporter == null)
                throw new InvalidOperationException();

            IReadOnlyCollection<ReaderEntityPair> newEntities = ReaderToEntityAdapter.CreateEntities( records, Reporter );

            using ( _suppressWorkflows ? new WorkflowRunContext { DisableTriggers = true } : null )
            using ( _mergeRecords ? new ResourceKeyHelper.OverwriteMatchingResources( ) : null )
            {
                SaveEntities( newEntities );
            }
        }

        /// <summary>
        /// Save the entities.
        /// </summary>
        /// <param name="newEntities">Mapping of readers to their entities.</param>
        private void SaveEntities( IReadOnlyCollection<ReaderEntityPair> newEntities )
        {
            var pairsToSave = newEntities
                .Where( pair => !Reporter.HasErrors( pair.ObjectReader ) && pair.Entity != null );


            if ( _testRun )
            {
                int count = pairsToSave.Count( );
                Reporter.ReportOk( count );
                Reporter.Flush( );
                return;
            }

            try
            {
                var entities = pairsToSave.Select( pair => pair.Entity );

                _entitySaver.SaveEntities( entities );

                int count = entities.Count( );
                Reporter.ReportOk( count );
            }
            catch
            {
                foreach ( var pair in pairsToSave )
                {
                    IObjectReader reader = pair.ObjectReader;
                    if ( Reporter.HasErrors( reader ) )
                        continue;

                    try
                    {
                        // Re-create entity, because it seems that a duplicate key violation in the batch save munches up the entity in the pair.
                        IEntity entity = ReaderToEntityAdapter.CreateEntity( reader, Reporter );
                        entity.Save( );
                        Reporter.ReportOk( );
                    }
                    catch ( Exception ex )
                    {
                        Reporter.ReportError( pair.ObjectReader, ex.Message );
                    }
                }
            }
            finally
            {
                Reporter.Flush( );
            }
        }
    }
}
