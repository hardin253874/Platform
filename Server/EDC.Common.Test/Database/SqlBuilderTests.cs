// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database;
using NUnit.Framework;

namespace EDC.Test.Database
{
    [TestFixture]
    public class SqlBuilderTests
    {
        [Test]
        [TestCase( "a", "%", "%", "%a%" )]
        [TestCase( "a", null, null, "a" )]
        [TestCase( "_", null, null, "[_]" )]
        [TestCase( "[", null, null, "[[]" )]
        [TestCase( "%", null, null, "[%]" )]
        [TestCase( "_[%'", "%", "%", "%[_][[][%]'%" )]
        public void Test_BuildSafeLikeParameter( string input, string prefix, string suffix, string expected )
        {
            string actual = SqlBuilder.BuildSafeLikeParameter( input, prefix, suffix );
            Assert.That( actual, Is.EqualTo( expected ) );
        }

        [Test]
        [TestCase( "a", "%", "%", "'%a%'" )]
        [TestCase( "a", null, null, "'a'" )]
        [TestCase( "_", null, null, "'[_]'" )]
        [TestCase( "[", null, null, "'[[]'" )]
        [TestCase( "%", null, null, "'[%]'" )]
        [TestCase( "_[%'", "%", "%", "'%[_][[][%]''%'" )]
        public void Test_BuildSafeLikeStatement( string input, string prefix, string suffix, string expected )
        {
            string expect2 = " like " + expected;
            string actual = SqlBuilder.BuildSafeLikeStatement( input, prefix, suffix );
            Assert.That( actual, Is.EqualTo( expect2 ) );
        }
    }
}
