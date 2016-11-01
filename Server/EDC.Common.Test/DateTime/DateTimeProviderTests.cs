// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using NUnit.Framework;
using ReadiNow.Common;

namespace EDC.Test
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    class DateTimeProviderTests
    {
        [Test]
        public void Test_Now_ReturnLocalKind( )
        {
            DateTime result = DateTimeProvider.Instance.Now;
            Assert.That( result.Kind, Is.EqualTo( DateTimeKind.Local ) );
        }

        [Test]
        public void Test_UtcNow_ReturnUtcKind( )
        {
            DateTime result = DateTimeProvider.Instance.UtcNow;
            Assert.That( result.Kind, Is.EqualTo( DateTimeKind.Utc ) );
        }

        [Test]
        public void Test_Today_ReturnLocalKind( )
        {
            DateTime result = DateTimeProvider.Instance.Today;
            Assert.That( result.Kind, Is.EqualTo( DateTimeKind.Local ) );
            Assert.That( result.TimeOfDay, Is.EqualTo( TimeSpan.Zero ) );
        }
    }
}
