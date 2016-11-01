// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Test.Model
{
    [TestFixture]
    public class UpgradeIdProviderTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void GetUpgradeId( )
        {
            long id = Entity.GetId( "core:type" );
            Guid upgradeId = Factory.UpgradeIdProvider.GetUpgradeId( id );

            Assert.That( upgradeId, Is.EqualTo( new Guid( "c0c6de86-2c65-47d2-bd5d-1751eed77378" ) ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetUpgradeIds( )
        {
            long id1 = Entity.GetId( "core:type" );
            long id2 = Entity.GetId( "core:resource" );
            IDictionary<long, Guid> result = Factory.UpgradeIdProvider.GetUpgradeIds( new[ ] { id1, id2, -1 } );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.Count, Is.EqualTo( 2 ) );
            Assert.That( result[ id1 ], Is.EqualTo( new Guid( "c0c6de86-2c65-47d2-bd5d-1751eed77378" ) ) );
            Assert.That( result[ id2 ], Is.EqualTo( new Guid( "69224903-ca05-48cb-a13d-786c49f08e18" ) ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetIdFromUpgradeId( )
        {
            long id = Factory.UpgradeIdProvider.GetIdFromUpgradeId( new Guid( "c0c6de86-2c65-47d2-bd5d-1751eed77378" ) );

            Assert.That( id, Is.EqualTo( WellKnownAliases.CurrentTenant.Type ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetIdsFromUpgradeIds( )
        {
            Guid id1 = new Guid( "c0c6de86-2c65-47d2-bd5d-1751eed77378" );
            Guid id2 = new Guid( "69224903-ca05-48cb-a13d-786c49f08e18" );
            Guid[ ] upgradeIds = new Guid[ ]
            {
                id1, id2, new Guid( "ffffffff-0000-0000-aaaa-ffffffffffff" )
            };
            IDictionary<Guid, long> result = Factory.UpgradeIdProvider.GetIdsFromUpgradeIds( upgradeIds );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.Count, Is.EqualTo( 2 ) );
            Assert.That( result[ id1 ], Is.EqualTo( WellKnownAliases.CurrentTenant.Type ) );
            Assert.That( result[ id2 ], Is.EqualTo( WellKnownAliases.CurrentTenant.Resource ) );
        }
    }
}

