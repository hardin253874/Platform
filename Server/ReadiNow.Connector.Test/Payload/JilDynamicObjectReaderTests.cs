// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Dynamic;
using ReadiNow.Connector.Payload;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Core;
using NUnit.Framework.Constraints;
using System.Globalization;

namespace ReadiNow.Connector.Test.Payload
{
    [TestFixture]
    public class JilDynamicObjectReaderTests
    {
        [TestCase( "{'a':123}", 123 )]
        [TestCase( "{'a':null}", null )]
        [TestCase( "{'z':null}", null )]
        public void Test_GetInt(string json, int? expect)
        {
            IObjectReader reader = GetReader( json );
            Assert.That( reader.GetInt( "a" ), Is.EqualTo( expect ) );
        }

        [TestCase( "{'a':'abc'}", "abc" )]
        [TestCase( "{'a':'123'}", "123" )]
        [TestCase( "{'a':null}", null )]
        [TestCase("{'z':null}", null)]
        public void Test_GetString( string json, string expect )
        {
            IObjectReader reader = GetReader( json );
            Assert.That( reader.GetString( "a" ), Is.EqualTo( expect ) );
        }

        [TestCase( "{'a':123}", 123.0 )]
        [TestCase( "{'a':123.4}", 123.4 )]
        [TestCase( "{'a':null}", null )]
        [TestCase("{'z':null}", null)]
        public void Test_GetDecimal( string json, double? expect )
        {
            decimal? expect2 = ( decimal? ) expect;

            IObjectReader reader = GetReader( json );
            Assert.That( reader.GetDecimal( "a" ), Is.EqualTo( expect2 ) );
        }

        [TestCase( "{'a':true}", true )]
        [TestCase( "{'a':false}", false )]
        [TestCase( "{'a':null}", null )]
        [TestCase("{'z':null}", null)]
        public void Test_GetBoolean( string json, bool? expect )
        {
            IObjectReader reader = GetReader( json );
            Assert.That( reader.GetBoolean( "a" ), Is.EqualTo( expect ) );
        }

        [TestCase("{'a':null}")]
        [TestCase("{'z':null}")]
        public void Test_GetObject_Null( string json )
        {
            IObjectReader reader = GetReader( json );
            Assert.That( reader.GetObject( "a" ), Is.Null );
        }

        [TestCase( "{'a':{'b':1}}" )]
        public void Test_GetObject_NonNull( string json )
        {
            IObjectReader reader = GetReader( json );
            IObjectReader child = reader.GetObject( "a" );
            Assert.That( child, Is.Not.Null );
            Assert.That( child, Is.TypeOf<JilDynamicObjectReader>( ) );
            Assert.That( child.GetInt("b"), Is.EqualTo(1) );
        }

        [TestCase( "{'a':{'b':1}}" )]
        public void Test_GetObject_SameInstance( string json )
        {
            IObjectReader reader = GetReader( json );
            Assert.That( reader.GetObject( "a" ), Is.SameAs(reader.GetObject( "a" )) );
        }

        [TestCase( "{'a':null}" )]
        [TestCase( "{'z':null}" )]
        public void Test_GetObjectList_Null( string json )
        {
            IObjectReader reader = GetReader( json );
            IReadOnlyList<IObjectReader> list = reader.GetObjectList( "a" );

            Assert.That( list, Is.Null );
        }

        [TestCase( "{'a':[]}" )]
        public void Test_GetObjectList_Empty( string json )
        {
            IObjectReader reader = GetReader( json );
            IReadOnlyList<IObjectReader> list = reader.GetObjectList( "a" );

            Assert.That( list, Is.Not.Null );
            Assert.That( list, Is.Empty );
        }

        [TestCase( "{'a':[null]}" )]
        public void Test_GetObjectList_SingleNull( string json )
        {
            IObjectReader reader = GetReader( json );
            IReadOnlyList<IObjectReader> list = reader.GetObjectList( "a" );

            Assert.That( list, Is.Not.Null );
            Assert.That( list, Has.Count.EqualTo(1) );
            Assert.That( list [ 0 ], Is.Null );
        }

        [TestCase( "{'a':[{'b':1},{'c':2}]}" )]
        public void Test_GetObjectList_Values( string json )
        {
            IObjectReader reader = GetReader( json );
            IReadOnlyList<IObjectReader> list = reader.GetObjectList( "a" );

            Assert.That( list, Is.Not.Null );
            Assert.That( list, Has.Count.EqualTo( 2 ) );
            Assert.That( list [ 0 ], Is.Not.Null );
            Assert.That( list [ 1 ], Is.Not.Null );
            Assert.That( list [ 0 ].GetInt( "b" ), Is.EqualTo( 1 ) );
            Assert.That( list [ 1 ].GetInt( "c" ), Is.EqualTo( 2 ) );
        }

        [TestCase( "{'a':[{'b':1}]}" )]
        public void Test_GetObjectList_SameInstance( string json )
        {
            IObjectReader reader = GetReader( json );
            Assert.That( reader.GetObjectList( "a" ), Is.SameAs( reader.GetObjectList( "a" ) ) );
        }

        [TestCase( "null" )]
        public void Test_Static_GetObjectList_Null( string json )
        {
            IDynamicMetaObjectProvider provider = GetDynamicProvider( json, Is.Null );
            IReadOnlyList<IObjectReader> list = JilDynamicObjectReader.GetObjectList( provider );

            Assert.That( list, Is.Null );
        }

