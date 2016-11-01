// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using ReadiNow.Expressions.Tree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Test.Builder
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class LanguageManagerTests
    {
        [Test]
        public void Test_HasCasts()
        {
            Assert.That(LanguageManager.Instance.Casts, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Test_HasFunctions()
        {
            Assert.That(LanguageManager.Instance.Functions, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Test_HasOperators()
        {
            Assert.That(LanguageManager.Instance.Operators, Has.Count.GreaterThan(0));
        }
    }
}
