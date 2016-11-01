// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System;
using EDC.ReadiNow.Core;
using ReadiNow.DocGen;

namespace ReadiNow.DocGen.Test
{
    [TestFixture]
    public class ActivationTests
    {
        [Test]
        public void DocumentGenerator()
        {
            IDocumentGenerator instance = Factory.DocumentGenerator;
            Assert.That(instance, Is.TypeOf<Generator>());

            Generator generator = (Generator)instance;
            Assert.That(generator.ExternalServices, Is.Not.Null);
            Assert.That(generator.ExternalServices.ExpressionCompiler, Is.Not.Null);
            Assert.That(generator.ExternalServices.ExpressionRunner, Is.Not.Null);
            Assert.That(generator.ExternalServices.ScriptNameResolver, Is.Not.Null);
        }
    }
}
