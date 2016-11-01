// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiNow.Annotations;

namespace ReadiNow.Connector.Interfaces
{
    /// <summary>
    /// Creates an IReaderToEntityAdapter that can apply an IObjectReader to an entity.
    /// </summary>
    public interface IReaderToEntityAdapterProvider
    {
        /// <summary>
        /// Creates an adapter that knows how to write objects (e.g. JSON) into entities for a particular type of mapping.
        /// </summary>
        /// <param name="apiResourceMappingId">ID of the API mapping entity model that describes how to map object data to entities.</param>
        /// <param name="settings">Adapter settings.</param>
        [NotNull]
        IReaderToEntityAdapter GetAdapter( long apiResourceMappingId, [NotNull] ReaderToEntityAdapterSettings settings );
    }


    /// <summary>
    /// Settings for adapter
    /// </summary>
    public class ReaderToEntityAdapterSettings
    {
        /// <summary>
        /// Name of the time zone that should be used whenever date-times need to be converted to local time.
        /// </summary>
        public string TimeZoneName { get; set; }

        /// <summary>
        /// If true, then the target member name is used for reporting. If false, then the source member name is used.
        /// </summary>
        public bool UseTargetMemberNameForReporting { get; set; }
    }
}
