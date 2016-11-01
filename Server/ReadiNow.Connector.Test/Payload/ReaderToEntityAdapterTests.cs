// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Connector.Payload;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Processing;

namespace ReadiNow.Connector.Test.Payload
{
    /// <summary>
    /// Tests for ReaderToEntityAdapter class.
    /// </summary>
    [TestFixture]
    public class ReaderToEntityAdapterTests
    {
        [Test]
        public void Test_NullProcessor( )
        {
            Assert.Throws<ArgumentNullException>( ( ) => new ReaderToEntityAdapter( null, null, null ) );
        }

        [TestCase( false )]
        [TestCase( true )]
        public void Test_FillEntity( bool fillEntities )
        {
            Mock<IObjectReader> mockReader;
            IObjectReader reader;
            Mock<IEntity> mockEntity;
            IEntity entity;
            ReaderToEntityAdapter adapter;
            List<MemberProcessor> memberProcessors;
            IImportReporter reporter = new Mock<IImportReporter>( ).Object;

            // Setup reader
            mockReader = new Mock<IObjectReader>( MockBehavior.Strict );
            mockReader.Setup( x => x.GetString( "member1" ) ).Returns( "value1" ).Verifiable( "member1 not called" );
            mockReader.Setup( x => x.GetString( "member2" ) ).Returns( "value2" ).Verifiable( "member2 not called" );
            reader = mockReader.Object;

            // Setup entity
            mockEntity = new Mock<IEntity>( MockBehavior.Strict );
            mockEntity.Setup( x => x.SetField( 1, "value1" ) ).Verifiable( "field1 not set" );
            mockEntity.Setup( x => x.SetField( 2, "value2" ) ).Verifiable( "field2 not set" );
            entity = mockEntity.Object;

            // Set up adapter
            int called = 0;
            memberProcessors = new List<MemberProcessor>( );
            memberProcessors.Add( new MemberProcessor( ( r, e, rpt ) =>
            {
                string value = r.GetString( "member1" );
                e.SetField( 1, value );
                called++;
            } ) );
            memberProcessors.Add( new MemberProcessor( ( r, e, rpt ) =>
            {
                string value = r.GetString( "member2" );
                e.SetField( 2, value );
                called++;
            } ) );
            adapter = new ReaderToEntityAdapter( memberProcessors, ()=> { throw new Exception( "Assert false" ); }, null );

            // Go!
            if ( fillEntities )
            {
                adapter.FillEntities( new ReaderEntityPair(reader, entity).ToEnumerable(), reporter );
            }
            else
            {
                adapter.FillEntity( reader, entity, ConnectorRequestExceptionReporter.Instance );
            }
            

            // Verify everything called
            mockReader.VerifyAll( );
            mockEntity.VerifyAll( );
            Assert.That( called, Is.EqualTo(2), "Processor should be called twice." );

            // Entity.Save should not be called, which gets verified here because of Strict mode.            
        }

        [TestCase( false )]
        [TestCase( true )]
        public void Test_CreateEntity( bool createEntities )
        {
            Mock<IObjectReader> mockReader;
            IObjectReader reader;
            Mock<IEntity> mockEntity;
            IEntity entity;
            ReaderToEntityAdapter adapter;
            List<MemberProcessor> memberProcessors;
            IImportReporter reporter = new Mock<IImportReporter>( ).Object;

            // Setup reader
            mockReader = new Mock<IObjectReader>( MockBehavior.Strict );
            mockReader.Setup( x => x.GetString( "member1" ) ).Returns( "value1" ).Verifiable( "member1 not called" );
            mockReader.Setup( x => x.GetString( "member2" ) ).Returns( "value2" ).Verifiable( "member2 not called" );
            reader = mockReader.Object;

            // Setup entity
            mockEntity = new Mock<IEntity>( MockBehavior.Strict );
            mockEntity.Setup( x => x.SetField( 1, "value1" ) ).Verifiable( "field1 not set" );
            mockEntity.Setup( x => x.SetField( 2, "value2" ) ).Verifiable( "field2 not set" );
            entity = mockEntity.Object;

            // Set up adapter
            int called = 0;
            memberProcessors = new List<MemberProcessor>( );
            memberProcessors.Add( new MemberProcessor( ( r, e, rpt ) =>
            {
                string value = r.GetString( "member1" );
                e.SetField( 1, value );
                called++;
            } ) );
            memberProcessors.Add( new MemberProcessor( ( r, e, rpt ) =>
            {
                string value = r.GetString( "member2" );
                e.SetField( 2, value );
                called++;
            } ) );
            Func<IEntity> instanceFactory = ( ) => mockEntity.Object;
            adapter = new ReaderToEntityAdapter( memberProcessors, instanceFactory, null );

            // Go!
            IEntity result;
            if ( createEntities )
                result = adapter.CreateEntities( reader.ToEnumerable( ), reporter ).First( ).Entity;
            else
                result = adapter.CreateEntity( reader, ConnectorRequestExceptionReporter.Instance );

            Assert.That( result, Is.SameAs( mockEntity.Object ) );

            // Verify everything called
            mockReader.VerifyAll( );
            mockEntity.VerifyAll( );
            Assert.That( called, Is.EqualTo( 2 ), "Processor should be called twice." );

            // Entity.Save should not be called, which gets verified here because of Strict mode.            
        }
    }
}
