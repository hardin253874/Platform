// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    /// <summary>
    /// Tests for UserRuleSet class.
    /// </summary>
    [TestFixture]
    public class UserRuleSetTests
    {
        [Test]
        public void Test_NullArgument( )
        {
            Assert.Throws<ArgumentNullException>( ( ) => new UserRuleSet( null ) );
        }

        [Test]
        public void Test_Empty( )
        {
            var set1 = new UserRuleSet( new List<long>( ) );
            var set2 = new UserRuleSet( new List<long>( ) );
            Assert.That( set1, Is.EqualTo( set2 ) );
            Assert.That( set1.GetHashCode(), Is.EqualTo(set2.GetHashCode()));
        }

        [Test]
        public void Test_Equals( )
        {
            var set1 = new UserRuleSet( new List<long>( ) { 1, 2, 3 } );
            var set2 = new UserRuleSet( new List<long>( ) { 1, 2, 3 } );
            Assert.That( Equals( set1, set2 ), Is.True );
        }

        [Test]
        public void Test_InOrder( )
        {
            var set1 = new UserRuleSet( new List<long>( ) { 1, 2, 3 } );
            var set2 = new UserRuleSet( new List<long>( ) { 1, 2, 3 } );
            Assert.That( set1, Is.EqualTo( set2 ) );
            Assert.That( set1.GetHashCode( ), Is.EqualTo( set2.GetHashCode( ) ) );
        }

        [Test]
        public void Test_OutOfOrder( )
        {
            var set1 = new UserRuleSet( new List<long>( ) { 3, 2, 1 } );
            var set2 = new UserRuleSet( new List<long>( ) { 2, 3, 1 } );
            Assert.That( set1, Is.EqualTo( set2 ) );
            Assert.That( set1.GetHashCode( ), Is.EqualTo( set2.GetHashCode( ) ) );
        }

        [Test]
        public void Test_Different( )
        {
            var set1 = new UserRuleSet( new List<long>( ) { 1, 2, 4 } );
            var set2 = new UserRuleSet( new List<long>( ) { 1, 2, 3 } );
            Assert.That( set1, Is.Not.EqualTo( set2 ) );
        }
    }
}
