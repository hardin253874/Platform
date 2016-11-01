// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.Core;
using NUnit.Framework;

namespace EDC.Test.Core
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

        [Test]
        public void Template_NullStr()
        {
            Assert.That(
                () => StringExtensions.Template(null, new Dictionary<string, string>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("str"));
        }

        [Test]
        public void Template_NullTemplate()
        {
            Assert.That(
                () => StringExtensions.Template("foo", null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("template"));
        }

        [Test]
        [TestCase("", "")]
        [TestCase("b", "b")]
        [TestCase("a", "A")]
        [TestCase("ab", "Ab")]
        [TestCase("ba", "bA")]
        [TestCase("baa", "bAA")]
        [TestCase("aab", "AAb")]
        [TestCase("aba", "AbA")]
        public void Template_SingleTemplateEntry(string str, string expectedResult)
        {
            Assert.That(
                str.Template(new Dictionary<string, string> { { "a", "A"} }),
                Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase("", "")]
        [TestCase("b", "b")]
        [TestCase("a", "A")]
        [TestCase("ab", "Ab")]
        [TestCase("ba", "bA")]
        [TestCase("baa", "bAA")]
        [TestCase("aab", "AAb")]
        [TestCase("aba", "AbA")]
        [TestCase("cbb", "cBB")]
        [TestCase("bbc", "BBc")]
        [TestCase("bcb", "bcb")]
        [TestCase("abb", "ABB")]
        [TestCase("bba", "BBA")]
        [TestCase("acbb", "AcBB")]
        [TestCase("bbca", "BBcA")]
        [TestCase("aabb", "AABB")]
        public void Template_TwoTemplateEntries(string str, string expectedResult)
        {
            Assert.That(
                str.Template(new Dictionary<string, string> { { "a", "A" }, { "bb", "BB" } }),
                Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase("", "")]
        [TestCase("b", "b")]
        [TestCase("a", "a")]
        [TestCase("<%=a", "<%=a")]
        [TestCase("<%=ab", "<%=ab")]
        [TestCase("a%>", "a%>")]
        [TestCase("ba%>", "ba%>")]
        [TestCase("<%=a%>", "A")]
        [TestCase("<%=a%>b", "Ab")]
        [TestCase("b<%=a%>", "bA")]
        [TestCase("b<%=a%>b", "bAb")]
        public void Template_Decorator(string str, string expectedResult)
        {
            Assert.That(
                str.Template(new Dictionary<string, string> { { "a", "A" } }, s => "<%=" + s + "%>"),
                Is.EqualTo(expectedResult));
        }
    }
}
