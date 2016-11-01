// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using ReadiNow.Annotations;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Service;

namespace ReadiNow.Connector.Processing
{
    /// <summary>
    /// Mechanism for receiving import progress updates and writing them to an import run config.
    /// </summary>
    class ImportRunReporter : IImportReporter
    {
        private readonly HashSet<IObjectReader> _readersWithErrors = new HashSet<IObjectReader>( );
        private ImportRun _importRun;

        private int _recordsFailed;
        private int _recordsSucceeded;
        private StringBuilder _message;
        private List<string> _failReasons;
        private bool _messageChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importRun">A Writable reference to the import run entity that represents this import.</param>
        public ImportRunReporter( [NotNull] ImportRun importRun ) 
        {
            if ( importRun == null )
                throw new ArgumentNullException( nameof( importRun ) );
            if ( importRun.IsReadOnly )
                throw new ArgumentException( "Import run entity is read-only.", nameof( importRun ) );

            _importRun = importRun;
        }


        /// <summary>
        /// Report the failure to import a record.
        /// </summary>
        /// <param name="objectReader">The record that failed.</param>
        /// <param name="message">The cause of failure.</param>
        public void ReportError( IObjectReader objectReader, string message )
        {
            // Strip off any API connector platform error codes
            string errorMessage = Messages.GetErrorText( message );

            string location = objectReader.GetLocation( );
            if ( location != null )
                errorMessage = string.Concat( location, ": ", errorMessage );

            if ( _failReasons == null )
                _failReasons = new List<string>( );
            if ( _message == null )
                _message = new StringBuilder( _importRun.ImportMessages );

            if ( !_readersWithErrors.Contains( objectReader ) )
            {
                _readersWithErrors.Add( objectReader );
                _recordsFailed++;
            }

            _failReasons.Add( errorMessage );
            _messageChanged = true;
        }


        /// <summary>
        /// Report that rows were imported correctly.
        /// </summary>
        /// <param name="numberIncrement">The number of additional successfully imported records.</param>
        public void ReportOk( int numberIncrement = 1 )
        {
            _recordsSucceeded += numberIncrement;
        }


        /// <summary>
        /// Flush reporing information out to the 'import run' entity.
        /// </summary>
        public void Flush( )
        {
            // Reorder messages so that they're in ascending row order
            // (Batching causes messages to appear all over the place)
            if ( _failReasons != null )
            {
                RowNumberComparer comparer = new RowNumberComparer( );
                _failReasons.Sort( comparer );

                // Move messages into string builder.
                foreach ( string message in _failReasons )
                    _message.AppendLine( message );
                _failReasons.Clear( );
            }

            using ( new SecurityBypassContext( ) )
            {
                _importRun.ImportRecordsSucceeded = _recordsSucceeded;
                _importRun.ImportRecordsFailed = _recordsFailed;
                if ( _messageChanged )
                    _importRun.ImportMessages = _message.ToString( );
                _importRun.Save( );
            }
        }

        /// <summary>
        /// Returns true if errors were seen for the object.
        /// </summary>
        /// <param name="objectReader">The object reader.</param>
        public bool HasErrors( IObjectReader objectReader )
        {
            return _readersWithErrors.Contains( objectReader );
        }
    }

    public class RowNumberComparer : IComparer<string>
    {
        public int Compare( string x, string y )
        {
            if ( x == null )
                return 1;
            if ( y == null )
                return -1;
            int xStart = x.IndexOf( ' ' );
            int yStart = y.IndexOf( ' ' );
            int xEnd = x.IndexOf( ':', xStart );
            int yEnd = y.IndexOf( ':', yStart );
            if ( xStart == -1 || xEnd == -1 )
                return 1;
            if ( yStart == -1 || yEnd == -1 )
                return -1;

            string xPart = x.Substring( xStart, xEnd - xStart );
            string yPart = y.Substring( yStart, yEnd - yStart );

            int xNum;
            int yNum;
            if ( !int.TryParse( xPart, out xNum ) )
                return 1;
            if ( !int.TryParse( yPart, out yNum ) )
                return -1;

            return Comparer<int>.Default.Compare( xNum, yNum );
        }
    }
}
