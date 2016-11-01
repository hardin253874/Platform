// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    /// Creates a sample table from a spreadsheet.
    /// </summary>
    internal class SampleDataCreator
    {
        const int NumberOfSampleRows = 10;

        /// <summary>
        /// Read sample data from a spreadsheet reader.
        /// </summary>
        /// <param name="stream">The spreadsheet.</param>
        /// <param name="settings">Settings.</param>
        /// <param name="service">The reader.</param>
        /// <returns></returns>
        public SampleTable CreateSample( Stream stream, DataFileReaderSettings settings, IDataFileReaderService service )
        {
            if ( stream == null )
                throw new ArgumentNullException( nameof( stream ) );
            if ( settings == null )
                throw new ArgumentNullException( nameof( settings ) );
            if ( service == null )
                throw new ArgumentNullException( nameof( service ) );

            // Read field list
            using ( IDataFile dataFile = service.OpenDataFile( stream, settings ) )
            {
                SheetMetadata metadata = dataFile.ReadMetadata( );

                SampleTable table = new SampleTable( );
                table.Columns = metadata.Fields.Select(
                    field =>
                        new SampleColumn
                        {
                            ColumnName = field.Key,
                            Name = field.Title
                        }
                    ).ToList( );

                // Read records
                var records = dataFile.GetObjects( ).Take( NumberOfSampleRows );

                // Convert to sample rows
                List<SampleRow> sampleRows = new List<SampleRow>( );

                foreach ( IObjectReader record in records )
                {
                    // Read values
                    var values = metadata.Fields.Select( field =>
                    {
                        try
                        {
                            return record.GetString( field.Key );
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    } ).ToList( );

                    // Create sample row
                    SampleRow row = new SampleRow
                    {
                        Values = values
                    };
                    sampleRows.Add( row );
                }

                table.Rows = sampleRows;
                return table;
            }
        }
    }
}
