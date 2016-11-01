// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;
using ReadiNow.Connector.Interfaces;

namespace ReadiNow.Connector.Payload
{
    /// <summary>
    /// Applies an IObjectReader to an entity.
    /// </summary>
    /// <remarks>
    /// E.g. copies data in from a JSON object structure into the entity.
    /// Does not perform the entity save.
    /// Note: the ReaderToEntityAdapterProvider performs all static operations up front to maximise performance.
    /// </remarks>
    class ReaderToEntityAdapter : IReaderToEntityAdapter
    {
        private readonly List<MemberProcessor> _memberProcessors;

        private readonly Func<IEntity> _instanceFactory;

        private readonly MemberProcessor _defaultsProcessor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="memberProcessors">A list of processors that know what members to process and how to process them.</param>
        /// <param name="instanceFactory">Callback for creating entity instances.</param>
        /// <param name="defaultsProcessor">A processor for setting default values.</param>
        internal ReaderToEntityAdapter( List<MemberProcessor> memberProcessors, Func<IEntity> instanceFactory, [CanBeNull] MemberProcessor defaultsProcessor )
        {
            if ( memberProcessors == null )
                throw new ArgumentNullException( "memberProcessors" );
            if ( instanceFactory == null )
                throw new ArgumentNullException( "instanceFactory" );

            _memberProcessors = memberProcessors;
            _instanceFactory = instanceFactory;
            _defaultsProcessor = defaultsProcessor;
        }

        /// <summary>
        /// Sets fields. Does not call save.
        /// </summary>
        /// <param name="reader">The source of data for this entity.</param>
        /// <param name="entity">The entity to be filled.</param>
        /// <param name="reporter">Repoter for errors.</param>
        /// <exception cref="System.ArgumentNullException">
        /// reader
        /// or
        /// entity
        /// </exception>
        public void FillEntity( IObjectReader reader, IEntity entity, IImportReporter reporter )
        {
            if ( reader == null )
                throw new ArgumentNullException( "reader" );
            if ( entity == null )
                throw new ArgumentNullException( "entity" );

            var pair = new ReaderEntityPair(reader, entity);
            FillEntities(pair.ToEnumerable(), reporter );
        }

        /// <summary>
        /// Sets fields on multiple entities. Does not call save.
        /// </summary>
        /// <param name="readerEntityPairs">The source of the object readers and the corresponding entities to write them in to.</param>
        /// <param name="reporter">Repoter for errors.</param>
        /// <exception cref="FormatException">Thrown if the object reader value was unacceptably formatted for the target field.</exception>
        public void FillEntities( IEnumerable<ReaderEntityPair> readerEntityPairs, IImportReporter reporter )
        {
            if ( readerEntityPairs == null )
                throw new ArgumentNullException( "readerEntityPairs" );

            foreach ( var memberProcessor in _memberProcessors )
            {
                memberProcessor.Action( readerEntityPairs, reporter );
            }
        }

        /// <summary>
        /// Creates a new instance and fills the fields. Does not call save.
        /// </summary>
        /// <param name="reader">The source of data for this entity.</param>
        /// <param name="reporter">Repoter for errors.</param>
        public IEntity CreateEntity( IObjectReader reader, IImportReporter reporter )
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            IReadOnlyCollection<ReaderEntityPair> result = CreateEntities(reader.ToEnumerable(), reporter );
            return result.First().Entity;
        }

        /// <summary>
        /// Creates new instance and fills the fields. Does not call save.
        /// </summary>
        /// <param name="readers">The list of readers for these entities.</param>
        /// <param name="reporter">Repoter for errors.</param>
        /// <exception cref="FormatException">Thrown if the object reader value was unacceptably formatted for the target field.</exception>
        public IReadOnlyCollection<ReaderEntityPair> CreateEntities( IEnumerable<IObjectReader> readers, IImportReporter reporter )
        {
            if ( readers == null )
                throw new ArgumentNullException( "readers" );

            // Create instances
            List<ReaderEntityPair> pairs;
            pairs = readers.Select( reader => new ReaderEntityPair( reader, _instanceFactory( ) ) ).ToList( );

            // Set default values
            _defaultsProcessor?.Action( pairs, reporter );

            // Fill
            FillEntities( pairs, reporter );

            return pairs;
        }
    }



}
