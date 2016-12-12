// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiNow.Connector.Interfaces;

namespace ReadiNow.Connector.Processing
{
    /// <summary>
    /// Implementation of IImportReporter that throws a ConnectorRequestException whenever an error is reported.
    /// </summary>
    internal class ConnectorRequestExceptionReporter : IImportReporter
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static readonly ConnectorRequestExceptionReporter Instance = new ConnectorRequestExceptionReporter( );

        /// <summary>
        /// Report the failure to import a record.
        /// </summary>
        /// <param name="objectReader">The record that failed.</param>
        /// <param name="message">The cause of failure.</param>
        public void ReportError( IObjectReader objectReader, string message )
        {
            throw new ConnectorRequestException( message );
        }


        /// <summary>
        /// Report that rows were imported correctly.
        /// </summary>
        /// <param name="numberIncrement">The number of additional successfully imported records.</param>
        public void ReportOk( int numberIncrement = 1 )
        {
            // Do nothing
        }


        /// <summary>
        /// Flush reporing information out to the 'import run' entity.
        /// </summary>
        public void Flush( )
        {
            // Do nothing
        }


        /// <summary>
        /// Interface for service that can receive import results, as they occur.
        /// </summary>
        public bool HasErrors( IObjectReader objectReader )
        {
            return false;
        }
    }
}
