// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class StringEntityAccessControlCheckTraceTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_TraceCheckAccess_NullResults()
        {
            Assert.That(
                () =>
                new StringEntityAccessControlCheckTrace().TraceCheckAccess(null, new Collection<EntityRef>(),
                                                             new EntityRef(RequestContext.GetContext().Identity.Id), null, 0),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("results"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TraceCheckAccess_NullPermissions()
        {
            Assert.That(
                () =>
                new StringEntityAccessControlCheckTrace().TraceCheckAccess(new Dictionary<long, bool>(), null,
                                                             new EntityRef(RequestContext.GetContext().Identity.Id), null, 0),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TraceCheckAccess_PermissionsContainsNull()
        {
            Assert.That(
                () =>
                new StringEntityAccessControlCheckTrace().TraceCheckAccess(new Dictionary<long, bool>(),
                                                             new Collection<EntityRef>() { null },
                                                             new EntityRef(RequestContext.GetContext().Identity.Id), null, 0),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TraceCheckAccess_NullUser()
        {
            Assert.That(
                () =>
                new StringEntityAccessControlCheckTrace().TraceCheckAccess(new Dictionary<long, bool>(), new Collection<EntityRef>(),
                                                             null, null, 0),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TraceCheckTypeAccess_NullResults( )
        {
            Assert.That(
                () =>
                new StringEntityAccessControlCheckTrace().TraceCheckTypeAccess(null, new EntityRef( ), new EntityRef(),
                                                             new EntityRef[0], 0),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("results"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TraceCheckTypeAccess_NullPermission( )
        {
            Assert.That(
                ( ) =>
                new StringEntityAccessControlCheckTrace( ).TraceCheckTypeAccess( new Dictionary<long, bool>( ), null, new EntityRef( ), new EntityRef [ 0 ], 0 ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "permission" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TraceCheckTypeAccess_NullUser( )
        {
            Assert.That(
                () =>
                new StringEntityAccessControlCheckTrace().TraceCheckTypeAccess( new Dictionary<long, bool>(), new EntityRef( ), null, new EntityRef[0], 0),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));            
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TraceCheckTypeAccess_NullEntityTypes( )
        {
            Assert.That(
                () =>
                new StringEntityAccessControlCheckTrace().TraceCheckTypeAccess( new Dictionary<long, bool>(), new EntityRef( ), new EntityRef(), null, 0),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TraceCheckTypeAccess_EntityTypesContainsNull( )
        {
            Assert.That(
                () =>
                new StringEntityAccessControlCheckTrace().TraceCheckTypeAccess( new Dictionary<long, bool>(), new EntityRef( ), new EntityRef(), new EntityRef[] { null }, 0),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }
        
    }
}
