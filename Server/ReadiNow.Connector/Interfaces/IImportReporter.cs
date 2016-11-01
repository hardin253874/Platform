// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiNow.Annotations;

namespace ReadiNow.Connector.Interfaces
{
    /// <summary>
    /// Interface for service that can receive import results, as they occur.
    /// </summary>
    public interface IImportReporter
    {
        /// <summary>
        /// Indicates that an error occurred while processing the specified record.
        /// </summary>
        /// <param name="objectReader">The record that failed.</param>
        /// <param name="message">The reason for failure.</param>
        void ReportError( [NotNull] IObjectReader objectReader, [NotNull] string message );

        /// <summary>
        /// Indicates that one or more additional rows were successfully processed.
        /// </summary>
        /// <param name="number">The number of rows that succeeded.</param>
        void ReportOk(int number = 1);

        /// <summary>
        /// Write notification of results so far.
        /// </summary>
        void Flush( );

        /// <summary>
        /// Returns true if errors were seen for the object.
        /// </summary>
        /// <param name="objectReader">The object reader.</param>
        bool HasErrors( IObjectReader objectReader );
    }
}
