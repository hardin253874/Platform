// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Sources;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test.Sources
{
    /// <summary>
    ///     The application manager tests class.
    /// </summary>
    [TestFixture]
    public class CloneEntityMemberRequestFactoryTests
    {
        /// <summary>
        ///     Tests the get dependents.
        /// </summary>
        [Test]
        [TestCase( "core:report", 15 )]
        [TestCase( "core:workflow", 15 )]
        [TestCase( "core:chart", 8 )]
        [TestCase( "core:type", 10 )]
        [TestCase( "console:screen", 8 )]
        [TestCase( "console:customEditForm", 8 )]
        [TestCase( "core:api", 5 )]
        [TestCase( "core:importConfig", 5 )]
        [RunAsDefaultTenant]
        public void Run( string alias, int minNodeCount )
        {
            var entityRepository = Factory.EntityRepository;
            var factory = new CloneEntityMemberRequestFactory( entityRepository );
            long typeId = new EntityRef( alias ).Id;

            EntityMemberRequest request = factory.CreateRequest( typeId );

            Assert.That( request, Is.Not.Null );

            long nodeCount = request.WalkNodes( ).Count( );
            Assert.That( nodeCount, Is.GreaterThan( minNodeCount ) );
            Assert.That( nodeCount, Is.LessThan( 50 ) );
        }
    }
}
