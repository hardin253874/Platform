// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using NUnit.Framework;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Test.Model
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase( null, null )]
        [TestCase( "", null )]
        [TestCase( "abc", "abc" )]
        [TestCase( "abc\r\ndef", "abc\ndef" )]
        [TestCase( "abc\n\rdef", "abc\ndef" )]
        [TestCase( "abc\rdef", "abc\ndef" )]
        [TestCase( "abc\ndef", "abc\ndef" )]
        public void NormalizeForDatabase( string input, string expected )
        {
            string result = StringExtensions.NormalizeForDatabase( input );
            Assert.That( result, Is.EqualTo( expected ) );
        }

        [TestCase( null, null )]
        [TestCase( "", null )]
        [TestCase( "abc", "abc" )]
        [TestCase( "abc\r\ndef", "abc def" )]
        [TestCase( "abc\n\rdef", "abc def" )]
        [TestCase( "abc\rdef", "abc def" )]
        [TestCase( "abc\ndef", "abc def" )]
        [TestCase( "\r\nasdf", "asdf" )]
        [TestCase( "\n\rdef", "def" )]
        [TestCase( "\rdef", "def" )]
        [TestCase( "\ndef", "def" )]
        public void NormalizeForSingleLine( string input, string expected )
        {
            string result = StringExtensions.NormalizeForSingleLine( input );
            Assert.That( result, Is.EqualTo( expected ) );
        }
    }
}
