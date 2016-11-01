// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace EDC.Test
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    class StringHelpersTests
    {
        [TestCase( null, null )]
        [TestCase( "", "" )]
        [TestCase( "\r", "" )]
        [TestCase( "\n", "" )]
        [TestCase( "\r\n", "" )]
        [TestCase( "A\rB", "A B" )]
        [TestCase( "A\nB", "A B" )]
        [TestCase( "A\r\nB", "A B" )]
        [TestCase( "\nA\n\nB\n", "A B" )]
        [TestCase( "\n A \n\n B \n", "A B" )]
        [TestCase( "\nA\n", "A" )]
        [TestCase( "  \n Multi \n \n Line \n ", "Multi Line" )]
        public void ToSingleLine( string input, string expected )
        {
            string actual = StringHelpers.ToSingleLine( input );
            Assert.That( actual, Is.EqualTo( expected ) );
        }
    }
}
