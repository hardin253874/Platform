// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Autofac;
using System;
using EDC.IO;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using Moq;
using ReadiNow.Connector.ImportSpreadsheet;
using ReadiNow.Connector.Payload;
using ReadiNow.Connector.Service;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Processing;
using ReadiNow.Connector.Spreadsheet;
using ReadiNow.Core;

namespace ReadiNow.Connector.Test
{
    [TestFixture]
    public class ActivationTests
    {
        [Test]
        public void Autofac_IComponentContext( )
        {
            IComponentContext instance = Factory.Current.Resolve<IComponentContext>( );
            Assert.That( instance, Is.Not.Null );
        }

        [Test]
        public void IDynamicObjectReaderService_Instance( )
        {
            IDynamicObjectReaderService instance = Factory.Current.Resolve<IDynamicObjectReaderService>( );
            Assert.That( instance, Is.TypeOf<JilDynamicObjectReaderService>( ) );
        }

        [Test]
        public void IConnectorService_Instance( )
        {
            IConnectorService instance = Factory.Current.Resolve<IConnectorService>( );
            Assert.That( instance, Is.TypeOf<ExceptionServiceLayer>( ) );

            instance = (instance as ExceptionServiceLayer).InnerService;
            Assert.That(instance, Is.TypeOf<ApiKeySecurity>());

            ApiKeySecurity apiKeySecurity = ( ApiKeySecurity ) instance;
            Assert.That( apiKeySecurity.InnerService, Is.TypeOf<ConnectorService>( ) );
        }

        [Test]
        public void IEndpointResolver_Instance( )
        {
            IEndpointResolver instance = Factory.Current.Resolve<IEndpointResolver>( );
            Assert.That( instance, Is.TypeOf<EndpointResolver>( ) );
        }

        [Test]
        public void IReaderToEntityAdapterProvider_Instance( )
        {
            IReaderToEntityAdapterProvider instance = Factory.Current.Resolve<IReaderToEntityAdapterProvider>( );
            Assert.That( instance, Is.TypeOf<ReaderToEntityAdapterProvider>( ) );
        }

        [Test]
        public void IResourceResolverProvider_Instance( )
        {
            IResourceResolverProvider instance = Factory.Current.Resolve<IResourceResolverProvider>( );
            Assert.That( instance, Is.TypeOf<ResourceResolverProvider>( ) );
        }

        [Test]
        public void ISpreadsheetImporter_Instance( )
        {
            ISpreadsheetImporter instance = Factory.Current.Resolve<ISpreadsheetImporter>( );
            Assert.That( instance, Is.TypeOf<SpreadsheetImporter>( ) );
        }

        [Test]
        public void ISpreadsheetInspector_Instance( )
        {
            ISpreadsheetInspector instance = Factory.Current.Resolve<ISpreadsheetInspector>( );
            Assert.That( instance, Is.TypeOf<SpreadsheetInspector>( ) );
        }

        [Test]
        public void CsvFileReaderService_Instance( )
        {
            Func<ImportFormat, IDataFileReaderService> factory = Factory.Current.Resolve< Func<ImportFormat, IDataFileReaderService> >( );
            IDataFileReaderService instance = factory( ImportFormat.CSV );
            Assert.That( instance, Is.TypeOf<CsvFileReaderService>( ) );
        }

        [Test]
        public void ExcelFileReaderService_Instance( )
        {
            Func<ImportFormat, IDataFileReaderService> factory = Factory.Current.Resolve<Func<ImportFormat, IDataFileReaderService>>( );
            IDataFileReaderService instance = factory( ImportFormat.Excel );
            Assert.That( instance, Is.TypeOf<ExcelFileReaderService>( ) );
        }

        [Test]
        public void ExcelFileReaderService_Instance_ZipFiles( )
        {
            Func<ImportFormat, IDataFileReaderService> factory = Factory.Current.Resolve<Func<ImportFormat, IDataFileReaderService>>( );
            IDataFileReaderService instance = factory( ImportFormat.CSV | ImportFormat.Zip );
            Assert.That( instance, Is.TypeOf<ZipFileReaderService>( ) );

            ZipFileReaderService zipReader = ( ZipFileReaderService ) instance;
            Assert.That( zipReader.InnerReaderService, Is.TypeOf<CsvFileReaderService>( ) );
        }

        [Test]
        public void IImportRunWorker_Instance( )
        {
            IImportRunWorker importRunWorker = Factory.Current.Resolve<IImportRunWorker>( );
            Assert.That( importRunWorker, Is.InstanceOf< ImportRunWorker>() );

            ImportRunWorker worker = ( ImportRunWorker ) importRunWorker;

            Assert.That( worker.FileRepository, Is.InstanceOf<FileRepository>() );
            FileRepository fileRepo = ( FileRepository ) worker.FileRepository;
            Assert.That( fileRepo.Name, Is.EqualTo( "Temporary" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void RecordImporter_Factory( )
        {
            IReaderToEntityAdapter adapter = new Mock<IReaderToEntityAdapter>( ).Object;
            IImportReporter reporter = new Mock<IImportReporter>().Object;
            ApiResourceMapping mapping = new ApiResourceMapping( );

            RecordImporter.Factory recordImporterFactory = Factory.Current.Resolve<RecordImporter.Factory>( );
            IRecordImporter recordImporter = recordImporterFactory(adapter, reporter, mapping, true );

            Assert.That( recordImporter, Is.TypeOf<RecordImporter>( ) );

            RecordImporter importer = (RecordImporter) recordImporter;
            Assert.That( importer.ReaderToEntityAdapter, Is.EqualTo( adapter ) );
            Assert.That( importer.Reporter, Is.EqualTo( reporter ) );
        }
    }
}
