// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using ReadiNow.EntityGraph.GraphModel;
using ReadiNow.EntityGraph.Parser;

namespace ReadiNow.EntityGraph.Test
{
    [TestFixture]
    public class ActivationTests
    {
        [Test]
        public void GraphEntityRepository()
        {
            IEntityRepository instance = Factory.GraphEntityRepository;
            Assert.That(instance, Is.TypeOf<IdResolvingEntityRepository>());

            IdResolvingEntityRepository idResolving = (IdResolvingEntityRepository)instance;
            Assert.That(idResolving.Inner, Is.TypeOf<GraphEntityRepository>());
        }

        [Test]
        public void RequestParser()
        {
            IRequestParser instance = Factory.RequestParser;
            Assert.That(instance, Is.TypeOf<RequestParser>());

            RequestParser requestParser = (RequestParser)instance;
            Assert.That(requestParser.EntityRepository, Is.Not.Null);
        }
    }
}
