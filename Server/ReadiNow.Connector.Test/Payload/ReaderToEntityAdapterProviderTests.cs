// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Model;
using ReadiNow.Connector.Payload;
using ReadiNow.Connector.Interfaces;
using EDC.ReadiNow.Core;
using ReadiNow.Connector.Processing;

namespace ReadiNow.Connector.Test.Payload
{
    /// <summary>
    /// Tests for ReaderToEntityAdapterProvider class.
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    public class ReaderToEntityAdapterProviderTests
    {
        [Test]
        public void Test_EmptyEntity( )
        {
            var provider = GetProvider( );
            var settings = new ReaderToEntityAdapterSettings();

            Assert.Throws<ArgumentOutOfRangeException>( ( ) => provider.GetAdapter( 0, settings ) );
            Assert.Throws<ArgumentOutOfRangeException>( ( ) => provider.GetAdapter( -1, settings ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NullSettings( )
        {
            var provider = GetProvider( );
            var mapping = new ApiResourceMapping( );

            Assert.Throws<ArgumentNullException>( ( ) => provider.GetAdapter( mapping.Id, null ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_InvalidMapping( )
        {
            var provider = GetProvider( );
            var settings = new ReaderToEntityAdapterSettings( );
            var notAMapping = new TopMenu( );

            Assert.Throws<ArgumentException>( ( ) => provider.GetAdapter( notAMapping.Id, settings ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_FillEntity_Empty( )
        {
            var provider = GetProvider( );
            var mapping = new ApiResourceMapping( );
            var type = new EntityType( );
            mapping.MappedType = type;
            type.Save( );
            var settings = new ReaderToEntityAdapterSettings( );

            var adapter = provider.GetAdapter( mapping.Id, settings );
            Assert.That( adapter, Is.Not.Null );

            var mockReader = new Mock<IObjectReader>( MockBehavior.Strict );
            var mockEntity = new Mock<IEntity>( MockBehavior.Strict );

            // Ensure nothing gets called on either
            adapter.FillEntity( mockReader.Object, mockEntity.Object, ConnectorRequestExceptionReporter.Instance );

            mockReader.VerifyAll( );
            mockEntity.VerifyAll( );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CreateEntity_Empty( )
        {
            var provider = GetProvider( );
            var mapping = new ApiResourceMapping( );
            var type = new EntityType( );
            mapping.MappedType = type;
            type.Save( );
            var settings = new ReaderToEntityAdapterSettings( );

            var adapter = provider.GetAdapter( mapping.Id, settings );
            Assert.That( adapter, Is.Not.Null );

            var mockReader = new Mock<IObjectReader>( MockBehavior.Strict );

            // Ensure nothing gets called on either
            IEntity instance = adapter.CreateEntity( mockReader.Object, ConnectorRequestExceptionReporter.Instance );
            Assert.That( instance, Is.Not.Null );
            Assert.That( instance.TypeIds.First(), Is.EqualTo(type.Id) );

            mockReader.VerifyAll( );
        }

        private IReaderToEntityAdapterProvider GetProvider( )
        {
            IReaderToEntityAdapterProvider provider = Factory.Current.Resolve<IReaderToEntityAdapterProvider>( );
            Assert.That( provider, Is.InstanceOf<ReaderToEntityAdapterProvider>( ) );
            return provider;
        }
    }
}
