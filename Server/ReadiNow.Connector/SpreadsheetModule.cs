// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using Autofac.Extras.AttributeMetadata;
using EDC.IO;
using EDC.ReadiNow.Model;
using ReadiNow.Connector.ImportSpreadsheet;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Processing;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector
{
    /// <summary>
    /// Autofac dependency injection module for spreadsheet readers.
    /// </summary>
    public class SpreadsheetModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            // Register ImportSpreadsheetInterface
            builder.RegisterType<SpreadsheetImporter>( )
                .As<ISpreadsheetImporter>( );

            // Register SheetInspector
            // (internal support for ImportSpreadsheetInterface)
            builder.RegisterType<SpreadsheetInspector>( )
                .As<ISpreadsheetInspector>( );

            // Register a resolver function that can look up implementations by enum
            builder.Register<Func<ImportFormat, IDataFileReaderService>>(
                context =>
                {
                    var innerContext = context.Resolve<IComponentContext>( );
                    return importFormat => ResolveDataReader( innerContext, importFormat );
                } );

            // Excel reader
            builder.Register( context => new ExcelFileReaderService( ) )
                .Keyed<IDataFileReaderService>( ImportFormat.Excel );

            // CSV reader
            builder.Register( context => new CsvFileReaderService( ) )
                .Keyed<IDataFileReaderService>( ImportFormat.CSV );
            builder.Register( context => new CsvFileReaderService( ) )
                .Keyed<IDataFileReaderService>( ImportFormat.Tab );

            // Register ImportRunWorker
            // (internal)
            builder.RegisterType<ImportRunWorker>( )
                .As<IImportRunWorker>( )
                .WithAttributeFilter( );

            // Register RecordImporter
            // (internal)
            builder.RegisterType<RecordImporter>()
                .As<IRecordImporter>( );
        }

        private static IDataFileReaderService ResolveDataReader( IComponentContext context, ImportFormat importFormat )
        {
            ImportFormat baseFormat = importFormat & ~ImportFormat.Zip;
            var res = context.ResolveKeyed<IDataFileReaderService>( baseFormat );
            if ( ( importFormat & ImportFormat.Zip ) == ImportFormat.Zip )
            {
                res = new ZipFileReaderService( res );
            }
            return res;
        }
    }
}
