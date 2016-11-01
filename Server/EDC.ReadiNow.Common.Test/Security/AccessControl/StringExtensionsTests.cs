// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Core;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void Truncate_NullValue()
        {
            Assert.That(
                () => StringExtensions.Truncate(null, 1),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("value"));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        public void Truncate_IncorrectMaxLength(int maxLength)
        {
            Assert.That(
                () => "foo".Truncate(maxLength),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("maxLength"));
        }

        [Test]
        [TestCase("", 1, "")]
        [TestCase("a", 1, "a")]
        [TestCase("ab", 1, "a")]
        [TestCase("", 2, "")]
        [TestCase("a", 2, "a")]
        [TestCase("ab", 2, "ab")]
        [TestCase("abc", 2, "ab")]
        public void Truncate(string value, int maxLength, string expectedResult)
        {
            Assert.That(
                value.Truncate(maxLength),
                Is.EqualTo(expectedResult));
        }

        [Test]
        public void Chunk_NullValue()
        {
            Assert.That(
                () => StringExtensions.Chunk(null, 1),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("value"));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        public void Chunk_IncorrectMaxLength(int chunkSize)
        {
            Assert.That(
                () => "foo".Chunk(chunkSize),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("chunkSize"));
        }

        [Test]
        [TestCase("", 1, "")]
        [TestCase("a", 1, "a")]
        [TestCase("ab", 1, "a,b")]
        [TestCase("", 2, "")]
        [TestCase("a", 2, "a")]
        [TestCase("ab", 2, "ab")]
        [TestCase("abc", 2, "ab,c")]
        [TestCase("abcd", 2, "ab,cd")]
        public void Chunk(string value, int chunkSize, string expectedResult)
        {
            Assert.That(
                value.Chunk(chunkSize),
                Is.EquivalentTo(expectedResult.Split(new [] {","}, StringSplitOptions.None)));            
        }
    }
}
