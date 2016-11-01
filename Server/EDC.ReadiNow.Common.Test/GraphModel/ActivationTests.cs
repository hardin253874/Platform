// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using ReadiNow.EntityGraph.GraphModel;

namespace ReadiNow.EntityGraph.Test.GraphModel
{
    [TestFixture]
    public class ActivationTests
    {
        [Test]
        public void GraphEntityRepository_Instance( )
        {
            IEntityRepository instance = Factory.GraphEntityRepository;
            Assert.That( instance, Is.Not.Null );
        }
        
        [Test]
        public void GraphEntityRepository_Wiring( )
        {
            IEntityRepository instance = Factory.GraphEntityRepository;
            
            Assert.That( instance, Is.TypeOf<IdResolvingEntityRepository>( ), "instance" );

            IdResolvingEntityRepository idRepo = (IdResolvingEntityRepository)instance;
            Assert.That( idRepo.Inner, Is.TypeOf<GraphEntityRepository>( ), "idRepo.Inner" );

            GraphEntityRepository graphRepo = (GraphEntityRepository)idRepo.Inner;
            Assert.That( graphRepo.EntityAccessControlService, Is.Not.Null, "graphRepo.EntityAccessControlService" );
        }

    }

}
