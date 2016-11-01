// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;

namespace ReadiNow.Connector.Interfaces
{
    /// <summary>
    /// Applies an IObjectReader to an entity.
    /// </summary>
    /// <remarks>
    /// Typically the instance itself knows how to convert a specific type of entity.
    /// Use <see cref="IReaderToEntityAdapterProvider"/> to create an adapter for a specific API mapping.
    /// </remarks>
    public interface IReaderToEntityAdapter
    {
        /// <summary>
        /// Sets fields. Does not call save.
        /// </summary>
        /// <param name="reader">The source of data for this entity.</param>
        /// <param name="entity">The entity to be filled.</param>
        /// <param name="reporter">Optional. Reporter for errors.</param>
        /// <exception cref="FormatException">Thrown if the object reader value was unacceptably formatted for the target field.</exception>
        void FillEntity( [NotNull] IObjectReader reader, [NotNull] IEntity entity, [NotNull] IImportReporter reporter );

        /// <summary>
        /// Sets fields on multiple entities. Does not call save.
        /// </summary>
        /// <param name="readerEntityPairs">The source of the object readers and the corresponding entities to write them in to.</param>
        /// <param name="reporter">Optional. Reporter for errors.</param>
        void FillEntities( [NotNull] IEnumerable<ReaderEntityPair> readerEntityPairs, [NotNull] IImportReporter reporter );

        /// <summary>
        /// Creates a new instance and fills the fields. Does not call save.
        /// </summary>
        /// <param name="reader">The source of data for this entity.</param>
        /// <param name="reporter">Optional. Reporter for errors.</param>
        /// <exception cref="FormatException">Thrown if the object reader value was unacceptably formatted for the target field.</exception>
        [NotNull]
        IEntity CreateEntity( [NotNull] IObjectReader reader, [NotNull] IImportReporter reporter );

        /// <summary>
        /// Creates new instance and fills the fields. Does not call save.
        /// </summary>
        /// <param name="readers">The list of readers for these entities.</param>
        /// <param name="reporter">Optional. Reporter for errors.</param>
        [NotNull]
        IReadOnlyCollection<ReaderEntityPair> CreateEntities( [NotNull] IEnumerable<IObjectReader> readers, [NotNull] IImportReporter reporter );
    }


    /// <summary>
    /// Tuple of IObjectReader and IEntity.
    /// </summary>
    public class ReaderEntityPair : Tuple<IObjectReader, IEntity>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ReaderEntityPair(IObjectReader reader, IEntity entity) : base (reader, entity)
            {
            }

        /// <summary>
        /// The (source) object reader.
        /// </summary>
        public IObjectReader ObjectReader
        {
            get { return Item1; }
        }

        /// <summary>
        /// The (target) entity.
        /// </summary>
        public IEntity Entity
        {
            get { return Item2; }
        }
    }

}