        [TestCase( "[]" )]
        public void Test_Static_GetObjectList_Empty( string json )
        {
            IDynamicMetaObjectProvider provider = GetDynamicProvider( json );
            IReadOnlyList<IObjectReader> list = JilDynamicObjectReader.GetObjectList( provider );

            Assert.That( list, Is.Not.Null );
            Assert.That( list, Is.Empty );
        }

        [TestCase( "[{'a':1}]" )]
        public void Test_Static_GetObjectList_Single( string json )
        {
            IDynamicMetaObjectProvider provider = GetDynamicProvider( json );
            IReadOnlyList<IObjectReader> list = JilDynamicObjectReader.GetObjectList( provider );

            Assert.That( list, Is.Not.Null );
            Assert.That( list, Has.Count.EqualTo( 1 ) );
            Assert.That( list [ 0 ], Is.Not.Null );
            Assert.That( list [ 0 ].GetInt( "a" ), Is.EqualTo( 1 ) );
        }

        [TestCase("{'a':['abc']}", 1, "abc")]
        [TestCase("{'a':['abc','def']}", 2, "abc")]
        [TestCase("{'a':['']}", 1, "")]
        [TestCase("{'a':[]}", 0, null)]
        [TestCase("{'a':null}", null, null)]
        [TestCase("{'z':null}", null, null)]
        public void Test_GetStringList(string json, int? expectCount, string expectFirst)
        {
            IObjectReader reader = GetReader(json);
            IReadOnlyList<string> list = reader.GetStringList("a");

            if (expectCount == null)
            {
                Assert.That(list, Is.Null);
                return;
            }
            else
            {
                Assert.That(list.Count, Is.EqualTo(expectCount));
                if (expectCount > 0)
                    Assert.That(list[0], Is.EqualTo(expectFirst));
            }
        }

        [TestCase( "{'a':['1234.1',null]}" )]
        public void Test_GetStringList_FormatException( string json )
        {
            IObjectReader reader = GetReader( json );
            Assert.Throws<FormatException>( ( ) => reader.GetStringList( "a" ) );
        }

        [TestCase("{'a':[1234]}", 1, 1234)]
        [TestCase("{'a':[1234, 5678]}", 2, 1234)]
        [TestCase("{'a':[-1234]}", 1, -1234)]
        [TestCase("{'a':[0]}", 1, 0)]
        [TestCase("{'a':[]}", 0, null)]
        [TestCase("{'a':null}", null, null)]
        [TestCase("{'z':null}", null, null)]
        public void Test_GetIntList(string json, int? expectCount, int expectFirst)
        {
            IObjectReader reader = GetReader(json);
            IReadOnlyList<int> list = reader.GetIntList("a");

            if (expectCount == null)
            {
                Assert.That(list, Is.Null);
                return;
            }
            else
            {
                Assert.That(list.Count, Is.EqualTo(expectCount));
                if (expectCount > 0)
                    Assert.That(list[0], Is.EqualTo(expectFirst));
            }
        }

        [TestCase( "{'a':[1234,null]}" )]
        public void Test_GetIntList_FormatException( string json )
        {
            IObjectReader reader = GetReader( json );
            Assert.Throws<FormatException>( ( ) => reader.GetIntList( "a" ) );
        }

        [TestCase("{'a':[1234.1]}", 1, 1234.1)]
        [TestCase("{'a':[1234.1, 5678.1]}", 2, 1234.1)]
        [TestCase("{'a':[-1234]}", 1, -1234.0)]
        [TestCase("{'a':[0]}", 1, 0.0)]
        [TestCase("{'a':[]}", 0, null)]
        [TestCase("{'a':null}", null, null)]
        [TestCase("{'z':null}", null, null)]
        public void Test_GetDecimalList(string json, int? expectCount, double? expectFirst)
        {
            IObjectReader reader = GetReader(json);
            IReadOnlyList<decimal> list = reader.GetDecimalList("a");

            if (expectCount == null)
            {
                Assert.That(list, Is.Null);
                return;
            }
            else
            {
                decimal? expectDecimal = null;
                if (expectFirst != null)
                    expectDecimal = (decimal)expectFirst.Value;
                Assert.That(list.Count, Is.EqualTo(expectCount));
                if (expectCount > 0)
                    Assert.That(list[0], Is.EqualTo(expectFirst));
            }
        }

        [TestCase( "{'a':[1234.1,null]}")]
        public void Test_GetDecimalList_FormatException( string json )
        {
            IObjectReader reader = GetReader( json );
            Assert.Throws<FormatException>( ( ) => reader.GetDecimalList( "a" ) );
        }

        public static IObjectReader GetReader( string json )
        {
            IDynamicMetaObjectProvider dynamicProvider = GetDynamicProvider( json );
            return Factory.Current.Resolve<IDynamicObjectReaderService>( ).GetObjectReader( dynamicProvider );
            //return new JilDynamicObjectReader( dynamicProvider );
        }

        static IDynamicMetaObjectProvider GetDynamicProvider( string json, IResolveConstraint constraint = null )
        {
            if ( json != null )
            {
                json = json.Replace( "'", @"""" );
            }

            object raw = JilHelpers.Deserialize<object>( json );

            if ( constraint != null )
            {
                Assert.That( raw, constraint );
            }
            else
            {
                Assert.That( raw, Is.InstanceOf<IDynamicMetaObjectProvider>( ) );
            }
            IDynamicMetaObjectProvider dynamicProvider = ( IDynamicMetaObjectProvider ) raw;
            return dynamicProvider;
        }
    }
}
